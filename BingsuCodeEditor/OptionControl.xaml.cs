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

namespace BingsuCodeEditor
{
    /// <summary>
    /// OptionControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OptionControl : UserControl
    {
        private string optionfilename;

        public OptionControl()
        {
            InitializeComponent();
        }

        private CodeTextEditor codeTextEditor;

        Dictionary<string, Color> HighLightList;


        public void OpenOption(CodeTextEditor codeTextEditor, string optionfilename)
        {
            this.optionfilename = optionfilename;

            this.codeTextEditor = codeTextEditor;

            HighLightList = codeTextEditor.GetCurrentHighLight();

            ColorPallet.Children.Clear();

            foreach (var item in HighLightList.Keys)
            {
                if (codeTextEditor.IsDark)
                {
                    if (item.IndexOf("Dark") == -1) continue;
                }
                else
                {
                    if (item.IndexOf("Light") == -1) continue;
                }
                OptionControlItem optionitem = new OptionControlItem(HighLightList, item);

                ColorPallet.Children.Add(optionitem);
            }


            CBFontSize.Items.Clear();
            for (int i = 6; i < 64; i++)
            {
                CBFontSize.Items.Add(i);
                if (codeTextEditor.aTextEditor.FontSize == i)
                {
                    CBFontSize.SelectedIndex = CBFontSize.Items.Count - 1;
                }
            }


            CBTabSize.Items.Clear();
            CBTabSize.Items.Add(2);
            CBTabSize.Items.Add(4);
            CBTabSize.Items.Add(8);
            CBTabSize.Items.Add(12);

            CBTabSize.SelectedItem = codeTextEditor.aTextEditor.Options.IndentationSize;
            cbShowlineNumber.IsChecked = codeTextEditor.aTextEditor.ShowLineNumbers;
            cbConvertTapToSpace.IsChecked = codeTextEditor.aTextEditor.Options.ConvertTabsToSpaces;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            codeTextEditor.aTextEditor.FontSize= (int)CBFontSize.SelectedItem;
            codeTextEditor.aTextEditor.Options.IndentationSize = (int)CBTabSize.SelectedItem;
            codeTextEditor.aTextEditor.ShowLineNumbers = (bool)cbShowlineNumber.IsChecked;
            codeTextEditor.aTextEditor.Options.ConvertTabsToSpaces = (bool)cbConvertTapToSpace.IsChecked;



            codeTextEditor.SaveOption(HighLightList, optionfilename);
            codeTextEditor.LoadOption(optionfilename);
            this.Visibility = Visibility.Collapsed;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            codeTextEditor.ResetOption(optionfilename);
            OpenOption(codeTextEditor, optionfilename);
        }
    }
}
