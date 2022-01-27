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
            Container rcontainer = new Container();

            int currentscope = 0;
            bool forstart = false;
            string scope = "start";

            List<Block> forblocks = new List<Block>();


            TOKEN tk;
            while (!IsEndOfList())
            {
                tk = GetCurrentToken();
                if (tk.StartOffset > startindex)
                {
                    break;
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

                                rcontainer.vars.Add(b);
                                break;
                            case "for":
                                forblocks.Clear();
                                forstart = true;

                                break;
                            case "import":
                                //import File as t;
                                //import File;

                                tk = GetCurrentToken();


                                tk = GetCurrentToken();



                                //Container를 분석한다.

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

                            case "function":
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
                                scope = scope.Remove(scope.Length - t, t);
                                break;
                        }

                        break;
                }
            }


            //스코프 정리
            rcontainer.currentScope = scope;



            return rcontainer;
        }



        public override Block BlockAnalyzer(string type)
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
                    throw new Exception();
                }
            }

            //선언한 경우
            while (!CheckCurrentToken(TOKEN_TYPE.Symbol, ";"))
            {
                //문장의 끝이 아닌 동안 진행
                tk = GetCurrentToken();

            }
            //문장의 끝

            return new Block(varconst, varname, vartype, varvalue);
        }



        public override Function FunctionAnalyzer()
        {
            throw new NotImplementedException();
        }
    }
}
