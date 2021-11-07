﻿using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingsuCodeEditor.EpScript
{
    class EpScriptAnalyzer : CodeAnalyzer
    {
        public EpScriptAnalyzer(TextEditor textEditor) : base(textEditor, false)
        {
            string[] keywords = {"object", "static", "once", "if", "else", "while", "for", "function", "foreach",
        "return", "switch", "case", "break", "var", "const", "import", "as", "continue" ,  "true", "True", "false", "False"};


            foreach (var item in keywords)
            {
                //토큰 입력
                AddSubType(item, TOKEN_TYPE.KeyWord);

                //자동완성 초기 입력
                completionDatas.Add(new KewWord(CompletionWordType.KeyWord, item));
            }
            //[tab]for(var [i] = 0; [i] < [Length] ; [i]++)\n[tab]{\n[tab][tabonce][Content]\n[tab]}
            Template.Add("for", " (var [i] = 0; [i] < [Length]; [i]++) {\n[tab][tabonce][Content]\n[tab]}");


            codeFoldingManager = new EpScriptFoldingManager(textEditor);
        }

        
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



        public override TOKEN TokenBlockAnalyzer(string text, int index, out int outindex)
        {
            int sindex = index;
            char t = text[index];
            int tlen = text.Length;
            string block = t.ToString();


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

                TOKEN token = new TOKEN(sindex, type, block);
                outindex = index;
                return token;
            }
            else if (t == '/')
            {
                char nt = text[index + 1];

                if (nt == '/')
                {
                    //LineCommnet 개행 문자까지 반복 (\r)
                    t = text[++index];
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

                    TOKEN token = new TOKEN(sindex, type, block);
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


                    TOKEN_TYPE type = TOKEN_TYPE.LineComment;

                    //block = block.Replace("\r", "");

                    TOKEN token = new TOKEN(sindex, type, block);
                    outindex = index;
                    return token;
                }
            }
            else if (t == '<')
            {
                char nt = text[index + 1];

                if (nt == '?')
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

                    TOKEN token = new TOKEN(sindex, type, block);
                    outindex = index;
                    return token;
                }
            }

            outindex = -1;
            return null;
        }

        public override void GetCompletionList(IList<ICompletionData> data)
        {
            for (int i = 0; i < completionDatas.Count; i++)
            {
                data.Add(new CodeCompletionData(completionDatas[i]));
            }


            //TODO:분석된 토큰으로 자동완성을 만든다.
        }

        public override void TokenAnalyzer()
        {
            //TODO:토큰 분석 로직
            //tokens에 직접 접근하여 분석한다.

            //네임스페이스를 분석 후 토큰을 추가한다
            //GetTokens(Context, -1) 이런식으로 가져와서 분석한다.

            TOKEN ctoken = GetToken(-1);

            for (int i = 0; i < Tokens.Count; i++)
            {

            }
        }



        #region #############자동완성#############

        public class KewWord : PreCompletionData
        {
            public KewWord(CompletionWordType completionType, string name) : base(completionType, name)
            {
            }

            //키워드 이름
            public override string listheader
            {
                get
                {
                    return name;
                }
            }
            public override string ouputstring
            {
                get
                {
                    return name;
                }
            }
            public override string desc
            {
                get
                {
                    return name;
                }
            }
        }
        #endregion
    }
}
