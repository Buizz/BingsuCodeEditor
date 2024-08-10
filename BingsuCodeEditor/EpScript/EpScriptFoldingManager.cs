using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingsuCodeEditor.EpScript
{
    public class EpScriptFoldingManager : CodeFoldingManager
    {
        public EpScriptFoldingManager(TextEditor aTextEditor) : base(aTextEditor)
        {
        }

        public override void FodlingExec(List<CodeAnalyzer.TOKEN> Tokens, int len, List<NewFolding> Foldings)
        {
            Stack<int> startOffset = new Stack<int>();
            Stack<int> regionOffset = new Stack<int>();
            for (int i = 0; i < Tokens.Count; i++)
            {
                CodeAnalyzer.TOKEN tk = Tokens[i];
                if (tk.Type == CodeAnalyzer.TOKEN_TYPE.Symbol)
                {
                    if (tk.Value == "{")
                    {
                        startOffset.Push(tk.StartOffset);
                    }
                    else if (tk.Value == "}")
                    {
                        if (startOffset.Count == 0) return;
                        int st = startOffset.Pop();
                        int et = tk.EndOffset;

                        Foldings.Add(new NewFolding(st, et));
                    }
                    if (tk.Value == "[")
                    {
                        startOffset.Push(tk.StartOffset);
                    }
                    else if (tk.Value == "]")
                    {
                        if (startOffset.Count == 0) return;
                        int st = startOffset.Pop();
                        int et = tk.EndOffset;

                        Foldings.Add(new NewFolding(st, et));
                    }
                }
                else if(tk.Type == CodeAnalyzer.TOKEN_TYPE.Comment)
                {
                    if (tk.Value == "/*region*/")
                    {
                        regionOffset.Push(tk.StartOffset);
                    }
                    else if (tk.Value == "/*endregion*/")
                    {
                        if (regionOffset.Count == 0) return;
                        int st = regionOffset.Pop();
                        int et = tk.EndOffset;

                        Foldings.Add(new NewFolding(st, et));
                    }
                }
            }


            //TODO : 폴딩 로직 짜야됨
        }
    }
}
