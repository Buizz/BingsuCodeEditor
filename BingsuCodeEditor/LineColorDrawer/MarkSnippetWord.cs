using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
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

        private string keyword;

        public bool IsSnippetStart = false;

        private List<SnippetToken> snippetTokens = new List<SnippetToken>();
        private class SnippetToken
        {
            public string value;
            public int index;
            public int Length;

            public int temp;

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
            IsSnippetStart = false;
        }

        string lastLinetext;
        public bool CheckOuter()
        {
            DocumentLine line = textEditor.Document.GetLineByOffset(startoffset);
            lastLinetext = textEditor.Document.GetText(line.Offset, line.Length);
            //컨텐츠 위치 밖에서 바꾸면사라짐
            //if(!(startoffset <= textEditor.CaretOffset && textEditor.CaretOffset < endoffset))
            //{
            //    return true;
            //}

            return false;
        }

        string fstr;
        string lstr;
        string linetext;

        bool IsTypeChange = false;

        int tokenindex;
        public bool TypeChangeStart()
        {
            DocumentLine line = textEditor.Document.GetLineByOffset(startoffset);
            linetext = textEditor.Document.GetText(line.Offset, line.Length);



            //여기서 인터널 체크
            int i = GetSnippetIndex();
            tokenindex = i;
            if (i != -1)
            {
                //인터널인 경우
                //snippetTokens[i].Length += count;

                
                //int loffset = snippetTokens[i].index - line.Offset + keyword.Length;
                int loffset = snippetTokens[i].index - line.Offset + startoffset;
                if(loffset < 0)
                {
                    return false;
                }
                fstr = linetext.Substring(0, loffset);
                lstr = linetext.Substring(loffset + snippetTokens[i].Length);
                IsTypeChange = true;
                return true;
            }
            else
            {
                IsTypeChange = false;
                return false;
                //SnippetToken ft = snippetTokens.First();
                //SnippetToken lt = snippetTokens.Last();

                //int s = ft.index;
                //int e = lt.index + lt.value.Length;
            }
        }
        public void TypeChangeEnd()
        {
            if (!IsTypeChange)
            {
                return;
            }
            IsTypeChange = false;


            int lastselect = textEditor.SelectionStart;
            int lastselectlen = textEditor.SelectionLength;



            DocumentLine line = textEditor.Document.GetLineByOffset(startoffset);
            string newlinetext = textEditor.Document.GetText(line.Offset, line.Length);


            int s = newlinetext.IndexOf(fstr);
            int e = newlinetext.LastIndexOf(lstr);
            if(s != 0)
            {
                return;
            }
            string cstr = newlinetext.Substring(fstr.Length, e - fstr.Length);


            string tempkeyword = "";
            for (int i = 0; i < snippetTokens[tokenindex].Length; i++)
            {
                tempkeyword += "@";
            }

            //교체를 위해 이전 내용을 되돌린다.
            textEditor.Document.Replace(line.Offset + fstr.Length, cstr.Length, tempkeyword);


            //내용을 뒤에서 부터 바꾼다.
            for (int i = snippetTokens.Count - 1; 0 <= i; i--)
            {
                if (snippetTokens[i].value == snippetTokens[tokenindex].value)
                {
                    //같은 내용일 경우
                    textEditor.Document.Replace(startoffset + snippetTokens[i].index, snippetTokens[i].Length, cstr);
                }
            }

            int dif = 0;
            //i번 인덱스가 어떤 내용으로 바뀌었는지 체크한다.
            for (int i = 0; i < snippetTokens.Count; i++)
            {
                if (snippetTokens[i].value == snippetTokens[tokenindex].value)
                {
                    //같은 내용일 경우
                    int lastlen = cstr.Length - snippetTokens[i].Length;

                    dif += lastlen;

                    snippetTokens[i].Length = cstr.Length;
                    for (int t = i + 1; t < snippetTokens.Count; t++)
                    {
                        snippetTokens[t].index += lastlen;
                    }
                }
            }

            textEditor.SelectionStart = lastselect;
            textEditor.SelectionLength = lastselectlen;

            ContentPos += dif;
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
            //SnippetToken nextToken;

            SnippetToken selectToken = null;


            List<int> startoffsets = new List<int>();

            List<SnippetToken> temp = new List<SnippetToken>();

            List<string> containtoken = new List<string>();
            for (int k = 0; k < snippetTokens.Count; k++)
            {
                if (containtoken.Contains(snippetTokens[k].value))
                {
                    //지나온 토큰에 저장된 토큰이 있을 경우 패스
                    continue;
                }

                containtoken.Add(snippetTokens[k].value);
                if (k > i)
                {
                    //현재 선택한 토근보다 뒤에 있는 토큰을 선택
                    if(currentToken.value != snippetTokens[k].value)
                    {
                        //현재토큰이 아닐 경우
                        selectToken = snippetTokens[k];
                        break;
                    }
                }
            }
            if(selectToken == null)
            {
                selectToken = snippetTokens.First();
            }

            //for (int k = 0; k < snippetTokens.Count; k++)
            //{
            //    nextToken = snippetTokens[(k + i) % snippetTokens.Count];

            //    nextToken.temp = k;

            //    if (nextToken.value != currentToken.value)
            //    {
            //        selectToken = nextToken;
            //        break;
            //        //토큰 내용이 다를 경우 선택
            //        temp.Add(nextToken);
            //    }
            //}
            ////temp.Sort((x, y) => x.index.CompareTo(y.index * 100 + y.temp));
            //temp.Sort((x, y) => x.temp.CompareTo( y.temp));

            //selectToken = temp.First();
            if(selectToken != null)
            {
                textEditor.SelectionStart = selectToken.index + startoffset;
                textEditor.SelectionLength = selectToken.Length;
            }
   

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


        public string Start(string keyword, string str, int lineNumber)
        {
            this.lineNumber = lineNumber;
            this.startoffset = textEditor.CaretOffset;

            this.keyword = keyword;

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
                    if (line.Offset > startoffset + item.index)
                    {
                        Clear();
                        return;
                    }
                    if (startoffset + item.index + item.Length > line.EndOffset)
                    {
                        Clear();
                        return;
                    }


                    ChangeLinePart(startoffset + item.index, startoffset + item.index + item.Length, ApplyChanges);
                }
            }
            
        }

        void ApplyChanges(VisualLineElement element)
        {
            element.TextRunProperties.SetBackgroundBrush(new SolidColorBrush(Color.FromArgb(64, 230, 200, 200)));
            // This is where you do anything with the line
            //element.TextRunProperties.SetForegroundBrush(Brushes.Red);
        }
    }
}
