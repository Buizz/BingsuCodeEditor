using BingsuCodeEditor.AutoCompleteToken;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BingsuCodeEditor.CodeAnalyzer;

namespace BingsuCodeEditor
{
    public class ErrorToken
    {
        public ErrorToken(string Message, int start, int end)
        {
            this.Message = Message;
            this.Start = start;
            this.End = end;
        }
        public string Message;
        public int Start;
        public int End;
        public int Line = -1;
        public int Column = -1;
    }
    public abstract class TokenAnalyzer
    {
        private List<TOKEN> tklist;
        protected int index;



        public List<ErrorToken> ErrorList = new List<ErrorToken>();
        public List<ErrorToken> TempErrorList = new List<ErrorToken>();
        public bool IsError
        {
            get
            {
                return ErrorList.Count > 0;
            }
            set
            {
                if(value == false)
                {
                    TempErrorList.Clear();
                }
            }
        }

        //public string ErrorMessage;
        //public int ErrorIndex;


        //오류가 나왔을 경우 반환해야됨


        public void Init(List<TOKEN> tklist)
        {
            IsError = false;
            this.tklist = tklist;
            index = 0;
        }


        public void Complete(TextEditor textEditor)
        {
            textEditor.Dispatcher.Invoke(new Action(() =>
            {
                for (int i = 0; i < TempErrorList.Count; i++)
                {
                    DocumentLine line;
                    try
                    {
                        line = textEditor.Document.GetLineByOffset((int)TempErrorList[i].Start);
                    }
                    catch (System.ArgumentOutOfRangeException)
                    {
                        break;
                    }

                    TempErrorList[i].Line = line.LineNumber;
                    TempErrorList[i].Column = TempErrorList[i].Start - line.Offset;
                }
         

                ErrorList.Clear();
                ErrorList.AddRange(TempErrorList);
            }), System.Windows.Threading.DispatcherPriority.Normal);
        }



        /// <summary>
        /// 현재 토큰을 가져옵니다. null이 나올 수도 있습니다.
        /// </summary>
        /// <returns></returns>
        public TOKEN GetCurrentToken()
        {
            //if (IsError)
            //{
            //    throw new Exception();
            //}

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
            //if (IsError)
            //{
            //    throw new Exception();
            //}

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
                    //ThrowException("인덱스가 토근의 최대 크기를 넘겼습니다.");
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }


        public void ThrowException(string message, TOKEN tk)
        {
            //에러가 났을 경우
            IsError = true;
            if(tk is null)
            {
                TempErrorList.Add(new ErrorToken(message, 0, 0));
            }
            else
            {
                TempErrorList.Add(new ErrorToken(message, tk.StartOffset, tk.EndOffset));
            }


            //List<TOKEN> tklist = GetTokenList();
            //if (tklist.Count < index)
            //{
            //    //ErrorList.Add(new ErrorToken(message, 0, 0));
            //}
            //else
            //{
            //    TOKEN tk = tklist[index];

            //}




            //각종 줄 정보를 남긴다..
            return;
            //throw new Exception(message);
        }


        public abstract Container ConatainerAnalyzer(int startindex = int.MaxValue);
        public abstract Block BlockAnalyzer(string type);
        public abstract Function FunctionAnalyzer();
    }
}
