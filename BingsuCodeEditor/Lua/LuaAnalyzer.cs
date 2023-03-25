using BingsuCodeEditor.AutoCompleteToken;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using static BingsuCodeEditor.CodeAnalyzer;

namespace BingsuCodeEditor.Lua
{
    class LuaAnalyzer : CodeAnalyzer
    {
        public static ImportManager importManager;
        public static string DEFAULTFUNCFILENAME = "DEFAULTFUNCTIONLIST";
        public static Container _DefaultFuncContainer;
        public static Container DefaultFuncContainer
        {
            get
            {
                return _DefaultFuncContainer;
            }
        }

        public override ImportManager StaticImportManager
        {
            get
            {
                return LuaAnalyzer.importManager;
            }
        }

        public override Container GetDefaultContainer
        {
            get
            {
                return _DefaultFuncContainer;
            }
        }

        public override void SetImportManager(ImportManager importManager)
        {
            LuaAnalyzer.importManager = importManager;

            if (_DefaultFuncContainer == null)
            {
                _DefaultFuncContainer = new Container(this);

                foreach (var item in importManager.GetFIleList())
                {
                    Container c = GetContainer(importManager.GetFIleContent(item));

                    _DefaultFuncContainer.funcs.AddRange(c.funcs);
                }

                foreach (var item in _DefaultFuncContainer.funcs)
                {
                    if (!string.IsNullOrEmpty(item.comment)) item.ReadComment("ko-KR");
                }
            }

            if (!importManager.IsCachedContainer(DEFAULTFUNCFILENAME))
            {
                importManager.UpdateContainer(DEFAULTFUNCFILENAME, _DefaultFuncContainer);
            }

        }



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


            LuaAnalyzer.DEFAULTFUNCFILENAME = "DEFAULTFUNCTIONLIST";


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


            tokenAnalyzer = new LuaTokenAnalyzer(this);
            secondtokenAnalyzer = new LuaTokenAnalyzer(this);
            //codeFoldingManager = new LuaFoldingManager(textEditor);
        }
        public override bool AutoInsert(string text)
        {
            return false;
        }

        public override bool AutoRemove()
        {
            return false;
        }
        public override TOKEN TokenBlockAnalyzer(string text, int index, out int outindex, int caretoffset)
        {
            int sindex = index;
            char t = text[index];
            int tlen = text.Length;
            string block = t.ToString();

            outindex = -1;

            if (t == '"')
            {
                block = "";
                //LineCommnet 개행 문자까지 반복 (\r)

                bool IsSpec = false;
                do
                {
                    block += t.ToString();


                    index++;
                    if (index >= tlen)
                    {
                        break;
                    }
                    t = text[index];


                    if (t == '"' && !IsSpec)
                    {
                        break;
                    }

                    if (t == '\\')
                    {
                        IsSpec = true;
                    }
                    else
                    {
                        IsSpec = false;
                    }


                } while (index < tlen);
                block += '"';

                TOKEN_TYPE type = TOKEN_TYPE.String;

                TOKEN token = new TOKEN(sindex, type, block, caretoffset);
                outindex = index;
                return token;
            }
            else if (t == '-')
            {
                if (index + 1 >= tlen)
                {
                    return null;
                }
                char nt = text[index + 1];

                if (nt == '-')
                {


                    index++;
                    if (index + 1 >= tlen)
                    {
                        return null;
                    }

                    t = text[index + 1];
                    if(t == '[')
                    {
                        int equalcount = 0;
                        //MulitComment =====[ 카운팅 가 나올 떄 까지 반복


                        index += 2;
                        if (index >= tlen)
                        {
                            return null;
                        }
                        t = text[index];
                        while (t != '[' && index < tlen)
                        {
                            t = text[index++];

                            if (t == '=')
                            {
                                equalcount++;
                            }
                        }
                        // t ='='

                        int checkequalcount = -1;

                        index++;
                        if (index >= tlen)
                        {
                            return null;
                        }
                        block = "";
                        do
                        {
                            index++;
                            if (index >= tlen)
                            {
                                //사실상 오류임..
                                break;
                            }
                            if (t == ']')
                            {
                                if(checkequalcount == -1)
                                {
                                    checkequalcount = 0;
                                }else if(checkequalcount == equalcount)
                                {
                                    block = block.Substring(0, block.Length - equalcount - 2);
                                    //주석
                                    break;
                                }
                                else
                                {
                                    checkequalcount = -1;
                                }

                            }

                            if(t == '=' && checkequalcount >= 0)
                            {
                                checkequalcount++;
                            }

                            t = text[index];
                            block += t.ToString();
                        } while (index < tlen);
                        index--;


                        TOKEN_TYPE type = TOKEN_TYPE.Comment;

                        //block = block.Replace("\r", "");

                        TOKEN token = new TOKEN(sindex, type, block, caretoffset);
                        outindex = index;
                        return token;
                    }
                    else
                    {
                        //LineCommnet 개행 문자까지 반복 (\r)
                        do
                        {
                            block += t.ToString();
                            index++;
                            if (index >= tlen)
                            {
                                break;
                            }
                            t = text[index];
                        } while (index < tlen && (t != '\n'));
                        index--;


                        TOKEN_TYPE type = TOKEN_TYPE.LineComment;

                        block = block.Replace("\r", "");

                        TOKEN token = new TOKEN(sindex, type, block, caretoffset);
                        outindex = index;
                        return token;
                    }
                    
                }
            }
         

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
                                foreach (var item in importManager.GetImportedFileList())
                                {
                                    data.Add(new CodeCompletionData(new ImportFileItem(CompletionWordType.nameSpace, item)));
                                }
                                return true;
                            }
                            else
                            {
                                foreach (var item in importManager.GetImportedFileList())
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
                    List<string> filelist = importManager.GetImportedFileList(maincontainer.folderpath);
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
            if (DefaultFuncContainer != null) { DefaultFuncContainer.GetAllItems(data, "st"); }
            if (importManager != null)
            {
                foreach (var pullpath in importManager.GetImportedFileList())
                {
                    if(pullpath != FilePath)
                    {
                        if (importManager.IsFileExist(pullpath))
                        {
                            if (!importManager.IsCachedContainer(pullpath))
                            {
                                //파일이 변형되었을 경우
                                importManager.UpdateContainer(pullpath, GetContainer(importManager.GetFIleContent(pullpath)));
                            }
                        }
                        importManager.GetContainer(pullpath).GetAllItems(data, "st");
                    }
                }
            }

            


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
                        case "function":
                            cl = CursorLocation.FunctionName;
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
            if (tokenlist == null) return null;

            List<string> strs = new List<string>();

            foreach (var item in tokenlist)
            {
                string rval;
                if(strs.Count == 0 && item.Value.IndexOf("@") == 0)
                {
                    rval = item.Value.Replace("@", "");
                }
                else
                {
                    rval = item.Value;
                }

                strs.Add(rval);
            }

            return GetObjectFromName(strs, startcontainer, findType, data, scope);
        }

