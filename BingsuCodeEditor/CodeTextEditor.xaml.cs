using BingsuCodeEditor.EpScript;
using BingsuCodeEditor.Lua;
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

        #region #############프라이빗#############
        private bool LeftCtrlDown;
        private bool LeftShiftDown;

        private DispatcherTimer dispatcherTimer;
        private CodeEditorOptions codeEditorOptions;
        private CodeAnalyzer codeAnalyzer;
        private Thread thread;
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (codeAnalyzer != null && (thread == null || !thread.IsAlive))
            {
                string codeText = aTextEditor.Text;

                thread = new Thread(()=>
                {
                    DateTime dateTime = DateTime.Now;

                    codeAnalyzer.Apply(codeText);


                    aTextEditor.Dispatcher.Invoke(new Action(() =>
                    {
                        ToolTip.Text = DateTime.Now.Subtract(dateTime).ToString();
                    }), DispatcherPriority.Normal);
                });
                thread.Start();
            }
        }
        #endregion

        #region #############옵션#############
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

        private CodeType CurrentcodeType;
       
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

        #endregion

        #region #############초기화#############

        public LanguageData Lan = new LanguageData();
        private void InitCtrl()
        {
            aTextEditor.Options.ConvertTabsToSpaces = true;

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



            


            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(500);

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


        CustomCompletionWindow completionWindow;
        #region #############키 입력#############
        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            //캐럿 분석을 실행
            //스페이스 등을 입력시 자동완성 입력 끝내기.

            if (e.Text.Length > 0 && completionWindow != null)
            {
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
            //선택이 다중일 경우 사용하지 않음
            //엔터링에서 분석한 정보를 토대로, 자동완성창을 열거나 자동완성 목록을 생성
            //자동완성중이 아니면 마지막에 자동입력 실행


            if (e.Text == ".")
            {
                // Open code completion after the user has pressed dot:
                completionWindow = new CustomCompletionWindow(aTextEditor.TextArea);

                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                data.Add(new MyCompletionData("Item1"));
                data.Add(new MyCompletionData("Item2"));
                data.Add(new MyCompletionData("Item3"));
                completionWindow.Show();
                completionWindow.Closed += delegate {
                    completionWindow = null;
                };
            }
        }

        private void aTextEditor_TextChanged(object sender, EventArgs e)
        {

        }
        #endregion

        #region #############KeyDown#############
        private void aTextEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.LeftCtrl:
                    TBCtrlValue.Visibility = Visibility.Visible;
                    LeftCtrlDown = true;
                    break;
                case Key.LeftShift:
                    TBShiftValue.Visibility = Visibility.Visible;
                    LeftShiftDown = true;
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
                        SearchPanelClose();
                    break;
            }
        }
        private void aTextEditor_PreviewKeyUp(object sender, KeyEventArgs e)
        {
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

        private void aTextEditor_MouseHover(object sender, MouseEventArgs e)
        {
            var pos = aTextEditor.GetPositionFromPoint(e.GetPosition(aTextEditor));
            if (pos != null)
            {
                toolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse;
                //toolTip.PlacementTarget = this; // required for property inheritance
                toolTip.Content = pos.ToString();
                toolTip.IsOpen = true;
                e.Handled = true;
            }
        }

        private void aTextEditor_MouseHoverStopped(object sender, MouseEventArgs e)
        {
            toolTip.IsOpen = false;
        }

        

        #endregion
    }
}
