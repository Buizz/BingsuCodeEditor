using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingsuCodeEditor.Lua
{
    class LuaAnalyzer : CodeAnalyzer
    {
        public LuaAnalyzer(TextEditor textEditor) : base(textEditor, false)
        {
            AddSubType("function", TOKEN_TYPE.KeyWord);

            codeFoldingManager = new LuaFoldingManager(textEditor);
        }

        public override void GetCompletionList(IList<ICompletionData> data)
        {
            throw new NotImplementedException();
        }
    }
}