        public override object GetObjectFromName(List<string> objectname, Container startcontainer, FindType findType, IList<ICompletionData> data = null, string scope = "st")
        {
            //imported1.var1;
            //imported1.const1.object1;
            //const1.object1;
            //maincontainer.vars[0].

            //참조 찾아보고 없으면 패스
            //if (!startcontainer.CheckIdentifier(scope, objectname[0]))
            //{
            //    return null;
            //}
            if (objectname.Count == 0) return null;

            Container ccon = startcontainer;
            Container objcon = null;
            string folderpath = folder;
            int index = 0;
            string lscope = scope;
            while (true)
            {
                string objname = objectname[index];


                Block var = null;
                Container obj = null;
                Function func = null;
                ImportedNameSpace importedNameSpace = null;



                if (objcon != null)
                {
                    var = objcon.vars.Find(x => (x.blockname == objname && lscope.Contains(x.scope)));
                    obj = objcon.objs.Find(x => (x.mainname == objname));
                    func = objcon.funcs.Find(x => (x.funcname == objname && lscope.Contains(x.scope)));
                }
                else
                {
                    var = ccon.vars.Find(x => (x.blockname == objname && lscope.Contains(x.scope)));
                    obj = ccon.objs.Find(x => (x.mainname == objname));
                    func = ccon.funcs.Find(x => (x.funcname == objname && lscope.Contains(x.scope)));
                    importedNameSpace = ccon.importedNameSpaces.Find(x => (x.shortname == objname));
                }

                if (LuaAnalyzer.DefaultFuncContainer != null)
                {
                    if (var == null)
                    {
                        var = LuaAnalyzer.DefaultFuncContainer.vars.Find(x => (x.blockname == objname && lscope.Contains(x.scope)));
                    }
                    if (obj == null)
                    {
                        obj = LuaAnalyzer.DefaultFuncContainer.objs.Find(x => (x.mainname == objname));
                    }
                    if (func == null)
                    {
                        func = LuaAnalyzer.DefaultFuncContainer.funcs.Find(x => (x.funcname == objname && lscope.Contains(x.scope)));
                    }
                }

                //if (objname == "this" && objectname.Count == 1)
                //{
                //    //자기참조일 경우
                //    if (findType == FindType.AutoComplete)
                //    {
                //        //마지막 부분이므로 해당 콘테이너의 내용을 모두 넣는다.
                //        ccon.GetAllItems(data, "st.O" + startcontainer.mainname);
                //    }
                //}


                if (importedNameSpace != null)
                {
                    string nsfile = importedNameSpace.mainname;
                    string pullpath = importManager.GetPullPath(nsfile, folderpath);

                    if (importManager.IsFileExist(pullpath))
                    {
                        if (!importManager.IsCachedContainer(pullpath))
                        {
                            //파일이 변형되었을 경우
                            importManager.UpdateContainer(pullpath, GetContainer(importManager.GetFIleContent(pullpath)));
                        }

                        ccon = importManager.GetContainer(pullpath);

                        if (pullpath.IndexOf(".") != -1)
                        {
                            folderpath = pullpath.Substring(0, pullpath.LastIndexOf("."));
                        }
                        else
                        {
                            folderpath = "";
                        }

                        lscope = "st";//스코프 초기화
                        index++;
                        if (index == objectname.Count)
                        {
                            if (findType == FindType.AutoComplete)
                            {
                                //마지막 부분이므로 해당 콘테이너의 내용을 모두 넣는다.
                                ccon.GetAllItems(data, lscope);
                            }
                            else if (findType == FindType.All)
                            {
                                return importedNameSpace;
                            }

                            break;
                        }
                    }
                    else
                    {
                        return null;
                    }

                }
                else if (func != null)
                {
                    if (findType == FindType.Func || findType == FindType.All)
                    {
                        //마지막 찻수인경우
                        if (index + 1 == objectname.Count)
                        {
                            //함수 찾는거면 이 func를 돌려주면 됨.ㅋㅋ
                            return func;
                        }

                        break;
                    }
                    else if (findType == FindType.AutoComplete)
                    {
                        //자동완성이면 해당 함수의 반환타입을 주사
                        string rtype = "";

                        foreach (var item in func.returntype)
                        {
                            if (rtype != "") rtype += ",";
                            rtype += item.Value;
                        }


                        if (rtype == null) return null;

                        List<string> tname = new List<string>();
                        tname.AddRange(rtype.Split('.'));
                        object _obj = GetObjectFromName(tname, ccon, FindType.Obj);

                        if (_obj != null)
                        {
                            objcon = (Container)_obj;
                        }
                        else
                        {
                            return null;
                        }


                        if (index + 1 == objectname.Count)
                        {
                            //마지막 찻수인 경우 오브젝트의 요소들 반환
                            objcon.GetAllItems(data, objcon.GetInitObjectNameSpacee(), noargFlag: true);
                            break;
                        }

                    }


                    return null;
                }
                else if (obj != null)
                {
                    if (findType == FindType.Obj)
                    {
                        if (index + 2 == objectname.Count)
                        {
                            switch (objectname[index + 1])
                            {
                                case "cast":
                                case "alloc":
                                    return obj;
                            }
                        }
                    }




                    if (index + 1 == objectname.Count)
                    {
                        if (findType == FindType.AutoComplete)
                        {
                            //마지막 부분이므로 해당 콘테이너의 내용을 모두 넣는다.
                            data.Add(new CodeCompletionData(new ObjectItem(CompletionWordType.Function, "cast")));
                            data.Add(new CodeCompletionData(new ObjectItem(CompletionWordType.Function, "alloc")));
                            data.Add(new CodeCompletionData(new ObjectItem(CompletionWordType.Function, "free")));
                        }
                        else if (findType == FindType.Obj || findType == FindType.All)
                        {
                            return obj;
                        }


                        break;
                    }
                    else
                    {

                        if (index + 1 < objectname.Count)
                        {
                            string last = objectname[index + 1];

                            if (last == "alloc" || last == "cast")
                            {
                                if (index + 2 == objectname.Count)
                                {
                                    //마지막이 alloc, cast인 경우
                                    if (findType == FindType.AutoComplete)
                                    {
                                        obj.GetAllItems(data, obj.GetInitObjectNameSpacee(), noargFlag: true);
                                        break;
                                    }
                                    else if (findType == FindType.Func)
                                    {
                                        if (last == "alloc")
                                        {
                                            //생성자 함수 가져오기
                                            func = obj.funcs.Find(x => (x.funcname == "constructor" && obj.GetInitObjectNameSpacee().Contains(x.scope)));
                                            return func;
                                        }
                                    }
                                    return null;
                                }
                                else
                                {
                                    //오브젝트를 참조하는 것.
                                    lscope = obj.GetInitObjectNameSpacee();
                                    objcon = obj;
                                    index += 2;
                                }
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else if (var != null)
                {
                    //변수의 정의를 참조하여 타입을 확인.
                    //obj이면 해당 obj의 멤버변수 ex var t = a.b.c(); 이런식이면
                    //a.b.c를 탐색해야됨.
                    //var.values
                    //obj일 경우 obj는 콘테이너이므로... 함수 찾는 과정이면 한단계 더 들어갈 수 있음.
                    object _obj;


                    if (!string.IsNullOrEmpty(var.blocktype))
                    {
                        List<string> list = new List<string>();

                        list.Add(var.blocktype);


                        //타입이 지정되어 있을 경우
                        if (ccon.IsObject)
                        {
                            //오브젝트이면 밖에서 찾아야 함니다.

                        }
                        _obj = GetObjectFromName(list, ccon, FindType.Obj);

                        if (_obj == null && DefaultFuncContainer != null)
                        {
                            _obj = GetObjectFromName(list, DefaultFuncContainer, FindType.Obj);
                        }


                        if (objcon != null && var.blocktype == "selftype")
                        {
                            _obj = objcon;
                        }
                    }
                    else
                    {
                        //그 외
                        _obj = GetObjectFromName(var.values, ccon, FindType.Obj);
                        if (_obj == null && DefaultFuncContainer != null)
                        {
                            _obj = GetObjectFromName(var.values, DefaultFuncContainer, FindType.Obj);
                        }
                        if (_obj == null)
                        {
                            //함수 일 가능성이 있음
                            _obj = GetObjectFromName(var.values, ccon, FindType.Func);
                            if (_obj == null && DefaultFuncContainer != null)
                            {
                                _obj = GetObjectFromName(var.values, DefaultFuncContainer, FindType.Func);
                            }
                            if (_obj != null)
                            {
                                Function funcobject = (Function)_obj;
                                List<string> list = new List<string>();

                                foreach (var item in funcobject.returntype)
                                {
                                    list.Add(item.Value);
                                }
                                //리턴값을 읽기

                                _obj = GetObjectFromName(list, ccon, FindType.Obj);
                            }
                        }
                    }


                    if (_obj != null)
                    {
                        Container varobject = (Container)_obj;
                        ccon = varobject;
                        lscope = "st";//스코프 초기화
                        index++;
                        if (index == objectname.Count)
                        {
                            if (findType == FindType.AutoComplete)
                            {
                                //마지막 부분이므로 해당 콘테이너의 내용을 모두 넣는다.
                                ccon.GetAllItems(data, ccon.GetInitObjectNameSpacee());
                            }
                            else if (findType == FindType.All)
                            {
                                return _obj;
                            }


                            break;
                        }
                    }
                    else
                    {
                        //아니면 아래에 있는거 사용하기
                        if (index + 1 == objectname.Count)
                        {
                            if (findType == FindType.AutoComplete)
                            {
                                //마지막 부분이므로 해당 콘테이너의 내용을 모두 넣는다.
                                data.Add(new CodeCompletionData(new ObjectItem(CompletionWordType.Function, "getValueAddr")));
                            }
                            else if (findType == FindType.All)
                            {
                                return var;
                            }

                            break;
                        }
                        else
                        {
                            return null;
                        }
                    }



                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        public override string GetTooiTipText(TOKEN token)
        {
            switch (token.Type)
            {
                case TOKEN_TYPE.Identifier:
                    break;
                default:
                    return "";
            }


            List<TOKEN> t = tokenAnalyzer.GetTokenListFromTarget(token, true);

            string rstr = "";
            foreach (var item in t)
            {
                if (rstr != "")
                {
                    rstr += ".";
                }

                rstr += item.Value;
            }

            object o = GetObjectFromName(t, maincontainer, FindType.All, null, token.scope);

            if (o != null)
            {
                switch (o.GetType().Name)
                {
                    case "Block":
                        Block var = (Block)o;
                        rstr = var.blockdefine + " " + rstr;
                        if (!string.IsNullOrEmpty(var.blocktype))
                        {
                            rstr += ":" + var.blocktype;
                        }

                        if (var.values != null && var.values.Count != 0)
                        {
                            string v = "";
                            foreach (var item in var.values)
                            {
                                if (v != "")
                                {
                                    v += ".";
                                }

                                v += item.Value;
                            }

                            rstr += " = " + v;
                        }
                        if (var.IsArg)
                        {
                            rstr = "(매게변수) " + rstr;
                        }

                        break;
                    case "Container":
                        Container obj = (Container)o;
                        if (rstr == obj.mainname)
                        {
                            rstr = "object " + rstr;
                        }
                        else
                        {
                            rstr = rstr + ":" + obj.mainname;
                        }
                        break;
                    case "EpScriptFunction":
                        Function func = (Function)o;
                        rstr = GetFuncToolTip(t).Trim();
                        break;
                    case "ImportedNameSpace":
                        ImportedNameSpace importedNameSpace = (ImportedNameSpace)o;
                        rstr = "import " + importedNameSpace.mainname + " as " + importedNameSpace.shortname;
                        break;
                }
            }

            //Block var = null;
            //Container obj = null;
            //Function func = null;
            //ImportedNameSpace importedNameSpace = null;
            return rstr;
        }

        public override void SetCommentLine(int start, int end, CommentType commentType)
        {
            throw new NotImplementedException();
        }
    }
}
