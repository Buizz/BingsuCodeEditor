using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BingsuCodeEditor.CodeAnalyzer;

namespace BingsuCodeEditor.AutoCompleteToken
{
    public class Block
    {
        //변수나 배열, 오브젝트등입니다.

        //오브젝트의 경우 Container이 이어져 있습니다.


        public string scope;


        //const, var
        public string blockdefine;

        //객체
        public string blocktype;

        //변수 명
        public string blockname;

        public bool IsArg;

        public string rawtext;

        public List<CodeAnalyzer.TOKEN> values;


        public PreCompletionData preCompletion;

        public Block(string blockdefine, string blockname, string blocktype = "", List<CodeAnalyzer.TOKEN> values = null, bool IsArg = false)
        {
            this.blockdefine = blockdefine;
            this.blocktype = blocktype;
            this.blockname = blockname;
            this.values = values;
            this.IsArg = IsArg;

            if(blockdefine == "var")
            {
                preCompletion = new ObjectItem(CompletionWordType.Variable, blockname, block:this);
            }
            else
            {
                preCompletion = new ObjectItem(CompletionWordType.Const, blockname, block: this);
            }
        }
    }
}
