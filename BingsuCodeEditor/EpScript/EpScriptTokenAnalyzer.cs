using BingsuCodeEditor.AutoCompleteToken;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BingsuCodeEditor.CodeAnalyzer;

namespace BingsuCodeEditor.EpScript
{
    public class EpScriptTokenAnalyzer : TokenAnalyzer
    {
        public EpScriptTokenAnalyzer(CodeAnalyzer codeAnalyzer) : base(codeAnalyzer)
        {
        }

        public override Container ConatainerAnalyzer(int startindex = int.MaxValue)
        {
            IsError = false;

            Container rcontainer = new Container(codeAnalyzer);
            Container obj = null;
            Container cc = rcontainer;


            bool isinstartoffset = true;

            int currentscope = 0;
            bool forstart = false;
            string scope = "st";
            string lastblockscope = "st";
            

            List<Block> forblocks = new List<Block>();



            TOKEN tk = null;

            while (!IsEndOfList())
            {
                tk = GetCurrentToken();
                tk.scope = scope;
                if(tk == null)
                {
                    break;
                }
                if (tk.StartOffset > startindex && isinstartoffset)
                {
                    isinstartoffset = false;
                    lastblockscope = scope;
                    //break;
                }

                //함수 안쪽에 있으면 함수

                //object에서 두번 일하기 싫으면 구조 잘 파악하기.

                switch (tk.Type)
                {
                    case TOKEN_TYPE.KeyWord:
                        switch (tk.Value)
                        {
                            case "const":
                            case "var":
                                List<Block> b = BlockAnalyzer(cc, scope, tk, startindex, main: rcontainer);


                                foreach (var item in b)
                                {
                                    item.scope = scope;


                                    if (cc.CheckIdentifier(scope, item.blockname))
                                    {
                                        ThrowException("변수 " + item.blockname + "는 이미 선언되어 있습니다.", tk, 1);
                                    }


                                    if (forstart)
                                    {
                                        forblocks.Add(item);
                                    }

                                    //자동완성에만 영향을 줘야됨..
                                    //범위를 벗어날 경우 오브젝트에 넣지 않는다.
                                    //if (isinstartoffset)
                                    //{
                                    //    cc.vars.Add(item);
                                    //}
                                    cc.vars.Add(item);
                                }
                                break;
                            case "foreach":
                                if (!CheckCurrentToken(TOKEN_TYPE.Symbol, "("))
                                {
                                    ThrowException("foreach문이 잘못되었습니다. (가 와야합니다.", tk);
                                }
                                forblocks.Clear();
                                forstart = true;
                                while (true)
                                {
                                    tk = GetCurrentToken();

                                    if(tk.Type != TOKEN_TYPE.Identifier)
                                    {
                                        ThrowException("foreach 변수 자리에는 식별자를 선언해야 합니다.", tk);

                                        break;
                                    }
                                    Block item = new Block("const", tk.Value);

                                    forblocks.Add(item);

                                    cc.vars.Add(item);
                                    if (!CheckCurrentToken(TOKEN_TYPE.Symbol, ","))
                                    {
                                        break;
                                    }
                                }

                                break;
                            case "for":
                                forblocks.Clear();
                                forstart = true;

                                break;
                            case "import":
                                //import File as t;
                                //import File;
                                int importstart = tk.EndOffset;

                                tk = GetCurrentToken();
                                List<TOKEN> t = GetTokenListFromTarget(tk, saveIndex:false, IsNamespace: true);

                                
                                int importend = t.Last().EndOffset + 1;

                        


                                string filename = "";
                                //tk = GetCurrentToken();
                                foreach (var item in t)
                                {
                                    if(filename != "")
                                    {
                                        filename += ".";
                                    }
                                    filename += item.Value;
                                }


                                string nspace;

                                if (CheckCurrentToken(TOKEN_TYPE.Symbol, ";"))
                                {
                                    //특별 지정자 없이 임포트
                                    cc.importedNameSpaces.Add(new ImportedNameSpace(filename, ""));
                                }
                                else if(CheckCurrentToken(TOKEN_TYPE.KeyWord, "as"))
                                {
                                    //특별 지정자
                                    tk = GetCurrentToken();
                                    nspace = tk.Value;
                                    if (!CheckCurrentToken(TOKEN_TYPE.Symbol, ";"))
                                    {
                                        ThrowException("import문이 정상적으로 종료되지 않았습니다.", tk);
                                    }
                                    cc.importedNameSpaces.Add(new ImportedNameSpace(filename, nspace));
                                }
                                else
                                {
                                    //잘못된 지정자
                                    ThrowException("import문이 정상적으로 종료되지 않았습니다.", tk);
                                }
                                if (importstart <= startindex && startindex <= importend)
                                {
                                    rcontainer.cursorLocation = CursorLocation.ImportFile;
                                }
                                //임포트 매니저에게 파일 이름 확인 시키기.
                                //본인의 이름과 파일 이름을 넘겨야함.
                                break;
                            case "function":
                                Function function = FunctionAnalyzer(startindex, scope);

                                //if(rcontainer.funcs.Find(x=> ((x.funcname == function.funcname) && (x.IsPredefine == false))) != null)
                                //{
                                //    ThrowException("함수 " + function.funcname + "는 중복 선언되었습니다.", tk, 1);
                                //}

                                if (cc.CheckIdentifier(scope, function.funcname))
                                {
                                    ThrowException("함수 " + function.funcname + "는 이미 선언되어 있습니다.", tk, 1);
                                }

                                function.scope = scope;


                                //범위를 벗어날 경우 오브젝트에 넣지 않는다.
                                if (isinstartoffset)
                                {
                                    if(function.cursorLocation != CursorLocation.None)
                                    {
                                        cc.cursorLocation = function.cursorLocation;
                                        rcontainer.cursorLocation = function.cursorLocation;
                                    }

                                    if (!function.IsInCursor)
                                    {
                                        cc.funcs.Add(function);
                                    }
                                }
                                //매게변수 추가
                                foreach (var item in function.args)
                                {
                                    Block bl = new Block("var", item.argname, item.argtype, IsArg: true);
                                    bl.scope = scope + "." + (currentscope + 1);
                                    cc.vars.Add(bl);
                                }

                                //function fname(args){}
                                ///******/function fname(args){}
                                /***
                                 * @Type
                                 * F
                                 * @Summary.ko-KR
                                 * [loc]에 존재하는 [player]의 [unit]을 반환합니다.
                                 *
                                 * @param.player.ko-KR
                                 * 유닛의 소유 플레이어입니다.
                                 *
                                 * @param.unit.ko-KR
                                 * 유닛입니다.
                                 *
                                 * @param.loc.ko-KR
                                 * 로케이션입니다.
                                ***/
                                break;
                            case "object":
                                tk = GetCurrentToken();
                                tk.scope = scope;
                                if(tk == null)
                                {
                                    cc.cursorLocation = CursorLocation.ObjectDefine;
                                    continue;
                                }
                                if (tk.StartOffset <= startindex && startindex <= tk.EndOffset)
                                {
                                    cc.cursorLocation = CursorLocation.ObjectDefine;
                                }
                                if (tk.Type != TOKEN_TYPE.Identifier)
                                {
                                    ThrowException("Object의 이름에는 식별자가 와야 합니다.", tk);
                                    continue;
                                }
                                

                                string objname = tk.Value;


                                if (cc.CheckIdentifier(scope, objname))
                                {
                                    ThrowException("Object " + objname + "는 이미 선언되어 있습니다.", tk, 1);
                                    continue;
                                }

                                if (!CheckCurrentToken(TOKEN_TYPE.Symbol, "{"))
                                {
                                    ThrowException("Object는 '{'로 시작해야합니다.", tk);
                                    continue;
                                }
                                else
                                {
                                    scope += "." + "O" + objname;
                                }

                                obj = new Container();
                                obj.mainname = objname;
                                obj.IsObject = true;
                                obj.cursorLocation = cc.cursorLocation;

                                cc = obj;
                                //object linkedList{
                                //    var front : linkedUnit;
                                //    var rear : linkedUnit;
                                //    var size;
                                //    function append(un : linkedUnit){
                                //        this.size++;
                                //        if(!this.front){
                                //            this.front=un;
                                //            this.rear=un;
                                //        }
                                //        else{
                                //            this.rear.next=un;
                                //            this.rear=un;
                                //        }
                                //    }
                                //};

                                break;
                        }

                        break;
                    case TOKEN_TYPE.Symbol:
                        //심불 {} ;
                        //스코프를 정의,
                        switch (tk.Value)
                        {
                            case "{":
                                //새 스코프를 정의
                                currentscope++;
                                scope += "." + currentscope;


                                if (forstart)
                                {
                                    foreach (var item in forblocks)
                                    {
                                        item.scope = scope;
                                    }

                                    forstart = false;
                                    //포문 끝
                                }
                                break;
                            case "}":
                                //이전 스코프로 되돌림
                                int t = currentscope.ToString().Length + 1;

                                if (obj != null)
                                {
                                    if (scope == "st.O" + obj.mainname)
                                    {
                                        if (!CheckCurrentToken(TOKEN_TYPE.Symbol, ";"))
                                        {
                                            ThrowException("Object는  ;로 끝나야 합니다.", tk);
                                        }
                                        cc = rcontainer;

                                        cc.objs.Add(obj);
                                        obj = null;
                                    }
                                }
                                if (scope == "st")
                                {
                                    //닫을 수 없는데 닫음
                                    ThrowException("'{}'가 제대로 닫히지 않았습니다.", tk);
                                }
                                else
                                {
                                    int lastlen = scope.Split('.').Last().Length + 1;

                                    scope = scope.Remove(scope.Length - lastlen, lastlen);
                                    //currentscope = int.Parse(scope.Split('.').Last());
                                }
   

                                break;
                 
                        }

                        break;
                    case TOKEN_TYPE.Identifier:
                        //키워드일 경우
                        //네임스페이스인지 아닌지 확인

                        //함수 분석 여기서하기.
                        //각 TOKEN에다가 현재 토큰이 어디에 속하는지 체크하기.
                        //,는 반갈라서 앞쪽인지 뒤쪽인지 확인.
                        IdentifierFAnalyzer(cc, scope, tk, startindex, main: rcontainer);
                        break;
                    default:


                        break;
                }
            }


            //스코프 정리
            rcontainer.currentScope = lastblockscope;
            
            if (scope != "st")
            {
                ThrowException("'{}'가 제대로 닫히지 않았습니다.", tk);
            }


            return rcontainer;
        }



