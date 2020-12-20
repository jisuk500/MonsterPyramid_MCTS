using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Data;
using System.Windows.Media.Imaging;
using MonsterPyramid.Model;

namespace MonsterPyramid.Resources.Converters.ValueConverters
{
    class StoneEnumImageCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Stones curp = (Stones)value;

            switch (curp)
            {
                case Stones.Pepe:
                    {
                        return new BitmapImage(new Uri(@"/Resources/Images/Stones/pepe.png",UriKind.Relative));
                    }
                case Stones.Pink:
                    {
                        return new BitmapImage(new Uri(@"/Resources/Images/Stones/pink.png", UriKind.Relative));
                    }
                case Stones.Slime:
                    {
                        return new BitmapImage(new Uri(@"/Resources/Images/Stones/slime.png", UriKind.Relative));
                    }
                case Stones.Octo:
                    {
                        return new BitmapImage(new Uri(@"/Resources/Images/Stones/octo.png", UriKind.Relative));
                    }
                case Stones.Mush:
                    {
                        return new BitmapImage(new Uri(@"/Resources/Images/Stones/mush.png", UriKind.Relative));
                    }
                case Stones.Special:
                    {
                        return new BitmapImage(new Uri(@"/Resources/Images/Stones/special.png", UriKind.Relative));
                    }
                case Stones.None:
                    {
                        return new BitmapImage();
                    }
                case Stones.Skipped:
                    {
                        return new BitmapImage();
                    }
                default:
                    {
                        return new BitmapImage();
                    }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
