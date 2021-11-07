﻿using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace BingsuCodeEditor
{
    public abstract class CodeAnalyzer
    {
        public CodeAnalyzer(TextEditor textEditor, bool IsSpaceCheck)
        {
            this.textEditor = textEditor;
            this.IsSpaceCheck = IsSpaceCheck;

            Template = new Dictionary<string, string>();
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
            Tab,
            Special
        }


        
        //Tab으로 자동완성 되는 기능 이걸 토큰에 넣을지 고민중
        public Dictionary<string, string> Template;

        public CursorLocation cursorLocation;
        public enum CursorLocation
        {
            Keyword,
            FunctionName,
            FunctionArgName,
            FunctionArgType,
            ImportFile,
            ImportNameSpace,
            ObjectName
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
            Variable,
            Special
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



        public bool IsLineEnd
        {
            get
            {
                var currentLine = textEditor.Document.GetLineByOffset(textEditor.CaretOffset);
                int len = currentLine.Length;

                int lineoffset = currentLine.Offset;
                //라인의 끝일 경우
                if (textEditor.CaretOffset == lineoffset + len)
                {
                    return true;
                }

                return false;
            }
        }


        public string LineString(bool left = false)
        {
            var currentLine = textEditor.Document.GetLineByOffset(textEditor.CaretOffset);
            int len = currentLine.Length;

            int lineoffset = currentLine.Offset;

            int offset = textEditor.CaretOffset - lineoffset;

            string line = textEditor.Document.GetText(lineoffset, len);

            if (left)
            {
                return line.Substring(0, offset);
            }
            else
            {
                return line.Substring(offset);
            }
        }



        public abstract void GetCompletionList(IList<ICompletionData> data);

        public abstract void TokenAnalyzer();
        public abstract TOKEN TokenBlockAnalyzer(string text, int index, out int outindex);


        public abstract void AutoInsert(string text);
        public abstract bool AutoRemove();


        public bool AutoDefaultRemove()
        {
            if(textEditor.SelectionLength != 0)
            {
                return false;
            }

            TOKEN ftk = GetToken(-1);
            TOKEN ttk = GetToken(0);

            if (IsLineEnd)
            {
                return false;
            }

            if (
                ftk != null && ttk != null && (
                (ftk.Value == "{" && ttk.Value == "}")
                || (ftk.Value == "[" && ttk.Value == "]")
                || (ftk.Value == "(" && ttk.Value == ")")))
            {
                textEditor.SelectionStart -= 1;
                textEditor.SelectionLength = 2;
                textEditor.SelectedText = "";
                return true;
            }

            if(ttk != null && ttk.Value == "\"\""){
                textEditor.SelectionStart -= 1;
                textEditor.SelectionLength = 2;
                textEditor.SelectedText = "";
                return true;
            }



            return false;
        }


        private bool workComplete = false;
        public bool WorkCompete
        {
            get
            {
                return workComplete;
            }
        }

        private int currenttokenIndex;
        private int currentoffset;

        public int CurrentTokenIndex
        {
            get
            {
                return currenttokenIndex;
            }
        }


        public int GetTokenCount()
        {
            return Tokens.Count;
        }

        protected List<TOKEN> Tokens = new List<TOKEN>();
        
        
        /// <summary>
        /// 토근의 상대적 위치를 구합니다.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public TOKEN GetToken(int index)
        {
            if(currenttokenIndex == -1)
            {
                return null;
            }

            int cmpindex = index;


            if (index < 0)
            {
                cmpindex = currenttokenIndex + index;
            }
            else
            {
                cmpindex = currenttokenIndex + index;
            }

            if (cmpindex < 0)
            {
                return null;
            }
            if (cmpindex >= Tokens.Count)
            {
                return null;
            }

            TOKEN tk = Tokens[cmpindex];

            //if(index == 0)
            //{
            //    if (tk.CheckOffset(currentoffset))
            //    {
            //        return tk;
            //    }
            //    else
            //    {
            //        return null;
            //    }
            //}

            return tk;
            //if (tk.CheckOffset(currentoffset))
            //{
            //    return tk;
            //}

            //return null;
        }


        public TOKEN SearchToken(int offset, bool isfull = false)
        {
            for (int i = 0; i < Tokens.Count; i++)
            {
                TOKEN tk = Tokens[i];

                if (tk.CheckOffset(offset, isfull))
                {
                    return tk;
                }
            }

            return null;
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

            public bool CheckOffset(int offset, bool Isfull = false)
            {
                if (Isfull)
                {
                    return (StartOffset <= offset && offset <= EndOffset);
                }
                else
                {
                    return (StartOffset <= offset && offset < EndOffset);
                }
            }

            public int StartOffset;
            public int EndOffset;
            public TOKEN_TYPE Type;
            public string Value;


            public string Special;
        }
        



        private void addToken(List<TOKEN> tokenlist, TOKEN token, int caretoffset, bool initText)
        {
            if (initText)
            {
                token.StartOffset = -1;
                token.EndOffset = -1;
            }

            tokenlist.Add(token);

            if (initText)
            {
                return;
            }


            if (token.CheckOffset(caretoffset))
            {
                currenttokenIndex = tokenlist.Count() - 1;
            }
        }
        public List<TOKEN> GetTokenList(string text, int caretoffset, bool initText = false)
        {
            if(caretoffset == -1)
            {
                caretoffset = text.Length;
            }


            List<TOKEN> tokenlist = new List<TOKEN>();



            //Tokens.Clear();

            int tlen = text.Length;

            for (int i = 0; i < tlen; i++)
            {
                char t = text[i];
                int sindex = i;

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
                    TOKEN token = new TOKEN(sindex, type, block);
                    addToken(tokenlist, token, caretoffset, initText);
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

                    TOKEN token = new TOKEN(sindex, type, block);
                    addToken(tokenlist, token, caretoffset, initText);
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

                    TOKEN token = new TOKEN(sindex, type, block);
                    addToken(tokenlist, token, caretoffset, initText);
                }
                else
                {
                    //나머지 괄호나 
                    //t가 숫자일 경우 숫자
                    string block = t.ToString();
                    bool isComment = false;

                    //Comment 만약 마지막 문자면 주석일 이유가 없으니까.
                    if (i + 1 < tlen)
                    {
                        int outindex;

                        TOKEN token = TokenBlockAnalyzer(text, i, out outindex);

                        if(token != null)
                        {
                            isComment = true;
                            addToken(tokenlist, token, caretoffset, initText);
                            i = outindex;
                        }
                    }

                    if (!isComment)
                    {
                        TOKEN_TYPE type = TOKEN_TYPE.Symbol;

                        TOKEN token = new TOKEN(sindex, type, block);
                        addToken(tokenlist, token, caretoffset, initText);
                    }
                }
            }

            return tokenlist;
        }



        private Dictionary<string, string> additionalstring = new Dictionary<string, string>();

        public void ResetAdditionalstring()
        {
            additionalstring.Clear();
        }
        public void AddAdditionalstring(string key, string value)
        {
            if (additionalstring.ContainsKey(key))
            {
                additionalstring[key] = value;
            }
            else
            {
                additionalstring.Add(key, value);
            }
        }


        public void Apply(string text, int caretoffset)
        {
            currentoffset = caretoffset;
            currenttokenIndex = -1;

            workComplete = false;
            List<TOKEN> tempList = new List<TOKEN>();

            List<string> addlist = additionalstring.Values.ToList();
            for (int i = 0; i < additionalstring.Count; i++)
            {
                //GetTokens(addlist[i], -1)
            }
            tempList.AddRange(GetTokenList(text, caretoffset));

            if(currenttokenIndex == -1)
            {
                currenttokenIndex = Tokens.IndexOf(SearchToken(currentoffset, true));
            }


            //여기다가 토큰을 분석하자
            TokenAnalyzer();


            codeFoldingManager.FoldingUpdate(Tokens, text.Length);


            textEditor.Dispatcher.Invoke(new Action(() => {
                Tokens.Clear();
                Tokens.AddRange(tempList);

                workComplete = true;
            }), DispatcherPriority.Normal);
        }
    }
}
