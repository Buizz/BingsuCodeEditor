using BingsuCodeEditor;
using BingsuCodeEditor.AutoCompleteToken;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingsuCodeEditorTest.epScript
{
    public class epsImportManager : ImportManager
    {



        public override string GetDefaultFunctions()
        {
            throw new NotImplementedException();
        }


        public override string GetFIleContent(string pullpath)
        {
            throw new NotImplementedException();
        }

        public override List<string> GetFileList(string basefilename = "")
        {
            List<string> rlist = new List<string>();
            rlist.Add("TriggerEditor.SCArchive");

            return rlist;
        }


    }
}
