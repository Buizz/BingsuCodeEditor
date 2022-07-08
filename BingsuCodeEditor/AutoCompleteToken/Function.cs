using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingsuCodeEditor.AutoCompleteToken
{
    public struct CalledFunction{
        public List<string> funcname;
        public int argindex;
    }

    public abstract class Function
    {
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
        //원본 내용


        public string scope;
        public CodeAnalyzer.CursorLocation cursorLocation;

        public PreCompletionData preCompletion;

        public bool IsInCursor = false;
        public bool IsPredefine = false;

        //함수 이름
        public string funcname;
        public string funcsummary;

        public string returntype;

        public List<Arg> args = new List<Arg>();
        //Arg
        public class Arg
        {
            public string argname;
            public string argtype;
        }
        protected Dictionary<string, string> argsummary = new Dictionary<string, string>();

        public string GetArgSummary(string arg)
        {
            if (argsummary.ContainsKey(arg))
            {
                return argsummary[arg];
            }
            else
            {
                return "";
            }
        }


        public abstract void ReadComment(string comment);
    }
}
