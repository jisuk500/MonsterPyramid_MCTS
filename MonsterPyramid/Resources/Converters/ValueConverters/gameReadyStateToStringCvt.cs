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
    class gameReadyStateToStringCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ReadyState curState = (ReadyState)value;

            switch (curState)
            {
                case ReadyState.SelectPlayerPos:
                    {
                        return "자신의 초기 위치를 선택해주세요.";
                    }
                case ReadyState.SetInitialStones:
                    {
                        return "자신의 초기 말 상태를 구성해주세요.";
                    }
                case ReadyState.GameReady:
                    {
                        return "게임 시작이 가능합니다!";
                    }
                case ReadyState.Gaming:
                    {
                        return "게임 시작!";
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
