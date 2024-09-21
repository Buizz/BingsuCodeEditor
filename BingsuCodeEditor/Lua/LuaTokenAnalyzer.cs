using BingsuCodeEditor.AutoCompleteToken;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BingsuCodeEditor.CodeAnalyzer;

namespace BingsuCodeEditor.Lua
{
    public class LuaTokenAnalyzer : TokenAnalyzer
    {
        public LuaTokenAnalyzer(CodeAnalyzer codeAnalyzer) : base(codeAnalyzer)
        {
        }

        public override Container ConatainerAnalyzer(int startindex = int.MaxValue)
        {
            IsError = false;

            Container rcontainer = new Container(codeAnalyzer);
            Container cc = rcontainer;


            bool isinstartoffset = true;

            int currentscope = 0;
            string scope = "st";
            string lastblockscope = "st";
            

            TOKEN tk = null;

            while (!IsEndOfList())
            {
                tk = GetCurrentToken();
                if(tk == null)
                {
                    break;
                }
                tk.scope = scope;
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
                            case "function":

                                string lastscope = scope;

                                currentscope++;
                                scope += "." + (currentscope).ToString().PadLeft(4, '0');


                                Function function = FunctionAnalyzer(rcontainer, startindex, scope);

                                //if(rcontainer.funcs.Find(x=> ((x.funcname == function.funcname) && (x.IsPredefine == false))) != null)
                                //{
                                //    ThrowException("함수 " + function.funcname + "는 중복 선언되었습니다.", tk, 1);
                                //}

                                if (cc.CheckIdentifier(lastscope, function.funcname))
                                {
                                    ThrowException("함수 " + function.funcname + "는 이미 선언되어 있습니다.", tk, 1);
                                }

                                function.scope = lastscope;


                   
                                if (function.cursorLocation != CursorLocation.None)
                                {
                                    cc.cursorLocation = function.cursorLocation;
                                    rcontainer.cursorLocation = function.cursorLocation;
                                }

                                cc.funcs.Add(function);
                                //매게변수 추가
                                foreach (var item in function.args)
                                {
                                    Block bl = new Block(rcontainer, "var", item.argname, tk, item.argtype, IsArg: true);
                                    bl.Scope = scope;
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
                            case "if":
                            case "while":
                            case "repeat":
                            case "for":
                                //새 스코프를 정의
                                currentscope++;
                                scope += "." + (currentscope).ToString().PadLeft(4, '0');

                                break;
                            case "end":
                            case "until":
                                //이전 스코프로 되돌림
                                int t = currentscope.ToString().Length + 1;

                         
                                if (scope == "st")
                                {
                                    //닫을 수 없는데 닫음
                                    ThrowException("'end'등 스코프가 마무리되지 않았습니다.", tk);
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
                ThrowException("'end'등 스코프가 마무리되지 않았습니다.", tk);
            }


            return rcontainer;
        }



        public List<TOKEN> IdentifierFAnalyzer(Container container, string scope, TOKEN ctk, int startindex, int argindex = -1, Container main = null)
        {
            string fname = ctk.Value;
            ctk.scope = scope;

            int argstartindex = ctk.StartOffset;
            List<TOKEN> tlist = new List<TOKEN>();

       


            tlist.Add(ctk);
            if (argindex != -1)
            {
                //상속받았을 경우
                ctk.argindex = argindex;
            }

            if(!container.CheckIdentifier(scope, fname))
            {
                Block block = new Block(container, "var", fname, ctk);
                block.Scope = scope;
                container.vars.Add(block);
                //ThrowException(fname + "는 선언되지 않았습니다.", ctk);
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
                            IdentifierFAnalyzer(container, scope, tk, startindex, cargindex, main:main);
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




        public Function FunctionAnalyzer(Container container, int startindex, string scope)
        {
            Function function = new LuaFunction(container, null);

            TOKEN commenttoken = GetCommentTokenIten(-2);

            if(commenttoken != null)
            {
                function.comment = commenttoken.Value;
            }

            TOKEN tk = GetCurrentToken();
            if (tk == null) return function;
            function.StartToken = tk;

            tk.scope = scope;
            CursorLocation cl = CursorLocation.None;
            int argstartoffset = tk.EndOffset;
            int argendoffset = 0;
            string funcname = "";
            int findex = CurrentInedx;


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

             
                tk = GetCurrentToken();
                

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
                


                function.args.Add(arg);
            }

            findex = CurrentInedx;

      
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