using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BingsuCodeEditor.LineColorDrawer
{
    public class MarkSnippetWord : DocumentColorizingTransformer
    {
        private int lineNumber;
        private int startoffset;
        private int endoffset;
        private TextEditor textEditor;


        private List<SnippetToken> snippetTokens = new List<SnippetToken>();
        private class SnippetToken
        {
            public string value;
            public int index;

            public SnippetToken(int index, string value)
            {
                this.index = index;
                this.value = value;
            }
        }

        private int ContentPos;



        public bool PosChange()
        {
            //컨텐츠 위치 밖에서 바꾸면사라짐
            if(startoffset <= textEditor.CaretOffset && textEditor.CaretOffset < endoffset)
            {
                return false;
            }

            return true;
        }



        public void SelectFirstItem()
        {
            textEditor.SelectionStart = snippetTokens[0].index + startoffset;
        }


        public void NextItem()
        {

        }



        public bool CheckSnippetInternal()
        {
            int coffset = textEditor.CaretOffset - startoffset;
            foreach (var item in snippetTokens)
            {
                if(item.index <= coffset && coffset <= item.index + item.value.Length)
                {
                    return true;
                }
            }


            return false;
        }

        public string Start(string str, int lineNumber)
        {
            this.lineNumber = lineNumber;
            this.startoffset = textEditor.CaretOffset;



            Regex regex = new Regex(@"\[[^\[^\]]+\]");

            MatchCollection mc = regex.Matches(str);


            for (int i = 0; i < mc.Count; i++)
            {
                int index = mc[i].Index;
                string value = mc[i].Value;

                if(value == "[Content]")
                {
                    ContentPos = index - (i * 2);
                    continue;
                }
   
                value = value.Replace("[", "").Replace("]", "");



                snippetTokens.Add(new SnippetToken(index - (i * 2), value));
            }


            foreach (var item in snippetTokens)
            {
                str = str.Replace("[" + item.value + "]", item.value);
            }
            str = str.Replace("[Content]", "");


            this.endoffset = this.startoffset + str.Length;


            return str;
        }

        public void GotoContent()
        {
            textEditor.CaretOffset = this.startoffset + ContentPos;
        }


        public MarkSnippetWord(TextEditor textEditor)
        {
            this.textEditor = textEditor;
        }

        protected override void ColorizeLine(ICSharpCode.AvalonEdit.Document.DocumentLine line)
        {
            if (!line.IsDeleted && line.LineNumber == lineNumber)
            {
                //ChangeLinePart(line.Offset, line.EndOffset, ApplyChanges);
                foreach (var item in snippetTokens)
                {
                    ChangeLinePart(startoffset + item.index, startoffset + item.index + item.value.Length, ApplyChanges);
                }
            }
            
        }

        void ApplyChanges(VisualLineElement element)
        {
            element.TextRunProperties.SetBackgroundBrush(new SolidColorBrush(Color.FromArgb(192, 255, 228, 0)));
            // This is where you do anything with the line
            //element.TextRunProperties.SetForegroundBrush(Brushes.Red);
        }
    }
}
