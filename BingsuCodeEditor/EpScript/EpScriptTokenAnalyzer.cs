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
        public override Container ConatainerAnalyzer(int startindex = int.MaxValue)
        {
            IsError = false;

            Container rcontainer = new Container();


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
                                Block b = BlockAnalyzer(tk.Value);

                                b.scope = scope;

                                if (forstart)
                                {
                                    forblocks.Add(b);
                                }

                                //범위를 벗어날 경우 오브젝트에 넣지 않는다.
                                if (isinstartoffset)
                                {
                                    rcontainer.vars.Add(b);
                                }
                                break;
                            case "for":
                                forblocks.Clear();
                                forstart = true;

                                break;
                            case "import":
                                //import File as t;
                                //import File;

                                tk = GetCurrentToken();
                                string filename = tk.Value;
                                string nspace;

                                if (CheckCurrentToken(TOKEN_TYPE.Symbol, ";"))
                                {
                                    //특별 지정자 없이 임포트
                                    rcontainer.importedNameSpace.Add(new ImportedNameSpace(filename, ""));
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
                                    rcontainer.importedNameSpace.Add(new ImportedNameSpace(filename, nspace));
                                }
                                else
                                {
                                    //잘못된 지정자
                                    ThrowException("import문이 정상적으로 종료되지 않았습니다.", tk);
                                }

                                //임포트 매니저에게 파일 이름 확인 시키기.
                                //본인의 이름과 파일 이름을 넘겨야함.
                                break;
                            case "function":
                                Function function = FunctionAnalyzer(startindex);

                                function.scope = scope;


                                //범위를 벗어날 경우 오브젝트에 넣지 않는다.
                                if (isinstartoffset)
                                {
                                    if(function.cursorLocation != CursorLocation.None)
                                    {
                                        rcontainer.cursorLocation = function.cursorLocation;
                                    }
                                    rcontainer.funcs.Add(function);
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
                                //object linkedList{
                                //    var front;
                                //    var rear;
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
                                if(scope == "st")
                                {
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

                    default:
                        if (tk.Type == TOKEN_TYPE.Identifier)
                        {
                            //키워드일 경우
                            //네임스페이스인지 아닌지 확인

                        } 

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



        public Block BlockAnalyzer(string type)
        {
            int tindex = index;

            //var vname = 값;
            //var front: linkedUnit;
            //const vname = 값;

            string varconst = type;

            TOKEN tk = GetCurrentToken();
            string varname = tk.Value;
            string vartype = "";
            List<TOKEN> varvalue = null;

            if (CheckCurrentToken(TOKEN_TYPE.Symbol, ":"))
            {
                //타입과 같이 선언한 경우
                tk = GetCurrentToken();
                vartype = tk.Value;
            }

            if (CheckCurrentToken(TOKEN_TYPE.Symbol, "="))
            {
                varvalue = GetTokenList();
            }
            else
            {
                if (varconst == "const")
                {
                    //const일 경우는 선언만 있으면 오류 출력
                    ThrowException("const는 선언 후 대입해줘야 합니다.", tk);
                }
            }

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

            return new Block(varconst, varname, vartype, varvalue);
        }


        public Function FunctionAnalyzer(int startindex)
        {
            Function function = new Function();
            TOKEN tk = GetCurrentToken();

            

            if (tk.Type != TOKEN_TYPE.Identifier)
            {
                ThrowException("함수의 이름에는 식별자가 와야 합니다.", tk);
            }
            string funcname = tk.Value;
            function.funcname = funcname;


            CursorLocation cl = CursorLocation.None;

            int argstartoffset = tk.EndOffset;
            int argendoffset = 0;

            if (!CheckCurrentToken(TOKEN_TYPE.Symbol, "("))
            {
                ThrowException("함수의 이름 다음에는 인자선언이 와야 합니다.", tk);
            }


            string errormsg = "";
            bool isException = false;

            while (true)
            {
                Function.Arg arg = new Function.Arg();

                tk = GetCurrentToken();

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
                            isException = true;
                            errormsg = "잘못된 인자 선언입니다. )가 와야합니다.";
                            break;
                        }
                        if (tk.Value == ")")
                        {
                            argendoffset = tk.EndOffset;
                            break;
                        }
                        //인자가 없을 수 있음.
                    }

                    argendoffset = tk.EndOffset;
                    isException = true;
                    errormsg = "잘못된 인자 선언입니다. 인자 이름이 와야 합니다.";
                    break;
                }

                recheck:

                tk = GetCurrentToken();
                if(tk.Type != TOKEN_TYPE.Symbol)
                {
                    //무조건 심불이 와야됨
                    argendoffset = tk.EndOffset;
                    isException = true;
                    errormsg = "잘못된 인자 선언입니다. ) , :가 와야합니다.";
                    break;
                }

                if(tk.Value == ")")
                {
                    argendoffset = tk.EndOffset;
                    break;
                }
                else if (tk.Value == ",")
                {

                }
                else if (tk.Value == ":")
                {
                    tk = GetCommentTokenIten();
                    if(tk.Type == TOKEN_TYPE.Identifier)
                    {
                        //일반 타입
                        arg.argtype = tk.Value;
                    }
                    else if (tk.Type == TOKEN_TYPE.Comment)
                    {
                        //특수처리된 타입
                        arg.argtype = tk.Value;
                    }
                    else
                    {
                        if (tk.StartOffset <= startindex && startindex <= tk.EndOffset)
                        {
                            cl = CursorLocation.FunctionArgType;
                        }
                        argendoffset = tk.EndOffset;
                        isException = true;
                        errormsg = "인자 타입을 선언해야 합니다.";
                        break;
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

            if (cl == CursorLocation.None)
            {
                if (argstartoffset <= startindex && startindex <= argendoffset)
                {
                    cl = CursorLocation.FunctionArgName;
                }
            }

            function.cursorLocation = cl;

            if (isException)
            {
                ThrowException(errormsg, tk);
            }

            function.preCompletion = new ObjectItem(CompletionWordType.Function, funcname);

            return function;
        }

    }
}