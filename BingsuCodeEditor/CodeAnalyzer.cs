using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingsuCodeEditor
{
    public abstract class CodeAnalyzer
    {
        public CodeAnalyzer(TextEditor textEditor, bool IsSpaceCheck)
        {
            this.textEditor = textEditor;
            this.IsSpaceCheck = IsSpaceCheck;
        }
        private bool IsSpaceCheck;


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
            Symbol,
            WhiteSpace,
            Tab
        }

        private Dictionary<string, TOKEN_TYPE> dcTokensubType = new Dictionary<string, TOKEN_TYPE>();
        public void AddSubType(string subtype, TOKEN_TYPE type)
        {
            dcTokensubType.Add(subtype, type);
        }



        public enum CompletionWordType
        {
            Action,
            Condiction,
            Const,
            Function,
            KeyWord,
            nameSpace,
            Setting,
            Variable
        }


        public List<PreCompletionData> completionDatas = new List<PreCompletionData>();
        public void ResetCompletionData(CompletionWordType completionWordType)
        {
            completionDatas.RemoveAll((t) =>
            {
                return t.completionWordType == completionWordType;
            });
        }
        public void AddCompletionData(PreCompletionData preCompletionData)
        {
            completionDatas.Add(preCompletionData);
        }



        public abstract class PreCompletionData
        {
            public CompletionWordType completionWordType;


            public PreCompletionData(CompletionWordType completionType, string name)
            {
                this.completionWordType = completionType;
                this.name = name;
            }

            //키워드 이름
            protected string name;
            public abstract string listheader { get; }
            public abstract string ouputstring { get; }
            public abstract string desc { get; }



            //자동 입력을 위한 프리셋
            public string AutoInsert;
        }


        public abstract void GetCompletionList(IList<ICompletionData> data);




        private bool workComplete = false;
        public bool WorkCompete
        {
            get
            {
                return workComplete;
            }
        }

        private int currentIndex;


        protected List<TOKEN> Tokens = new List<TOKEN>();
        public TOKEN GetToken(int index)
        {
            int cmpindex = index;


            if (index < 0)
            {
                cmpindex = currentIndex + index;
            }

            if (cmpindex < 0)
            {
                return null;
            }
            if (cmpindex >= Tokens.Count)
            {
                return null;
            }

            return Tokens[cmpindex];
        }


        public class TOKEN
        {
            public TOKEN(int Offset, TOKEN_TYPE Type, string Value)
            {
                this.StartOffset = Offset;
                this.EndOffset = Offset + Value.Length;
                this.Type = Type;
                this.Value = Value;
            }

            public int StartOffset;
            public int EndOffset;
            public TOKEN_TYPE Type;
            public string Value;


            public string Special;
        }
        

        private void addToken(TOKEN token, int offset)
        {
            Tokens.Add(token);
            if (token.StartOffset < offset && offset < token.EndOffset)
            {
                currentIndex = Tokens.Count();
            }
        }

        public void Apply(string text, int offset)
        {
            workComplete = false;

            Tokens.Clear();

            int tlen = text.Length;

            for (int i = 0; i < tlen; i++)
            {
                char t = text[i];


                if (char.IsDigit(t))
                {
                    //t가 숫자일 경우 숫자
                    string block = "";

                    TOKEN_TYPE type = TOKEN_TYPE.Number;
                    do
                    {
                        block += t.ToString();
                        i++;
                        if (i >= tlen)
                        {
                            break;
                        }
                        t = text[i];
                    } while (i < tlen && char.IsLetterOrDigit(t) | t == '.' | t == '+');

                    i--;
                    TOKEN token = new TOKEN(i, type, block);
                    addToken(token, offset);
                }
                else if (char.IsLetter(t))
                {
                    //키워드 또는 식별자
                    string block = "";

                    do
                    {
                        block += t.ToString();
                        i++;
                        if (i >= tlen)
                        {
                            break;
                        }
                        t = text[i];
                    } while (i < tlen && char.IsLetterOrDigit(t) | t == '_');
                    i--;

                    TOKEN_TYPE type = TOKEN_TYPE.Identifier;
                    if (dcTokensubType.ContainsKey(block))
                    {
                        if (dcTokensubType[block] == TOKEN_TYPE.KeyWord)
                        {
                            type = TOKEN_TYPE.KeyWord;
                        }
                    }

                    TOKEN token = new TOKEN(i, type, block);
                    addToken(token, offset);
                }
                else if (char.IsWhiteSpace(t))
                {
                    if (!IsSpaceCheck)
                        continue;

                    //키워드 또는 식별자
                    string block = "";

                    do
                    {
                        block += t.ToString();
                        i++;
                        if (i >= tlen)
                        {
                            break;
                        }
                        t = text[i];
                    } while (i < tlen && char.IsWhiteSpace(t));
                    i--;

                    TOKEN_TYPE type = TOKEN_TYPE.WhiteSpace;

                    TOKEN token = new TOKEN(i, type, block);
                    addToken(token, offset);
                }
                else
                {
                    //나머지 괄호나 
                    //t가 숫자일 경우 숫자
                    string block = t.ToString();

                    TOKEN_TYPE type = TOKEN_TYPE.Symbol;

                    TOKEN token = new TOKEN(i, type, block);
                    addToken(token, offset);
                }
            }


            
            codeFoldingManager.FoldingUpdate(Tokens, text.Length);
            workComplete = true;
        }
    }
}
