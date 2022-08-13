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

        private void SetSummary(string type, string sectype, string content, string argtype)
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
                    if (!argsummary.ContainsKey(sectype)) {
                        argsummary.Add(sectype, content);
                    }
                    Arg arg = args.Find((x) => x.argname == sectype);

                    if(arg != null)
                    {
                        arg.argtype = argtype;
                    }

                    break;
            }


        }

        public override void ReadComment(string launage)
        {
            argsummary.Clear();
            /***
--[================================[
@Language.ko-KR
@Summary
[Player]의 [Unit]의 유닛보유수를 [Amount]만큼 [Modifier]합니다.
@Group
유닛보유수
@param.Unit.TrgUnit
@param.Player.TrgPlayer
@param.Modifier.TrgModifier
@param.Amount.Number


@Language.en-US
@Summary
[Player]의 [Unit]의 유닛보유수를 [Amount]만큼 [Modifier]합니다.
@Group
유닛보유수
@param.Unit.TrgUnit
@param.Player.TrgPlayer
@param.Modifier.TrgModifier
@param.Amount.Number
]================================]
            ***/
            if (string.IsNullOrEmpty(comment)) return;

            string c = this.comment.Trim();

            string[] line = c.Split('\n');

            string ctype = "";
            string cargtype = "";
            string csectype = "";
            string clan = "";
            string content = "";
            for (int i = 0; i < line.Count(); i++)
            {
                if (string.IsNullOrEmpty(line[i])) continue;
                string[] o = line[i].Split('.');



                switch (o[0].Trim())
                {
                    case "@Language":
                        clan = o.Last().Trim();
                        break;
                    case "@Summary":
                    case "@param":
                        if (ctype != "")
                        {
                            //마지막으로 읽은 ctype정리하기.
                            if (clan == launage)
                            {
                                SetSummary(ctype, csectype, content.Trim(), cargtype.Trim());
                            }
                        }
                        ctype = o[0].Trim();
                        if (o[0].Trim() == "@param")
                        {
                            csectype = o[1];
                            cargtype = o[2];
                        }

                        content = "";
                        break;
                    default:
                        content += line[i].Trim();
                        break;
                }
            }

            //마지막으로 읽은 ctype정리하기.
            if (clan == launage)
            {
                SetSummary(ctype, csectype, content.Trim(), cargtype.Trim());
            }


        }
    }
}
