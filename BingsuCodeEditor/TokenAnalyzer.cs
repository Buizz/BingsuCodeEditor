﻿using BingsuCodeEditor.AutoCompleteToken;
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
        protected CodeAnalyzer codeAnalyzer;
        public TokenAnalyzer(CodeAnalyzer codeAnalyzer)
        {
            this.codeAnalyzer = codeAnalyzer;
        }


        protected List<TOKEN> tklist;

        protected int index;
        public int CurrentInedx
        {
            get { return index; }
            set { index = value; }
        }


        public TOKEN GetLastToken
        {
            get
            {
                if(tklist.Count <= index)
                {
                    return null;
                }

                return GetSafeTokenIten();
            }
        }


        public TOKEN GetSafeTokenIten(bool GoToReverse = false, int _i = 0)
        {
            PassComment(GoToReverse);

            if(index < 0)
            {
                return null;
            }


            if (tklist.Count > index + _i)
            {
                return tklist[index + _i];
            }

            if (tklist.Count > index)
            {
                return tklist[index];
            }

            return null;
        }



        public TOKEN GetCommentTokenIten(int tindex = 0)
        {
            int i = index + tindex;
            if (0 <= i && i < tklist.Count)
                return tklist[index + tindex];

            return null;
        }



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
        /// 현재 토큰을 가져옵니다. null이 나올 수도 있습니다. 현재 토근을 가져온 뒤에 다음 토큰을 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public TOKEN GetCurrentToken(bool GoToReverse = false)
        {
            if (IsEndOfList())
            {
                return null;
            }

            TOKEN rtk = GetSafeTokenIten(GoToReverse);

            if (GoToReverse){index--;}else{index++;}

            return rtk;
        }

        /// <summary>
        /// 토큰 네임스페이스를 가져옵니다.
        /// a.b.c.d등 .과 키로 이루어진 리스트입니다.
        /// </summary>
        /// <returns></returns>
        public List<TOKEN> GetTokenList(bool IsReverse = false, bool Iscontaindot = false)
        {
            List<TOKEN> rlist = new List<TOKEN>();
            //토큰 네임스페이스를 가져옵니다.
            //a.b.c.d등 .과 키로 이루어진 리스트입니다.

            IsEndOfList(true);
               
            while (true)
            {

                TOKEN tk = GetSafeTokenIten(IsReverse);
                if (CheckCurrentToken(TOKEN_TYPE.Identifier))
                {
                    rlist.Add(tk);
                    tk = GetSafeTokenIten(IsReverse);
                }

                if (IsEndOfList(true))
                {
                    break;
                }
                if (tk.Type == TOKEN_TYPE.Number)
                {
                    rlist.Add(tk);
                    return rlist;
                }
                if (Iscontaindot)
                {
                    if (!CheckCurrentToken(TOKEN_TYPE.Symbol, ".", addedlist:rlist))
                    {
                        break;
                    }
                }
                else
                {
                    if (!CheckCurrentToken(TOKEN_TYPE.Symbol, "."))
                    {
                        break;
                    }
                }
            
            }
     
            return rlist;
        }


        /// <summary>
        /// 토큰 네임스페이스를 가져옵니다.
        /// a.b.c.d등 .과 키로 이루어진 리스트입니다.
        /// </summary>
        /// <returns></returns>
        public List<TOKEN> GetTokenListFromTarget(TOKEN target, bool IsReverse = false, bool saveIndex = true, bool IsNamespace = false)
        {
            List<TOKEN> rlist = new List<TOKEN>();
            //토큰 네임스페이스를 가져옵니다.
            //a.b.c.d등 .과 키로 이루어진 리스트입니다.

            int savedindex = index;


            index = tklist.IndexOf(target);

            CheckCurrentToken(TOKEN_TYPE.Symbol, ".", IsReverse: IsReverse);

            while (true)
            {
                if (index >= tklist.Count || index < 0)
                {
                    break;
                }

                IsEndOfList(true);
                if (index < 0)
                {
                    break;
                }
                TOKEN tk = GetCurrentToken(IsReverse);

                if (IsNamespace && tk.Type == TOKEN_TYPE.Symbol && tk.Value == ".")
                {
                    rlist.Add(tk);
                    tk = GetCurrentToken(IsReverse);
                }


                if (IsReverse)
                {
                    if (tk.Type == TOKEN_TYPE.Symbol && tk.Value == ")")
                    {
                        int bracecount = 1;
                        while (true)
                        {
                            if (index >= tklist.Count || index < 0)
                            {
                                break;
                            }
                            IsEndOfList(true);

                            tk = GetCurrentToken(IsReverse);

                            if (tk.Type == TOKEN_TYPE.Symbol)
                            {
                                switch (tk.Value)
                                {
                                    case ")":
                                        bracecount++;
                                        break;
                                    case "(":
                                        bracecount--;
                                        break;
                                }
                            }
                            if (bracecount == 0)
                            {
                                tk = GetCurrentToken(IsReverse);
                                break;
                            }
                        }
                    }
                }




                if (tk.Type != TOKEN_TYPE.Identifier)
                {
                    break;
                }

                rlist.Add(tk);


                IsEndOfList(true);
                if (index < 0)
                {
                    break;
                }

                if (!CheckCurrentToken(TOKEN_TYPE.Symbol, ".", IsReverse: IsReverse))
                {
                    break;
                }
            }
            if (IsReverse)
            {
                rlist.Reverse();
            }

            if(saveIndex) index = savedindex;

            return rlist;
        }


        /// <summary>
        /// 다음 토큰의 타입과 내용을 확인합니다.
        /// 만약 옳은 토큰일 경우 다음 인덱스로 진행합니다.
        /// </summary>
        /// <returns></returns>
        public bool CheckCurrentToken(TOKEN_TYPE ttype, string value = null, bool IsReverse = false, bool IsDirect = false, List<TOKEN> addedlist = null)
        {
            //if (IsError)
            //{
            //    throw new Exception();
            //}

            IsEndOfList(true);


            TOKEN ntk = GetSafeTokenIten();
            if (ntk == null) return false;
            if (IsDirect)
            {
                TOKEN ltk = GetSafeTokenIten(_i: 1);
                if (ltk == null) return false;

                if (ntk.EndOffset != ltk.StartOffset)
                {
                    return false;
                }
            }



            if (ttype != ntk.Type)
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

            if (IsReverse)
            {
                index--;
            }
            else
            {
                index++;
            }
            if(addedlist != null)
            {
                addedlist.Add(ntk);
            }
            return true;
        }
        public bool IsEndOfList(bool IsExist = false)
        {
            if (index >= tklist.Count)
            {
                if (IsExist)
                {
                    ThrowException("인덱스가 토근의 최대 크기를 넘겼습니다.", null);
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public int PassComment(bool GoToReverse = false)
        {
            if (IsEndOfList(false))
            {
                return index;
            }

            if (index >= tklist.Count || index < 0)
            {
                return index;
            }

            while (tklist[index].Type == TOKEN_TYPE.Comment || tklist[index].Type == TOKEN_TYPE.LineComment)
            {
                if (GoToReverse)
                {
                    index--;
                }
                else
                {
                    index++;
                }
                if (IsEndOfList(false))
                {
                    return index;
                }
            }
            return index;
        }



        public void ThrowException(string message, TOKEN tk, int len = 0)
        {
            //에러가 났을 경우
            IsError = true;

            if(tk == null)
            {
                ErrorToken errorToken = new ErrorToken(message, 0, 0);
                TempErrorList.Add(errorToken);
            }
            else
            {
                int index = tklist.IndexOf(tk);

                TOKEN starttarget = tklist[index];
                TOKEN endtarget = tklist[index + len];

                ErrorToken errorToken = new ErrorToken(message, starttarget.StartOffset, endtarget.EndOffset);
                starttarget.errorToken = errorToken;
                endtarget.errorToken = errorToken;
                TempErrorList.Add(errorToken);

                //for (int i = 0; i <= len; i++)
                //{
                //    if(tklist.Count > i + index)
                //    {
                //        TOKEN target = tklist[i + index];

                //        ErrorToken errorToken = new ErrorToken(message, target.StartOffset, target.EndOffset);
                //        target.errorToken = errorToken;
                //        TempErrorList.Add(errorToken);
                //    }                    
                //}
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
    }
}
