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
            switch (pullpath)
            {
                case "functest":
                    return "function test(a,b){}";
                case "a":
                    return "import b as b1;";
                case "b":
                    return "import c as c1;";
                case "c":
                    return "var ctest;";
                case "d.f":
                    return "var ctest;";
                case "DEFAULTFUNCTIONLIST":
                    return "var ctest;";
                    //return System.IO.File.ReadAllText("epscriptfunction.txt");
            }
            return "var test1;";
        }

        public override List<string> GetFIleList()
        {
            List<string> rlist = new List<string>();
            //rlist.Add("TriggerEditor.SCArchive");
            rlist.Add("a");
            rlist.Add("b");
            rlist.Add("c");
            rlist.Add("d.f");
            rlist.Add("functest");

            return rlist;
        }

        public override List<string> GetImportedFileList(string basefilename = "")
        {
            throw new NotImplementedException();
        }
    }
}
