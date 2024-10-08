﻿using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BingsuCodeEditor.CodeAnalyzer;

namespace BingsuCodeEditor.AutoCompleteToken
{
    public class Container
    {
        //Object와 NameSpace가 들어온다.

        public string currentScope;

        public CursorLocation cursorLocation;

        public class InnerFuncInfor
        {
            public bool IsInnerFuncinfor = false;
            public List<TOKEN> funcename;
            public int argindex;
        }
        public InnerFuncInfor innerFuncInfor;


        public string pullpath;

        public string folderpath;

        public bool IsObject;
        public string mainname;
        public string shortname;

        public string GetInitObjectNameSpacee()
        {
            return "st.O" + mainname;
        }


        //각각의 콘테이너는 별칭을 가짐.
        //Ex
        //	1. Import main as m;
        //	2. Import main;
        //	3. Object human()
        //1번의 경우 메인 이름 main, 별칭 m
        //2번의 경우 메인 이름 main 별칭 *(별칭이 없다는 의미)
        //3번의 경우 메인 이름 human

        //predefine된 네임스페이스가 들어가야 할거같음...
        public List<ImportedNameSpace> importedNameSpaces;


        public List<Block> vars;
        public List<Container> objs;

        public List<Function> funcs;

        private CodeAnalyzer codeAnalyzer;

        public Container(CodeAnalyzer codeAnalyzer)
        {
            this.codeAnalyzer = codeAnalyzer;

            importedNameSpaces = new List<ImportedNameSpace>();
            innerFuncInfor = new InnerFuncInfor();
            vars = new List<Block>();
            objs = new List<Container>();
            funcs = new List<Function>();
            identifiercache = new Dictionary<string, List<string>>();
        }

        private Dictionary<string, List<string>> identifiercache;
        /// <summary>
        /// 식별자가 정의되어 있으면 true를 반환합니다.
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public bool CheckIdentifier(string scope, string funcname, bool IsExtra = false, bool funcdefine = false)
        {
            //if (identifiercache.ContainsKey(scope))
            //{
            //    if(identifiercache[scope].IndexOf(funcname) != -1)
            //    {
            //        return true;
            //    }
            //}


            Block var;

            Container m = this;
            if (scope.IndexOf("st.O") != -1)
            {
                //string[] scopes = scope.Split('.');
                //string initscope = scopes[0] + "." + scopes[1];

                string objname = scope.Split('.')[1].Substring(1);

                Container _obj = objs.Find(x => (x.mainname == objname));

                if (_obj != null)
                {
                    m = _obj;
                }
            }


            if (funcname == "this")
            {
                return true;
            }
            else
            {
                var = m.vars.Find(x => (x.BlockName == funcname && scope.Contains(x.Scope)));
            }

            Container obj = m.objs.Find(x => (x.mainname == funcname));
            Function func;
            if (funcdefine == true)
            {
                func = m.funcs.Find(x => (x.funcname == funcname && scope.Contains(x.scope) && !x.IsPredefine));
            }
            else
            {
                func = m.funcs.Find(x => (x.funcname == funcname && scope.Contains(x.scope)));
            }


            ImportedNameSpace importedNameSpace = importedNameSpaces.Find(x => (x.shortname == funcname));

     

            if(var == null && obj == null && func == null && importedNameSpace == null)
            {
                if (!IsExtra)
                {
                    if(codeAnalyzer.StaticImportManager != null )
                    {
                        Container importcontainer = codeAnalyzer.GetDefaultContainer;
                        if(importcontainer != null) return importcontainer.CheckIdentifier("st", funcname, true);
                    }
                }
                return false;
            }

            List<string> names;
            if (!identifiercache.ContainsKey(scope))
            {
                names = new List<string>();
                identifiercache.Add(scope, names);
            }
            else
            {
                names = identifiercache[scope];
            }

            //if (identifiercache.ContainsKey("st"))
            //{
            //    if (identifiercache["st"].Count > 10000)
            //    {

            //    }
            //}
            
            if(names.IndexOf(funcname) == -1)
            {
                names.Add(funcname);
            }



            return true;
        }


        public void GetAllItems(IList<ICompletionData> data, string scope, bool noargFlag = false)
        {
            foreach (var item in importedNameSpaces)
            {
                if (string.IsNullOrEmpty(item.shortname))
                {
                    continue;
                }
                data.Add(new CodeCompletionData(item.preCompletion));
            }

            foreach (var item in funcs.FindAll(x => scope.Contains(x.scope)))
            {
                if (!item.IsInCursor)
                {
                    data.Add(new CodeCompletionData(item.preCompletion));
                }
            }

            foreach (var item in objs)
            {
                data.Add(new CodeCompletionData(new ObjectItem(CompletionWordType.Variable, item.mainname)));
            }
            if (noargFlag)
            {
                foreach (var item in vars.FindAll(x => scope.Contains(x.Scope) && !x.IsArg))
                {
                    data.Add(new CodeCompletionData(item.PreCompletion));
                }
            }
            else
            {
                foreach (var item in vars.FindAll(x => scope.Contains(x.Scope)))
                {
                    data.Add(new CodeCompletionData(item.PreCompletion));
                }
            }
     

        }

    }
}
