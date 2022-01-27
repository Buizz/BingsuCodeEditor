using BingsuCodeEditor.AutoCompleteToken;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BingsuCodeEditor.CodeAnalyzer;

namespace BingsuCodeEditor
{
    public abstract class TokenAnalyzer
    {
        private List<TOKEN> tklist;
        protected int index;

        public bool IsError = false;
        public string ErrorMessage;
        public int ErrorIndex;


        //오류가 나왔을 경우 반환해야됨


        public void Init(List<TOKEN> tklist)
        {
            IsError = false;
            this.tklist = tklist;
            index = 0;
        }


        //public void Complete()
        //{
        //    IsError = false;
        //}



        /// <summary>
        /// 현재 토큰을 가져옵니다. null이 나올 수도 있습니다.
        /// </summary>
        /// <returns></returns>
        public TOKEN GetCurrentToken()
        {
            if (IsError)
            {
                throw new Exception();
            }

            IsEndOfList(true);

            return tklist[index++];
        }

        /// <summary>
        /// 토큰 네임스페이스를 가져옵니다.
        /// a.b.c.d등 .과 키로 이루어진 리스트입니다.
        /// </summary>
        /// <returns></returns>
        public List<TOKEN> GetTokenList()
        {
            List<TOKEN> rlist = new List<TOKEN>();
            //토큰 네임스페이스를 가져옵니다.
            //a.b.c.d등 .과 키로 이루어진 리스트입니다.

            IsEndOfList(true);

      

            while (true)
            {
                TOKEN tk = GetCurrentToken();
                if (tk.Type == TOKEN_TYPE.Identifier)
                {
                    rlist.Add(tk);
                }
                if (tk.Type == TOKEN_TYPE.Number)
                {
                    rlist.Add(tk);
                    return rlist;
                }


                if (!CheckCurrentToken(TOKEN_TYPE.Symbol, "."))
                {
                    break;
                }
            }
     




            return rlist;
        }


        /// <summary>
        /// 다음 토큰의 타입과 내용을 확인합니다.
        /// 만약 옳은 토큰일 경우 다음 인덱스로 진행합니다.
        /// </summary>
        /// <returns></returns>
        public bool CheckCurrentToken(TOKEN_TYPE ttype, string value = null)
        {
            if (IsError)
            {
                throw new Exception();
            }

            IsEndOfList(true);

            TOKEN ntk = tklist[index];
            if(ttype != ntk.Type)
            {
                return false;
            }

            if(value != null)
            {
                if (value != ntk.Value)
                {
                    return false;
                }
            }

            index++;
            return true;
        }
        public bool IsEndOfList(bool IsExist = false)
        {
            if (index >= tklist.Count)
            {
                if (IsExist)
                {
                    ErrorMessage = "인덱스가 토근의 최대 크기를 넘겼습니다.";
                    ErrorIndex = -1;

                    IsError = true;
                    throw new Exception();
                }
                return true;
            }
            else
            {
                return false;
            }
        }




        public abstract Container ConatainerAnalyzer(int startindex = int.MaxValue);
        public abstract Block BlockAnalyzer(string type);
        public abstract Function FunctionAnalyzer();
    }
}
