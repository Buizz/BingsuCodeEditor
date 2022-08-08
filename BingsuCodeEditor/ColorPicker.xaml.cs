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
    /// ColorPicker.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        public ColorPicker()
        {

            // 디자이너에서 이 호출이 필요합니다.
            InitializeComponent();

            // InitializeComponent() 호출 뒤에 초기화 코드를 추가하세요.
            LinearGradientBrush LinearGradientBrush = new LinearGradientBrush();
            LinearGradientBrush.StartPoint = new Point(0.5, 0);
            LinearGradientBrush.EndPoint = new Point(0.5, 1);
            LinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 255, 0, 0), 0.02));
            LinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 255, 255, 0), 0.167));
            LinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 0, 255, 0), 0.334));
            LinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 0, 255, 255), 0.501));
            LinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 0, 0, 255), 0.668));
            LinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 255, 0, 255), 0.835));
            LinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 255, 0, 0), 0.975));
            HueBar.Background = LinearGradientBrush;

            TextBoxuseable = true;
        }

        public void InitColor(Color color)
        {
            SVPanelDrag = false;

            FristColor.Background = new SolidColorBrush(color);

            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;

            RText.Text = R.ToString();
            GText.Text = G.ToString();
            BText.Text = B.ToString();
            AText.Text = A.ToString();

            ColorToHSV(color, ref Saturation, ref Value);

            double RealWidth = SVPanel.ActualWidth;
            double RealHeight = SVPanel.ActualHeight;
            PickerStylus.Margin = new Thickness(Saturation * RealWidth - 7, (1 - Value) * RealHeight - 7, 0, 0);

            ColorHSVRefresh();
        }

        public event RoutedEventHandler ColorSelect;

        public double PHue { get; set; }
        private double Hue
        {
            get
            {
                return PHue;
            }
            set
            {
                PHue = value;
                double RealHeight = HueBar.ActualHeight;
                HueBarGage.Margin = new Thickness(0, (((Hue / 360) * RealHeight) * 2 - RealHeight - 6), 0, 0);
                BaseColor.Background = new SolidColorBrush(ColorFromHSV(Hue, 1, 1));
            }
        }
        private double Saturation;
        private double Value;


        private byte PR;
        private byte PG;
        private byte PB;
        private byte PA;
        private byte R
        {
            get
            {
                return PR;
            }
            set
            {
                PR = value;
            }
        }
        private byte G
        {
            get
            {
                return PG;
            }
            set
            {
                PG = value;
            }
        }
        private byte B
        {
            get
            {
                return PB;
            }
            set
            {
                PB = value;
            }
        }
        private byte A
        {
            get
            {
                return PA;
            }
            set
            {
                PA = value;
            }
        }


        private void ColorHSVRefresh()
        {
            Color MainColor = ColorFromHSV(Hue, Saturation, Value);
            TextBoxuseable = false;
            PR = MainColor.R;
            PG = MainColor.G;
            PB = MainColor.B;
            RText.Text = PR.ToString();
            GText.Text = PG.ToString();
            BText.Text = PB.ToString();


            TextBoxuseable = true;

            CurrentColor.Background = new SolidColorBrush(MainColor);
        }

        private void ColorRGBRefresh()
        {
            Color MainColor = Color.FromRgb(R, G, B);

            ColorToHSV(MainColor, ref Saturation, ref Value);
            // ColorHSVRefresh()
            double RealWidth = SVPanel.ActualWidth;
            double RealHeight = SVPanel.ActualHeight;
            PickerStylus.Margin = new Thickness(Saturation * RealWidth - 7, (1 - Value) * RealHeight - 7, 0, 0);

            CurrentColor.Background = new SolidColorBrush(MainColor);

            LastColor.Background = new SolidColorBrush(Color.FromArgb(A, R, G, B));
            ColorSelect.Invoke(Color.FromArgb(A, R, G, B), new RoutedEventArgs());
        }

        private bool HueBarDrag = false;
        private void HueBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double RealHeight = HueBar.ActualHeight;

            Hue = (e.GetPosition(HueBar).Y / RealHeight) * 360;

            if (Hue < 0)
                Hue = 0;
            if (Hue > 360)
                Hue = 360;
            ColorHSVRefresh();
            // MsgBox(Hue & " " & ColorFromHSV(Hue, 1, 1).ToString)
            HueBarDrag = true;
        }

        private void HueBar_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            //if (HueBarDrag)
            //{
            //    double RealHeight = HueBar.ActualHeight;

            //    Hue = (e.GetPosition(HueBar).Y / RealHeight) * 360;

            //    if (Hue < 0)
            //        Hue = 0;
            //    if (Hue > 360)
            //        Hue = 360;
            //    ColorHSVRefresh();
            //}
        }

        private void HueBar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LastColor.Background = new SolidColorBrush(Color.FromArgb(A, R, G, B));
            ColorSelect.Invoke(Color.FromArgb(A, R, G, B), e);
            HueBarDrag = false;
        }

        private bool SVPanelDrag = false;
        private void SVPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double RealWidth = SVPanel.ActualWidth;
            double RealHeight = SVPanel.ActualHeight;

            Saturation = e.GetPosition(SVPanel).X / RealWidth;
            Value = 1 - e.GetPosition(SVPanel).Y / RealHeight;

            PickerStylus.Margin = new Thickness(Saturation * RealWidth - 7, (1 - Value) * RealHeight - 7, 0, 0);
            ColorHSVRefresh();

            SVPanelDrag = true;
        }

        private void SVPanel_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LastColor.Background = new SolidColorBrush(Color.FromArgb(A, R, G, B));
            ColorSelect.Invoke(Color.FromArgb(A, R, G, B), e);


            SVPanelDrag = false;
        }

        private void SVPanel_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            //if (SVPanelDrag)
            //{
            //    double RealWidth = SVPanel.ActualWidth;
            //    double RealHeight = SVPanel.ActualHeight;

            //    Saturation = e.GetPosition(SVPanel).X / RealWidth;
            //    Value = 1 - e.GetPosition(SVPanel).Y / RealHeight;

            //    PickerStylus.Margin = new Thickness(Saturation * RealWidth - 7, (1 - Value) * RealHeight - 7, 0, 0);
            //    ColorHSVRefresh();
            //}
        }




        public void ColorToHSV(Color tcolor, ref double saturation, ref double value)
        {
            System.Drawing.Color color = System.Drawing.Color.FromArgb(tcolor.A, tcolor.R, tcolor.G, tcolor.B);


            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));
            Hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1.0D - (1.0D * min / max);
            value = max / 255.0D;
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);
            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, (byte)v, (byte)t, (byte)p);
            else if (hi == 1)
                return Color.FromArgb(255, (byte)q, (byte)v, (byte)p);
            else if (hi == 2)
                return Color.FromArgb(255, (byte)p, (byte)v, (byte)t);
            else if (hi == 3)
                return Color.FromArgb(255, (byte)p, (byte)q, (byte)v);
            else if (hi == 4)
                return Color.FromArgb(255, (byte)t, (byte)p, (byte)v);
            else
                return Color.FromArgb(255, (byte)v, (byte)p, (byte)q);
        }

        private bool TextBoxuseable = false;
        private void RText_TextChanged(object sender, KeyEventArgs e)
        {
            if (TextBoxuseable & e.Key == Key.Enter)
            {
                long tlong;
                if (!long.TryParse(RText.Text, out tlong))
                {
                    RText.Text = R.ToString();
                }


                if (tlong > 255)
                {
                    RText.Text = "255";
                    tlong = 255;
                }
                if (tlong < 0)
                {
                    RText.Text = "0";
                    tlong = 0;
                }

                R = (byte)tlong;
                ColorRGBRefresh();
            }
        }

        private void GText_TextChanged(object sender, KeyEventArgs e)
        {
            if (TextBoxuseable & e.Key == Key.Enter)
            {
                long tlong;
                if (!long.TryParse(GText.Text, out tlong))
                {
                    GText.Text = G.ToString();
                }
                if (tlong > 255)
                {
                    GText.Text = "255";
                    tlong = 255;
                }
                if (tlong < 0)
                {
                    GText.Text = "0";
                    tlong = 0;
                }

                G = (byte)tlong;
                ColorRGBRefresh();
            }
        }

        private void BText_TextChanged(object sender, KeyEventArgs e)
        {
            if (TextBoxuseable & e.Key == Key.Enter)
            {
                long tlong;
                if (!long.TryParse(BText.Text, out tlong))
                {
                    BText.Text = B.ToString();
                }

                if (tlong > 255)
                {
                    BText.Text = "255";
                    tlong = 255;
                }
                if (tlong < 0)
                {
                    BText.Text = "0";
                    tlong = 0;
                }

                B = (byte)tlong;
                ColorRGBRefresh();
            }
        }

        private void AText_TextChanged(object sender, KeyEventArgs e)
        {
            if (TextBoxuseable & e.Key == Key.Enter)
            {
                long tlong;
                if (!long.TryParse(AText.Text, out tlong))
                {
                    AText.Text = A.ToString();
                }

                if (tlong > 255)
                {
                    AText.Text = "255";
                    tlong = 255;
                }
                if (tlong < 0)
                {
                    AText.Text = "0";
                    tlong = 0;
                }

                A = (byte)tlong;
                ColorRGBRefresh();
            }
        }

        private void RText_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TextBoxuseable)
            {
                long tlong;
                if (!long.TryParse(RText.Text, out tlong))
                {
                    RText.Text = R.ToString();
                }

                if (tlong > 255)
                {
                    RText.Text = "255";
                    tlong = 255;
                }
                if (tlong < 0)
                {
                    RText.Text = "0";
                    tlong = 0;
                }

                R = (byte)tlong;
                ColorRGBRefresh();
            }
        }

        private void GText_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TextBoxuseable)
            {
                long tlong;
                if (!long.TryParse(GText.Text, out tlong))
                {
                    GText.Text = G.ToString();
                }

                if (tlong > 255)
                {
                    GText.Text = "255";
                    tlong = 255;
                }
                if (tlong < 0)
                {
                    GText.Text = "0";
                    tlong = 0;
                }

                G = (byte)tlong;
                ColorRGBRefresh();
            }
        }

        private void BText_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TextBoxuseable)
            {
                long tlong;
                if (!long.TryParse(BText.Text, out tlong))
                {
                    BText.Text = B.ToString();
                }

                if (tlong > 255)
                {
                    BText.Text = "255";
                    tlong = 255;
                }
                if (tlong < 0)
                {
                    BText.Text = "0";
                    tlong = 0;
                }

                B = (byte)tlong;
                ColorRGBRefresh();
            }
        }

        private void AText_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TextBoxuseable)
            {
                long tlong;
                if (!long.TryParse(AText.Text, out tlong))
                {
                    AText.Text = A.ToString();
                }

                if (tlong > 255)
                {
                    AText.Text = "255";
                    tlong = 255;
                }
                if (tlong < 0)
                {
                    AText.Text = "0";
                    tlong = 0;
                }
                A = (byte)tlong;
            }
        }

        private void UserControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (SVPanelDrag)
            {
                double RealWidth = SVPanel.ActualWidth;
                double RealHeight = SVPanel.ActualHeight;

                Saturation = e.GetPosition(SVPanel).X / RealWidth;
                Value = 1 - e.GetPosition(SVPanel).Y / RealHeight;

                if (Saturation < 0)
                    Saturation = 0;
                if (Saturation > 1)
                    Saturation = 1;
                if (Value < 0)
                    Value = 0;
                if (Value > 1)
                    Value = 1;

                PickerStylus.Margin = new Thickness(Saturation * RealWidth - 7, (1 - Value) * RealHeight - 7, 0, 0);
                ColorHSVRefresh();
            }
            if (HueBarDrag)
            {
                double RealHeight = HueBar.ActualHeight;

                Hue = (e.GetPosition(HueBar).Y / RealHeight) * 360;
                if (Hue < 0)
                    Hue = 0;
                if (Hue > 360)
                    Hue = 360;

                ColorHSVRefresh();
            }
        }

        private void UserControl_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (SVPanelDrag | HueBarDrag)
            {
                SVPanelDrag = false;
                HueBarDrag = false;
                LastColor.Background = new SolidColorBrush(Color.FromArgb(A, R, G, B));
                ColorSelect.Invoke(Color.FromArgb(A, R, G, B), e);
            }
        }
    }
}
