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
        CefSharp.Wpf.ChromiumWebBrowser browser;
        public BlocklyEditor()
        {
            InitializeComponent();




            string localPath = System.Environment.CurrentDirectory;

            
            browser = new CefSharp.Wpf.ChromiumWebBrowser(localPath + @"/BlocklySetting/StartPage.html");
            JsHandler jh = new JsHandler();
            browser.JsDialogHandler = jh;

            BrowserGrid.Child = browser;

            browser.LoadingStateChanged += WebBrowser_LoadingStateChanged;

        }

        private async void WebBrowser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                var toolboxXML = System.IO.File.ReadAllText(@"BlocklySetting\Toolbox.xml");
                var workspaceXML = System.IO.File.ReadAllText(@"BlocklySetting\TempWorkspace.xml");

     

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









        public class JsHandler : IJsDialogHandler
        {
            public bool OnBeforeUnloadDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, string messageText, bool isReload, IJsDialogCallback callback)
            {
                return true;
            }

            public void OnDialogClosed(IWebBrowser chromiumWebBrowser, IBrowser browser)
            {
                return;
            }

            public bool OnJSDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, CefJsDialogType dialogType, string messageText, string defaultPromptText, IJsDialogCallback callback, ref bool suppressMessage)
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(1000);
                }

                switch (dialogType)
                {
                    case CefJsDialogType.Prompt: // alert
                        callback.Continue(true, "asd");
                        return true;
                    case CefJsDialogType.Alert: // alert
                        //MessageBox.Show(messageText, "Notice", MessageBoxButton.OK);
                        callback.Continue(true);
                        return true;
                    case CefJsDialogType.Confirm: // confirm
                        var result = MessageBox.Show(messageText, "Notice", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        callback.Continue(result == MessageBoxResult.Yes);
                        return true;
                }
                return false;
            }

            public void OnResetDialogState(IWebBrowser chromiumWebBrowser, IBrowser browser)
            {
                return;
            }


        }
    }
}
