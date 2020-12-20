using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using MonsterPyramid.Model;

namespace MonsterPyramid.Resources.Converters.ValueConverters
{
    public class PlayerEnumCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Players p = (Players)value;

            switch (p)
            {
                case Players.P1:
                    {
                        return "P1";
                    }
                case Players.P2:
                    {
                        return "P2";
                    }
                case Players.Me:
                    {
                        return "Me";
                    }
                case Players.None:
                    {
                        return "";
                    }
                default:
                    {
                        return "";
                    }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
