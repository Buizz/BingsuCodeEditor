using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace BingsuCodeEditor
{
    public partial class CodeTextEditor : UserControl
    {
        public string OptionFilePath;
        private const string OptionFIleName = "CodeEditorSetting.ini";

        private Dictionary<string, Color> HighLightColor = new Dictionary<string, Color>();
        public Dictionary<string, Color> GetCurrentHighLight()
        {
            //Dictionary<string, Color> HighLightList = new Dictionary<string, Color>();
            //foreach (var item in aTextEditor.SyntaxHighlighting.NamedHighlightingColors)
            //{
            //    HighLightList.Add(item.Name, item.Foreground.GetColor(null).Value);
            //}

            return HighLightColor;
        }

        private Color ColorFromString(string colorcode)
        {
            //#FFAABBCC
            //#AABBCC
            Color rcolor = new Color();

            colorcode = colorcode.Substring(1);

            List<int> colorlist = new List<int>();
            for (int i = 0; i < colorcode.Length; )
            {
                string t = colorcode.Substring(i, 2);
                int code = int.Parse(t, System.Globalization.NumberStyles.HexNumber);


                colorlist.Add(code);
                i += 2;
            }

            if(colorlist.Count > 3)
            {
                colorlist.RemoveAt(0);
            }

            rcolor.A = 255;


            rcolor.R = (byte) colorlist[0];
            rcolor.G = (byte) colorlist[1];
            rcolor.B = (byte) colorlist[2];


            return rcolor;
        }

        public void SaveOption(Dictionary<string, Color> HighLightList = null, string optionfilename = "")
        {
            if (!System.IO.Directory.Exists(OptionFilePath))
            {
                return;
                //저장 불가능
            }

            if (HighLightList == null)
            {
                HighLightList = GetCurrentHighLight();
            }
            if(HighLightList.Count == 0)
            {
                return;
            }

            //파일을 생성하고 새로 불러온다.
            FileStream fs = new FileStream(OptionFilePath + optionfilename + OptionFIleName, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);

            sw.WriteLine("ShowLineNumbers:" + ShowLineNumbers);
            sw.WriteLine("IndentationSize:" + aTextEditor.Options.IndentationSize);
            sw.WriteLine("ConvertTabsToSpaces:" + aTextEditor.Options.ConvertTabsToSpaces);
            sw.WriteLine("FontSize:" + aTextEditor.FontSize);

         

            foreach (var item in HighLightList)
            {
                sw.WriteLine(item.Key + ":" + item.Value.ToString());
            }


            sw.Close();
            fs.Close();
            //ShowLineNumbers
            //aTextEditor.Options.IndentationSize
            //aTextEditor.Options.ConvertTabsToSpaces
        }



        public void LoadOption(string optionfilename)
        {
            this.optionfilename = optionfilename;
            if (!System.IO.Directory.Exists(OptionFilePath))
            {
                //로드 불가능
                return;
            }

            //설정 파일에서 설정을 불러온다.
            if (!System.IO.File.Exists(OptionFilePath + optionfilename + OptionFIleName))
            {
                HighLightColor.Clear();
                //설정 파일이 없으므로 새로 만들어 준다.

                {
                    XmlDocument xd = new XmlDocument();
                    xd.Load(GetDefaultHighLight(HighLighting, true));
                    XmlNodeList nodeList = xd.GetElementsByTagName("Color");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        string name = nodeList[i].Attributes["name"].Value;
                        string color = nodeList[i].Attributes["foreground"].Value;

                        HighLightColor.Add(HighLighting + ".Dark." + name, ColorFromString(color));
                    }
                }
                {
                    XmlDocument xd = new XmlDocument();
                    xd.Load(GetDefaultHighLight(HighLighting, false));
                    XmlNodeList nodeList = xd.GetElementsByTagName("Color");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        string name = nodeList[i].Attributes["name"].Value;
                        string color = nodeList[i].Attributes["foreground"].Value;

                        HighLightColor.Add(HighLighting + ".Light." + name, ColorFromString(color));
                    }
                }




                return;
            }
            FileStream fs = new FileStream(OptionFilePath + optionfilename + OptionFIleName, FileMode.Open);
            StreamReader sr = new StreamReader(fs);

            string t = sr.ReadToEnd();

            sr.Close();
            fs.Close();

            string[] lines = t.Split('\n');
            HighLightColor.Clear();
            foreach (var item in lines)
            {
                if (string.IsNullOrEmpty(item)) continue;

                string opname = item.Split(':')[0].Trim();
                string value = item.Split(':')[1].Trim();

                switch (opname)
                {
                    case "ShowLineNumbers":
                        bool trueflag = bool.Parse(value);
                        ShowLineNumbers = trueflag;
                        break;
                    case "IndentationSize":
                        int intendsize = int.Parse(value);
                        aTextEditor.Options.IndentationSize = intendsize;
                        break;
                    case "ConvertTabsToSpaces":
                        bool convertflag = bool.Parse(value);
                        aTextEditor.Options.ConvertTabsToSpaces = convertflag;
                        break;
                    case "FontSize":
                        int fontsize = int.Parse(value);
                        CBFontSize.SelectedItem = fontsize;
                        aTextEditor.FontSize = fontsize;
                        break;
                    default:
                        //색 컬러
                        HighLightColor.Add(opname, ColorFromString(value));
                        break;
                }
            }

            {
                XmlDocument xd = new XmlDocument();
                xd.Load(GetDefaultHighLight(HighLighting, _isdark));
                XmlNodeList nodeList = xd.GetElementsByTagName("Color");
                for (int i = 0; i < nodeList.Count; i++)
                {
                    string name = nodeList[i].Attributes["name"].Value;
                    string color = nodeList[i].Attributes["foreground"].Value;

                    string colorname;
                    if (_isdark)
                    {
                        colorname = HighLighting + ".Dark." + name;
                    }
                    else
                    {
                        colorname = HighLighting + ".Light." + name;
                    }


                    ((XmlElement) nodeList[i]).SetAttribute("foreground", HighLightColor[colorname].ToString());
                }

                MemoryStream ms = new MemoryStream();
                xd.Save(ms);

                ms.Position = 0;
                //Dic다 만들고 기본컬러 불러옴
                SetCustomHighLight(new XmlTextReader(ms));


                ms.Close();
            }
        }

        public void ResetOption(string optionfilename)
        {
            if (System.IO.File.Exists(OptionFilePath + optionfilename + OptionFIleName))
            {
                //세팅파일 제거
                System.IO.File.Delete(OptionFilePath + optionfilename + OptionFIleName);
            }
            ShowLineNumbers = true;
            aTextEditor.Options.IndentationSize = 4;
            aTextEditor.Options.ConvertTabsToSpaces = true;
            aTextEditor.FontSize = 12;

            LoadOption(optionfilename);
        }




        public bool ShowLineNumbers
        {
            get
            {
                return aTextEditor.ShowLineNumbers;
            }
            set
            {
                aTextEditor.ShowLineNumbers = value;
            }
        }
    }
}
