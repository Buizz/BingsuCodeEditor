using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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


        public string preText = "";


        public string Outputstring
        {
            get
            {
                if (preCompletionData != null) return preText + preCompletionData.outputstring;
                return "";
            }
        }

        // 검색 될때 기준
        public string Text
        {
            get {
                if (preCompletionData != null)
                {
                    if (preCompletionData.listheader.Length > 2 && preCompletionData.listheader.First() == '"' && preCompletionData.listheader.First() == preCompletionData.listheader.Last())
                    {
                        return preText + preCompletionData.listheader.Substring(1, preCompletionData.listheader.Length - 2);
                    }
                    else
                    {
                        return preText + preCompletionData.listheader;
                    }

                }
                return "";
            }
            private set {

            }
        }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content
        {
            get {
                if (preCompletionData != null) { 
                    if(preCompletionData.listheader.Length > 2 && preCompletionData.listheader.First() == '"' && preCompletionData.listheader.First() == preCompletionData.listheader.Last())
                    {
                        return preText + preCompletionData.listheader.Substring(1, preCompletionData.listheader.Length - 2);
                    }
                    else
                    {
                        return preText + preCompletionData.listheader;
                    }

                }
                return "";
            }
        }

        public object Description
        {
            get { if (preCompletionData != null) return preCompletionData.desc;
                return "";
            }
        }


        public double Priority
        {
            get {
                //switch (preCompletionData.completionWordType)
                //{
                //    case CompletionWordType.Variable:
                //        return 0;
                //    case CompletionWordType.Const:
                //        return 1;
                //    case CompletionWordType.Action:
                //        return 2;
                //    case CompletionWordType.Condiction:
                //        return 3;
                //    case CompletionWordType.KeyWord:
                //        return 4;
                //    case CompletionWordType.Function:
                //        return 5;
                //    case CompletionWordType.nameSpace:
                //        return 6;
                //    case CompletionWordType.Setting:
                //        return 7;
                //}


                if (preCompletionData != null)
                    return preCompletionData.Priority;
                return 100;
            }
        }


        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            string r = this.Outputstring;

            List<char> hexlist = new List<char>();
            for (int i = 0; i < r.Length; i++)
            {
                int charint = (int)r[i];

                if (charint <= 31)
                {
                    hexlist.Add(r[i]);
                }
            }

            foreach (var item in hexlist)
            {
                r = r.Replace(item.ToString(), "\\x" + ((int)item).ToString("X").PadLeft(2,'0'));
            }
      

            textArea.Document.Replace(completionSegment, r);
        }
    }
}
