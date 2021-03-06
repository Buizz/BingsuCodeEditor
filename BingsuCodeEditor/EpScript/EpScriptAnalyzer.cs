using BingsuCodeEditor.AutoCompleteToken;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingsuCodeEditor.EpScript
{
    partial class EpScriptAnalyzer : CodeAnalyzer
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
                completionDatas.Add(new KewWordItem(CompletionWordType.KeyWord, item));
            }
            //[tab]for(var [i] = 0; [i] < [Length] ; [i]++)\n[tab]{\n[tab][tabonce][Content]\n[tab]}
            Template.Add("if", " ([true]) {\n[tab][tabonce][Content]\n[tab]}");
            Template.Add("for", " (var [i] = [0]; [i] < [Length]; [i]++) {\n[tab][tabonce][Content]\n[tab]}");
            Template.Add("function", " [FuncName]([Arg]) {\n[tab][tabonce][Content]\n[tab]}");

            tokenAnalyzer = new EpScriptTokenAnalyzer();
            secondtokenAnalyzer = new EpScriptTokenAnalyzer();
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


                    TOKEN_TYPE type = TOKEN_TYPE.Comment;

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

        public override bool GetCompletionList(IList<ICompletionData> data, bool IsNameSpaceOpen = false, string DataType = "")
        {
            string scope = maincontainer.currentScope;



            //TODO:분석된 토큰으로 자동완성을 만든다.
            if (IsNameSpaceOpen)
            {                
                TOKEN ctkn = GetToken(0);
                if(ctkn == null) return true;

                List<TOKEN> t = tokenAnalyzer.GetTokenListFromTarget(ctkn, true);
                //imported1.var1;
                //imported1.const1.object1;
                //const1.object1;
                //maincontainer.vars[0].

                if(t.Count == 0) return true;
    


                //네임스페이스를 찾아보고 없으면 패스
                if (maincontainer.CheckIdentifier(scope, t[0].Value))
                {
                    return true;
                }



                Container ccon = maincontainer;
                string folderpath = folder;
                int index = 0;
                string lscope = scope;
                while (true)
                {
                    string objname = t[index].Value;
                    Block var = ccon.vars.Find(x => (x.blockname == objname && lscope.Contains(x.scope)));
                    Container obj = ccon.objs.Find(x => (x.mainname == objname));
                    Function func = ccon.funcs.Find(x => (x.funcname == objname && lscope.Contains(x.scope)));
                    ImportedNameSpace importedNameSpace = ccon.importedNameSpaces.Find(x => (x.shortname == objname));



                    if (importedNameSpace != null)
                    {
                        string nsfile = importedNameSpace.mainname;
                        string pullpath = importManager.GetPullPath(nsfile, folderpath);
                        folderpath = importManager.GetFloderPath(nsfile, ccon.folderpath);

                        if (importManager.IsFileExist(pullpath))
                        {
                            if(importManager.IsCachedContainer(pullpath))
                            {
                                //파일이 변형되었을 경우
                                importManager.UpdateContainer(pullpath,GetContainer(importManager.GetFIleContent(pullpath)));
                            }

                            ccon = importManager.GetContainer(pullpath);
                            lscope = "st";//스코프 초기화
                            index++;
                            if (index == t.Count)
                            {
                                //마지막 부분이므로 해당 콘테이너의 내용을 모두 넣는다.


                                break;
                            }
                        }
                        else
                        {
                            return true;
                        }

                    }
                    else if (func != null)
                    {

                    }
                    else if (obj != null)
                    {

                    }
                    else if (var != null)
                    {

                    }
                    else
                    {
                        return true;
                    }


                }



                //foreach (var item in t)
                //{
                //    PreCompletionData preCompletionData = new ImportFileItem(CompletionWordType.nameSpace, item.Value);
                //    data.Add(new CodeCompletionData(preCompletionData));
                //}

                return true;
            }
            switch (cursorLocation)
            {
                case CursorLocation.FunctionArgType:
                case CursorLocation.VarTypeDefine:
                    if(cursorLocation == CursorLocation.FunctionArgType)
                    {
                        data.Add(new CodeCompletionData(new VarType(CompletionWordType.ArgType, "func함수인자")));
                    }
                    data.Add(new CodeCompletionData(new VarType(CompletionWordType.ArgType, "test테스트")));
                    
                    return true;
            }


            if (base.GetCompletionList(data))
            {
                return true;
            }


            foreach (var item in maincontainer.importedNameSpaces)
            {
                if (string.IsNullOrEmpty(item.shortname))
                {
                    continue;
                }
                data.Add(new CodeCompletionData(item.preCompletion));
            }



            foreach (var item in maincontainer.funcs.FindAll(x => scope.Contains(x.scope)))
            {
                data.Add(new CodeCompletionData(item.preCompletion));
            }

            foreach (var item in maincontainer.vars.FindAll(x => scope.Contains(x.scope)))
            {
                data.Add(new CodeCompletionData(item.preCompletion));
            }


            return true;
        }



        //Analyzer오류 분석

        public override void TokenAnalyze(int caretoffset = int.MaxValue)
        {
            //TODO:토큰 분석 로직
            //tokens에 직접 접근하여 분석한다.

            //네임스페이스를 분석 후 토큰을 추가한다
            //GetTokens(Context, -1) 이런식으로 가져와서 분석한다.

            //최근 네임스페이스를 저장하고 해당 파일들이 변형되었는지 체크한다.

            //ResetCompletionData(CompletionWordType.Function);


            //Action
            //Condiction
            //Function
            //일반적인 함수들

            //nameSpace
            //Const
            //Variable
            //오브젝트들

            //Setting(Property)

            //ArgType
            //KeyWord
            //Special


            //함수와 오브젝트의 요소들을 저장해야함.


            //cursorLocation 현재 위치를 적습니다.
            CursorLocation cl = CursorLocation.None;

            TOKEN ctoken = GetToken(0);
            TOKEN btoken = GetToken(-1);
            TOKEN bbtoken = GetToken(-2);

            if(ctoken != null)
            {
                if(ctoken.Type == TOKEN_TYPE.Symbol && ctoken.Value == ";")
                {
                    ctoken = GetToken(-1);
                    btoken = GetToken(-2);
                    bbtoken = GetToken(-3);
                }
            }

            if (btoken != null)
            {
                if(btoken.Type == TOKEN_TYPE.KeyWord)
                {
                    switch(btoken.Value)
                    {
                        case "var":
                        case "const":
                        case "as":
                            cl = CursorLocation.ObjectName;
                            break;
                        case "function":
                            cl = CursorLocation.FunctionName;
                            break;
                        case "import":
                            cl = CursorLocation.ImportFile;
                            break;
                    }
                }
            }


            if(cl == CursorLocation.None)
            {
                if(ctoken != null && ctoken.Type == TOKEN_TYPE.Symbol && ctoken.Value == ":")
                {
                    cl = CursorLocation.VarTypeDefine;
                }
                //if(ctoken != null)
                //{
                //    int argpos;
                //    tokenAnalyzer.GetWritedFunction(ctoken, out argpos);
                //}
            }


            cursorLocation = cl;


            //토근 분석에 사용되는 요소
            tokenAnalyzer.Init(Tokens);

            try
            {
                maincontainer = tokenAnalyzer.ConatainerAnalyzer(caretoffset);
                if(maincontainer.cursorLocation != CursorLocation.None)
                {
                    cursorLocation = maincontainer.cursorLocation;
                }
            }
            catch (Exception e)
            {
                TOKEN errortoken = tokenAnalyzer.GetLastToken;
                if(!(errortoken is null))
                {
                    tokenAnalyzer.ThrowException(e.ToString(), tokenAnalyzer.GetLastToken);
                }
                //tokenAnalyzer.ErrorMessage;

                //return;
            }
            if (tokenAnalyzer.IsError)
            {
                //토큰 분석 오류
            }


            tokenAnalyzer.Complete(textEditor);
        }



    }
}