        public List<Block> BlockAnalyzer(Container container, string scope, TOKEN ctk, int startindex, Container main = null)
        {
            List<Block> blocks = new List<Block>();

            int tindex = index;

            //var vname = 값;
            //var front: linkedUnit;
            //const vname = 값;
            string type = ctk.Value;
            string varconst = type;

            TOKEN tk;

            List<TOKEN> varvalue = null;
            while (true)
            {
                tk = GetCurrentToken();
                if (tk.Type != TOKEN_TYPE.Identifier)
                {
                    ThrowException("변수 선언은 식별자가 와야합니다.", tk);
                }

                string varname = tk.Value;
                string vartype = "";
                tk.scope = scope;

                if (CheckCurrentToken(TOKEN_TYPE.Symbol, ":"))
                {
                    //타입과 같이 선언한 경우
                    tk = GetCurrentToken();
                    vartype = tk.Value;
                }

                blocks.Add(new Block(varconst, varname, vartype, varvalue));
                if (CheckCurrentToken(TOKEN_TYPE.Symbol, ","))
                {
                    //다중 선언일 경우
                }
                else
                {
                    //아닐 경우
                    break;
                }
            }
      




            if (CheckCurrentToken(TOKEN_TYPE.Symbol, "="))
            {
                int index = 0;
                while (true)
                {
                    tk = GetCurrentToken();
                    varvalue = IdentifierFAnalyzer(container, scope, tk, startindex, main: main);
                    if (blocks.Count <= index)
                    {
                        ThrowException("대입 식의 수가 맞지 않습니다.", tk);
                        break;
                    }
                    blocks[index++].values = varvalue;

                    if(!CheckCurrentToken(TOKEN_TYPE.Symbol, ","))
                    {
                        break;
                    }
                }
               

            }
            else
            {
                if (varconst == "const")
                {
                    //const일 경우는 선언만 있으면 오류 출력
                    ThrowException("const는 선언 후 대입해줘야 합니다.", tk);
                }
            }


            //new Block(varconst, varname, vartype, varvalue);
            //const t = func();
            //const t = func1() + func2();
            //const t = (func1() + func2());

            //선언한 경우
            //while (!CheckCurrentToken(TOKEN_TYPE.Symbol, ";"))
            //{
            //    //문장의 끝이 아닌 동안 진행
            //    tk = GetCurrentToken();

            //}
            //문장의 끝

            return blocks;
        }

