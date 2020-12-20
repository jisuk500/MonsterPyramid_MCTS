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
    /// 플레이어 행동 로그 저장 클래스
    /// </summary>
    public class PlayerActLog : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 이 로그의 번호
        /// </summary>
        public int logOrder { get; private set; }
        /// <summary>
        /// 이 로그를 찍히게 만든 boardcellinfo 클래스
        /// </summary>
        public BoardCellInfo refBoardCellInfo {get;set;}
        /// <summary>
        /// 이 로그가 선택되었는지 결정하는 클래스
        /// </summary>
        private bool isSelected_internal;
        public bool isSelected {
            get
            {
                return isSelected_internal;
            }
            set
            {
                isSelected_internal = value;
                if (!isSkippingLog)
                {
                    refBoardCellInfo.stoneEmphasize = value;
                }
            }
        }
        /// <summary>
        /// 이 로그가 스키핑 으로 인해 생겨난 거인지 
        /// </summary>
        public bool isSkippingLog { get; set; }
        /// <summary>
        /// 스키핑 로그의 주인 불쌍하누ㅋㅋ
        /// </summary>
        public Players skippingPlayer { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="logOrder_">이 로그의 번호</param>
        /// <param name="refBoardCellInfo_">이 로그를 찍히게 만든 boardcellinfo 클래스</param>
        public PlayerActLog(int logOrder_,BoardCellInfo refBoardCellInfo_ = null,bool isSkippingLog_ = false, Players skippedPlayer_ = Players.None)
        {
            if ((1 <= logOrder_) && (logOrder_ <= 36))
            {
                logOrder = logOrder_;
            }
            else
            {
                logOrder = -1;
            }
            isSelected = false;

            refBoardCellInfo = refBoardCellInfo_;
            isSkippingLog = isSkippingLog_;
            skippingPlayer = skippedPlayer_;
        }
    }
}
