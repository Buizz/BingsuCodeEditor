using BingsuCodeEditor;
using BingsuCodeEditorTest.epScript;
using Dragablz;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
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

namespace BingsuCodeEditorTest
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        ImportManager importManager;
        public MainWindow()
        {
            InitializeComponent();


            importManager = new epsImportManager();
            MainTabablzControl.NewItemFactory = NewItemFunc;
            //CodeEditor.AddAdditionalstring("default", "function ab");

          
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            //CodeEditor.Deactivated();
        }

        private bool IsDark = true;
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            IsDark = !IsDark;

            SetTheme();
        }

        private void SetTheme()
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();
            if (IsDark)
            {
                theme.SetBaseTheme(Theme.Dark);
                paletteHelper.SetTheme(theme);
                //CodeEditor.IsDark = true;
            }
            else
            {
                theme.SetBaseTheme(Theme.Light);
                paletteHelper.SetTheme(theme);
                //CodeEditor.IsDark = false;
            }
        }


        private void NewItem_Click(object sender, RoutedEventArgs e)
        {
            //TabablzControl.AddItem(NewItemFunc(),null , AddLocationHint.First);
            TabItem tab = (TabItem)NewItemFunc();
            MainTabablzControl.Items.Add(tab);
            MainTabablzControl.SelectedItem = tab;
        }

        private object NewItemFunc()
        {
            TabItem tab = new TabItem();
            BingsuCodeEditor.CodeTextEditor codeTextEditor = new BingsuCodeEditor.CodeTextEditor();
            codeTextEditor.Syntax = BingsuCodeEditor.CodeTextEditor.CodeType.epScript;
            codeTextEditor.SetFilePath = "Tool.build";
            codeTextEditor.IsDark = true;
            codeTextEditor.SetImportManager(importManager);

            codeTextEditor.OptionFilePath = @"F:\Users\Desktop\테스트\";
            codeTextEditor.LoadOption("");

            tab.Content = codeTextEditor;
            tab.Header = "새 파일";

            return tab;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetTheme();
            TabItem tab = (TabItem)NewItemFunc();
            MainTabablzControl.Items.Add(tab);
            MainTabablzControl.SelectedItem = tab;
        }
    }
}
