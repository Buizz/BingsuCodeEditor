using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingsuCodeEditor.Lua
{
    class LuaAnalyzer : CodeAnalyzer
    {
        public LuaAnalyzer(TextEditor textEditor) : base(textEditor)
        {
            AddSubType("function", TOKEN_TYPE.KeyWord);

            codeFoldingManager = new LuaFoldingManager(textEditor);
        }
    }
}
