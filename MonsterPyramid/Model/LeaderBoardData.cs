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
    /// 리더보드 스코어 자료형 클래스
    /// </summary>
    public class LeaderBoardData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 이 칸의 플레이어 데이터
        /// </summary>
        public PlayerData curPlayerData {get;set;}

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="curPlayerData_">이 칸의 플레이어 데이터</param>
        public LeaderBoardData(PlayerData curPlayerData_)
        {
            curPlayerData = curPlayerData_;
        }

        
    }
}
