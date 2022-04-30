using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BingsuCodeEditor.CodeAnalyzer;

namespace BingsuCodeEditor.AutoCompleteToken
{
    public class ImportedNameSpace
    {
        public string mainname;
        public string shortname;

        public ImportedNameSpace(string mainname, string shortname)
        {
            this.mainname = mainname;
            this.shortname = shortname;

            //if (string.IsNullOrEmpty(shortname))
            //{
            //    return;
            //}

            preCompletion = new ImportFileItem(CompletionWordType.nameSpace, shortname);
        }


        public PreCompletionData preCompletion;
    }
}
