using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Windows.Documents;
using System.Security.RightsManagement;

namespace MonsterPyramid.Model
{
    /// <summary>
    /// 게임 보드의 전체적인 데이터 클래스
    /// </summary>
    public class BoardData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 현재 보드의 셀 상태들 콜렉션
        /// </summary>
        public ObservableCollection<BoardCellInfo> BoardCellState { get; set; }

        
        /// <summary>
        /// 현재 보드의 셀 상태들을 초기화함. left right down 셀 포인터 추가
        /// </summary>
        public void BoardCellStateInitialize()
        {
            BoardCellState = new ObservableCollection<BoardCellInfo>();
            //바닥 상태의 셀들
            for(int i=0;i<8;i++)
            {
                BoardCellState.Add(new BoardCellInfo(i,true));
            }
            //바닥이 아닌 상태의 셀들
            for(int i=8;i<36;i++)
            {
                BoardCellState.Add(new BoardCellInfo(i, false));
            }

            //left랑 right down 셀들 연결 초기화
            int startIndex = 0;
            int layerwidth = 0;
            for(int i=1;i<=7;i++)
            {
                startIndex += 9 - i;
                layerwidth = 8 - i;
                for(int si = startIndex;si<startIndex+layerwidth;si++)
                {
                    BoardCellState[si].leftDownCellData = BoardCellState[si - layerwidth -1];
                    BoardCellState[si].rightDownCellData = BoardCellState[si - layerwidth];
                }
            }

        }

        /// <summary>
        /// 현재 보드에서 모든 셀의 돌 놓을 수 있음 정보를 갱신
        /// </summary>
        public void refreshAllStoneAvailable()
        {
            foreach(var cell in BoardCellState)
            {
                cell.refreshStoneAvailableDatas();
            }
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public BoardData()
        {
            BoardCellStateInitialize();
        }

    }

    /// <summary>
    /// 보드 셀 하나의 정보
    /// </summary>
    public class BoardCellInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 이 셀 정보의 인덱스
        /// </summary>
        public int curIndex { get; set; }
        /// <summary>
        /// 이 셀이 가지고 있는 stone 정보
        /// </summary>
        public Stones curStone { get; set; }
        /// <summary>
        /// 이 셀의 stone을 놓은 플레이어 정보
        /// </summary>
        public Players owner { get; set; }
        /// <summary>
        /// 이 셀 자체를 강조
        /// </summary>
        public bool cellEmphasize { get; set; }
        /// <summary>
        /// 이 셀 자체를 강조하고 마우스 호버링
        /// </summary>
        public bool cellEmphasizeHover { get; set; }
        /// <summary>
        /// 이 셀의 이미지를 강조
        /// </summary>
        public bool stoneEmphasize { get; set; }

        public Stones estimatedStone { get; set; }
        public bool cellEstimation { get; set; }

        /// <summary>
        /// 이 셀의 왼쪽 아래 셀 데이터
        /// </summary>
        public BoardCellInfo leftDownCellData { get; set; }
        /// <summary>
        /// 이 셀의 오른쪽 아래 셀 데이터
        /// </summary>
        public BoardCellInfo rightDownCellData { get; set; }

        /// <summary>
        /// 이 셀의 돌을 놓을 수 있는지 여부를 판별하는 데이터
        /// </summary>
        public ObservableCollection<bool> stoneAvailableDatas { get; set; }
        /// <summary>
        /// 이 셀이 바닥에 있는 셀인지 판별하는 bool
        /// </summary>
        public bool isFloorCell { get;private set; }

        /// <summary>
        /// 이 셀의 돌을 놓을 수 있는지 여부를 판별하는 bool 배열을 left와 right down cell에 따라서 갱신
        /// </summary>
        public void refreshStoneAvailableDatas()
        {
            //이 칸에 돌이 없다면? 고려해봄
            if(curStone == Stones.None)
            {
                if(isFloorCell) //이게 바닥 셀이라면? 전부  true
                {
                    for(int i=0;i<6;i++)
                    {
                        stoneAvailableDatas[i] = true;
                    }
                }
                else
                {
                    //만약 왼쪽과 아래쪽 전부 다 돌이 없는게 아니라면 선택해서 ㄱ
                    if ((leftDownCellData.curStone != Stones.None) && (rightDownCellData.curStone != Stones.None))
                    {
                        //그런데 그중에 special이 섞여있다면? 전부 false
                        if ((leftDownCellData.curStone == Stones.Special) || (rightDownCellData.curStone == Stones.Special))
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                stoneAvailableDatas[i] = false;
                            }
                        }
                        else//정상적인 아랫층
                        {
                            stoneAvailableDatas[(int)leftDownCellData.curStone] = true;
                            stoneAvailableDatas[(int)rightDownCellData.curStone] = true;
                            stoneAvailableDatas[(int)Stones.Special] = true;
                        }

                    }
                    else // 둘중 하나라도 None이 있다면, 아래가 비었다는 뜻이므로 전부 false
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            stoneAvailableDatas[i] = false;
                        }
                    }
                }
               
            }
            else //이 칸에 돌이 있다면 고민할필요없이 다 false
            {
                for(int i=0;i<6;i++)
                {
                    stoneAvailableDatas[i] = false;
                }
            }
        }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="isFloor">바닥에 있는 셀인지 체크</param>
        /// <param name="curIndex_">이 셀 정보의 인덱스</param>
        /// <param name="leftDownCellData_">이 셀의 왼쪽 아래의 셀 정보</param>
        /// <param name="rightDownCellData_">이 셀의 오른쪽 아래의 셀 정보</param>
        public BoardCellInfo(int curIndex_,bool isFloor = false ,BoardCellInfo leftDownCellData_ = null, BoardCellInfo rightDownCellData_ = null)
        {
            curIndex = curIndex_;

            leftDownCellData = leftDownCellData_;
            rightDownCellData = rightDownCellData_;

            isFloorCell = isFloor;

            stoneAvailableDatas = new ObservableCollection<bool>();
            for(int i=0;i<6;i++)
            {
                stoneAvailableDatas.Add(isFloor);
            }

            curStone = Stones.None;
            owner = Players.None;

            cellEmphasize = false;
            cellEmphasizeHover = false;
            stoneEmphasize = false;

            estimatedStone = Stones.None;
            cellEstimation = false;
        }

    }
}
