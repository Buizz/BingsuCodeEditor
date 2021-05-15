using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingsuCodeEditor
{
    public class CodeAnalyzer
    {
        public CodeAnalyzer(TextEditor textEditor)
        {
            this.textEditor = textEditor;
        }



        protected TextEditor textEditor;
        protected CodeFoldingManager codeFoldingManager;
        public enum TOKEN_TYPE
        {
            String,
            Number,
            Identifier,
            LineComment,
            Comment,
            Value,
            KeyWord,
            Symbo
        }

        private Dictionary<string, TOKEN_TYPE> dcTokensubType = new Dictionary<string, TOKEN_TYPE>();
        public void AddSubType(string subtype, TOKEN_TYPE type)
        {
            dcTokensubType.Add(subtype, type);
        }


        public List<TOKEN> Tokens = new List<TOKEN>();
        public class TOKEN
        {
            public TOKEN(int Offset, TOKEN_TYPE Type, string Value)
            {
                this.Offset = Offset;
                this.Type = Type;
                this.Value = Value;
            }

            public int Offset;
            public TOKEN_TYPE Type;
            public string Value;
        }

        public void Apply(string text)
        {
            Tokens.Clear();

            int tlen = text.Length;

            for (int i = 0; i < tlen; i++)
            {
                char t = text[i];

            }


            codeFoldingManager.FoldingUpdate(Tokens, text.Length);
        }
    }
}
