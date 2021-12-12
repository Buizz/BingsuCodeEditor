using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BingsuCodeEditor.CodeAnalyzer;

namespace BingsuCodeEditor
{
    public abstract class PreCompletionData
    {
        public CompletionWordType completionWordType;


        public PreCompletionData(CompletionWordType completionType, string name)
        {
            this.completionWordType = completionType;
            this.name = name;
        }

        //키워드 이름
        protected string name;
        public abstract string listheader { get; }
        public abstract string ouputstring { get; }
        public abstract string desc { get; }



        //자동 입력을 위한 프리셋
        public string AutoInsert;
    }
}
