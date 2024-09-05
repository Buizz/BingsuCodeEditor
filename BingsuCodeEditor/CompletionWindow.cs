using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace BingsuCodeEditor
{
    /// <summary>
    /// The code completion window.
    /// </summary>
    public class CustomCompletionWindow : BingsuCodeEditor.CompletionWindowBase
    {
        readonly CompletionList completionList = new CompletionList();
        ToolTip toolTip = new ToolTip();

        /// <summary>
        /// Gets the completion list used in this completion window.
        /// </summary>
        public CompletionList CompletionList
        {
            get { return completionList; }
        }

        /// <summary>
        /// Creates a new code completion window.
        /// </summary>
        public CustomCompletionWindow(TextArea textArea) : base(textArea)
        {
            // keep height automatic
            this.CloseAutomatically = true;
            this.MaxHeight = 300;
            //this.SizeToContent = SizeToContent.Height;
            // prevent user from resizing window to 0x0
            //this.MinHeight = 15;
            //this.MinWidth = 30;


            this.AllowsTransparency = true;




            toolTip.PlacementTarget = this;
            toolTip.Placement = PlacementMode.Right;
            toolTip.Closed += toolTip_Closed;



            AttachEvents();
        }

        private string inputText;
        private bool IsNameSpaceOpen;

        public void Open(string input, bool NoStartWithStartText = false)
        {
            this.MinWidth = 0;
            this.Width = 0;
            this.MinHeight = 0;
            Height = 0;
            this.SizeToContent = SizeToContent.Manual;


            this.inputText = input;
            this.IsNameSpaceOpen = NoStartWithStartText;



            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.MinHeight = 15;
            this.Width = 240;
            this.MinWidth = 80;
            this.Height = 270;


            
            this.Content = completionList;


            this.Background = (System.Windows.Media.Brush)Application.Current.Resources["MaterialDesignToolBarBackground"];
            this.Foreground = (System.Windows.Media.Brush)Application.Current.Resources["MaterialDesignBody"];
            completionList.ListBox.Background = (System.Windows.Media.Brush)Application.Current.Resources["MaterialDesignToolBarBackground"];
            completionList.ListBox.Foreground = (System.Windows.Media.Brush)Application.Current.Resources["MaterialDesignBody"];
            completionList.ListBox.BorderBrush = (System.Windows.Media.Brush)Application.Current.Resources["MaterialDesignPaper"];


            //if (!NoStartWithStartText)
            //{
            //    this.Visibility = Visibility.Hidden;
            //}


            Show();



            if (!string.IsNullOrEmpty(input))
            {
                if (!NoStartWithStartText)
                {
                    completionList.SelectItem(input);
                    if (this.StartOffset != 0)
                    {
                        this.StartOffset -= input.Length;
                    }
                }
            }
            YOffset += 100;

            UpdatePosition();
        }

        public void ForceUpdatePosition()
        {
            UpdatePosition();
        }


        #region ToolTip handling
        void toolTip_Closed(object sender, RoutedEventArgs e)
        {
            // Clear content after tooltip is closed.
            // We cannot clear is immediately when setting IsOpen=false
            // because the tooltip uses an animation for closing.
            if (toolTip != null)
                toolTip.Content = null;
        }

        void completionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string input = this.TextArea.Document.GetText(StartOffset, this.TextArea.Caret.Offset - StartOffset);
            int t;
            if (int.TryParse(input, out t))
            {
                Close();
                return;
            }

            tooltipOpen();
            ListVisbleChange();
            
        }

        void tooltipOpen()
        {
            var item = completionList.SelectedItem;
            if (item == null)
                return;
            object description = item.Description;
            if (description != null)
            {
                string descriptionText = description as string;
                if (descriptionText != null)
                {
                    toolTip.Content = new TextBlock
                    {
                        Text = descriptionText,
                        TextWrapping = TextWrapping.Wrap
                    };
                }
                else
                {
                    toolTip.Content = description;
                }
                toolTip.IsOpen = true;
            }
            else
            {
                toolTip.IsOpen = false;
            }
        }
        #endregion

        void ListVisbleChange()
        {
            try
            {
                if (completionList.ListBox.Items.Count == 0)
                {
                    Visibility = Visibility.Hidden;
                    toolTip.IsOpen = false;
                }
                else
                {
                    Visibility = Visibility.Visible;
                }
            }
            catch (InvalidOperationException)
            {
            }
        }





        void completionList_InsertionRequested(object sender, EventArgs e)
        {
            Close();
            // The window must close before Complete() is called.
            // If the Complete callback pushes stacked input handlers, we don't want to pop those when the CC window closes.
            CodeCompletionData item = (CodeCompletionData)completionList.SelectedItem;
            if (item != null)
            {
                //if(StartOffset == EndOffset)
                //{
                //    return;
                //}

                item.Complete(this.TextArea, new AnchorSegment(this.TextArea.Document, this.StartOffset, this.EndOffset - this.StartOffset), e);
                //if (item.Text.IndexOf("/") != -1)
                //{
                //    item.Complete(this.TextArea, new AnchorSegment(this.TextArea.Document, this.StartOffset - 1, this.EndOffset - this.StartOffset + 1), e);
                //}
                //else
                //{
                //    item.Complete(this.TextArea, new AnchorSegment(this.TextArea.Document, this.StartOffset, this.EndOffset - this.StartOffset), e);
                //}
            }
        }

        void AttachEvents()
        {
            this.completionList.InsertionRequested += completionList_InsertionRequested;
            this.completionList.SelectionChanged += completionList_SelectionChanged;
            this.TextArea.Caret.PositionChanged += CaretPositionChanged;
            this.TextArea.MouseWheel += textArea_MouseWheel;
            this.TextArea.PreviewTextInput += textArea_PreviewTextInput;
        }


        /// <inheritdoc/>
        protected override void DetachEvents()
        {
            this.completionList.InsertionRequested -= completionList_InsertionRequested;
            this.completionList.SelectionChanged -= completionList_SelectionChanged;
            this.TextArea.Caret.PositionChanged -= CaretPositionChanged;
            this.TextArea.MouseWheel -= textArea_MouseWheel;
            this.TextArea.PreviewTextInput -= textArea_PreviewTextInput;
            base.DetachEvents();
        }

        /// <inheritdoc/>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (toolTip != null)
            {
                toolTip.IsOpen = false;
                toolTip = null;
            }
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled)
            {
                completionList.HandleKey(e);
            }
        }


        void textArea_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = RaiseEventPair(this, PreviewTextInputEvent, TextInputEvent,
                                        new TextCompositionEventArgs(e.Device, e.TextComposition));


        }

        void textArea_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = RaiseEventPair(GetScrollEventTarget(),
                                        PreviewMouseWheelEvent, MouseWheelEvent,
                                        new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta));
        }

        UIElement GetScrollEventTarget()
        {
            if (completionList == null)
                return this;
            return completionList.ScrollViewer ?? completionList.ListBox ?? (UIElement)completionList;
        }

        /// <summary>
        /// Gets/Sets whether the completion window should close automatically.
        /// The default value is true.
        /// </summary>
        public bool CloseAutomatically { get; set; }

        /// <inheritdoc/>
        protected override bool CloseOnFocusLost
        {
            get { return this.CloseAutomatically; }
        }

        /// <summary>
        /// When this flag is set, code completion closes if the caret moves to the
        /// beginning of the allowed range. This is useful in Ctrl+Space and "complete when typing",
        /// but not in dot-completion.
        /// Has no effect if CloseAutomatically is false.
        /// </summary>
        public bool CloseWhenCaretAtBeginning { get; set; }

        void CaretPositionChanged(object sender, EventArgs e)
        {
            int offset = this.TextArea.Caret.Offset;
            if (offset == this.StartOffset)
            {
                if (CloseAutomatically && CloseWhenCaretAtBeginning)
                {
                    Close();
                }
                else
                {
                    completionList.SelectItem(string.Empty);
                    completionList.SelectedItem = null;
                }
                return;
            }
            if (offset < this.StartOffset || offset > this.EndOffset)
            {
                if (CloseAutomatically)
                {
                    Close();
                }
            }
            else
            {
                TextDocument document = this.TextArea.Document;
                if (document != null)
                {
                    if (this.StartOffset != -1 && completionList.ListBox != null)
                    {
                        completionList.SelectItem(document.GetText(this.StartOffset, offset - this.StartOffset));
                    }
                }
            }
        }
    }
}