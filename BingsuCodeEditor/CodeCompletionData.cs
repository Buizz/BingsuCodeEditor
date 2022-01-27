using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static BingsuCodeEditor.CodeAnalyzer;

namespace BingsuCodeEditor
{
    /// Implements AvalonEdit ICompletionData interface to provide the entries in the
    /// completion drop down.
    public class CodeCompletionData : ICompletionData
    {
        PreCompletionData preCompletionData;

        public CodeCompletionData( PreCompletionData preCompletionData)
        {
            this.preCompletionData = preCompletionData;
        }


        public ImageSource Image
        {
            get {
                switch (preCompletionData.completionWordType)
                {
                    case CompletionWordType.Action:
                        return ResourceManager.IconAction;
                    case CompletionWordType.Condiction:
                        return ResourceManager.IconCondiction;
                    case CompletionWordType.Const:
                        return ResourceManager.IconConst;
                    case CompletionWordType.KeyWord:
                        return ResourceManager.IconKeyWord;
                    case CompletionWordType.Function:
                        return ResourceManager.IconFunction;
                    case CompletionWordType.nameSpace:
                        return ResourceManager.IconnameSpace;
                    case CompletionWordType.Setting:
                        return ResourceManager.IconSetting;
                    case CompletionWordType.Variable:
                        return ResourceManager.IconVariable;
                }
                return null;
            }
        }

        public string Text
        {
            get {
                return preCompletionData.ouputstring;
            }
            private set {

            }
        }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content
        {
            get { return preCompletionData.listheader; }
        }

        public object Description
        {
            get { return preCompletionData.desc; }
        }


        public double Priority
        {
            get { return preCompletionData.Priority; }
        }


        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }
    }
}
