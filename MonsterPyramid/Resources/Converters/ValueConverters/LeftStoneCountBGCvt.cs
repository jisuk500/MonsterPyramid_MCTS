using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Data;
using System.Windows.Media;

namespace MonsterPyramid.Resources.Converters.ValueConverters
{
    class LeftStoneCountBGCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int c = (int)value;

            if(c==0)
            {
                return Brushes.Gray;
            }
            else if(c<=3)
            {
                return Brushes.Red;
            }
            else if(c<=6)
            {
                return Brushes.Blue;
            }
            else if(c<=12)
            {
                return Brushes.Green;
            }
            else
            {
                return Brushes.Green;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
