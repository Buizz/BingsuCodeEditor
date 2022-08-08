using BingsuCodeEditor.AutoCompleteToken;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingsuCodeEditor.Lua
{
    internal class LuaFunction : Function
    {

        private void SetSummary(string type, string sectype, string content)
        {
            switch (type)
            {
                case "@Type":
                    special = content;
                    if (preCompletion != null)
                    {
                        switch (content)
                        {
                            case "A":
                                preCompletion.completionWordType = CodeAnalyzer.CompletionWordType.Action;
                                break;
                            case "C":
                                preCompletion.completionWordType = CodeAnalyzer.CompletionWordType.Condiction;
                                break;
                        }
                    }
                    break;
                case "@Summary":
                    funcsummary = content;
                    break;
                case "@param":
                    if(!argsummary.ContainsKey(sectype)) argsummary.Add(sectype, content);
                    break;
            }


        }

        public override void ReadComment(string launage)
        {
            argsummary.Clear();
            /***
             * @Type
             * A
             * @Summary.ko-KR
             * [Time]만큼 기다립니다.(사용하지 마세요)
             * @Summary.en-US
             * Wait for [Time] milliseconds. (Not recommended to use)
             * 
             * @param.Time.ko-KR
             * 기다리는 시간입니다.
             * @param.Time.en-US
             * 기다리는 시간입니다.
            ***/
            //function Wait(Time) { }
            if (string.IsNullOrEmpty(comment)) return;

            string c = this.comment.Replace("/***", "");
            c = c.Replace("***/", "");
            c = c.Replace(" * ", "");

            string[] line = c.Split('\n');


            string ctype = "";
            string csectype = "";
            string clan = "";
            string content = "";
            for (int i = 0; i < line.Count(); i++)
            {
                if (string.IsNullOrEmpty(line[i])) continue;
                string[] o = line[i].Split('.');



                switch (o[0].Trim())
                {
                    case "@Type":
                    case "@Summary":
                    case "@param":
                        if(ctype != "") {
                            //마지막으로 읽은 ctype정리하기.
                            if (ctype == "@Type" || clan == launage)
                            {
                                SetSummary(ctype, csectype, content.Trim());
                            }
                        }
                        ctype = o[0].Trim();
                        clan = o.Last().Trim();
                        if (o[0].Trim() == "@param") csectype = o[1];

                        content = "";
                        break;
                    default:
                        content += line[i].Trim();
                        break;
                }
            }

            //마지막으로 읽은 ctype정리하기.
             if (ctype == "@Type"  || clan == launage)
            {
                SetSummary(ctype, csectype, content.Trim());
            }


        }
    }
}