        public List<TOKEN> IdentifierFAnalyzer(Container container, string scope, TOKEN ctk, int startindex, int argindex = -1, Container main = null)
        {
            string fname = ctk.Value;
            ctk.scope = scope;

            int argstartindex = ctk.StartOffset;
            List<TOKEN> tlist = new List<TOKEN>();

            if(ctk.Type != TOKEN_TYPE.Identifier)
            {
                if (ctk.Type == TOKEN_TYPE.Symbol && ctk.Value == "[")
                {
                    tlist.Add(new TOKEN(0, TOKEN_TYPE.Identifier, "EUDArray", 0));
                }
                else if (ctk.Type == TOKEN_TYPE.Number)
                {
                    tlist.Add(new TOKEN(0, TOKEN_TYPE.Number, ctk.Value, 0));
                }
                return tlist;
            }
       


            tlist.Add(ctk);
            if (argindex != -1)
            {
                //상속받았을 경우
                ctk.argindex = argindex;
            }

            if(fname != "this" && !container.CheckIdentifier(scope, fname))
            {
                ThrowException(fname + "는 선언되지 않았습니다.", ctk);
            }

            TOKEN tk = null;//GetCurrentToken();

            if (CheckCurrentToken(TOKEN_TYPE.Symbol, "."))
            {
                //.이므로 이어지는 토큰
                tlist.AddRange(GetTokenList());
                //tk = GetCurrentToken();
            }

            CheckFunc:
            //선언된 함수의 시작
            if (CheckCurrentToken(TOKEN_TYPE.Symbol, "("))
            {
                int innercount = 1;

                int cargindex = 0;
             
                //innercount가 0이 될때까지 진행
                while (true)
                {
                    if (IsEndOfList())
                    {
                        if(tk != null)
                        {
                            ThrowException(fname + " 괄호가 정상적으로 닫히지 않았습니다.", tk);
                        }
                        break;
                    }
                    tk = GetCurrentToken();
                    tk.scope = scope;
                    tk.argindex = cargindex;
                    tk.funcname = tlist;

                    switch (tk.Type)
                    {
                        case TOKEN_TYPE.Identifier:
                            IdentifierFAnalyzer(container, scope, ctk, startindex, cargindex, main:main);
                            break;
                        case TOKEN_TYPE.Symbol:
                            //, ( ) 등이 있을 수 있다.
                            if(tk.Value == ")")
                            {
                                innercount--;
                            }
                            else if (tk.Value == ",")
                            {
                                if (!main.innerFuncInfor.IsInnerFuncinfor &&
                                    argstartindex <= startindex &&  startindex <= tk.StartOffset)
                                {
                                    //내부함수가 이미 결정되어 있지 않을 경우
                                    main.innerFuncInfor.IsInnerFuncinfor = true;
                                    main.innerFuncInfor.argindex = cargindex;
                                    main.innerFuncInfor.funcename = tlist;
                                }
                                argstartindex = tk.StartOffset;
                                cargindex++;
                            }
                            else if (tk.Value == "(")
                            {
                                innercount++;
                            }
                            break;
                    }
                    if (innercount == 0)
                    {
                        if (!main.innerFuncInfor.IsInnerFuncinfor &&
                            argstartindex <= startindex && startindex <= tk.EndOffset)
                        {
                            //내부함수가 이미 결정되어 있지 않을 경우
                            main.innerFuncInfor.IsInnerFuncinfor = true;
                            main.innerFuncInfor.argindex = cargindex;
                            main.innerFuncInfor.funcename = tlist;
                        }
                        argstartindex = tk.StartOffset;
                        break;
                    }
                }
            }

            if (CheckCurrentToken(TOKEN_TYPE.Symbol, ".", IsDirect:true))
            {
                //함수 다음에 이어지는 것이므로 순환루트로다시 이동한다.
                tlist.AddRange(GetTokenList());
                goto CheckFunc;
            }
            //이 외에는 아웃.

            return tlist;
            //단일 토큰
        }




