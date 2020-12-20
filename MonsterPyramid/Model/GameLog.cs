using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MonsterPyramid.Model
{
    /// <summary>
    /// 게임 로그 클래스. 한 게임 전체를 로깅한다.
    /// </summary>
    public class GameLog
    {
        ObservableCollection<ObservableCollection<PlayerData>> initalPlayerDatas { get; set; }
        ObservableCollection<ObservableCollection<PlayerActLog>> playerActLogs { get; set; }
        ObservableCollection<ObservableCollection<PlayerData>> finalPlayerDatas { get; set; }
    }

}
