using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BingsuBlocklyEpsEditor
{
    /// <summary>
    /// BlocklyEditor.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BlocklyEditor : UserControl
    {
        public BlocklyEditor()
        {
            InitializeComponent();
            
            browser.LoadHtml(System.IO.File.ReadAllText(@"BlocklySetting\BlocklyHTML.html"));
            //browser.BrowserSettings.AcceptLanguageList = "ko-KR";



            browser.LoadingStateChanged += WebBrowser_LoadingStateChanged;

        }

        private async void WebBrowser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                var toolboxXML = System.IO.File.ReadAllText(@"BlocklySetting\blockyToolbox.xml");
                var workspaceXML = System.IO.File.ReadAllText(@"BlocklySetting\blockyWorkspace.xml");
                ////ddddffddssssssdddssdsddsssd//sssssddddssssdddddd


                var rtn = await browser.EvaluateScriptAsync("init", new object[] { toolboxXML, workspaceXML });
                if (rtn.Success != true)
                {
                    MessageBox.Show(rtn.Message);
                };
            }
        }


        private async void showCodeButton_Click(object sender, RoutedEventArgs e)
        {

            var rtn = await browser.EvaluateScriptAsync("showCode", new object[] {  });
            if (rtn.Success == true)
            {
                MessageBox.Show(rtn.Result.ToString());
            }
            else
            {
                MessageBox.Show(rtn.Message);
            };
            //var result = browser.InvokeScript("showCode", new object[] { });
            //MessageBox.Show(result.ToString());
        }


    }
}
