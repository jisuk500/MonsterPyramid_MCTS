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
    /// 해당 플레이어 데이터
    /// </summary>
    public class PlayerData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 이 플레이어의 현재 점수
        /// </summary>
        public int score { get; set; }
        /// <summary>
        /// 이 플레이어가 누구인지 표시
        /// </summary>
        public Players player { get; set; }
        /// <summary>
        /// 이 플레이어의 순위
        /// </summary>
        public Ranking curRanking { get; set; }
        /// <summary>
        /// 남은 말의 개수. 12개부터 시작함
        /// </summary>
        public int leftStoneCount { get; set; }
        /// <summary>
        /// 이 플레이어의 차례인가
        /// </summary>
        public bool nowTurn { get; set; }
        /// <summary>
        /// 이 플레이어를 처음 시작 위치로 선택하려 하는가
        /// </summary>
        public bool isSelected { get; set; }

        /// <summary>
        /// 이 플레이어가 소유한 돌 정보 콜렉션. 상대방의 경우, 소유가 확정된 돌을 표시
        /// </summary>
        public ObservableCollection<StoneCount> StonePossesionInfo { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="curRanking_">이 플레이어의 현재 순위</param>
        /// <param name="player_">이 플레이어가 누구인지 표시</param>
        /// <param name="score_">이 플레이어의 순위</param>
        public PlayerData(Ranking curRanking_, Players player_, int score_ = 0)
        {
            this.curRanking = curRanking_;
            this.player = player_;
            this.score = score_;

            StonePossesionInfo = new ObservableCollection<StoneCount>();
            for(int i=0;i<6;i++)
            {
                StonePossesionInfo.Add(new StoneCount((Stones)i, 0));
            }

            leftStoneCount = 12;
            nowTurn = false;
            isSelected = false;
        }

    }

    /// <summary>
    /// 돌 보유 정보 페어
    /// </summary>
    public class StoneCount : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 어떠한 돌인지
        /// </summary>
        public Stones stone { get; set; }
        /// <summary>
        /// 몇개 소유중인지
        /// </summary>
        public int count { get; set; }
        /// <summary>
        /// 이 돌이 선택된 상태인가
        /// </summary>
        public bool selected { get; set; }
        /// <summary>
        /// 해당 돌이 강조된 상태인가
        /// </summary>
        public bool emphasized { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="stone_">어떠한 돌의 개수를 표시할 것인가</param>
        /// <param name="count_">몇개를 소유중인가</param>
        public StoneCount(Stones stone_, int count_ = 0)
        {
            stone = stone_;
            count = count_;
            emphasized = false;
            selected = false;
        }

    }


}
