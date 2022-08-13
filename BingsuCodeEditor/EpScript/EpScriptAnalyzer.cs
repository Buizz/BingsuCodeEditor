﻿using BingsuCodeEditor.AutoCompleteToken;
using BingsuCodeEditor.Lua;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingsuCodeEditor.EpScript
{
    partial class EpScriptAnalyzer : CodeAnalyzer
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

        public override ImportManager ChildImportManager
        {
            get
            {
                return EpScriptAnalyzer.importManager;
            }
        }

        public override void SetImportManager(ImportManager importManager)
        {
            EpScriptAnalyzer.importManager = importManager;

            if (!importManager.IsCachedContainer(DEFAULTFUNCFILENAME))
            {
                Container c = this.GetContainer(importManager.GetFIleContent(DEFAULTFUNCFILENAME));
                importManager.UpdateContainer(DEFAULTFUNCFILENAME, c);
            }
            if (_DefaultFuncContainer == null)
            {
                _DefaultFuncContainer = importManager.GetContainer(DEFAULTFUNCFILENAME);

                foreach (var item in _DefaultFuncContainer.funcs)
                {
                    if (!string.IsNullOrEmpty(item.comment)) item.ReadComment("ko-KR");
                }
            }

        }


        public EpScriptAnalyzer(TextEditor textEditor) : base(textEditor, false)
        {
            string[] keywords = {"object", "static", "once", "if", "else", "while", "for", "function", "foreach",
        "return", "switch", "case", "break", "var", "const", "import", "as", "continue" , "true", "True", "false", "False"};


            //[tab]for(var [i] = 0; [i] < [Length] ; [i]++)\n[tab]{\n[tab][tabonce][Content]\n[tab]}
            Template.Add("if", " ([true]) {\n[tab][tabonce][Content]\n[tab]}");
            Template.Add("while", " ([true]) {\n[tab][tabonce][Content]\n[tab]}");
            Template.Add("switch", " ([Var]) {\n[tab][tabonce][Content]\n[tab]}");
            Template.Add("for", " (var [i] = [0]; [i] < [Length]; [i]++) {\n[tab][tabonce][Content]\n[tab]}");
            Template.Add("foreach", " ([Var] : [Func]) {\n[tab][tabonce][Content]\n[tab]}");
            Template.Add("function", " [FuncName]([Arg]) {\n[tab][tabonce][Content]\n[tab]}");
            Template.Add("object", " [objname]{\n[tab][tabonce][Content]\n[tab]};");
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


            FuncPreChar.Add("$");
            FuncPreChar.Add("@");


            tokenAnalyzer = new EpScriptTokenAnalyzer(this);
            secondtokenAnalyzer = new EpScriptTokenAnalyzer(this);
            codeFoldingManager = new EpScriptFoldingManager(textEditor);


            luaAnalyzer = new LuaAnalyzer(textEditor);
            luaTokenAnalyzer = new LuaTokenAnalyzer(luaAnalyzer);
        }

        public LuaAnalyzer luaAnalyzer;
        public LuaTokenAnalyzer luaTokenAnalyzer;


        public override void AutoInsert(string text)
        {
            if(textEditor.SelectionLength == 0)
            {
                //if(text == "*")
                //{
                //    TOKEN tk = GetToken(0);

                //    if(tk.Value == "/")
                //    {
                //        textEditor.SelectedText = "*/";
                //        textEditor.SelectionLength = 0;
                //    }
                //}
            }
            else
            {
                
                //""로 감싸기 등
                //int len = textEditor.SelectionLength;

                //switch (text)
                //{
                //    case "\"":
                //        textEditor.SelectionLength = 0;
                //        textEditor.SelectedText = "\"";
                //        textEditor.SelectionStart += len + 1;
                //        textEditor.SelectedText = "\"";
                //        textEditor.SelectionStart -= len + 2;
                //        textEditor.SelectionLength = len + 2;
                //        break;
                //}


            }
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
            else if (t == '/')
            {
                if (index + 1 >= tlen)
                {
                    return null;
                }
                char nt = text[index + 1];

                if (nt == '/')
                {
                    //LineCommnet 개행 문자까지 반복 (\r)
                    index++;
                    if (index >= tlen)
                    {
                        return null;
                    }
                    t = text[index];
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
                else if (nt == '*')
                {
                    //MulitComment */가 나올 떄 까지 반복
                    char lastchar = ' ';
                    t = text[++index];
                    do
                    {
                        block += t.ToString();
                        index++;
                        if (index >= tlen)
                        {
                            //사실상 오류임..
                            break;
                        }
                        if(lastchar == '*' && t == '/')
                        {
                            //주석
                            break;
                        }


                        lastchar = t;
                        t = text[index];
                    } while (index < tlen);
                    index--;


                    TOKEN_TYPE type = TOKEN_TYPE.Comment;

                    //block = block.Replace("\r", "");

                    TOKEN token = new TOKEN(sindex, type, block, caretoffset);
                    outindex = index;
                    return token;
                }
            }
            else if (t == '<')
            {
                if (index + 1 >= tlen)
                {
                    return null;
                }
                char nt = text[index + 1];

                if (nt == '?')
                {
                    //MulitComment */가 나올 떄 까지 반복
                    char lastchar = ' ';
                    index++;
                    if (index>= tlen)
                    {
                        return null;
                    }
                    t = text[index];
                    do
                    {
                        block += t.ToString();
                        index++;
                        if (index >= tlen)
                        {
                            //사실상 오류임..
                            break;
                        }
                        if (lastchar == '?' && t == '>')
                        {
                            //주석
                            break;
                        }


                        lastchar = t;
                        t = text[index];
                    } while (index < tlen);
                    index--;


                    TOKEN_TYPE type = TOKEN_TYPE.Special;

                    //block = block.Replace("\r", "");

                    TOKEN token = new TOKEN(sindex, type, block, caretoffset);
                    outindex = index;
                    return token;
                }
            }

            return null;
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



                if (scope.IndexOf("st.O") != -1)
                {
                    List<string> tname = new List<string>();
                    tname.Add(scope.Split('.')[1].Substring(1));
                    object _obj = GetObjectFromName(tname, ccon, FindType.Obj);

                    if (_obj != null)
                    {
                        objcon = (Container)_obj;
                    }

                    if (objname == "this")
                    {
                        //this가 나오면 un같은거는 안된다.

                        lscope = objcon.GetInitObjectNameSpacee();
                        index++;
                        if (index == objectname.Count)
                        {
                            if (findType == FindType.AutoComplete)
                            {
                                //마지막 부분이므로 해당 콘테이너의 내용을 모두 넣는다.
                                if (objcon != null) objcon.GetAllItems(data, objcon.GetInitObjectNameSpacee(), noargFlag:true);
                            }
                            break;
                        }
                        continue;
                    }
                }





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

                if(EpScriptAnalyzer.DefaultFuncContainer != null)
                {
                    if (var == null)
                    {
                        var = EpScriptAnalyzer.DefaultFuncContainer.vars.Find(x => (x.blockname == objname && lscope.Contains(x.scope)));
                    }
                    if (obj == null)
                    {
                        obj = EpScriptAnalyzer.DefaultFuncContainer.objs.Find(x => (x.mainname == objname));
                    }
                    if (func == null)
                    {
                        func = EpScriptAnalyzer.DefaultFuncContainer.funcs.Find(x => (x.funcname == objname && lscope.Contains(x.scope)));
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
                            }else if (findType == FindType.All)
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
                        string rtype = func.returntype;

                        if(rtype == null) return null;

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
                            switch(objectname[index + 1])
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
                                    }else if (findType == FindType.Func)
                                    {
                                        if(last == "alloc")
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

                        if(_obj == null && EpScriptAnalyzer.DefaultFuncContainer != null)
                        {
                            _obj = GetObjectFromName(list, EpScriptAnalyzer.DefaultFuncContainer, FindType.Obj);
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
                        if (_obj == null && EpScriptAnalyzer.DefaultFuncContainer != null)
                        {
                            _obj = GetObjectFromName(var.values, EpScriptAnalyzer.DefaultFuncContainer, FindType.Obj);
                        }
                        if (_obj == null)
                        {
                            //함수 일 가능성이 있음
                            _obj = GetObjectFromName(var.values, ccon, FindType.Func);
                            if (_obj == null && EpScriptAnalyzer.DefaultFuncContainer != null)
                            {
                                _obj = GetObjectFromName(var.values, EpScriptAnalyzer.DefaultFuncContainer, FindType.Func);
                            }
                            if (_obj != null)
                            {
                                Function funcobject = (Function)_obj;
                                List<string> list = new List<string>();
                                list.Add(funcobject.returntype);
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
                            }else if (findType == FindType.All)
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




        /// <summary>
        /// 이름으로 부터 파일을 가져옴
        /// </summary>
        public override object GetObjectFromName(List<TOKEN> tokenlist, Container startcontainer, FindType findType, IList<ICompletionData> data = null,  string scope = "st")
        {
            if (tokenlist == null) return null;

            List<string> strs = new List<string>();

            foreach (var item in tokenlist)
            {
                strs.Add(item.Value);
            }

            return GetObjectFromName(strs, startcontainer, findType, data, scope);
        }

        public override bool GetCompletionList(IList<ICompletionData> data, bool IsNameSpaceOpen = false)
        {
            string scope = maincontainer.currentScope;
            
            TOKEN _t = GetToken(0);
            if(_t.Type == TOKEN_TYPE.Special)
            {
                //lua함수

                luaAnalyzer.Apply(_t.Value, 0);

                luaAnalyzer.GetCompletionList(data, IsNameSpaceOpen);

                return true;
            }


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


                            if(fname == "")
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
                if(ctkn == null) return true;

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
                if(cursorLocation == CursorLocation.ImportFile && importManager != null)
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
                    if(cursorLocation == CursorLocation.FunctionArgType)
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
                    if(func.args.Count <= maincontainer.innerFuncInfor.argindex)
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
            if(objcontainer != null)
            {
                data.Add(new CodeCompletionData(new ObjectItem(CompletionWordType.Const, "this")));
                objcontainer.GetAllItems(data, scope);
            }
            DefaultFuncContainer.GetAllItems(data, "st");



            return true;
        }



        //Analyzer오류 분석

        public override void TokenAnalyze(int caretoffset = int.MaxValue)
        {
            //TODO:토큰 분석 로직
            //tokens에 직접 접근하여 분석한다.

            //네임스페이스를 분석 후 토큰을 추가한다
            //GetTokens(Context, -1) 이런식으로 가져와서 분석한다.

            //최근 네임스페이스를 저장하고 해당 파일들이 변형되었는지 체크한다.

            //ResetCompletionData(CompletionWordType.Function);


            //Action
            //Condiction
            //Function
            //일반적인 함수들

            //nameSpace
            //Const
            //Variable
            //오브젝트들

            //Setting(Property)

            //ArgType
            //KeyWord
            //Special


            //함수와 오브젝트의 요소들을 저장해야함.


            //cursorLocation 현재 위치를 적습니다.
            CursorLocation cl = CursorLocation.None;


            //TOKEN clefttoken = GetToken(0, TOKEN.Side.Left);

            TOKEN ctoken = GetToken(0);
            TOKEN btoken = GetToken(-1);
            TOKEN bbtoken = GetToken(-2);

            if(ctoken != null)
            {
                if(ctoken.Type == TOKEN_TYPE.Symbol && ctoken.Value == ";")
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
                if(btoken.Type == TOKEN_TYPE.KeyWord)
                {
                    switch(btoken.Value)
                    {
                        case "var":
                        case "const":
                        case "as":
                            cl = CursorLocation.VarName;
                            break;
                        case "function":
                            cl = CursorLocation.FunctionName;
                            break;
                            break;
                        case "object":
                            cl = CursorLocation.ObjectDefine;
                            break;
                    }
                }
            }
            

            if(cl == CursorLocation.None)
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
                if(!(errortoken is null))
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
                if(rstr != "")
                {
                    rstr += ".";
                }

                rstr += item.Value;
            }

            object o = GetObjectFromName(t, maincontainer, FindType.All, null, token.scope);

            if(o != null)
            {
                switch (o.GetType().Name)
                {
                    case "Block":
                        Block var = (Block) o;
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
                        if(rstr == obj.mainname)
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
    }
}
