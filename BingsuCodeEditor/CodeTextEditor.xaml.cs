using BingsuCodeEditor.EpScript;
using BingsuCodeEditor.LineColorDrawer;
using BingsuCodeEditor.Lua;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

                            ToolTip.Text = "";
                            //CodeAnalyzer.TOKEN token = codeAnalyzer.GetToken(-1);
                            //ToolTip.AppendText(codeAnalyzer.GetTokenCount() + ":" + "Caret:" + aTextEditor.CaretOffset.ToString());
                            //if (token != null)
                            //{
                            //    ToolTip.AppendText("  " + token.StartOffset + ", " + token.EndOffset);
                            //    ToolTip.AppendText(", " + token.Value);
                            //}
                            CodeAnalyzer.TOKEN tk = codeAnalyzer.GetToken(0);
                            if (tk != null)
                            {
                                ToolTip.AppendText("TokenIndex : " + tk.Value.Replace("\r\n", "") + "\n");
                            }
                            else
                            {
                                ToolTip.AppendText("TokenIndex : null\n");
                            }
                            ToolTip.AppendText("cursorLocation : " + codeAnalyzer.cursorLocation.ToString() + "\n");
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
                            ToolTip.AppendText("  " + interval.ToString());



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


        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (codeAnalyzer != null && (thread == null || !thread.IsAlive))
            {
                string codeText = aTextEditor.Text;
                int offset = aTextEditor.CaretOffset;


                thread = new Thread(async () =>
                {
                    DateTime dateTime = DateTime.Now;

                    //코드 분석 실행
                    if (codeAnalyzer.WorkCompete)
                    {
                        codeAnalyzer.Apply(codeText, offset);
                        try
                        {
                            await aTextEditor.Dispatcher.InvokeAsync(new Action(() =>
                            {
                                CodeAnalyzer.TOKEN tk = codeAnalyzer.GetToken(0);
                                if (OpenSiginal)
                                {
                                    if (OpenIsNameSpaceOpen)
                                    {
                                        if (tk != null)
                                        {
                                            completionWindowOpen(OpenInput, OpenIsNameSpaceOpen);
                                            OpenSiginal = false;
                                        }
                                    }
                                    else
                                    {
                                        completionWindowOpen(OpenInput, OpenIsNameSpaceOpen);
                                        OpenSiginal = false;
                                    }
                                }


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

                                ToolTip.Text = "";
                                //CodeAnalyzer.TOKEN token = codeAnalyzer.GetToken(-1);
                                //ToolTip.AppendText(codeAnalyzer.GetTokenCount() + ":" + "Caret:" + aTextEditor.CaretOffset.ToString());
                                //if (token != null)
                                //{
                                //    ToolTip.AppendText("  " + token.StartOffset + ", " + token.EndOffset);
                                //    ToolTip.AppendText(", " + token.Value);
                                //}
                                if (tk != null)
                                {
                                    ToolTip.AppendText("TokenIndex : " + tk.Value.Replace("\r\n", "") + "\n");
                                }
                                else
                                {
                                    ToolTip.AppendText("TokenIndex : null\n");
                                }

                                ToolTip.AppendText("cursorLocation : " + codeAnalyzer.cursorLocation.ToString() + "\n");
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
                                ToolTip.AppendText("  " + interval.ToString());



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
        }
        #endregion

        #region #############옵션#############

        private CodeEditorOptions codeEditorOptions;
        public struct CodeEditorOptions
        {
            //글꼴이나 기타 하이라이팅
        }

        public CodeEditorOptions Options
        {
            get
            {
                return codeEditorOptions;
            }
            set
            {

            }
        }


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
                SetHighLight(CurrentcodeType);
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
                SetHighLight(CurrentcodeType);
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

        public bool ShowLineNumbers
        {
            get
            {
                return aTextEditor.ShowLineNumbers;
            }
            set
            {
                aTextEditor.ShowLineNumbers = value;
            }
        }
        #endregion

        #region #############외부연결함수#############
        public void Deactivated()
        {
            TBCtrlValue.Visibility = Visibility.Collapsed;
            LeftCtrlDown = false;
            TBShiftValue.Visibility = Visibility.Collapsed;
            LeftShiftDown = false;
        }

        public void CultureSetting()
        {
            TBLineText.Text = Lan.Line;
            TBColumnText.Text = Lan.Column;
        }

        private bool isdark;
        public bool IsDark
        {
            get
            {
                return isdark;
            }
            set
            {
                if (isdark != value)
                {
                    isdark = value;
                    SetHighLight(HighLighting);
                }
            }
        }

        public enum CodeType
        {
            epScript,
            Lua
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


        public void SetCustomAnalayer(Stream s, string HighlightName, string exten)
        {
            XmlTextReader reader = new XmlTextReader(s);

            IHighlightingDefinition highlightingDefinition = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);

            HighlightingManager.Instance.RegisterHighlighting(HighlightName, new string[] { exten }, highlightingDefinition);
            aTextEditor.SyntaxHighlighting = highlightingDefinition;
            ToolTip.SyntaxHighlighting = highlightingDefinition;
        }

       
        public void SetHighLight(CodeType highlight)
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

            if (isdark)
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
            ToolTip.SyntaxHighlighting = highlightingDefinition;
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




        #endregion

        #region #############초기화#############

        public LanguageData Lan = new LanguageData();
        private ErrorUnderLine errorUnderLine;
        private void InitCtrl()
        {
            markSnippetWord = new MarkSnippetWord(aTextEditor);


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
            dispatcherTimer.Start();


            aTextEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            aTextEditor.TextArea.TextEntered += TextArea_TextEntered;
            aTextEditor.TextArea.TextEntering += TextArea_TextEntering;
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
                CodeAnalyzer.TOKEN tk = codeAnalyzer.GetToken(0);
                if(tk == null)
                {
                    return false;
                }
                if (codeAnalyzer.Template.ContainsKey(tk.Value))
                {
                    string temp = codeAnalyzer.Template[tk.Value];

                    string tabonce = gettabspace(true);
                    string tab = gettabspace(false);

                    temp = temp.Replace("[tab]", tab);
                    temp = temp.Replace("[tabonce]", tabonce);


                    int line = aTextEditor.Document.GetLineByOffset(aTextEditor.CaretOffset).LineNumber;

                    temp = markSnippetWord.Start(tk.Value, temp, line);

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
                        SnippetCommand();
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
        private void CompletionWindowOpenAsync(string input, bool IsNameSpaceOpen = false)
        {
            if (OpenSiginal)
            {
                OpenInput += input;
            }
            else
            {
                OpenInput = input;
            }

            OpenSiginal = true;
            OpenIsNameSpaceOpen = IsNameSpaceOpen;
      

            //completionWindowOpen(input, IsNameSpaceOpen);
        }



        CustomCompletionWindow completionWindow;
        private  void completionWindowOpen(string input, bool IsNameSpaceOpen = false)
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
            if (string.IsNullOrWhiteSpace(input))
                return;


            CodeAnalyzer.TOKEN token = codeAnalyzer.GetToken(0);


            //자동완성 비활성(주석)
            if(token != null)
            {
                if (token.Type == CodeAnalyzer.TOKEN_TYPE.Comment || token.Type == CodeAnalyzer.TOKEN_TYPE.LineComment
                    || token.Type == CodeAnalyzer.TOKEN_TYPE.String || token.Type == CodeAnalyzer.TOKEN_TYPE.Special)
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



                case CodeAnalyzer.CursorLocation.ImportNameSpace:
                case CodeAnalyzer.CursorLocation.Keyword:
                    break;
                case CodeAnalyzer.CursorLocation.FunctionArgName:
                case CodeAnalyzer.CursorLocation.ObjectName:
                case CodeAnalyzer.CursorLocation.FunctionName:
                    return;
            }


            if (completionWindow != null)
            {
                return;
            }


            //현재 위치 확인
            completionWindow = new CustomCompletionWindow(aTextEditor.TextArea);



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
            completionWindow.Open(input, IsNameSpaceOpen);
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
        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            //TODO: 캐럿 분석을 실행
            //스페이스 등을 입력시 자동완성 입력 끝내기.

            if (e.Text.Length > 0 && completionWindow != null)
            {
                if(e.Text[0] == '.')
                {
                    //추가를 하긴 해야 할듯
                    //return;
                }


                if (!char.IsLetterOrDigit(e.Text[0]))
                {
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
                CodeAnalyzer.TOKEN ctoken = codeAnalyzer.GetToken(0);
                switch (e.Text)
                {
                    case "{":
                        if (codeAnalyzer.IsLineEnd || ctoken.Value == "}")
                        {
                            aTextEditor.SelectedText = "}";
                            aTextEditor.SelectionLength = 0;
                        }
                        break;
                    case "[":
                        if (codeAnalyzer.IsLineEnd || ctoken.Value == "]")
                        {
                            aTextEditor.SelectedText = "]";
                            aTextEditor.SelectionLength = 0;
                        }
                        break;
                    case "(":
                        if (codeAnalyzer.IsLineEnd || ctoken.Value == ")")
                        {
                            aTextEditor.SelectedText = ")";
                            aTextEditor.SelectionLength = 0;
                        }
                        break;
                    case "}":
                    case "]":
                    case ")":
                        if (ctoken != null && !codeAnalyzer.IsLineEnd && ctoken.Value == e.Text)
                        {
                            aTextEditor.SelectionLength = 1;
                            aTextEditor.SelectedText = "";
                            aTextEditor.SelectionLength = 0;
                        }
                        break;
                    case "\"":
                    case "\'":
                        if (ctoken == null && codeAnalyzer.IsLineEnd)
                        {
                            aTextEditor.SelectedText = e.Text;
                            aTextEditor.SelectionLength = 0;
                        }
                        else if (ctoken != null && !codeAnalyzer.IsLineEnd
                            && ctoken.Type == CodeAnalyzer.TOKEN_TYPE.String
                            && codeAnalyzer.LineString() == e.Text)
                        {
                            aTextEditor.SelectionLength = 1;
                            aTextEditor.SelectedText = "";
                            aTextEditor.SelectionLength = 0;
                        }
                        break;

                }
            }

            if (!LeftCtrlDown && !LeftShiftDown)
            {
                if (e.Text == ".")
                {
                    CompletionWindowOpenAsync(".", true);
                    //codeAnalyzer.AutoInsert("f");
                    return;
                }
                else
                {
                    CompletionWindowOpenAsync(e.Text);
                }
            }
        
            codeAnalyzer.AutoInsert(e.Text);
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


        private void aTextEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //1

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
            }


            if (markSnippetWord.IsSnippetStart)
            {
                if (!markSnippetWord.TypeChangeStart())
                {
                    SnippetEnd(false);
                }
            }


            switch (e.Key)
            {
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
                                    if (line.Replace(" ", "") == "")
                                    {
                                        e.Handled = true;
                                        //모든 문자열이 스페이스
                                        aTextEditor.SelectionStart -= 4;
                                        aTextEditor.SelectionLength = 4;
                                        aTextEditor.SelectedText = "";
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
                        CodeAnalyzer.TOKEN ftk = codeAnalyzer.GetToken(-1);
                        CodeAnalyzer.TOKEN ttk = codeAnalyzer.GetToken(0);




                        int spacecount = 0;
                        string tabstr = gettabspace(false);
                        spacecount = tabstr.Length;

                        int intendsize = aTextEditor.Options.IndentationSize;

                        if (spacecount % intendsize == 0)
                        {
                            string tabonce = gettabspace(true);


                            if (ftk != null)
                            {
                                if (ftk.Value == "{")
                                {
                                    if (codeAnalyzer.LineString() != "")
                                    {
                                        e.Handled = true;
                                        aTextEditor.SelectionLength = 0;
                                        aTextEditor.SelectedText = "\n" + tabstr + tabonce + "\n" + tabstr;
                                        aTextEditor.SelectionLength = 0;
                                        aTextEditor.SelectionStart += 5 + spacecount;
                                    }
                                }
                            }

                            if (ttk != null)
                            {
                                if (ttk.Value == "{")
                                {
                                    if (codeAnalyzer.LineString() == "")
                                    {
                                        e.Handled = true;
                                        aTextEditor.SelectionLength = 0;
                                        aTextEditor.SelectedText = "\n" + tabstr + tabonce + "\n" + tabstr;
                                        aTextEditor.SelectionLength = 0;
                                        aTextEditor.SelectionStart += 5 + spacecount;
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private string gettabspace(bool IsOnce)
        {
            int intendsize = aTextEditor.Options.IndentationSize;
            if (IsOnce)
            {
                string tabonce = "";
                for (int i = 0; i < intendsize; i++)
                {
                    tabonce += " ";
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
                        if (line[i] == ' ')
                        {
                            tabstr += " ";
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
        ToolTip toolTip = new ToolTip();


        private string lastCurrentToeknValue;



        private void aTextEditor_MouseHover(object sender, MouseEventArgs e)
        {
            var pos = aTextEditor.GetPositionFromPoint(e.GetPosition(aTextEditor));
            if (pos != null)
            {
                TextViewPosition cpos = (TextViewPosition)pos;


                //toolTip.PlacementTarget = this; // required for property inheritance

                int line = cpos.Line - 1;
                int col = cpos.Column - 1;

                DocumentLine dline = aTextEditor.Document.Lines[line];
                if (dline.Length == col)
                {
                    return;
                }
                

                int offset = dline.Offset + col;

                CodeAnalyzer.TOKEN tk = codeAnalyzer.SearchToken(offset, true);
                if(tk != null)
                {
                    toolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse;
                    toolTip.Content = tk.Value;
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

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
