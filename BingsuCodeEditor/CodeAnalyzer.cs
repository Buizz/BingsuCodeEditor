using BingsuCodeEditor.AutoCompleteToken;
using ICSharpCode.AvalonEdit;
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


            maincontainer = new AutoCompleteToken.Container(this);
            Template = new Dictionary<string, string>();
        }

        public abstract ImportManager StaticImportManager
        {
            get;
        }



        public abstract Container GetDefaultContainer
        {
            get;
        }


        public enum CommentType
        {
            Set,
            Clear,
            Toggle
        }
        public abstract void SetCommentLine(int start, int end, CommentType commentType);



        public abstract void SetImportManager(ImportManager importManager);

        public string folder
        {
            get
            {
                if (FilePath.IndexOf(".") == -1) return "";


                return FilePath.Substring(0, FilePath.IndexOf("."));
            }
        }
        public string FilePath = "";
        public void RefershImportContainer(Container container)
        {
            if (StaticImportManager == null) return;
            string pullpath = StaticImportManager.GetPullPath(FilePath);

            if (StaticImportManager.IsFileExist(pullpath))
            {
                if (!StaticImportManager.IsCachedContainer(pullpath))
                {
                    //파일이 변형되었을 경우
                    StaticImportManager.UpdateContainer(pullpath, container);
                }                
            }
        }


        private bool IsSpaceCheck;


        public List<string> FuncPreChar = new List<string>();
        public bool CheckIsLetter(string t)
        {
            return FuncPreChar.IndexOf(t) != -1;
        }
        public bool CheckIsLetter(char t)
        {
            return FuncPreChar.IndexOf(t.ToString()) != -1;
        }


        public TokenAnalyzer tokenAnalyzer;
        public TokenAnalyzer secondtokenAnalyzer;
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

        public Dictionary<string, string> Template;

        public CursorLocation cursorLocation;
        public enum CursorLocation
        {
            None,
            Keyword,
            FunctionName,
            FunctionArgName,
            FunctionArgType,
            ImportFile,
            ImportNameSpace,
            VarTypeDefine,
            VarName,
            ObjectDefine,
            CallFunction,
            ForEachDefine,
            ForFuncDefine
        }



        #region Container관리
        public AutoCompleteToken.Container maincontainer;

        public Container GetContainer(string filetext)
        {
            secondtokenAnalyzer.Init(GetTokenList(filetext));
            return secondtokenAnalyzer.ConatainerAnalyzer();
        }
        #endregion




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
            ArgType,
            Special
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


        public string LineDirectString(bool left = false)
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

        /// <summary>
        /// 분석중인 자동완성 데이터입니다.
        /// 비동기로 분석합니다.
        /// </summary>
        protected List<PreCompletionData> AnalyzercompletionDatas = new List<PreCompletionData>();

        protected List<PreCompletionData> completionDatas = new List<PreCompletionData>();
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





        public virtual bool GetCompletionList(IList<ICompletionData> data, bool IsNameSpaceOpen = false)
        {
            switch (cursorLocation)
            {
                default:
                    for (int i = 0; i < completionDatas.Count; i++)
                    {
                        data.Add(new CodeCompletionData(completionDatas[i]));
                    }
                    break;
            }

            return false;
        }


        public abstract string GetTooiTipText(TOKEN token);

        public abstract void TokenAnalyze(int caretoffset = int.MaxValue);
        public abstract TOKEN TokenBlockAnalyzer(string text, int index, out int outindex, int caretoffset);


        public abstract bool AutoInsert(string text);
        public abstract bool AutoRemove();



        public string GetDirectWord(int offset = -1, int len = 10)
        {
            string rstr = "";
            for (int i = 0; i < len; i++)
            {
                string t = GetDirectText(offset - i);

                if (string.IsNullOrWhiteSpace(t)) return rstr;


                rstr = t + rstr;
            }

            return rstr;
        }

        public string GetDirectText(int offset = 0)
        {
            int caret = textEditor.CaretOffset;

            if (caret + offset < 0) return "";

            if(textEditor.Document.TextLength <= caret + offset)
            {
                return "";
            }

            return textEditor.Document.GetCharAt(caret + offset).ToString();
        }




        public void DirectInsetText(string text, int offset = 0, bool IsMove = false)
        {
            if (textEditor.Document.TextLength < offset)
            {
                return;
            }
            if (IsMove)
            {
                textEditor.Document.Insert(offset, text, ICSharpCode.AvalonEdit.Document.AnchorMovementType.AfterInsertion);
            }
            else
            {
                textEditor.Document.Insert(offset, text, ICSharpCode.AvalonEdit.Document.AnchorMovementType.BeforeInsertion);
            }
        }

        public void DirectRemoveText(int len, int offset = 0)
        {
            if (textEditor.Document.TextLength < offset + len ||
                0 > offset)
            {
                return;
            }

            textEditor.Document.Remove(offset, len);
        }

        public void DirectInsetTextFromCaret(string text, int offset = 0, bool IsMove = false)
        {
            int caret = textEditor.CaretOffset;

            if (textEditor.Document.TextLength < caret + offset)
            {
                return;
            }
            if (IsMove)
            {
                textEditor.Document.Insert(caret + offset, text, ICSharpCode.AvalonEdit.Document.AnchorMovementType.AfterInsertion);
            }
            else
            {
                textEditor.Document.Insert(caret + offset, text, ICSharpCode.AvalonEdit.Document.AnchorMovementType.BeforeInsertion);
            }
        }

        public void DirectRemoveTextFromCaret(int len, int offset = 0)
        {
            int caret = textEditor.CaretOffset;

            if (textEditor.Document.TextLength < caret + offset + len ||
                0 > caret + offset)
            {
                return;
            }

            textEditor.Document.Remove(caret + offset, len);
        }

        public void AutoDefaultInsert(string input)
        {
            string next = GetDirectText();
            
            bool IsLineEnd = false;
            if (next == "" || next == "\r" || next == "\n") IsLineEnd = true;


            switch (input)
            {
                case "{":
                    if (IsLineEnd || next == "}")
                    {
                        DirectInsetTextFromCaret("}");
                    }
                    break;
                case "[":
                    if (IsLineEnd || next == "]")
                    {
                        DirectInsetTextFromCaret("]");
                    }
                    break;
                case "(":
                    if (IsLineEnd || next == ")")
                    {
                        DirectInsetTextFromCaret(")");
                    }
                    break;
                case "}":
                case "]":
                case ")":
                    if (!IsLineEnd && next == input)
                    {
                        DirectRemoveTextFromCaret(1);
                    }
                    break;
                case "\"":
                case "\'":
                    if (IsLineEnd)
                    {
                        DirectInsetTextFromCaret(input);
                    }
                    //else if (!IsLineEnd
                    //    && ctoken.Type == CodeAnalyzer.TOKEN_TYPE.String
                    //    && LineDirectString() == input)
                    //{
                    //    textEditor.SelectionLength = 1;
                    //    textEditor.SelectedText = "";
                    //    textEditor.SelectionLength = 0;
                    //}
                    break;
                //case ";":
                //    if(next == ")")
                //    {
                //        textEditor.CaretOffset++;
                //    }
                //    break;
            }
        }


        public bool AutoDefaultRemove()
        {
            if (textEditor.SelectionLength != 0)
            {
                return false;
            }

            //TOKEN ftk = GetToken(-1);
            //TOKEN ttk = GetToken(0);

            string fstr = GetDirectText(-1);
            string bstr = GetDirectText();


            bool IsLineEnd = false;
            if (bstr == "" || bstr == "\r") IsLineEnd = true;

            if (IsLineEnd)
            {
                return false;
            }

            if (
                (fstr == "{" && bstr == "}")
                || (fstr == "[" && bstr == "]")
                || (fstr == "(" && bstr == ")"))
            {
                DirectRemoveTextFromCaret(2, -1);
                return true;
            }

            if (bstr == "\"\"")
            {
                DirectRemoveTextFromCaret(2, -1);
                return true;
            }



            return false;
        }


        private bool workComplete = true;
        public bool WorkCompete
        {
            get
            {
                return workComplete;
            }
        }
        private string LastAnalyzeText;


        private int lasttokenIndex;

        private int _currenttokenIndex;
        private int currenttokenIndex
        {
            get
            {
                if (_currenttokenIndex == -1)
                {
                    //_currenttokenIndex = Tokens.IndexOf(SearchToken(currentoffset, true));
                }
                return _currenttokenIndex;
            }
        }
        private int currentoffset;

        public int CurrentTokenIndex
        {
            get
            {
                return currenttokenIndex;
            }
        }



        public void WaitToUpdate(TextEditor textEditor)
        {
            //while (!WorkCompete)
            //{

            //}
            //Apply(textEditor.Text, textEditor.CaretOffset);
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
        public TOKEN GetToken(int index, TOKEN.Side side = TOKEN.Side.None)
        {
            int cmpindex;


            if (currenttokenIndex == -1)
            {
                if (index == 0)
                {
                    //현대 위치가 없는것 이므로 null을 반환
                    return null;
                }
                else
                {
                    cmpindex = lasttokenIndex + index;
                }
            }
            else
            {
                cmpindex = currenttokenIndex + index;
            }

            if (cmpindex < 0 || cmpindex >= Tokens.Count)
            {
                return null;
            }


            TOKEN tk = Tokens[cmpindex];

            switch (side)
            {
                case TOKEN.Side.Left:
                    if (tk.side == TOKEN.Side.Right)
                    {
                        cmpindex++;
                        if (!(cmpindex < 0 || cmpindex >= Tokens.Count))
                        {
                            tk = Tokens[cmpindex];
                        }
                    }
                    break;
                case TOKEN.Side.Right:

                    if (tk.side == TOKEN.Side.Left)
                    {
                        cmpindex--;
                        if (!(cmpindex < 0 || cmpindex >= Tokens.Count))
                        {
                            tk = Tokens[cmpindex];
                        }
                    }

                    break;
            }

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




        public TOKEN SearchToken(double offset, bool isfull = false, bool IsNullable = false)
        {
            for (int i = 0; i < Tokens.Count; i++)
            {
                TOKEN tk = Tokens[i];


                if (tk.CheckOffset(offset, isfull))
                {
                    return tk;
                }
                if (!IsNullable)
                {
                    if (tk.EndOffset < offset && i + 1 < Tokens.Count)
                    {
                        TOKEN ntk = Tokens[i + 1];
                        if (ntk.StartOffset > offset)
                        {
                            return tk;
                        }
                    }
                }
              
            }

            return null;
        }



        public class TOKEN
        {
            public TOKEN(int Offset, TOKEN_TYPE Type, string Value, int caretoffset)
            {
                this.StartOffset = Offset;
                this.EndOffset = Offset + Value.Length;
                this.Type = Type;
                this.Value = Value;

                if (StartOffset == caretoffset)
                {
                    side = TOKEN.Side.Left;
                }
                else
                if (EndOffset == caretoffset)
                {
                    side = TOKEN.Side.Right;
                }


                funcname = new List<TOKEN>();
                argindex = -1;
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
            public bool CheckOffset(double offset, bool Isfull = false)
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

            public ErrorToken errorToken;

            public string Special;

            public int argindex;
            public List<TOKEN> funcname;

            public string scope = "st";

            public Side side = Side.None;


            public enum Side
            {
                Left,
                Right,
                OutSide,
                None
            }
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
            if (lasttokenIndex == -1 && token.StartOffset > caretoffset)
            {
                lasttokenIndex = tokenlist.Count() - 1;
            }

            if (token.CheckOffset(caretoffset))
            {
                _currenttokenIndex = tokenlist.Count() - 1;
            }
        }
        public List<TOKEN> GetTokenList(string text, int caretoffset = -1, bool initText = false)
        {
            if (caretoffset == -1)
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
                    TOKEN token = new TOKEN(sindex, type, block, caretoffset);
                    addToken(tokenlist, token, caretoffset, initText);
                }
                else if (char.IsLetter(t) || t == '_' || CheckIsLetter(t))
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
                    } while (i < tlen && char.IsLetterOrDigit(t) | t == '_' || CheckIsLetter(t));
                    i--;

                    TOKEN_TYPE type = TOKEN_TYPE.Identifier;
                    if (dcTokensubType.ContainsKey(block))
                    {
                        if (dcTokensubType[block] == TOKEN_TYPE.KeyWord)
                        {
                            type = TOKEN_TYPE.KeyWord;
                        }
                    }

                    TOKEN token = new TOKEN(sindex, type, block, caretoffset);
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

                    TOKEN token = new TOKEN(sindex, type, block, caretoffset);
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

                        TOKEN token = TokenBlockAnalyzer(text, i, out outindex, caretoffset);

                        if (token != null)
                        {
                            isComment = true;
                            addToken(tokenlist, token, caretoffset, initText);
                            i = outindex;
                        }
                    }

                    if (!isComment)
                    {
                        TOKEN_TYPE type = TOKEN_TYPE.Symbol;

                        TOKEN token = new TOKEN(sindex, type, block, caretoffset);
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


        public virtual void Apply(string text, int caretoffset)
        {
            workComplete = false;
            LastAnalyzeText = text;

            currentoffset = caretoffset;
            _currenttokenIndex = -1;
            lasttokenIndex = -1;

            List<TOKEN> tempList = new List<TOKEN>();

            List<string> addlist = additionalstring.Values.ToList();
            for (int i = 0; i < additionalstring.Count; i++)
            {
                //GetTokens(addlist[i], -1)
            }
            tempList.AddRange(GetTokenList(text, caretoffset));


            Tokens.Clear();
            Tokens.AddRange(tempList);



            if (currenttokenIndex == -1)
            {
                _currenttokenIndex = Tokens.IndexOf(SearchToken(currentoffset, true));
            }
            if (lasttokenIndex == -1)
            {
                lasttokenIndex = Tokens.Count;
            }

            //여기다가 토큰을 분석하자
            TokenAnalyze(caretoffset);

            if(codeFoldingManager != null)
            {
                codeFoldingManager.FoldingUpdate(Tokens, text.Length);
            }

            //에러판단

            workComplete = true;
        }

        public enum FindType
        {
            All,
            Func,
            Obj,
            AutoComplete
        }
        public abstract object GetObjectFromName(List<TOKEN> tokenlist, Container startcontainer, FindType findType, IList<ICompletionData> data = null, string scope = "st");
        public abstract object GetObjectFromName(List<string> objectname, Container startcontainer, FindType findType, IList<ICompletionData> data = null, string scope = "st");


        #region #############함수툴팁#############
        public string GetFuncToolTip(List<TOKEN> tklist = null)
        {
            //함수를 찾지 못했을 경우 빈문자 출력
            int argindex = -1;
            if (tklist == null)
            {
                argindex = maincontainer.innerFuncInfor.argindex;

                tklist = maincontainer.innerFuncInfor.funcename;
            }



            string funcname = "";
            if (tklist == null) return funcname;

            foreach (var item in tklist)
            {
                if (funcname != "") funcname += ".";
                funcname += item.Value;
            }


           
            Function func = (Function)GetObjectFromName(tklist, maincontainer, FindType.Func, scope: maincontainer.currentScope);
            
            
            if (func == null)
            {
                return funcname + "()\n" + "설명이 없습니다.";
            }

            string argstring = func.GetArgString(argindex);


            string rval = "";

            func.ReadComment("ko-KR");


            string argname = "";
            if (argindex < func.args.Count && argindex >= 0)
            {
                argname = func.args[argindex].argname;
            }

            rval = funcname + "(" + argstring + ")" + "\n" + func.funcsummary + "\n" + func.GetArgSummary(argname);


            return rval;
        }
        #endregion

        #region #############자동완성#############



        public class VarType : PreCompletionData
        {
            public VarType(CompletionWordType completionType, string name) : base(completionType, name)
            {
                Priority = 80;
            }

            //키워드 이름
            public override string listheader
            {
                get
                {
                    return name;
                }
            }
            public override string outputstring
            {
                get
                {
                    return name;
                }
            }
            public override string desc
            {
                get
                {
                    return name;
                }
            }
        }


        public class ObjectItem : PreCompletionData
        {
            public ObjectItem(CompletionWordType completionType, string name, Block block = null, Function function = null) : base(completionType, name)
            {
                Priority = 1;
                this.block = block;
                this.function = function;
            }

            private Block block;
            private Function function;


            //키워드 이름
            public override string listheader
            {
                get
                {
                    return name;
                }
            }
            public override string outputstring
            {
                get
                {
                    return name;
                }
            }
            public override string desc
            {
                get
                {
                    if (function != null)
                    {
                        return function.funcname + "(" + function.GetArgString() + ")\n" + function.funcsummary;
                    }
                    if (block != null)
                    {
                        return block.blockdefine;
                    }
                    return name;
                }
            }
        }


        public class ImportFileItem : PreCompletionData
        {
            public ImportFileItem(CompletionWordType completionType, string name) : base(completionType, name)
            {
                Priority = 70;
            }

            //키워드 이름
            public override string listheader
            {
                get
                {
                    return name;
                }
            }
            public override string outputstring
            {
                get
                {
                    return name;
                }
            }
            public override string desc
            {
                get
                {
                    return name;
                }
            }
        }


        public class KewWordItem : PreCompletionData
        {
            public KewWordItem(CompletionWordType completionType, string name, string desc = "") : base(completionType, name, desc: desc)
            {
                Priority = 95;
            }

            //키워드 이름
            public override string listheader
            {
                get
                {
                    return name;
                }
            }
            public override string outputstring
            {
                get
                {
                    return name;
                }
            }
            public override string desc
            {
                get
                {
                    if(string.IsNullOrEmpty(_desc))
                    {
                        return name;
                    }
                    else
                    {
                        return _desc;
                    }
                }
            }
        }


        public class CompletionItem : PreCompletionData
        {
            public CompletionItem(CompletionWordType completionType, string name, string outputstring) : base(completionType, name, outputstring)
            {
                Priority = 1000;

            }

            //키워드 이름
            public override string listheader
            {
                get
                {
                    return name;
                }
            }
            public override string outputstring
            {
                get
                {
                    if (string.IsNullOrEmpty(_outputstring))
                    {
                        return name;
                    }
                    else
                    {
                        return _outputstring;
                    }
                }
            }
            public override string desc
            {
                get
                {
                    return name;
                }
            }
        }

        #endregion
    }
}