        public Function FunctionAnalyzer(int startindex, string scope)
        {
            Function function = new LuaFunction();

            TOKEN commenttoken = GetCommentTokenIten(-2);

            if(commenttoken != null)
            {
                function.comment = commenttoken.Value;
            }

            TOKEN tk = GetCurrentToken();
            if (tk == null) return function;
            tk.scope = scope;
            CursorLocation cl = CursorLocation.None;
            int argstartoffset = tk.EndOffset;
            int argendoffset = 0;
            string funcname = "";
            int findex = CurrentInedx;

            if (tk.Type == TOKEN_TYPE.Symbol && tk.Value == "$") //특수함수
            {
                funcname += "$";
                tk = GetCurrentToken();
            }

            if (tk.Type != TOKEN_TYPE.Identifier)
            {
            

                ThrowException("함수의 이름에는 식별자가 와야 합니다.", tk);
                goto EndLabel;
            }

            funcname += tk.Value;
            function.funcname = funcname;



            if (!CheckCurrentToken(TOKEN_TYPE.Symbol, "("))
            {
                ThrowException("함수의 이름 다음에는 인자선언이 와야 합니다.", tk);
                goto EndLabel;
            }





            while (true)
            {
                Function.Arg arg = new Function.Arg();


                if (CheckCurrentToken(TOKEN_TYPE.Symbol, "*"))
                {
                    //인자형
                    arg.IsList = true;
                }
                tk = GetCurrentToken();
                tk.scope = scope;


                if(tk.Type == TOKEN_TYPE.Identifier)
                {
                    string argname = tk.Value;

                    arg.argname = argname;

                }
                else
                {
                    if(function.args.Count == 0)
                    {
                        if (tk.Type != TOKEN_TYPE.Symbol)
                        {
                            //무조건 심불이 와야됨
                            argendoffset = tk.EndOffset;
                            ThrowException("잘못된 인자 선언입니다. )가 와야합니다.", tk);
                            goto EndLabel;
                        }
                        if (tk.Value == ")")
                        {
                            argendoffset = tk.EndOffset;
                            break;
                        }
                        //인자가 없을 수 있음.
                    }

                    argendoffset = tk.EndOffset;
                    ThrowException("잘못된 인자 선언입니다. 인자 이름이 와야 합니다.", tk);
                    goto EndLabel;
                }

                recheck:

                tk = GetCommentTokenIten();
                if(tk.Type != TOKEN_TYPE.Symbol && tk.Type != TOKEN_TYPE.Comment)
                {
                    //무조건 심불이 와야됨
                    argendoffset = tk.EndOffset;
                    ThrowException("잘못된 인자 선언입니다. ) , :가 와야합니다.", tk);
                    goto EndLabel;
                }
                else if (tk.Type == TOKEN_TYPE.Comment)
                {
                    //특수처리된 타입
                    arg.argtype = tk.Value;
                    tk = GetCurrentToken();
                }
                else
                {
                    tk = GetCurrentToken();
                }


                if (tk.Value == ")")
                {
                    argendoffset = tk.EndOffset;
                    function.args.Add(arg);
                    break;
                }
                else if (tk.Value == ",")
                {

                }
                else if (tk.Value == "=")
                {
                    tk = GetCurrentToken();
                    arg.InitValue = tk.Value;
                }
                else if (tk.Value == ":")
                {
                    tk = GetCommentTokenIten();
                    if(tk.Type == TOKEN_TYPE.Identifier)
                    {
                        //일반 타입
                        arg.argtype = tk.Value;
                    }
                    else
                    {
                        if (tk.StartOffset <= startindex && startindex <= tk.EndOffset)
                        {
                            cl = CursorLocation.FunctionArgType;
                        }
                        argendoffset = tk.EndOffset;
                        ThrowException("인자 타입을 선언해야 합니다.", tk);
                        goto EndLabel;
                    }

                    if (tk.StartOffset <= startindex && startindex <= tk.EndOffset)
                    {
                        cl = CursorLocation.FunctionArgType;
                    }

                    GetCurrentToken();
                    goto recheck;
                }


                function.args.Add(arg);
            }

            findex = CurrentInedx;

            if (!IsEndOfList())
            {
                int brackcount = 0;
                int funcstartoffset = 0;
                int funcendoffset = 0;


                tk = GetCurrentToken();
                tk.scope = scope;


                if (tk.Type == TOKEN_TYPE.Symbol && tk.Value == ":")
                {
                    if (IsEndOfList())
                    {
                        ThrowException("반환 타입이 와야 합니다.", tk);
                        goto EndLabel;
                    }
                    tk = GetCurrentToken();

                    function.returntype = tk.Value;

                    if (IsEndOfList())
                    {
                        ThrowException("반환 타입이 와야 합니다.", tk);
                        goto EndLabel;
                    }
                    findex = CurrentInedx;
                    tk = GetCurrentToken();
                }


                if (tk.Type == TOKEN_TYPE.Symbol && tk.Value == ";")
                {
                    //그냥 끝내기
                    function.IsPredefine = true;
                    goto EndLabel;
                }
                else if (tk.Type == TOKEN_TYPE.Symbol && tk.Value == "{")
                {
                    brackcount += 1;
                    funcstartoffset = tk.StartOffset;
                }
                else
                {
                    ThrowException("함수의 선언이 잘못되었습니다.", tk);
                    goto EndLabel;
                }
                if (IsEndOfList())
                {
                    ThrowException("함수의 선언이 잘못되었습니다.", tk);
                    goto EndLabel;
                }
                tk = GetCurrentToken();
                tk.scope = scope;
                while (true)
                {
                    if (tk.Type == TOKEN_TYPE.Symbol && tk.Value == "{")
                    {
                        brackcount += 1;
                    }
                    else if (tk.Type == TOKEN_TYPE.Symbol && tk.Value == "}")
                    {
                        brackcount -= 1;
                    }

                    if(brackcount == 0)
                    {
                        funcendoffset = tk.StartOffset;

                        if (funcstartoffset <= startindex && startindex <= funcendoffset)
                        {
                            function.IsInCursor = true;
                        }
                        break;
                    }

                    if (IsEndOfList())
                    {
                        ThrowException("괄호가 제대로 마무리 되지 않았습니다.", tk);
                        goto EndLabel;
                    }
                    tk = GetCurrentToken();
                }
            }

            EndLabel:


            if (cl == CursorLocation.None)
            {
                if (argstartoffset <= startindex && startindex <= argendoffset)
                {
                    cl = CursorLocation.FunctionArgName;
                }
            }

            function.cursorLocation = cl;




            function.preCompletion = new ObjectItem(CompletionWordType.Function, funcname, function: function);

            CurrentInedx = findex;

            return function;
        }

    
    }
}