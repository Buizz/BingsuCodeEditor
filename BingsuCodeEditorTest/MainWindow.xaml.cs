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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            CodeEditor.AddAdditionalstring("default", "function ab");
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            CodeEditor.Deactivated();
        }

        private bool IsDark;
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

            PaletteHelper paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();
            if (!IsDark)
            {
                CodeEditor.IsDark = true;
                theme.SetBaseTheme(Theme.Dark);
                paletteHelper.SetTheme(theme);
                IsDark = true;
            }
            else
            {
                CodeEditor.IsDark = false;
                theme.SetBaseTheme(Theme.Light);
                paletteHelper.SetTheme(theme);
                IsDark = false;
            }

        }
    }
}
