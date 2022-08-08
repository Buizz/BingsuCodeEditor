using BingsuCodeEditor.AutoCompleteToken;
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
            string[] keywords = {"and", "break", "do", "else", "elseif",
                "end", "false", "for", "function", "if",
                "in", "local", "nil", "not", "or",
                "repeat", "return", "then", "true", "until",
                "while"};


            //[tab]for(var [i] = 0; [i] < [Length] ; [i]++)\n[tab]{\n[tab][tabonce][Content]\n[tab]}
            Template.Add("if", " [true] then\n[tab][tabonce][Content]\n[tab]end");
            Template.Add("while", " [true] do\n[tab][tabonce][Content]\n[tab]end");
            Template.Add("repeat", "\n[tab][tabonce][Content]\n[tab]until [true]");
            Template.Add("for", " [index] = [1], [Length] do\n[tab][tabonce][Content]\n[tab]end");
            Template.Add("function", " [FuncName]([Arg]) \n[tab][tabonce][Content]\n[tab]end");
            //Template.Add("/***", "\n[tab] * @Type\n[tab] * F\n[tab] * @Summary.ko-KR\n[tab] * [Summary]\n[tab] * @param.args.ko-KR\n[tab]***/[Content]");





            foreach (var item in keywords)
            {
                //토큰 입력
                AddSubType(item, TOKEN_TYPE.KeyWord);

                //자동완성 초기 입력
                if (Template.ContainsKey(item))
                {
                    completionDatas.Add(new KewWordItem(CompletionWordType.KeyWord, item, item + "\n참고:코드 조각을 삽입하려면 Tab키를 두번 누르세요."));
                }
                else
                {
                    completionDatas.Add(new KewWordItem(CompletionWordType.KeyWord, item));
                }
            }


            tokenAnalyzer = new LuaTokenAnalyzer();
            secondtokenAnalyzer = new LuaTokenAnalyzer();
            codeFoldingManager = new LuaFoldingManager(textEditor);
        }
        public override void AutoInsert(string text)
        {
            return;
        }

        public override bool AutoRemove()
        {
            return false;
        }
        public override TOKEN TokenBlockAnalyzer(string text, int index, out int outindex, int caretoffset)
        {
            outindex = 0;
            return null;
        }

        public override bool GetCompletionList(IList<ICompletionData> data, bool IsNameSpaceOpen = false)
        {
            string scope = maincontainer.currentScope;



            Container container = maincontainer;
            Container objcontainer = null;


            if (scope.IndexOf("st.O") != -1)
            {
                //string[] scopes = scope.Split('.');
                //string initscope = scopes[0] + "." + scopes[1];

                List<string> tname = new List<string>();
                tname.Add(scope.Split('.')[1].Substring(1));
                object _obj = GetObjectFromName(tname, maincontainer, FindType.Obj, scope: "st");

                if (_obj != null)
                {
                    objcontainer = (Container)_obj;
                }
            }





            switch (cursorLocation)
            {
                case CursorLocation.ImportFile:
                    //파일들이 뜨게 하는 것
                    if (importManager != null)
                    {
                        if (!IsNameSpaceOpen)
                        {
                            string fname = "";

                            TOKEN ctkn = GetToken(0, TOKEN.Side.Right);
                            List<TOKEN> t = tokenAnalyzer.GetTokenListFromTarget(ctkn, true);

                            List<string> strs = new List<string>();
                            for (int i = 0; i < t.Count - 1; i++)
                            {
                                strs.Add(t[i].Value);

                                fname += t[i].Value;
                                fname += ".";
                            }


                            if (fname == "")
                            {
                                foreach (var item in importManager.GetFileList())
                                {
                                    data.Add(new CodeCompletionData(new ImportFileItem(CompletionWordType.nameSpace, item)));
                                }
                                return true;
                            }
                            else
                            {
                                foreach (var item in importManager.GetFileList())
                                {
                                    string tstr = item;
                                    if (tstr.StartsWith(fname))
                                    {
                                        tstr = tstr.Replace(fname, "");
                                        data.Add(new CodeCompletionData(new ImportFileItem(CompletionWordType.nameSpace, tstr)));
                                    }
                                }
                                return true;
                            }
                        }
                    }
                    break;
            }


            //TODO:분석된 토큰으로 자동완성을 만든다.
            if (IsNameSpaceOpen)
            {
                TOKEN ctkn = GetToken(0, TOKEN.Side.Right);
                if (ctkn == null) return true;

                List<TOKEN> t = tokenAnalyzer.GetTokenListFromTarget(ctkn, true);
                //imported1.var1;
                //imported1.const1.object1;
                //const1.object1;
                //maincontainer.vars[0].

                //Item.cast(inven[i]). 이럴 경우 Item.cast를 t로 보낸다.
                //Item.cast함수의 반환타입을 구해야 한다.
                //t.Add(new TOKEN(0, TOKEN_TYPE.Identifier, "Item", 0));
                //t.Add(new TOKEN(0, TOKEN_TYPE.Identifier, "cast", 0));
                if (t.Count == 0) return true;


                List<string> strs = new List<string>();

                string fname = "";
                foreach (var item in t)
                {
                    strs.Add(item.Value);

                    fname += item.Value;
                    fname += ".";
                }

                string last = GetDirectText(0);
                if (cursorLocation == CursorLocation.ImportFile && importManager != null)
                {
                    bool IsImport = false;
                    List<string> filelist = importManager.GetFileList(maincontainer.folderpath);
                    List<string> autocmpfilelist = new List<string>();
                    foreach (var item in filelist)
                    {
                        if (item.StartsWith(fname))
                        {
                            //일치할 경우
                            IsImport = true;
                            autocmpfilelist.Add(item.Substring(fname.Length));
                        }
                    }
                    if (IsImport)
                    {
                        foreach (var item in autocmpfilelist)
                        {
                            data.Add(new CodeCompletionData(new ImportFileItem(CompletionWordType.nameSpace, item)));
                        }
                    }
                }



                GetObjectFromName(strs, container, FindType.AutoComplete, data, scope);


                //foreach (var item in t)
                //{
                //    PreCompletionData preCompletionData = new ImportFileItem(CompletionWordType.nameSpace, item.Value);
                //    data.Add(new CodeCompletionData(preCompletionData));
                //}

                return true;
            }
            switch (cursorLocation)
            {
                case CursorLocation.FunctionArgType:
                case CursorLocation.VarTypeDefine:
                    if (cursorLocation == CursorLocation.FunctionArgType)
                    {
                        foreach (var item in LuaDefaultCompletionData.GetCompletionKeyWordList())
                        {
                            data.Add(item);
                        }
                    }
                    else
                    {
                        foreach (var item in container.objs)
                        {
                            data.Add(new CodeCompletionData(new ObjectItem(CompletionWordType.Variable, item.mainname)));
                        }
                        foreach (var item in DefaultFuncContainer.objs)
                        {
                            data.Add(new CodeCompletionData(new ObjectItem(CompletionWordType.Variable, item.mainname)));
                        }
                    }

                    return true;
            }

            if (maincontainer.innerFuncInfor.IsInnerFuncinfor)
            {
                //인자
                Function func = (Function)GetObjectFromName(maincontainer.innerFuncInfor.funcename, maincontainer, FindType.Func);
                if (func != null)
                {
                    if (func.args.Count <= maincontainer.innerFuncInfor.argindex)
                    {
                        return true;
                    }
                    string argtype = func.args[maincontainer.innerFuncInfor.argindex].argtype;

                    foreach (var item in LuaDefaultCompletionData.GetCompletionDataList(argtype))
                    {
                        data.Add(item);
                    }
                }
            }
            else
            {
                if (base.GetCompletionList(data)) return true;
            }




            container.GetAllItems(data, scope);
            if (objcontainer != null)
            {
                data.Add(new CodeCompletionData(new ObjectItem(CompletionWordType.Const, "this")));
                objcontainer.GetAllItems(data, scope);
            }
            DefaultFuncContainer.GetAllItems(data, "st");



            return true;
        }

        public override void TokenAnalyze(int caretoffset = int.MaxValue)
        {
            CursorLocation cl = CursorLocation.None;


            //TOKEN clefttoken = GetToken(0, TOKEN.Side.Left);

            TOKEN ctoken = GetToken(0);
            TOKEN btoken = GetToken(-1);
            TOKEN bbtoken = GetToken(-2);

            if (ctoken != null)
            {
                if (ctoken.Type == TOKEN_TYPE.Symbol && ctoken.Value == ";")
                {
                    ctoken = GetToken(-1);
                    btoken = GetToken(-2);
                    bbtoken = GetToken(-3);
                }
                if (ctoken.Type == TOKEN_TYPE.KeyWord && ctoken.Value == "function")
                {
                    cl = CursorLocation.FunctionName;
                }
            }

            if (btoken != null)
            {
                if (btoken.Type == TOKEN_TYPE.KeyWord)
                {
                    switch (btoken.Value)
                    {
                        case "var":
                        case "const":
                        case "as":
                            cl = CursorLocation.VarName;
                            break;
                        case "function":
                            cl = CursorLocation.FunctionName;
                            break;
                        case "object":
                            cl = CursorLocation.ObjectDefine;
                            break;
                    }
                }
            }


            if (cl == CursorLocation.None)
            {
                if (ctoken != null && ctoken.Type == TOKEN_TYPE.Symbol && ctoken.Value == ":")
                {
                    cl = CursorLocation.VarTypeDefine;
                }
                if (btoken != null && btoken.Type == TOKEN_TYPE.Symbol && btoken.Value == ":")
                {
                    cl = CursorLocation.VarTypeDefine;
                }
                //if(ctoken != null)
                //{
                //    int argpos;
                //    tokenAnalyzer.GetWritedFunction(ctoken, out argpos);
                //}
            }


            cursorLocation = cl;


            //토근 분석에 사용되는 요소
            tokenAnalyzer.Init(Tokens);

            try
            {
                maincontainer = tokenAnalyzer.ConatainerAnalyzer(caretoffset);
                //RefershImportContainer(maincontainer);

                if (maincontainer.cursorLocation != CursorLocation.None)
                {
                    cursorLocation = maincontainer.cursorLocation;
                }
            }
            catch (Exception e)
            {
                TOKEN errortoken = tokenAnalyzer.GetLastToken;
                if (!(errortoken is null))
                {
                    tokenAnalyzer.ThrowException(e.ToString(), tokenAnalyzer.GetLastToken);
                }
                //tokenAnalyzer.ErrorMessage;

                //return;
            }
            if (tokenAnalyzer.IsError)
            {
                //토큰 분석 오류
            }


            tokenAnalyzer.Complete(textEditor);
        }

        public override object GetObjectFromName(List<TOKEN> tokenlist, Container startcontainer, FindType findType, IList<ICompletionData> data = null, string scope = "st")
        {
            return null;
        }

        public override object GetObjectFromName(List<string> objectname, Container startcontainer, FindType findType, IList<ICompletionData> data = null, string scope = "st")
        {
            return null;
        }

        public override string GetTooiTipText(TOKEN token)
        {
            return " t";
        }
    }
}
