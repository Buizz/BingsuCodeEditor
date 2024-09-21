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
        public Container ParentContainer;

        public string Scope;


        //const, var
        public string BlockDefine;

        //객체
        public string BlockType;

        //변수 명
        public string BlockName;

        public bool IsArg;

        public string RawText;

        public List<CodeAnalyzer.TOKEN> Values;

        public TOKEN StartToken;

        public PreCompletionData PreCompletion;

        public Block(Container parentcontainer, string blockdefine, string blockname, TOKEN StartToken, string blocktype = "", List<CodeAnalyzer.TOKEN> values = null, bool IsArg = false)
        {
            this.StartToken = StartToken;
            this.ParentContainer = parentcontainer;
            this.BlockDefine = blockdefine;
            this.BlockType = blocktype;
            this.BlockName = blockname;
            this.Values = values;
            this.IsArg = IsArg;

            if(blockdefine == "var")
            {
                PreCompletion = new ObjectItem(CompletionWordType.Variable, blockname, block:this);
            }
            else
            {
                PreCompletion = new ObjectItem(CompletionWordType.Const, blockname, block: this);
            }
        }
    }
}
