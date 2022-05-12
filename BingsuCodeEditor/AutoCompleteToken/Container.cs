using System;
using System.Collections.Generic;
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


        public string mainname;
        public string shortname;
        //각각의 콘테이너는 별칭을 가짐.
        //Ex
        //	1. Import main as m;
        //	2. Import main;
        //	3. Object human()
        //1번의 경우 메인 이름 main, 별칭 m
        //2번의 경우 메인 이름 main 별칭 *(별칭이 없다는 의미)
        //3번의 경우 메인 이름 human

        //predefine된 네임스페이스가 들어가야 할거같음...
        public List<ImportedNameSpace> importedNameSpace;


        public List<Block> vars;
        public List<Container> objs;

        public List<Function> funcs;

        public Container()
        {
            importedNameSpace = new List<ImportedNameSpace>();
            vars = new List<Block>();
            objs = new List<Container>();
            funcs = new List<Function>();
        }
    }
}
