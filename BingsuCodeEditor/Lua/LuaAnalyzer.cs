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
        public override void AutoInsert(string text)
        {
            throw new NotImplementedException();
        }

        public override bool AutoRemove()
        {
            throw new NotImplementedException();
        }
        public override TOKEN TokenBlockAnalyzer(string text, int index, out int outindex)
        {
            throw new NotImplementedException();
        }

        public override bool GetCompletionList(IList<ICompletionData> data, bool IsNameSpace = false)
        {
            throw new NotImplementedException();
        }

        public override void TokenAnalyze(int caretoffset = int.MaxValue)
        {
            throw new NotImplementedException();
        }
    }
}
