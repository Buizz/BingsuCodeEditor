using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
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
            public int Length;

            public SnippetToken(int index, string value)
            {
                this.index = index;
                this.value = value;
                this.Length = value.Length;
            }
        }

        private int ContentPos;

        public void Clear()
        {
            snippetTokens.Clear();
        }


        public bool CheckOuter()
        {
            //컨텐츠 위치 밖에서 바꾸면사라짐
            if(!(startoffset <= textEditor.CaretOffset && textEditor.CaretOffset < endoffset))
            {
                return true;
            }

            return false;
        }

        public void PosChange()
        {
            //여기서 인터널 체크
            int i = GetSnippetIndex();

            if (i != -1)
            {
                //인터널인 경우
                //snippetTokens[i].Length += count;
            }
            else
            {
                SnippetToken ft = snippetTokens.First();
                SnippetToken lt = snippetTokens.Last();

                int s = ft.index;
                int e = lt.index + lt.value.Length;
            }

        }




        public void SelectFirstItem()
        {
            textEditor.SelectionStart = snippetTokens[0].index + startoffset;
            textEditor.SelectionLength = snippetTokens[0].Length;
        }


        public void NextItem()
        {
            int i = GetSnippetIndex();
            if(i == -1)
            {
                return;
            }
            SnippetToken currentToken = snippetTokens[i];
            SnippetToken nextToken;

            List<int> startoffsets = new List<int>();

            List<SnippetToken> temp = new List<SnippetToken>();
            for (int k = 0; k < snippetTokens.Count; k++)
            {
                nextToken = snippetTokens[(k + i) % snippetTokens.Count];

                if(nextToken.value != currentToken.value)
                {
                    //토큰 내용이 다를 경우 선택
                    temp.Add(nextToken);
                }
            }
            temp.Sort((x, y) =>  x.index.CompareTo(y.index) );



            textEditor.SelectionStart = temp.First().index + startoffset;
            textEditor.SelectionLength = temp.First().Length;
        }


        public int GetSnippetIndex()
        {
            int coffset = textEditor.CaretOffset - startoffset;
            int i = 0;
            foreach (var item in snippetTokens)
            {
                if (item.index <= coffset && coffset <= item.index + item.Length)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        public bool CheckSnippetInternal()
        {
            int coffset = textEditor.CaretOffset - startoffset;
            foreach (var item in snippetTokens)
            {
                if(item.index <= coffset && coffset <= item.index + item.Length)
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
                    ChangeLinePart(startoffset + item.index, startoffset + item.index + item.Length, ApplyChanges);
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
