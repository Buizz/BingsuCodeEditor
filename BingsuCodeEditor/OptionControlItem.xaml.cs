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
    /// OptionControlItem.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OptionControlItem : UserControl
    {
        Dictionary<string, Color> dic;
        string colorname;

        public OptionControlItem(Dictionary<string, Color> dic, string colorname)
        {
            InitializeComponent();

            this.dic = dic;
            this.colorname = colorname;

            Colorize.Background = new SolidColorBrush(dic[colorname]);

            tbColorName.Text = colorname.Split('.').Last();
        }


        private void Colorize_Click(object sender, RoutedEventArgs e)
        {
            ColorPicker.InitColor(dic[colorname]);

            ColorPickerPopup.IsOpen = true;
        }

        private void ColorPicker_ColorSelect(object sender, RoutedEventArgs e)
        {
            Color color = (Color)sender;

            dic[colorname] = color;
            Colorize.Background = new SolidColorBrush(dic[colorname]);
        }
    }
}
