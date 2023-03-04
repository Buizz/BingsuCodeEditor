using BingsuCodeEditor.AutoCompleteToken;
using BingsuCodeEditor.EpScript;
using BingsuCodeEditor.LineColorDrawer;
using BingsuCodeEditor.Lua;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace BingsuCodeEditor
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CodeTextEditor : UserControl
    {

        #region #############프라이빗(코드분석)#############
        private bool LeftCtrlDown;
        private bool LeftShiftDown;
        private bool LeftAltDown;

        private DispatcherTimer dispatcherTimer;
        private CodeAnalyzer codeAnalyzer;

        private BackgroundWorker bg;
        private Thread thread;


        private DateTime markSameWordTimer;


        private void StartAnalyzeThread()
        {
            thread = new Thread(() =>
            {
                while (true)
                {
                    if(codeAnalyzer != null)
                    {
                        continue;
                    }


                    string codeText = aTextEditor.Text;
                    int offset = aTextEditor.CaretOffset;



                    DateTime dateTime = DateTime.Now;

                    //코드 분석 실행
                    if (codeAnalyzer.WorkCompete)
                    {
                        codeAnalyzer.Apply(codeText, offset);
                    }

                    try
                    {
                        aTextEditor.Dispatcher.Invoke(new Action(() =>
                        {
                            TimeSpan interval = DateTime.Now.Subtract(dateTime);
                            if (dateTime > markSameWordTimer)
                            {
                                highLightSelectItem();
                            }
                            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(interval.TotalMilliseconds * 2);



                            //오류 그리기
                            //DrawRedLine(0, 10);

                            //aTextEditor.TextArea.TextView.Redraw();




                            //테스트 트리거

                            //TestLog.Text = "";
                            ////CodeAnalyzer.TOKEN token = codeAnalyzer.GetToken(-1);
                            ////ToolTip.AppendText(codeAnalyzer.GetTokenCount() + ":" + "Caret:" + aTextEditor.CaretOffset.ToString());
                            ////if (token != null)
                            ////{
                            ////    ToolTip.AppendText("  " + token.StartOffset + ", " + token.EndOffset);
                            ////    ToolTip.AppendText(", " + token.Value);
                            ////}
                            //CodeAnalyzer.TOKEN tk = codeAnalyzer.GetToken(0);
                            //if (tk != null)
                            //{
                            //    TestLog.AppendText("TokenIndex : " + tk.Value.Replace("\r\n", "") + "\n");
                            //}
                            //else
                            //{
                            //    TestLog.AppendText("TokenIndex : null\n");
                            //}
                            //TestLog.AppendText("cursorLocation : " + codeAnalyzer.cursorLocation.ToString() + "\n");
                            //if (codeAnalyzer.tokenAnalyzer.IsError)
                            //{
                            //    foreach (var item in codeAnalyzer.tokenAnalyzer.ErrorList)
                            //    {
                            //        ToolTip.AppendText("Error : " + item.Message + "줄 : " + item.Line + "  열 : " + item.Column + "\n");
                            //    }
                            //}


                            //for (int i = -1; i <= 1; i++)
                            //{
                            //    ToolTip.AppendText(i + " : ");
                            //    CodeAnalyzer.TOKEN ftk = codeAnalyzer.GetToken(i);
                            //    if(ftk == null)
                            //    {
                            //        ToolTip.AppendText("null");
                            //    }
                            //    else
                            //    {
                            //        ToolTip.AppendText(ftk.Value);
                            //    }
                            //    ToolTip.AppendText("\n");
                            //}
                            //TestLog.AppendText("  " + interval.ToString());



                            //ErrorLogList.Items.Clear();

                            //ErrorLogList.Items.Add(codeAnalyzer.tokenAnalyzer.ErrorMessage);



                        }), DispatcherPriority.Normal);
                    }
                    catch (TaskCanceledException)
                    {

                    }
                }
            });
            thread.Start();
        }



        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            Deactivated();
        }


        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            codeAnalyzer = null;
        }

        DateTime bgStartTime;
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (codeAnalyzer != null && (bg == null || !bg.IsBusy))
            {
                string codeText = aTextEditor.Text;
                int offset = aTextEditor.CaretOffset;

                bg = new BackgroundWorker();

                bg.WorkerSupportsCancellation = true;
                bg.DoWork += Bg_DoWork;
                bg.RunWorkerCompleted += Bg_RunWorkerCompleted;

                object[] args = { codeText, offset };

                bgStartTime = DateTime.Now;
                bg.RunWorkerAsync(args);
            }else if (bg != null && bg.IsBusy)
            {
                if (DateTime.Now.Subtract(bgStartTime).TotalSeconds > 10)
                {
                    bg.CancelAsync();
                    ErrorText.Text = "컴파일 시간 초과 : 에디터를 재시작하세요.";

                }
            }
        }

        private void Bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Error != null)
            {
                //에러가 생길경우
                ErrorText.Text = e.Error.Message;
            }

        }

        private void Bg_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] args = (object[])e.Argument;

            string codeText = (string)args[0];
            int offset = (int)args[1];

            DateTime dateTime = DateTime.Now;

            //코드 분석 실행
            if (codeAnalyzer.WorkCompete)
            {
                codeAnalyzer.Apply(codeText, offset);
                CodeAnalyzer.TOKEN tk = codeAnalyzer.GetToken(0);

                //codeAnalyzer.maincontainer.innerFuncInfor.IsInnerFuncinfor

                bool isFuncInternall = false;
                string functooltiptext = "";
                int funclabelstartoffset = 0;
                if (codeAnalyzer.maincontainer.innerFuncInfor.IsInnerFuncinfor)
                {
                    isFuncInternall = true;
                    //int argindex = codeAnalyzer.maincontainer.innerFuncInfor.argindex;
                    funclabelstartoffset = codeAnalyzer.maincontainer.innerFuncInfor.funcename.First().StartOffset;


                    functooltiptext = codeAnalyzer.GetFuncToolTip().Trim();

                    //string currentfuncname = "";
                    //foreach (var item in codeAnalyzer.maincontainer.innerFuncInfor.funcename)
                    //{
                    //    if(currentfuncname != "")
                    //    {
                    //        currentfuncname += ".";
                    //    }
                    //    currentfuncname += item.Value;
                    //}
                    //툴팁텍스트를 아에 만들어서 보내자.

                }

                aTextEditor.Dispatcher.Invoke(new Action(() =>
                {
                    if (OpenSiginal)
                    {
                        if (OpenIsNameSpaceOpen)
                        {
                            if (tk != null)
                            {
                                completionWindowOpen(OpenInput, OpenIsNameSpaceOpen, OpenNoStartWithStartText);
                                OpenSiginal = false;
                            }
                        }
                        else
                        {
                            completionWindowOpen(OpenInput, OpenIsNameSpaceOpen, OpenNoStartWithStartText);
                            OpenSiginal = false;
                        }
                    }


                    TimeSpan interval = DateTime.Now.Subtract(dateTime);
                    if (dateTime > markSameWordTimer)
                    {
                        highLightSelectItem();
                    }
                    dispatcherTimer.Interval = TimeSpan.FromMilliseconds(interval.TotalMilliseconds * 2);

                    if (IsKeyDown)
                    {
                        IsKeyDown = false;


                        if (isFuncInternall)
                        {
                            functooltipTextBox.Text = functooltiptext;



                            if (IsKeyUDDown)
                            {
                                CloseTooltipBox();
                                OpenTooltipBox(funclabelstartoffset);
                                IsKeyUDDown = false;
                            }
                            else
                            {
                                OpenTooltipBox(funclabelstartoffset);
                            }
                        }
                    }

                    if (!codeAnalyzer.maincontainer.innerFuncInfor.IsInnerFuncinfor)
                    {
                        CloseTooltipBox();
                    }





                    #region ######################################################DEBUG######################################################
                    //오류 그리기
                    //DrawRedLine(0, 10);

                    aTextEditor.TextArea.TextView.Redraw();

                    //테스트 트리거
                    string debug = "";

                    //CodeAnalyzer.TOKEN token = codeAnalyzer.GetToken(-1);
                    //ToolTip.AppendText(codeAnalyzer.GetTokenCount() + ":" + "Caret:" + aTextEditor.CaretOffset.ToString());
                    //if (token != null)
                    //{
                    //    ToolTip.AppendText("  " + token.StartOffset + ", " + token.EndOffset);
                    //    ToolTip.AppendText(", " + token.Value);
                    //}
                    if (tk != null)
                    {
                        debug += "TokenIndex : " + tk.Value.Replace("\r\n", "") + "\n";
                    }
                    else
                    {
                        debug += "TokenIndex : null\n";
                    }

                    debug += "cursorLocation : " + codeAnalyzer.cursorLocation.ToString() + "\n";
                    //if (codeAnalyzer.tokenAnalyzer.IsError)
                    //{
                    //    foreach (var item in codeAnalyzer.tokenAnalyzer.ErrorList)
                    //    {
                    //        ToolTip.AppendText("Error : " + item.Message + "줄 : " + item.Line + "  열 : " + item.Column + "\n");
                    //    }
                    //}

                    //for (int i = -1; i <= 1; i++)
                    //{
                    //    ToolTip.AppendText(i + " : ");
                    //    CodeAnalyzer.TOKEN ftk = codeAnalyzer.GetToken(i);
                    //    if(ftk == null)
                    //    {
                    //        ToolTip.AppendText("null");
                    //    }
                    //    else
                    //    {
                    //        ToolTip.AppendText(ftk.Value);
                    //    }
                    //    ToolTip.AppendText("\n");
                    //}
                    debug += "  " + interval.ToString();

                    //TestLog.Text = debug;
                    //ErrorLogList.Items.Clear();

                    //ErrorLogList.Items.Add(codeAnalyzer.tokenAnalyzer.ErrorMessage);
                    #endregion
                }), DispatcherPriority.Normal);
            }
        }
        #endregion

        #region #############옵션#############

        private CodeType CurrentcodeType;
        public CodeType HighLighting
        {
            get
            {
                return CurrentcodeType;
            }
            set
            {
                CurrentcodeType = value;
                SetDefaultHighLight(CurrentcodeType);
            }
        }

        public CodeType Syntax
        {
            get
            {
                return CurrentcodeType;
            }
            set
            {
                CurrentcodeType = value;
                ScriptName.Text = value.ToString();
                SetDefaultHighLight(CurrentcodeType);
                SetAnalayer(CurrentcodeType);
            }
        }

        public bool ShowStatusBar
        {
            get
            {
                return (StatusBar.Visibility == Visibility.Visible);
            }
            set
            {
                if (value)
                {
                    StatusBar.Visibility = Visibility.Visible;
                }
                else
                {
                    StatusBar.Visibility = Visibility.Collapsed;
                }
            }
        }


        #endregion

        #region #############옵션 함수#############
        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            optionControl.OpenOption(this);
            optionControl.Visibility = Visibility.Visible;
        }

        public void TabSizeTextBoxRefresh()
        {
            if (aTextEditor.Options.ConvertTabsToSpaces)
            {
                tabSize.Content = "SpaceSize : " + aTextEditor.Options.IndentationSize;
            }
            else
            {
                tabSize.Content = "TabSize : " + aTextEditor.Options.IndentationSize;
            }
        }
        private void btnTabSize_Click(object sender, RoutedEventArgs e)
        {
            int intendsize = aTextEditor.Options.IndentationSize;



            switch (intendsize)
            {
                case 2:
                    intendsize = 4;
                    break;
                case 4:
                    intendsize = 8;
                    break;
                case 8:
                    intendsize = 12;
                    break;
                case 12:
                    intendsize = 2;
                    aTextEditor.Options.ConvertTabsToSpaces = !aTextEditor.Options.ConvertTabsToSpaces;
                    break;
            }
            aTextEditor.Options.IndentationSize = intendsize;



            if (aTextEditor.Options.ConvertTabsToSpaces)
            {
                tabSize.Content = "SpaceSize : " + intendsize;
            }
            else
            {
                tabSize.Content = "TabSize : " + intendsize;
            }
        }

        #endregion

        #region #############외부연결함수#############
        public event EventHandler Text_Change;
        public void Deactivated()
        {
            TBCtrlValue.Visibility = Visibility.Collapsed;
            LeftCtrlDown = false;
            TBShiftValue.Visibility = Visibility.Collapsed;
            LeftShiftDown = false;
            TBAltValue.Visibility = Visibility.Collapsed;   
            LeftAltDown = false;
        }

        public void CultureSetting()
        {
            TBLineText.Text = Lan.Line;
            TBColumnText.Text = Lan.Column;
        }

        private bool _isdark;
        public bool IsDark
        {
            get
            {
                return _isdark;
            }
            set
            {
                if (_isdark != value)
                {
                    _isdark = value;
                    SetDefaultHighLight(HighLighting);
                    functooltip.Background = (Brush)this.FindResource("MaterialDesignToolBarBackground");
                    functooltipTextBox.Background = (Brush)this.FindResource("MaterialDesignToolBarBackground");
                    functooltipTextBox.Foreground = (Brush)this.FindResource("MaterialDesignBody");

                    toolTip.Background = (Brush)this.FindResource("MaterialDesignToolBarBackground");
                    toolTipTextbox.Background = (Brush)this.FindResource("MaterialDesignToolBarBackground");
                    toolTipTextbox.Foreground = (Brush)this.FindResource("MaterialDesignBody");
                }
            }
        }


        public void SelectCurrentText()
        {
            CodeAnalyzer.TOKEN tk = codeAnalyzer.SearchToken(aTextEditor.CaretOffset, true, true);

            if(tk != null)
            {
                if (tk.Type == CodeAnalyzer.TOKEN_TYPE.String)
                {
                    aTextEditor.SelectionStart = tk.StartOffset + 1;
                    aTextEditor.SelectionLength = tk.EndOffset - tk.StartOffset - 2;
                }
                else
                {
                    aTextEditor.SelectionStart = tk.StartOffset;
                    aTextEditor.SelectionLength = tk.EndOffset - tk.StartOffset;
                }
            }
        } 


        public enum CodeType
        {
            epScript,
            Lua
        }


        public List<int> SaveFolding()
        {
            return codeAnalyzer.codeFoldingManager.SaveFodling();
        }

        public void LoadFolding(List<int> foldedList)
        {
            codeAnalyzer.codeFoldingManager.LoadFodling(foldedList);
        }

        public string SelectedText
        {
            get
            {
                return aTextEditor.SelectedText;
            }
            set
            {
                aTextEditor.SelectedText = value;
            }
        }
        public string Text
        {
            get
            {
                return aTextEditor.Text;
            }
            set
            {
                aTextEditor.Text = value;
            }
        }

        public struct CustomShortCut
        {
            public CustomShortCut(Key SystemKey, Key Key, RoutedEventHandler eventhandle)
            {
                this.SystemKey = SystemKey;
                this.Key = Key;
                this.eventhandle = eventhandle;
            }

            public Key SystemKey;
            public Key Key;
            public RoutedEventHandler eventhandle;
        }

        public List<CustomShortCut> ShortCutList = new List<CustomShortCut>();
        public void AddCustomMenuBtn(string header, string shortcut, Key systemKey, Key key, RoutedEventHandler clickEvent)
        {
            MenuItem menuItem = new MenuItem();
            menuItem.Header = header;
            menuItem.InputGestureText = shortcut;
            menuItem.Click += clickEvent;

            ShortCutList.Add(new CustomShortCut(systemKey, key, clickEvent));

            contextMenu.Items.Add(menuItem);
        }



        public void SetAnalayer(CodeType codetype)
        {
            switch (codetype)
            {
                case CodeType.epScript:
                    codeAnalyzer = new EpScriptAnalyzer(aTextEditor);

                    break;
                case CodeType.Lua:
                    codeAnalyzer = new LuaAnalyzer(aTextEditor);
                    break;
            }
            errorUnderLine = new ErrorUnderLine(codeAnalyzer);
            aTextEditor.TextArea.TextView.LineTransformers.Add(errorUnderLine);
        }

        public void SetCustomAnalayer(CodeAnalyzer codeAnalyzer)
        {
            this.codeAnalyzer = codeAnalyzer;
        }


        //public void SetCustomAnalayer(Stream s, string HighlightName, string exten)
        //{
        //    XmlTextReader reader = new XmlTextReader(s);

        //    IHighlightingDefinition highlightingDefinition = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);

        //    HighlightingManager.Instance.RegisterHighlighting(HighlightName, new string[] { exten }, highlightingDefinition);
        //    aTextEditor.SyntaxHighlighting = highlightingDefinition;
        //    functooltipTextBox.SyntaxHighlighting = highlightingDefinition;
        //}

        private XmlTextReader GetDefaultHighLight(CodeType highlight, bool IsDark)
        {
            string HighlightName = "";
            switch (highlight)
            {
                case CodeType.epScript:
                    HighlightName = "EpsHighlighting";
                    break;
                case CodeType.Lua:
                    HighlightName = "LuaHighlighting";
                    break;
            }

            if (IsDark)
            {
                HighlightName += "Dark";
            }
            else
            {
                HighlightName += "Light";
            }

            Assembly assembly = Assembly.GetExecutingAssembly();

            Stream s = assembly.GetManifestResourceStream(this.GetType(), HighlightName + ".xshd");
            XmlTextReader reader = new XmlTextReader(s);

            return reader;
        }
       
        public void SetDefaultHighLight(CodeType highlight)
        {
            string HighlightName;
            string exten;
            switch (highlight)
            {
                case CodeType.epScript:
                    HighlightName = "EpsHighlighting";
                    exten = ".eps";
                    break;
                case CodeType.Lua:
                    HighlightName = "LuaHighlighting";
                    exten = ".lua";
                    break;
                default:
                    return;
            }

            if (_isdark)
            {
                HighlightName += "Dark";
            }
            else
            {
                HighlightName += "Light";
            }

            Assembly assembly = Assembly.GetExecutingAssembly();

            Stream s = assembly.GetManifestResourceStream(this.GetType(), HighlightName + ".xshd");

            XmlTextReader reader = new XmlTextReader(s);

            IHighlightingDefinition highlightingDefinition = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);

            HighlightingManager.Instance.RegisterHighlighting(HighlightName, new string[] { exten }, highlightingDefinition);
            aTextEditor.SyntaxHighlighting = highlightingDefinition;
            functooltipTextBox.SyntaxHighlighting = highlightingDefinition;
            toolTipTextbox.SyntaxHighlighting = highlightingDefinition;
        }


        public void SetCustomHighLight(XmlTextReader reader)
        {
            IHighlightingDefinition highlightingDefinition = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);

            HighlightingManager.Instance.RegisterHighlighting("Custom", new string[] { ".*" }, highlightingDefinition);
            aTextEditor.SyntaxHighlighting = highlightingDefinition;
            functooltipTextBox.SyntaxHighlighting = highlightingDefinition;
            toolTipTextbox.SyntaxHighlighting = highlightingDefinition;
        }



        public void ResetAdditionalstring()
        {
            if(codeAnalyzer != null)
            {
                codeAnalyzer.ResetAdditionalstring();
            }
        }
        public void AddAdditionalstring(string key, string value)
        {
            if (codeAnalyzer != null)
            {
                codeAnalyzer.AddAdditionalstring(key, value);
            }
        }

        public void SetImportManager(ImportManager importManager)
        {
            codeAnalyzer.SetImportManager(importManager);
        }



        #endregion

        #region #############초기화#############

        ToolTip toolTip;
        TextEditor toolTipTextbox;
        ToolTip functooltip;
        TextEditor functooltipTextBox;
        public LanguageData Lan = new LanguageData();
        private ErrorUnderLine errorUnderLine;
        private void InitCtrl()
        {
            markSnippetWord = new MarkSnippetWord(aTextEditor);

            //< avalonedit:TextEditor x:Name = "ToolTip" VerticalScrollBarVisibility = "Hidden"
            //                         Background = "{DynamicResource MaterialDesignToolBarBackground}" TextElement.Foreground = "{DynamicResource MaterialDesignBody}"
            //                         HorizontalScrollBarVisibility = "Hidden" IsReadOnly = "True" IsHitTestVisible = "False" Text = "툴팁입니다." />

            toolTip = new ToolTip();

            toolTipTextbox = new TextEditor();
            toolTipTextbox.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            toolTipTextbox.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            toolTipTextbox.IsReadOnly = true;
            toolTipTextbox.IsHitTestVisible = false;

            toolTip.Background = (Brush)this.FindResource("MaterialDesignToolBarBackground");
            toolTipTextbox.Background = (Brush)this.FindResource("MaterialDesignToolBarBackground");
            toolTipTextbox.Foreground = (Brush)this.FindResource("MaterialDesignBody");
            toolTip.Content = toolTipTextbox;

            functooltip = new ToolTip();
            functooltip.StaysOpen = false;

            functooltip.Unloaded += Functooltip_Unloaded;


            functooltipTextBox = new TextEditor();
            functooltipTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            functooltipTextBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            functooltipTextBox.IsReadOnly = true;
            functooltipTextBox.IsHitTestVisible = false;
            functooltipTextBox.MinWidth = 250;

            functooltip.Background = (Brush)this.FindResource("MaterialDesignToolBarBackground");
            functooltipTextBox.Background = (Brush)this.FindResource("MaterialDesignToolBarBackground");
            functooltipTextBox.Foreground = (Brush)this.FindResource("MaterialDesignBody");

            //functooltipTextBox.SetValue(Control.BackgroundProperty, "MaterialDesignToolBarBackground");
            //functooltipTextBox.SetValue(Control.ForegroundProperty, "MaterialDesignBody");

            functooltip.Content = functooltipTextBox;

            aTextEditor.SetValue(FoldingMargin.FoldingMarkerBrushProperty, Brushes.LightGray);
            aTextEditor.SetValue(FoldingMargin.SelectedFoldingMarkerBrushProperty, Brushes.LightPink);
            aTextEditor.SetValue(FoldingMargin.SelectedFoldingMarkerBackgroundBrushProperty, Brushes.LightGray);


            aTextEditor.Options.ConvertTabsToSpaces = true;
            aTextEditor.TextArea.SelectionCornerRadius = 0.1;
            aTextEditor.Options.HighlightCurrentLine = true;

            searchPanel = SearchPanel.Install(aTextEditor);
            CBFontSize.Items.Clear();
            for (int i = 6; i < 64; i++)
            {
                CBFontSize.Items.Add(i);
                if (aTextEditor.FontSize == i)
                {
                    CBFontSize.SelectedIndex = CBFontSize.Items.Count - 1;
                }
            }

            //StartAnalyzeThread();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);

            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            dispatcherTimer.Start();


            aTextEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            aTextEditor.TextArea.TextEntered += TextArea_TextEntered;
            aTextEditor.TextArea.TextEntering += TextArea_TextEntering;

            AddCustomMenuBtn("개요 확장/축소", "Ctrl+G", Key.LeftCtrl, Key.G, new RoutedEventHandler(new Action<object, RoutedEventArgs>((e, x) =>
            codeAnalyzer.codeFoldingManager.FoldingFlip(aTextEditor.SelectionStart, aTextEditor.SelectionLength)
            )));
            contextMenu.Items.Add(new Separator());
        }


        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if(dispatcherTimer != null)
            {
                dispatcherTimer.Tick -= DispatcherTimer_Tick;
                dispatcherTimer.Dispatcher.UnhandledException -= Dispatcher_UnhandledException;

                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);

                dispatcherTimer.Tick += DispatcherTimer_Tick;
                dispatcherTimer.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
                dispatcherTimer.Start();
            }
        }

        private void Functooltip_Unloaded(object sender, RoutedEventArgs e)
        {
            if (completionWindow != null)
            {
                completionWindow.ForceUpdatePosition();
            }
        }



        public CodeTextEditor()
        {
            InitializeComponent();

            InitCtrl();
        }
        #endregion

        #region #############기본컨트롤이벤트#############
        private void aTextEditor_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (LeftCtrlDown)
            {
                if (e.Delta > 0)
                {
                    if (CBFontSize.SelectedIndex < CBFontSize.Items.Count)
                        CBFontSize.SelectedIndex += 1;
                }
                else if (e.Delta < 0)
                {
                    if (CBFontSize.SelectedIndex > 0)
                        CBFontSize.SelectedIndex -= 1;
                }
            }
        }

        private void CBFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            aTextEditor.FontSize = (int)CBFontSize.SelectedItem;
            SaveOption();
        }
        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            TBLineValue.Text = aTextEditor.TextArea.Caret.Line.ToString();
            TBColumnValue.Text = aTextEditor.TextArea.Caret.Column.ToString();
        }
        #endregion


        #region #############코드스니핏#############
        private MarkSnippetWord markSnippetWord;
        private bool TabAutoSnippetStart()
        {
            if (markSnippetWord.IsSnippetStart)
            {
                SnippetEnd(false);
            }

            if (IsSingleSelected())
            {
                string key = codeAnalyzer.GetDirectWord();

                if (codeAnalyzer.Template.ContainsKey(key))
                {
                    string temp = codeAnalyzer.Template[key];

                    string tabonce = gettabspace(true);
                    string tab = gettabspace(false);

                    temp = temp.Replace("[tab]", tab);
                    temp = temp.Replace("[tabonce]", tabonce);


                    int line = aTextEditor.Document.GetLineByOffset(aTextEditor.CaretOffset).LineNumber;

                    temp = markSnippetWord.Start(key, temp, line);

                    aTextEditor.Document.Insert(aTextEditor.CaretOffset, temp);
                    

                    aTextEditor.TextArea.TextView.LineTransformers.Add(markSnippetWord);

                    SnippetCommand(true);


                    markSnippetWord.IsSnippetStart = true;

                    return true;
                }
            }
            return false;
        }

        private void SnippetEnd(bool IsContent)
        {
            foreach (var markSnippet in aTextEditor.TextArea.TextView.LineTransformers.OfType<MarkSnippetWord>().ToList())
            {
                aTextEditor.TextArea.TextView.LineTransformers.Remove(markSnippet);
            }
            if (IsContent)
            {
                markSnippetWord.GotoContent();
            }
            markSnippetWord.Clear();
        }


        private bool SnippetDraw(Key syskey, Key key)
        {
            if (!markSnippetWord.IsSnippetStart)
            {
                return false;
            }

            bool IsInternal = markSnippetWord.CheckSnippetInternal();

            //텝, 엔터가 편집중에 있으면 엔터 누르면 컨텐츠로 아니면 그냥 엔터 실행


            switch (key)
            {
                case Key.Tab:
                    if (IsInternal)
                    {
                        if(completionWindow == null)
                        {
                            SnippetCommand();
                        }
                        else
                        {
                            markSnippetWord.TypeChangeStart();
                            completionWindow.CompletionList.RequestInsertion(new EventArgs());
                            markSnippetWord.TypeChangeEnd();
                        }
                        return true;
                    }
                    break;
                case Key.Enter:
                    if (IsInternal)
                    {
                        SnippetEnd(true);
                        return true;
                    }


                    SnippetEnd(false);
                    return false;
                case Key.Escape:
                    SnippetEnd(false);
                    return true;
                default:
                    if (markSnippetWord.CheckOuter())
                    {
                        SnippetEnd(false);
                    }
                    break;
            }




            return false;
        }

        private void SnippetCommand(bool IsFirst = false)
        {
            if (IsFirst)
            {
                markSnippetWord.SelectFirstItem();
            }
            else 
            {
                markSnippetWord.NextItem();
            }
        }


        #endregion



        #region #############자동완성#############

        //private Thread cmpthread;


        private bool OpenSiginal = false;
        private string OpenInput;
        private bool OpenIsNameSpaceOpen;
        private bool OpenNoStartWithStartText;
        private void CompletionWindowOpenAsync(string input, bool IsNameSpaceOpen = false, bool NoStartWithStartText = false)
        {
            OpenInput = input;
            //if (OpenSiginal)
            //{
            //    OpenInput += input;
            //}
            //else
            //{
            //    OpenInput = input;
            //}

            OpenSiginal = true;
            OpenIsNameSpaceOpen = IsNameSpaceOpen;
            OpenNoStartWithStartText = NoStartWithStartText;

            //completionWindowOpen(input, IsNameSpaceOpen);
        }



        CustomCompletionWindow completionWindow;
        private  void completionWindowOpen(string input, bool IsNameSpaceOpen = false, bool NoStartWithStartText = false)
        {
            //선택이 다중일 경우 사용하지 않음
            //엔터링에서 분석한 정보를 토대로, 자동완성창을 열거나 자동완성 목록을 생성
            //문자가 앞에 있을 경우 자동완성 사용안함
            //주석 등의 범위에 있을 경우 자동완성 사용안함. 이거는 코드 아날라이저가 알려주기로함
            //자동완성중이 아니면 마지막에 자동입력 실행



            if (codeAnalyzer == null)
                return;
            if (input.Length != 1)
                return;
            if (input != " " && string.IsNullOrWhiteSpace(input))
                return;


            CodeAnalyzer.TOKEN token = codeAnalyzer.GetToken(0);


            //자동완성 비활성(주석)
            if(token != null)
            {
                if (token.Type == CodeAnalyzer.TOKEN_TYPE.Comment || token.Type == CodeAnalyzer.TOKEN_TYPE.LineComment
                    || token.Type == CodeAnalyzer.TOKEN_TYPE.String)
                {
                    if (completionWindow != null)
                    {
                        completionWindow.Close();
                    }
                    return;
                }
            }
            //자동완성 비활성(문장 작석 중)
            //int caret = aTextEditor.CaretOffset - 1;
            //string text = aTextEditor.Text;
            //if(text.Length > caret && caret >= 0)
            //{
            //    char current = text[caret];
            //    if (char.IsLetterOrDigit(current) || current == '_')
            //    {
            //        return;
            //    }
            //}

            switch (codeAnalyzer.cursorLocation)
            {
                case CodeAnalyzer.CursorLocation.None:
                    break;
                case CodeAnalyzer.CursorLocation.FunctionArgType:
                case CodeAnalyzer.CursorLocation.ImportFile:
                case CodeAnalyzer.CursorLocation.FunctionName:

                case CodeAnalyzer.CursorLocation.ImportNameSpace:
                case CodeAnalyzer.CursorLocation.Keyword:
                    break;
                case CodeAnalyzer.CursorLocation.ForEachDefine:
                case CodeAnalyzer.CursorLocation.FunctionArgName:
                case CodeAnalyzer.CursorLocation.VarName:
                case CodeAnalyzer.CursorLocation.ObjectDefine:
                    return;
            }


            if (completionWindow != null)
            {
                return;
            }


            //현재 위치 확인
            completionWindow = new CustomCompletionWindow(aTextEditor.TextArea);
            completionWindow.FuncToolTip = functooltip;


            //cmpthread = new Thread(() =>
            //{
            //    DateTime dateTime = DateTime.Now;

            //    codeAnalyzer.Apply(codeText);


            //    aTextEditor.Dispatcher.Invoke(new Action(() =>
            //    {
            //        ToolTip.Text = DateTime.Now.Subtract(dateTime).ToString();
            //    }), DispatcherPriority.Normal);
            //});
            //cmpthread.Start();

            //await ();

            codeAnalyzer.WaitToUpdate(aTextEditor);


            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;

            codeAnalyzer.GetCompletionList(data, IsNameSpaceOpen);

            //data.Add(new CodeCompletionData("function", CompletionWordType.KeyWord));
            //data.Add(new CodeCompletionData("for", CompletionWordType.KeyWord));


            completionWindow.Open(input, IsNameSpaceOpen | NoStartWithStartText);


            if (!IsNameSpaceOpen)
            {
                //completionWindow.CompletionList.SelectItem(input);
            }

            completionWindow.Closed += delegate {
                completionWindow = null;
            };


            //if (data.Count == 0)
            //{
            //    completionWindow.Close();
            //    return;
            //}
        }



        #endregion



        #region #############키 입력#############


        private void OpenTooltipBox(int startoffset)
        {
            if (!IsKeyUDDown)
            {
                if (functooltip.IsLoaded) return;
            }

            if (functooltip.IsOpen)
            {
                functooltip.IsOpen = false;
            }
            functooltip.IsOpen = true;

            //double OrginXPos = 0;
            //double OrginYPos = 0;

            //TextViewPosition StartPostion = aTextEditor.TextArea.Caret.Position;

            TextViewPosition StartPosition = new TextViewPosition(aTextEditor.Document.GetLocation(startoffset));
            StartPosition.VisualColumn -= 1;

            //Point p = aTextEditor.TextArea.TextView.GetVisualPosition(StartPostion, VisualYPosition.LineTop);
            //OrginXPos = p.X + 36;
            //OrginYPos = p.Y - 5 - aTextEditor.VerticalOffset + 18;


            TextView textView = aTextEditor.TextArea.TextView;


            Point visualLocation, visualLocationTop;

            visualLocation = textView.GetVisualPosition(StartPosition, VisualYPosition.LineBottom);
            visualLocationTop = textView.GetVisualPosition(StartPosition, VisualYPosition.LineTop);

            // PointToScreen returns device dependent units (physical pixels)
            Point location = textView.PointToScreen(visualLocation - textView.ScrollOffset);
            Point locationTop = textView.PointToScreen(visualLocationTop - textView.ScrollOffset);

            // Let's use device dependent units for everything
            Size completionWindowSize = new Size(functooltip.ActualWidth, functooltip.ActualHeight).TransformToDevice(textView);
            Rect bounds = new Rect(location, completionWindowSize);
            Rect workingScreen = System.Windows.Forms.Screen.GetWorkingArea(location.ToSystemDrawing()).ToWpf();
            if (!workingScreen.Contains(bounds))
            {
                if (bounds.Left < workingScreen.Left)
                {
                    bounds.X = workingScreen.Left;
                }
                else if (bounds.Right > workingScreen.Right)
                {
                    bounds.X = workingScreen.Right - bounds.Width;
                }
                if (bounds.Bottom > workingScreen.Bottom)
                {
                    bounds.Y = locationTop.Y - bounds.Height;
                    functooltip.Tag = true;
                }
                else
                {
                    functooltip.Tag = false;
                }
                if (bounds.Y < workingScreen.Top)
                {
                    bounds.Y = workingScreen.Top;
                }
            }
            // Convert the window bounds to device independent units
            bounds = bounds.TransformFromDevice(textView);
            functooltip.HorizontalOffset = bounds.X;
            functooltip.VerticalOffset = bounds.Y;


            //functooltip.PlacementTarget = aTextEditor;
            functooltip.Placement = System.Windows.Controls.Primitives.PlacementMode.Absolute;
            if(completionWindow != null)
            {
                completionWindow.ForceUpdatePosition();
            }
        }
        private void CloseTooltipBox()
        {
            functooltip.IsOpen = false;
        }
        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            //TODO: 캐럿 분석을 실행
            //스페이스 등을 입력시 자동완성 입력 끝내기.

            if (e.Text.Length > 0 && completionWindow != null)
            {
                char t = e.Text[0];

                if (t == '.')
                {
                    if (completionWindow.CompletionList.SelectedItem != null)
                    {
                        if (completionWindow.CompletionList.SelectedItem.Text.IndexOf(".") != -1)
                        {
                            return;
                        }
                    }
                    //추가를 하긴 해야 할듯
                    //return;
                }


                if (!char.IsLetterOrDigit(t))
                {
                    if (t == ' ')
                    {
                        if (completionWindow.CompletionList.SelectedItem != null)
                        {
                            if (completionWindow.CompletionList.SelectedItem.Text.IndexOf(" ") != -1)
                            {
                                return;
                            }
                        }
                    }
                                   
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }




        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            //3
            if(aTextEditor.SelectionLength == 0)
            {
                codeAnalyzer.AutoDefaultInsert(e.Text);
            }
            if (!LeftCtrlDown && !LeftShiftDown)
            {
                if (e.Text == ".")
                {
                    CompletionWindowOpenAsync(e.Text, true);
                    //codeAnalyzer.AutoInsert("f");
                    return;
                }
                else if (e.Text == ":" || e.Text == "(" || e.Text == ",")
                {
                    CompletionWindowOpenAsync(e.Text, NoStartWithStartText: true);
                    //codeAnalyzer.AutoInsert("f");
                    return;
                }
                else if (e.Text == " ")
                {
                    if(codeAnalyzer.maincontainer.innerFuncInfor.IsInnerFuncinfor)
                    CompletionWindowOpenAsync(e.Text, NoStartWithStartText: true);
                    //codeAnalyzer.AutoInsert("f");
                    return;
                }
                else
                {
                    if(e.Text.Length == 1)
                    {
                        char t = e.Text[0];

                        string bstr = codeAnalyzer.GetDirectText(-2);
                        char bchar = ' ';

                        if (bstr != "")
                        {
                            bchar = bstr[0];
                        }

                        if (!char.IsLetter(bchar) && bchar != '_')
                        {
                            if (char.IsLetter(t) || t == '_' || codeAnalyzer.FuncPreChar.IndexOf(t.ToString()) != -1)
                            {
                                CompletionWindowOpenAsync(e.Text);
                                return;
                            }
                        }
                       
                    }
                }
            }
        }

        private void aTextEditor_TextChanged(object sender, EventArgs e)
        {
            //2
            if(markSnippetWord.IsSnippetStart)
            {
                //markSnippetWord.PosChange();
                markSnippetWord.TypeChangeEnd();
            }

            //타이핑 했을 경우 색칠을 늦춘다.
            markSameWordTimer = DateTime.Now.AddMilliseconds(1000);
            foreach (var markSameWord in aTextEditor.TextArea.TextView.LineTransformers.OfType<MarkSameWord>().ToList())
            {
                aTextEditor.TextArea.TextView.LineTransformers.Remove(markSameWord);
            }
            if(Text_Change != null) Text_Change(sender, e);
        }


        private bool IsSingleSelected()
        {
            if (!aTextEditor.TextArea.Selection.IsMultiline && aTextEditor.SelectionLength == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private string GetLineText(DocumentLine line)
        {
            int len = line.Length;
            int lineoffset = line.Offset;

            return aTextEditor.Document.GetText(lineoffset, len);
        }

        private void LineChange(bool IsUp)
        {
            return;

            int ulen;
            int ulineoffset;
            string ulinestr;

            int dlen;
            int dlineoffset;
            string dlinestr;


            int sLength = aTextEditor.SelectionLength;
            int sStart = aTextEditor.SelectionStart;


            if (aTextEditor.TextArea.Selection.IsMultiline && sLength == 0)
            {
                return;
            }

            if (sLength == 0)
            {
                //단일 줄
                int uCaret = aTextEditor.SelectionStart;
                int dCaret = uCaret + aTextEditor.SelectionLength;

                var cline = aTextEditor.Document.GetLineByOffset(uCaret);

                int totallen = cline.Length;
                int startoffset = cline.Offset;

                if (IsUp)
                {
                    dlineoffset = startoffset;
                    dlen = totallen;

                    var tline = cline.PreviousLine; //upline
                    if(tline == null)
                    {
                        return;
                    }
                    ulineoffset = tline.Offset;
                    ulen = tline.Length;

                    sStart -= totallen + 2;
                }
                else
                {
                    ulineoffset = startoffset;
                    ulen = totallen;

                    var tline = cline.NextLine; //downline
                    if (tline == null)
                    {
                        return;
                    }
                    dlineoffset = tline.Offset;
                    dlen = tline.Length;

                    sStart += totallen + 2;
                }
            }
            else
            {
                //다중 줄
                int uCaret = aTextEditor.SelectionStart;
                int dCaret = uCaret + aTextEditor.SelectionLength;


                var uline = aTextEditor.Document.GetLineByOffset(uCaret);
                var dline = aTextEditor.Document.GetLineByOffset(dCaret);

                int unumber = uline.LineNumber;
                int dnumber = dline.LineNumber;



                int totallen = dline.Offset - uline.Offset + dline.Length;
                //int totallen = uline.Length + dline.Length + (dnumber - unumber + 1) * 2;
                int startoffset = uline.Offset;

                if (IsUp)
                {
                    dlineoffset = startoffset;
                    dlen = totallen;

                    var tline = uline.PreviousLine; //upline
                    if (tline == null)
                    {
                        return;
                    }
                    ulineoffset = tline.Offset;
                    ulen = tline.Length;

                    sStart -= ulen + 2;
                }
                else
                {
                    ulineoffset = startoffset;
                    ulen = totallen;

                    var tline = dline.NextLine; //downline
                    if (tline == null)
                    {
                        return;
                    }
                    dlineoffset = tline.Offset;
                    dlen = tline.Length;

                    sStart += dlen + 2;
                }
            }

            dlinestr = aTextEditor.Document.GetText(dlineoffset, dlen);
            ulinestr = aTextEditor.Document.GetText(ulineoffset, ulen);


            aTextEditor.Document.Replace(ulineoffset, ulen + dlen + 2, dlinestr + "\r\n" + ulinestr);


            aTextEditor.SelectionStart = sStart;
            aTextEditor.SelectionLength = sLength;



            //aTextEditor.Document.Replace(ulineoffset, ulen + dlen + 2, "");



            //var currentLine = aTextEditor.Document.GetLineByOffset(aTextEditor.CaretOffset);
            //var otherLine = currentLine.PreviousLine;
            //var currentLine = aTextEditor.Document.GetLineByOffset(aTextEditor.CaretOffset);
            //var otherLine = currentLine.NextLine;


            //if(upline == null || downline == null)
            //{
            //    return;
            //}






            //int ulen = upline.Length;
            //int ulineoffset = upline.Offset;
            //int ulinenumber = upline.LineNumber;

            //string ulinestr = aTextEditor.Document.GetText(ulineoffset, ulen);



            //int dlen = downline.Length;
            //int dlineoffset = downline.Offset;
            //int dlinenumber = downline.LineNumber;

            //string dlinestr = aTextEditor.Document.GetText(dlineoffset, dlen);


            //aTextEditor.Document.Replace(ulineoffset, ulen + dlen + 2, dlinestr + "\r\n" + ulinestr);


        }

        private void TabMove(bool IsLeft)
        {
            string tab = gettabspace(true);

            int tabcount = gettablen();
            var line = aTextEditor.Document.GetLineByOffset(aTextEditor.CaretOffset);



            int lineoffset = aTextEditor.CaretOffset - line.Offset;


            string linestr = aTextEditor.Document.GetText(line.Offset, line.Length);



            string linesub = "";
            if (IsLeft)
            {
                if(0 <= lineoffset - tabcount)
                {
                    linesub = linestr.Substring(lineoffset - tabcount, tabcount);
                }
            }
            else
            {
                if (linestr.Length >= lineoffset + tabcount)
                {
                    linesub = linestr.Substring(lineoffset, tabcount);
                }
            }

            if(linesub.Count() == tabcount)
            {
                if (aTextEditor.Options.ConvertTabsToSpaces)
                {
                    linesub = linesub.Replace(" ", "");
                }
                else
                {
                    linesub = linesub.Replace("\t", "");
                }

                if (linesub == "")
                {
                    if (IsLeft)
                    {
                        aTextEditor.CaretOffset -= tabcount - 1;
                    }
                    else
                    {
                        aTextEditor.CaretOffset += tabcount - 1;
                    }
                }
            }
          

        }



        private bool IsKeyDown = false;
        private bool IsKeyUDDown = false;
        private void aTextEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.SystemKey != Key.LeftCtrl && e.Key != Key.LeftCtrl)
            {

            }


            string input = e.Key.ToString();
            if (!LeftCtrlDown && !LeftShiftDown)
            {
                if (input == "OemPeriod")
                {
                    //
                    //codeAnalyzer.GetToken(0);
                    //.일경우 네임스페이스 확인
                    //return;
                }
                else
                {
                    //CompletionWindowOpenAsync(input);
                }
            }
            if (SnippetDraw(e.SystemKey, e.Key))
            {
                e.Handled = true;
                return;
            }
            switch (e.SystemKey)
            {
                case Key.LeftCtrl:
                    TBCtrlValue.Visibility = Visibility.Visible;
                    LeftCtrlDown = true;
                    break;
                case Key.LeftShift:
                    TBShiftValue.Visibility = Visibility.Visible;
                    LeftShiftDown = true;
                    break;
                case Key.LeftAlt:
                    TBAltValue.Visibility = Visibility.Visible;
                    LeftAltDown = true;
                    e.Handled = true;
                    break;
                case Key.Up:
                    if (LeftAltDown && !LeftShiftDown)
                    {
                        LineChange(true);
                    }
                    break;
                case Key.Down:
                    if (LeftAltDown && !LeftShiftDown)
                    {
                        LineChange(false);
                    }
                    break;
                case Key.ImeProcessed:
                    if (LeftCtrlDown)
                    {
                        TBCtrlValue.Visibility = Visibility.Collapsed;
                        LeftCtrlDown = false;
                        codeAnalyzer.SetCommentLine(aTextEditor.SelectionStart, aTextEditor.SelectionStart + aTextEditor.SelectionLength, CodeAnalyzer.CommentType.Toggle);
                    }
                    break;
            }


            if (functooltip.IsOpen)
            {
                if (e.Key == Key.Up || e.Key == Key.Down)
                {
                    IsKeyUDDown = true;
                }
            }

            if (markSnippetWord.IsSnippetStart)
            {
                if(e.Key != Key.Up && e.Key != Key.Down)
                {
                    if (!markSnippetWord.TypeChangeStart())
                    {
                        SnippetEnd(false);
                    }
                }
           
            }


            switch (e.Key)
            {
                case Key.Left:
                    TabMove(true);
                    break;
                case Key.Right:
                    TabMove(false);
                    break;
                case Key.Tab:
                case Key.Up:
                case Key.Down:
                case Key.Enter:
                    if (completionWindow != null)
                    {
                        if (completionWindow.CompletionList.ListBox.Items.Count == 0)
                        {
                            completionWindow.Close();
                        }
                    }
                    break;
            }

    


            switch (e.Key)
            {
                case Key.Back:
                    {
                        if (codeAnalyzer.AutoDefaultRemove())
                        {
                            e.Handled = true;
                            break;
                        }


                        if (codeAnalyzer.AutoRemove())
                        {
                            e.Handled = true;
                            break;
                        }


                        var currentLine = aTextEditor.Document.GetLineByOffset(aTextEditor.CaretOffset);
                        int len = currentLine.Length;
                        if (len % 4 == 0 && len != 0)
                        {
                            //4의 배수일 경우
                            if (aTextEditor.SelectionLength == 0 && aTextEditor.SelectionStart >= 4)
                            {
                                int lineoffset = currentLine.Offset;
                                //라인의 끝일 경우
                                if (aTextEditor.CaretOffset == lineoffset + len)
                                {
                                    string line = aTextEditor.Document.GetText(lineoffset, len);


                                    if (aTextEditor.Options.ConvertTabsToSpaces)
                                    {
                                        if (line.Replace(" ", "") == "")
                                        {
                                            e.Handled = true;
                                            //모든 문자열이 스페이스
                                            aTextEditor.SelectionStart -= aTextEditor.Options.IndentationSize;
                                            aTextEditor.SelectionLength = aTextEditor.Options.IndentationSize;
                                            aTextEditor.SelectedText = "";
                                        }
                                    }
                                    else
                                    {
                                        if (line.Replace("\t", "") == "")
                                        {
                                            e.Handled = true;
                                            //모든 문자열이 스페이스
                                            aTextEditor.SelectionStart -= 1;
                                            aTextEditor.SelectionLength = 1;
                                            aTextEditor.SelectedText = "";
                                        }
                                    }

                                   
                                }
                            }
                        }
                    }       
                    break;
                case Key.OemQuotes:
                    //""로 감싸기 등
                    {
                        int len = aTextEditor.SelectionLength;

                        if(len != 0)
                        {
                            e.Handled = true;
                            aTextEditor.SelectionLength = 0;
                            aTextEditor.SelectedText = "\"";
                            aTextEditor.SelectionLength = 0;
                            aTextEditor.SelectionStart += len + 1;
                            aTextEditor.SelectedText = "\"";
                            aTextEditor.SelectionStart -= len;
                            aTextEditor.SelectionLength = len;
                        }
                    }
                    break;
                case Key.LeftCtrl:
                    TBCtrlValue.Visibility = Visibility.Visible;
                    LeftCtrlDown = true;
                    break;
                case Key.LeftShift:
                    TBShiftValue.Visibility = Visibility.Visible;
                    LeftShiftDown = true;
                    break;
                case Key.LeftAlt:
                    TBAltValue.Visibility = Visibility.Visible;
                    LeftAltDown = true;
                    break;
                case Key.F:
                    if (LeftCtrlDown)
                    {
                        TBCtrlValue.Visibility = Visibility.Collapsed;
                        LeftCtrlDown = false;
                        SearchPanelOpen();
                    }
                    break;
                //case Key.ImeProcessed:
                case Key.OemQuestion:
                    if (LeftCtrlDown)
                    {
                        TBCtrlValue.Visibility = Visibility.Collapsed;
                        LeftCtrlDown = false;
                        codeAnalyzer.SetCommentLine(aTextEditor.SelectionStart, aTextEditor.SelectionStart + aTextEditor.SelectionLength, CodeAnalyzer.CommentType.Toggle);
                        e.Handled = true;
                    }
                    break;
                case Key.Escape:
                    if (IsSearchPanelOpen)
                    {
                        SearchPanelClose();
                    }                        
                    break;
                case Key.Tab:
                    if (TabAutoSnippetStart())
                    {
                        if(completionWindow != null)
                        {
                            completionWindow.Close();
                        }
                        e.Handled = true;
                    }                 
                    break;
                case Key.Enter:
                    {
                        string fstr = codeAnalyzer.GetDirectText(-1);
                        string bstr = codeAnalyzer.GetDirectText(0);

                        int spacecount = 0;
                        string tabstr = gettabspace(false);
                        spacecount = tabstr.Length;

                        string tabonce = gettabspace(true);

                        if (fstr == "{")
                        {
                            e.Handled = true;
                            codeAnalyzer.DirectInsetTextFromCaret("\n" + tabstr + tabonce, IsMove: true);

                            if (bstr == "}")
                            {
                                codeAnalyzer.DirectInsetTextFromCaret("\n" + tabstr, IsMove: false);
                            }
                        }
                    }
                    break;
            }
            if(e.Key.ToString().Length == 1)
            {
                if(ShortCut(e.Key)) e.Handled = true;
            }

            bool rval = codeAnalyzer.AutoInsert(e.Key.ToString());
            if(!e.Handled)
            {
                e.Handled = rval;
            }

            IsKeyDown = true;
        }


        private Key LastKey;
        private Key LastSystemKey;
        private bool ShortCut(Key key)
        {
            foreach (var item in ShortCutList)
            {
                if (item.Key == key)
                {
                    if (LeftCtrlDown && item.SystemKey == Key.LeftCtrl)
                    {
                        item.eventhandle.Invoke(this, new RoutedEventArgs());
                        return true;
                    }
                }               
            }


            if(key != Key.K && LastKey == Key.None)
            {
                return false;
            }


            //LeftCtrlDown  LeftCtrlDown  LeftAltDown
            switch(LastKey)
            {
                case Key.K:
                    switch (key)
                    {
                        case Key.U:
                            //주석 온
                            codeAnalyzer.SetCommentLine(aTextEditor.SelectionStart, aTextEditor.SelectionStart + aTextEditor.SelectionLength, CodeAnalyzer.CommentType.Clear);
                            LastKey = Key.None;
                            LastSystemKey = Key.None;
                            ShortCutText.Text = "";
                            return true;
                        case Key.C:
                            //주석 오프
                            codeAnalyzer.SetCommentLine(aTextEditor.SelectionStart, aTextEditor.SelectionStart + aTextEditor.SelectionLength, CodeAnalyzer.CommentType.Set);
                            LastKey = Key.None;
                            LastSystemKey = Key.None;
                            ShortCutText.Text = "";
                            return true;
                    }
                    break;
            }
   
            


            if (LastKey == Key.None)
            {
                if (LeftCtrlDown)
                {
                    LastKey = key;
                    ShortCutText.Text = "(" + LastKey.ToString();
                    LastSystemKey = Key.LeftCtrl;
                    ShortCutText.Text += " + " + LastSystemKey.ToString();
                    ShortCutText.Text += ")를 눌렀습니다.";
                }
                else
                {
                    ShortCutText.Text = "";
                }

            }
            else
            {
                ShortCutText.Text = "(" + LastKey.ToString();
                if (LastSystemKey != Key.None)
                {
                    ShortCutText.Text += " + " + LastSystemKey.ToString();
                }


                ShortCutText.Text += "," + key.ToString();
                if (LeftCtrlDown)
                {
                    ShortCutText.Text += " + " + Key.LeftCtrl.ToString();
                }


                ShortCutText.Text += ")는 단축키가 아닙니다.";
                LastKey = Key.None;
                LastSystemKey = Key.None;
                if (!LeftCtrlDown) return true;
            }
            return false;
        }


        private int gettablen()
        {
            int intendsize = aTextEditor.Options.IndentationSize;

            if (!aTextEditor.Options.ConvertTabsToSpaces)
            {
                intendsize = 1;
            }

            return intendsize;
        }

        private string gettabspace(bool IsOnce)
        {
            int intendsize = aTextEditor.Options.IndentationSize;

            string tabchar = " ";
            if (!aTextEditor.Options.ConvertTabsToSpaces)
            {
                intendsize = 1;
                tabchar = "\t";
            }


            if (IsOnce)
            {
                string tabonce = "";
                for (int i = 0; i < intendsize; i++)
                {
                    tabonce += tabchar;
                }
                return tabonce;
            }
            else
            {
                var currentLine = aTextEditor.Document.GetLineByOffset(aTextEditor.CaretOffset);
                int len = currentLine.Length;
                int lineoffset = currentLine.Offset;
                string line = aTextEditor.Document.GetText(lineoffset, len);

                string tabstr = "";
                if (len != 0)
                {
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (line[i] == tabchar[0])
                        {
                            tabstr += tabchar;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                return tabstr;
            }
        }




        private void aTextEditor_PreviewKeyUp(object sender, KeyEventArgs e)
        {

            switch (e.SystemKey)
            {
                case Key.LeftCtrl:
                    TBCtrlValue.Visibility = Visibility.Collapsed;
                    LeftCtrlDown = false;
                    break;
                case Key.LeftShift:
                    TBShiftValue.Visibility = Visibility.Collapsed;
                    LeftShiftDown = false;
                    break;
                case Key.LeftAlt:
                    TBAltValue.Visibility = Visibility.Collapsed;
                    LeftAltDown = false;
                    break;
            }


            switch (e.Key)
            {
                case Key.LeftCtrl:
                    TBCtrlValue.Visibility = Visibility.Collapsed;
                    LeftCtrlDown = false;
                    break;
                case Key.LeftShift:
                    TBShiftValue.Visibility = Visibility.Collapsed;
                    LeftShiftDown = false;
                    break;
                case Key.LeftAlt:
                    TBAltValue.Visibility = Visibility.Collapsed;
                    LeftAltDown = false;
                    break;
            }

            if (markSnippetWord.IsSnippetStart)
            {
                markSnippetWord.TypeChangeEnd();
            }
        }
        #endregion

        #region #############검색박스#############

        private SearchPanel searchPanel;
        public bool IsSearchPanelOpen
        {
            get
            {
                return !searchPanel.IsClosed;
            }
        }

        public string SetFilePath {
            get
            {
                return codeAnalyzer.FilePath;
            }
            set
            {
                codeAnalyzer.FilePath = value;
            }
        }

        public void SearchPanelOpen()
        {
            TextSearchBox.Visibility = Visibility.Visible;

            TextSearchBox.UpdateLayout();

            FindText.Focus();
            FindText.Text = aTextEditor.SelectedText;

            searchPanel.Open();
            searchPanel.Visibility = Visibility.Hidden;
        }


        public void SearchPanelClose()
        {
            searchPanel.Close();
            TextSearchBox.Visibility = Visibility.Collapsed;
            aTextEditor.Focus();
        }

        public void SearchNext()
        {
            searchPanel.FindNext();
        }

        private void SearchCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchPanelClose();
        }
        private void FindText_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchPanel.SearchPattern = FindText.Text;
        }
        private void FindText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (e.Key == Key.Enter)
                        SearchNext();
                    break;
                case Key.Escape:
                    SearchPanelClose();
                    break;
            }

        }
        private void ReplaceText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    ExecReplace();
                    break;
                case Key.Escape:
                    SearchPanelClose();
                    break;
            }
        }
        private void FindBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchNext();
            FindText.Focus();
        }

        private void ReplaceBtn_Click(object sender, RoutedEventArgs e)
        {
            ExecReplace();
        }
        private void ExecReplace()
        {
            if (aTextEditor.SelectedText == FindText.Text)
            {
                aTextEditor.SelectedText = ReplaceText.Text;
                ReplaceText.Focus();
            }
            else
            {
                SearchNext();
                aTextEditor.SelectedText = ReplaceText.Text;
                ReplaceText.Focus();
            }
        }

        private void ReplaceAllBtn_Click(object sender, RoutedEventArgs e)
        {

            aTextEditor.Text = aTextEditor.Text.Replace(FindText.Text, ReplaceText.Text);
            FindText.Focus();
        }
        #endregion

        #region #############문단분석#############


        private string lastCurrentToeknValue;



        private void aTextEditor_MouseHover(object sender, MouseEventArgs e)
        {
            Point mosPoint = e.GetPosition(aTextEditor);

            var pos = aTextEditor.GetPositionFromPoint(mosPoint);
            if (pos != null)
            {
                TextViewPosition cpos = (TextViewPosition)pos;

                Point relmosPoint = e.GetPosition(aTextEditor.TextArea.TextView);
                Point viewPoint = aTextEditor.TextArea.TextView.GetVisualPosition(cpos, VisualYPosition.TextMiddle)
                    - aTextEditor.TextArea.TextView.ScrollOffset;

                bool IsRight = false;
                if(relmosPoint.X > viewPoint.X)
                {
                    IsRight = true;
                }

                //toolTip.PlacementTarget = this; // required for property inheritance

                int line = cpos.Line - 1;
                int col = cpos.Column - 1;

                DocumentLine dline = aTextEditor.Document.Lines[line];
                if (dline.Length == col)
                {
                    return;
                }
                

                double offset = dline.Offset + col;

                if (IsRight)
                {
                    offset += 0.5;
                }
                else
                {
                    offset -= 0.5;
                }

                CodeAnalyzer.TOKEN tk = codeAnalyzer.SearchToken(offset, true, true);
                if(tk != null)
                {
                    if(tk.errorToken == null)
                    {
                        string t = codeAnalyzer.GetTooiTipText(tk);
                        if (t == "") return;

                        toolTipTextbox.Text = t;
                    }
                    else
                    {
                        toolTipTextbox.Text = tk.errorToken.Message;
                    }
                    //toolTip.Content = offset +"/" + tk.StartOffset + "," + tk.EndOffset;
                    toolTip.FontSize = aTextEditor.FontSize;
                    toolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse;
                    toolTip.IsOpen = true;
                }
                //TODO:호버분석 마무리
             
                e.Handled = true;
            }
        }

        private void aTextEditor_MouseHoverStopped(object sender, MouseEventArgs e)
        {
            toolTip.IsOpen = false;
        }



        #endregion


        #region #############Draw함수#############
        private void highLightSelectItem()
        {
            CodeAnalyzer.TOKEN token = codeAnalyzer.GetToken(0);

            if (token != null && token.Type == CodeAnalyzer.TOKEN_TYPE.Identifier)
            {
                if (lastCurrentToeknValue != token.Value)
                {
                    lastCurrentToeknValue = token.Value;
                    foreach (var markSameWord in aTextEditor.TextArea.TextView.LineTransformers.OfType<MarkSameWord>().ToList())
                    {
                        aTextEditor.TextArea.TextView.LineTransformers.Remove(markSameWord);
                    }

                    aTextEditor.TextArea.TextView.LineTransformers.Add(new MarkSameWord(token.Value));
                }
            }
            else
            {
                lastCurrentToeknValue = "";
                foreach (var markSameWord in aTextEditor.TextArea.TextView.LineTransformers.OfType<MarkSameWord>().ToList())
                {
                    aTextEditor.TextArea.TextView.LineTransformers.Remove(markSameWord);
                }
            }
        }





        #endregion

 
    }
}
