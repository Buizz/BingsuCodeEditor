using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingsuCodeEditor.Lua
{
    public class LuaFoldingManager : CodeFoldingManager
    {
        public LuaFoldingManager(TextEditor aTextEditor) : base(aTextEditor)
        {

        }

        public override void FodlingExec(List<CodeAnalyzer.TOKEN> Tokens, int len, List<NewFolding> Foldings)
        {
            //TODO : 폴딩 로직 짜야됨
            Foldings.Add(new NewFolding(0, len));
        }
    }
}
