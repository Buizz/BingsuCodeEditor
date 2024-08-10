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

        private List<string> specialKeyword = new List<string>();

        public EpScriptTokenAnalyzer(CodeAnalyzer codeAnalyzer) : base(codeAnalyzer)
        {
            string[] sp = {
                "Enemy", "Ally", "AlliedVictory",
"AtLeast", "AtMost", "Exactly",
"All",
"SetTo", "Add", "Subtract",
"Move", "Patrol", "Attack",
"P1", "P2", "P3", "P4", "P5", "P6", "P7", "P8", "P9", "P10", "P11", "P12", "CurrentPlayer", "Foes", "Allies", "NeutralPlayers", "AllPlayers", "Force1", "Force2", "Force3", "Force4", "NonAlliedVictoryPlayers",
"UnitProperty",
"Enable", "Disable", "Toggle",
"Ore", "Gas", "OreAndGas",
"Total", "Units", "Buildings", "UnitsAndBuildings", "Kills", "Razings", "KillsAndRazings", "Custom",
"Set", "Clear", "Toggle", "Random",
"Set", "Cleared", "None"};
            specialKeyword.AddRange(sp);
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
                                int foreachstart = tk.EndOffset;
                                forblocks.Clear();
                                forstart = true;
                                Block forvar = null;
                                while (true)
                                {
                                    tk = GetCurrentToken();

                                    if(tk == null)
                                    {
                                        ThrowException("foreach 변수 자리에는 식별자를 선언해야 합니다.", tk);
                                        break;
                                    }
                                    if(tk.Type != TOKEN_TYPE.Identifier)
                                    {
                                        ThrowException("foreach 변수 자리에는 식별자를 선언해야 합니다.", tk);

                                        break;
                                    }


                                    forvar = new Block("const", tk.Value);

                                    forvar.scope = scope;
                                    forblocks.Add(forvar);

                                    cc.vars.Add(forvar);
                                    if (!CheckCurrentToken(TOKEN_TYPE.Symbol, ","))
                                    {
                                        break;
                                    }
                                }
                                if (foreachstart <= startindex && startindex <= tk.EndOffset)
                                {
                                    cc.cursorLocation = CursorLocation.ForEachDefine;
                                }

                                tk = GetSafeTokenIten();
                                if (tk == null) break;
                                if (tk.Type == TOKEN_TYPE.Symbol && tk.Value == ":")
                                {
                                    if (tk.StartOffset <= startindex && startindex <= tk.EndOffset + 2)
                                    {
                                        cc.cursorLocation = CursorLocation.ForFuncDefine;
                                    }
                                    tk = GetCurrentToken();
                                    tk = GetCurrentToken();

                                    if(forvar != null)
                                    {
                                        forvar.values = IdentifierFAnalyzer(cc, scope, tk, startindex, main: rcontainer);
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

                                if(importstart < startindex && startindex < tk.StartOffset)
                                {
                                    //현재 공백으로 입력중
                                    rcontainer.cursorLocation = CursorLocation.ImportFile;
                                    break;
                                }


                                List<TOKEN> t = GetTokenListFromTarget(tk, saveIndex:false, IsNamespace: true, addSperator:true);
                                
                                if(t.Count == 0)
                                {
                                    ThrowException("import문이 정상적으로 종료되지 않았습니다.", tk);
                                    break;
                                }
                                int importend = 0;
                                if (t.Last().Value == ".")
                                {
                                    importend = 1;
                                    t.RemoveAt(t.Count - 1);
                                }

                                importend += t.Last().EndOffset;

                        


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
                                    if (!CheckCurrentToken(TOKEN_TYPE.Symbol, ";") || tk == null)
                                    {
                                        ThrowException("import문이 정상적으로 종료되지 않았습니다.", tk);
                                    }
                                    else
                                    {
                                        nspace = tk.Value;
                                        cc.importedNameSpaces.Add(new ImportedNameSpace(filename, nspace));
                                    }
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

                                if (cc.CheckIdentifier(scope, function.funcname, funcdefine:true))
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
                                }
                                cc.funcs.Add(function);
                                //if (!function.IsInCursor)
                                //{
                                //}
                                //매게변수 추가
                                foreach (var item in function.args)
                                {
                                    Block bl = new Block("var", item.argname, item.argtype, IsArg: true);
                                    bl.scope = scope + "." + (currentscope + 1).ToString().PadLeft(4, '0');
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
                                if(tk == null)
                                {
                                    cc.cursorLocation = CursorLocation.ObjectDefine;
                                    continue;
                                }
                                tk.scope = scope;
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

                                obj = new Container(codeAnalyzer);
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

                                scope += "." + (currentscope).ToString().PadLeft(4, '0');


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

            int tindex = tokenindex;

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
                if (tk == null) return blocks;
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
                int equalstarttokenindex = tokenindex;
                while (true)
                {
                    int starttokenindex = tokenindex;
                    tk = GetCurrentToken();
                    varvalue = IdentifierFAnalyzer(container, scope, tk, startindex, main: main);

                    int endtokenindex = tokenindex;


                    if (blocks.Count <= index)
                    {
                        if(blocks.Count == 1)
                        {
                            //튜플형
                            blocks[0].rawtext = GetTextFromTokenToEndLine(equalstarttokenindex, ";");
                            break;
                        }
                        else
                        {
                            ThrowException("대입 식의 수가 맞지 않습니다.", tk);
                            break;
                        }
                    }
                    else
                    {
                        if(varvalue.Count >= 1 && varvalue[0].Value == "EUDArray")
                        {
                            blocks[index].rawtext = GetTextFromTokenToEndLine(equalstarttokenindex, ";");
                        }
                        else
                        {
                            blocks[index].rawtext = GetTextFromToken(starttokenindex, endtokenindex);
                        }

                        blocks[index++].values = varvalue;
                    }

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
            List<TOKEN> tlist = new List<TOKEN>();

            if (ctk == null) return tlist;

            string fname = ctk.Value;
            ctk.scope = scope;

            int argstartindex = ctk.StartOffset;


            if (ctk.Type != TOKEN_TYPE.Identifier)
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

            if(specialKeyword.IndexOf(fname) == -1)
            {
                if (fname != "this" && !container.CheckIdentifier(scope, fname))
                {
                    if (!(fname.Length != 0 && fname[0] == '@'))
                    {
                        ThrowException(fname + "는 선언되지 않았습니다.", ctk);
                    }
                }
            }

            TOKEN tk = null;//GetCurrentToken();

            if (CheckCurrentToken(TOKEN_TYPE.Symbol, "."))
            {
                //.이므로 이어지는 토큰
                tlist.AddRange(GetTokenList());
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
                    //tlist.Add(tk);
                    switch (tk.Type)
                    {
                        case TOKEN_TYPE.Identifier:
                            //tlist.AddRange(IdentifierFAnalyzer(container, scope, tk, startindex, cargindex, main: main));
                            IdentifierFAnalyzer(container, scope, tk, startindex, cargindex, main: main);
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
            Function function = new EpScriptFunction();

            TOKEN commenttoken = GetCommentTokenIten(-2);

            if(commenttoken != null)
            {

                if (commenttoken.Type == TOKEN_TYPE.KeyWord && commenttoken.Value == "static")
                {
                    function.IsStatic = true;
                }else if (commenttoken.Type == TOKEN_TYPE.Comment)
                {
                    string[] lines = commenttoken.Value.Replace("\r", "").Split('\n');

                    string tabstr = "";
                    string result = "";
                    int index = 0;
                    foreach (var item in lines)
                    {
                        string ritem = "";
                        ritem = item;
                        if (item.IndexOf("/***") != -1 && index == 0)
                        {
                            //시작 부분 찾았음
                            index = 1;
                        }

                        int s = item.IndexOf(" * ");
                        if (s >= 0 && index > 0)
                        {
                            if (s != 0)
                            {
                                //다음 부분
                                if (tabstr == "")
                                {
                                    tabstr = item.Substring(0, s);
                                }
                                if (tabstr == item.Substring(0, tabstr.Length))
                                {
                                    //텝 부분이 똑같아야 됨
                                    ritem = item.Substring(tabstr.Length);
                                }
                                else
                                {
                                    break;
                                }
                            }

                            index = +1;
                        }

                        if (item.IndexOf("***/") != -1 && index > 0)
                        {
                            //마지막 부분
                            if (tabstr == item.Substring(0, tabstr.Length))
                            {
                                //텝 부분이 똑같아야 됨
                                ritem = item.Substring(tabstr.Length);
                            }
                            index = +1;
                        }

                        result += ritem + "\n";
                    }



                    function.comment = result;
                    function.ReadComment("ko-KR");
                }
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
                    arg.argtype = tk.Value.Replace("/", "").Replace("*", "");
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
                    int typestartindex = tk.StartOffset;
                    int typeendindex = tk.EndOffset;

                    if (tk.Type == TOKEN_TYPE.Identifier)
                    {
                        //일반 타입
                        arg.argtype = tk.Value;
                    }
                    else
                    {
                        if (typestartindex <= startindex && startindex <= typeendindex)
                        {
                            cl = CursorLocation.FunctionArgType;
                        }
                        argendoffset = tk.EndOffset;
                        ThrowException("인자 타입을 선언해야 합니다.", tk);
                        goto EndLabel;
                    }

                    if (typestartindex <= startindex && startindex <= typeendindex)
                    {
                        cl = CursorLocation.FunctionArgType;
                    }

                    tk = GetCurrentToken();

                    TOKEN nt = GetSafeTokenIten();
                    if(nt.Type == TOKEN_TYPE.Symbol && nt.Value == "(")
                    {
                        //타입이 함수일 경우
                        int bracecount = 1;

                        GetCurrentToken();
                        while (true)
                        {
                            nt = GetCurrentToken();

                            if (IsEndOfList())
                            {
                                ThrowException("괄호가 닫히지 않았습니다.", nt);
                                goto EndLabel;
                            }

                            if (nt.Type == TOKEN_TYPE.Symbol && nt.Value == "(")
                            {
                                bracecount ++;
                            }
                            else if (nt.Type == TOKEN_TYPE.Symbol && nt.Value == ")")
                            {
                                bracecount--;
                            }
                            if(bracecount == 0)
                            {
                                break;
                            }
                        }


                    }


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
                    List<TOKEN> tlist = GetTokenList();

                    function.returntype = tlist;

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

                if (tk == null)
                {
                    ThrowException("함수의 선언이 잘못되었습니다.", tk);
                    goto EndLabel;
                }
                tk.scope = scope;
                while (true && tk != null)
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