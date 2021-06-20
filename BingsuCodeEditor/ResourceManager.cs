using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BingsuCodeEditor
{
    public static class ResourceManager
    {
        public static ImageSource IconAction = new BitmapImage(new Uri("/BingsuCodeEditor;component/Resources/Action.png", UriKind.RelativeOrAbsolute));
        public static ImageSource IconCondiction = new BitmapImage(new Uri("/BingsuCodeEditor;component/Resources/Condiction.png", UriKind.RelativeOrAbsolute));
        public static ImageSource IconConst = new BitmapImage(new Uri("/BingsuCodeEditor;component/Resources/Const.png", UriKind.RelativeOrAbsolute));
        public static ImageSource IconFunction = new BitmapImage(new Uri("/BingsuCodeEditor;component/Resources/Function.png", UriKind.RelativeOrAbsolute));
        public static ImageSource IconKeyWord = new BitmapImage(new Uri("/BingsuCodeEditor;component/Resources/KeyWord.png", UriKind.RelativeOrAbsolute));
        public static ImageSource IconnameSpace = new BitmapImage(new Uri("/BingsuCodeEditor;component/Resources/nameSpace.png", UriKind.RelativeOrAbsolute));
        public static ImageSource IconSetting = new BitmapImage(new Uri("/BingsuCodeEditor;component/Resources/Setting.png", UriKind.RelativeOrAbsolute));
        public static ImageSource IconVariable = new BitmapImage(new Uri("/BingsuCodeEditor;component/Resources/Variable.png", UriKind.RelativeOrAbsolute));
    }
}
