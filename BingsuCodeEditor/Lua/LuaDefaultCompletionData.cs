﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BingsuCodeEditor.CodeAnalyzer;

namespace BingsuCodeEditor.Lua
{
    public static class LuaDefaultCompletionData
    {
        private static Dictionary<string, List<CodeCompletionData>> DefaultCompletionData = new Dictionary<string, List<CodeCompletionData>>();



        private static List<string> argstatictypes_list = new List<string>();


        private static string[] default_argtypes =
        {
            "TrgAllyStatus", "TrgComparison", "TrgCount", "TrgModifier", "TrgOrder",
            "TrgPlayer", "TrgProperty", "TrgPropState", "TrgResource", "TrgScore", "TrgSwitchAction", "TrgSwitchState",
            "TrgAIScript"
        };

        private static string[] default_dynamicargtypes =
        {
            "TrgLocation", "TrgSwitch", "TrgUnit"
        };

        private static string[] added_argtypes =
        {
            "FormatString", "Arguments", "Tbl", "UnitsDat", "WeaponsDat", "FlingyDat",
            "SpritesDat", "ImagesDat", "UpgradesDat", "TechdataDat", "OrdersDat", "Weapon", "Flingy",
            "Sprite", "Image", "Upgrade", "Tech", "Order", "Icon", "Portrait", "EUDScore", "SupplyType"
        };

        private static string[] added_dynamicargtypes =
        {
            "WAVName", "BGM"
        };




        public static Func<string, string[]> GetArgDataList;


        public static Func<string[]> GetArgKeyWordList;

        private static bool IsLoad = false;
        public static void Init()
        {
            IsLoad = true;

            argstatictypes_list.AddRange(default_argtypes);
            argstatictypes_list.AddRange(added_argtypes);

            if (GetArgDataList !=null)
            {
                foreach (var item in default_argtypes)
                {
                    string[] datas = GetArgDataList(item);

                    if (datas != null)
                    {
                        List<CodeCompletionData> completionDatas = new List<CodeCompletionData>();
                        foreach (var t in datas)
                        {
                            switch (item)
                            {
                                case "TrgLocation":
                                case "TrgSwitch":
                                case "TrgUnit":
                                case "TrgAIScript":
                                    completionDatas.Add(new CodeCompletionData(new CompletionItem(CompletionWordType.Const, "\"" + t + "\"", "\"" + t + "\"")));
                                    break;
                                default:
                                    completionDatas.Add(new CodeCompletionData(new CompletionItem(CompletionWordType.Const, t, t)));
                                    break;
                            }
                        }

                        DefaultCompletionData.Add(item, completionDatas);
                    }
                }

                foreach (var item in added_argtypes)
                {
                    string[] datas = GetArgDataList(item);

                    if (datas != null)
                    {
                        List<CodeCompletionData> completionDatas = new List<CodeCompletionData>();
                        foreach (var t in datas)
                            completionDatas.Add(new CodeCompletionData(new CompletionItem(CompletionWordType.Const, t, "\"" + t + "\"")));
                        DefaultCompletionData.Add(item, completionDatas);
                    }
                }
            }

            {
                List<string> keywordlist = new List<string>();

                keywordlist.AddRange(default_argtypes);
                keywordlist.AddRange(default_dynamicargtypes);
                keywordlist.AddRange(added_argtypes);
                keywordlist.AddRange(added_dynamicargtypes);

                List<CodeCompletionData> completionDatas = new List<CodeCompletionData>();
                foreach (var item in keywordlist)
                {
                    completionDatas.Add(new CodeCompletionData(new CompletionItem(CompletionWordType.Setting, item, item)));
                }
                DefaultCompletionData.Add("ARGKEYWORDLIST", completionDatas);
            }
        }


        public static List<CodeCompletionData> GetCompletionDataList(string Key)
        {
            if (!IsLoad) Init();

            List<CodeCompletionData> codeCompletionDatas = new List<CodeCompletionData>();

            if(argstatictypes_list.IndexOf(Key) != -1)
            {
                foreach (var item in DefaultCompletionData[Key])
                {
                    codeCompletionDatas.Add(item);
                }
            }
            else
            {
                //다이나믹일때
                if(GetArgDataList != null)
                {
                    foreach (var item in GetArgDataList(Key))
                    {
                        string text = item;

                        if(text.Split('|').Count() > 1)
                        {
                            text = text.Split('|').First();
                            if(text.Count() > 1 && text[0] == '"')
                            {
                                text = text + "\"";
                            }
                        }


                        

                        switch (Key)
                        {
                            case "TrgLocation":
                            case "TrgSwitch":
                            case "TrgUnit":
                            case "TrgAIScript":
                                codeCompletionDatas.Add(new CodeCompletionData(new CompletionItem(CompletionWordType.Const, text, "\"" + text + "\"")));
                                break;
                            default:
                                codeCompletionDatas.Add(new CodeCompletionData(new CompletionItem(CompletionWordType.Const, text, text)));
                                break;
                        }
                    }
                }
            }



            return codeCompletionDatas;
        }


        public static List<CodeCompletionData> GetCompletionKeyWordList()
        {
            if (!IsLoad) Init();

            List<CodeCompletionData> codeCompletionDatas = new List<CodeCompletionData>();

            codeCompletionDatas.AddRange(DefaultCompletionData["ARGKEYWORDLIST"]);

            if (GetArgKeyWordList != null)
            {
                string[] keywords = GetArgKeyWordList();

                foreach (var item in keywords)
                {
                    codeCompletionDatas.Add(new CodeCompletionData(new CompletionItem(CompletionWordType.Setting, item, item)));
                }
            }

            return codeCompletionDatas;
        }
    }
}
