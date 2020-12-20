using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MonsterPyramid.Command;
using MonsterPyramid.Model;
using MonsterPyramid.Model.MCTS;

namespace MonsterPyramid.ViewModel.MainWindow
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 현재 게임 넘버
        /// </summary>
        private int GameNum = 1;
        /// <summary>
        /// 현재 게임 세션
        /// </summary>
        public GameSession curGameSession { get; set; }
        /// <summary>
        /// 현재 놓으려고 선택중인 내 돌
        /// </summary>
        public Stones curSelectedMyStone { get; set; }

        /// <summary>
        /// MCTS AI의 결과
        /// </summary>
        public EstimationResult MCTSResult { get; set; }
        /// <summary>
        /// MCTS 계산용 백그라운드워커
        /// </summary>
        private BackgroundWorker MCTSBackgrounWorker {get;set; }

        public string progressbarText { get; set; }
        public bool progressbarProcessing { get; set; }

        public MainWindowViewModel()
        {
            curGameSession = new GameSession(GameNum);
            curSelectedMyStone = Stones.None;
            progressbarProcessing = false;
            progressbarText = "초기화 완료";
            MCTSBackgrounWorker = new BackgroundWorker();
            MCTSBackgrounWorker.DoWork += MCTSBackgrounWorker_DoWork;
            MCTSBackgrounWorker.RunWorkerCompleted += MCTSBackgrounWorker_RunWorkerCompleted;

            InitializeCommands();

            
        }

        

        //----기본 함수들


        //----기타 조종 함수들
        /// <summary>
        /// 게임 로그 포커스를 전부 없애는 함수
        /// </summary>
        public void gamelogFocusReset()
        {
            foreach (var a in curGameSession.playerActLogs)
            {
                a.isSelected = false;
            }
        }

        /// <summary>
        /// 게임보드 셀 마우스 올리기
        /// </summary>
        /// <param name="pos">해당 셀 번호</param>
        /// <returns>마우스 포인터를 바꿔야 할 경우 true</returns>
        public bool gameboardCellMouseEnter(int pos)
        {

            var cell = curGameSession.curBoardData.BoardCellState[pos];
            bool wasLog = false;
            bool wasCellEmphasize = false;
            //해당 위치에 대한 로그가 있을 경우, 그 로그 강조
            try
            {
                PlayerActLog log = curGameSession.playerActLogs.Single(x => x.refBoardCellInfo == cell);
                log.isSelected = true;
                wasLog = true;
            }
            catch { }

            //해당 자리가 emphasize가 되어있는 경우, 호버링 강조까지 킴
            if (cell.cellEmphasize == true)
            {
                cell.cellEmphasizeHover = true;
                wasCellEmphasize = true;
            }

            //로그가 있었거나, 해당 위치에 셀 강조가 있었을 경우, 마우스 커서 바꾸기 
            if (wasLog || wasCellEmphasize)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        /// <summary>
        /// 게임보드 셀 마우스 내리기
        /// </summary>
        /// <param name="pos">해당 셀 번호</param>
        /// <returns>마우스 커서를 바꿔야 하는지</returns>
        public bool gameboardCellMouseLeave(int pos)
        {
            var cell = curGameSession.curBoardData.BoardCellState[pos];
            bool wasLog = false;
            bool wasCellEmphasize = false;
            //해당 위치에 로그가 있었을 경우, 그 로그 강조 해제
            try
            {
                var log = curGameSession.playerActLogs.Single(x => x.refBoardCellInfo == cell);
                log.isSelected = false;
                wasLog = true;
            }
            catch { }

            //해당 위치에 cell emphasized가 있었을 경우, 추가 강조된거 제거
            if (cell.cellEmphasize == true)
            {
                cell.cellEmphasizeHover = false;
                wasCellEmphasize = true;
            }

            //로그 또는 해당 셀 강조 둘중 하나라도 처리했으면, 마우스 커서 변경
            if (wasLog || wasCellEmphasize)
            {
                return true;
            }
            else
            {
                return false;
            }


        }
        /// <summary>
        /// 게임보드 셀 마우스 좌클릭
        /// </summary>
        /// <param name="pos">해당 셀 번호</param>
        /// <returns>만약 해당 위치에 돌을 내가 놓았으면, 커서 변경</returns>
        public bool gameboardCellMouseLeftButtonClick(int pos)
        {
            var cell = curGameSession.curBoardData.BoardCellState[pos];
            //해당 셀이 강조에다가, 마우스 호버링 추가 강조까지 되었다면
            if ((cell.cellEmphasize == true) && (cell.cellEmphasizeHover == true))
            {
                //내 차례라면 - 선택한 돌을 놓는다.
                if (curGameSession.isMyTurn)
                {
                    //해당 칸에 돌 추가
                    curGameSession.act(pos, curSelectedMyStone);

                    //기존 강조 정보 리셋
                    cell.cellEmphasizeHover = false;
                    boardCellEmphasize(curSelectedMyStone, false);
                    boardCellClearAllEstimationEmphasize();

                    //새로운 강조 정보
                    boardCellAvailablesEmphasize();


                    curSelectedMyStone = Stones.None;
                    return true;
                }//내 차례가 아니라면, Context메뉴를 열어준다.
                else
                {

                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 처음 플레이어 위치를 지정하는데 마우스 엔터 이벤트
        /// </summary>
        public bool playerBorderSeletingMouseEnter(int pos)
        {
            if (curGameSession.myPositionSet == false)
            {
                curGameSession.playerDatas[pos].isSelected = true;
                return true;
            }

            return false;
        }
        /// <summary>
        /// 처음 플레이어 위치를 지정하는데 마우스 리브 이벤트a
        /// </summary>
        /// <param name="pos"></param>
        public bool playerBorderSeletingMouseLeave(int pos)
        {
            if (curGameSession.myPositionSet == false)
            {
                curGameSession.playerDatas[pos].isSelected = false;
                return true;
            }

            return false;
        }
        /// <summary>
        /// 처음 플레이어 위치를 지정하는데 마우스 좌클릭 이벤트
        /// </summary>
        /// <param name="pos"></param>
        public bool playerSetInitialPos(int pos)
        {
            if (curGameSession.gameReadyState == ReadyState.SelectPlayerPos)
            {
                curGameSession.selectInitialMePlayerPos(pos);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 처음 플레이어 초기 말 정할때 마우스 엔터 함수
        /// </summary>
        /// <param name="stone_"></param>
        /// <returns></returns>
        public bool addInitialStone_mouseEnter(Stones stone_)
        {
            if ((curGameSession.gameReadyState == ReadyState.SetInitialStones) || (curGameSession.gameReadyState == ReadyState.GameReady))
            {
                curGameSession.curStonesLeftInfo[(int)stone_].emphasized = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 처음 플레이어 초기 말 정할때 마우스 리브 함수
        /// </summary>
        /// <param name="stone_"></param>
        /// <returns></returns>
        public bool addInitialStone_mouseLeave(Stones stone_)
        {
            if ((curGameSession.gameReadyState == ReadyState.SetInitialStones) || (curGameSession.gameReadyState == ReadyState.GameReady))
            {
                curGameSession.curStonesLeftInfo[(int)stone_].emphasized = false;
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 처음 플레이어의 초기 돌을 더하는 함수
        /// </summary>
        /// <param name="stone_"></param>
        /// <returns></returns>
        public bool addInitialStone(Stones stone_)
        {
            if (curGameSession.gameReadyState == ReadyState.SetInitialStones)
            {
                curGameSession.addInitialStone(stone_);
                return false;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 처음 플레이어의 초기 돌을 빼는 함수
        /// </summary>
        /// <param name="stone_"></param>
        /// <returns></returns>
        public bool removeInitialStone(Stones stone_)
        {
            if ((curGameSession.gameReadyState == ReadyState.SetInitialStones) || (curGameSession.gameReadyState == ReadyState.GameReady))
            {
                curGameSession.removeIntialStone(stone_);
                return false;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 내 스톤 목록의 특정한 스톤에 마우스가 호버링 됐을때, emphasized, seleted 항목 제어
        /// </summary>
        /// <param name="stone_">마우스를 올린 stone</param>
        /// <returns>emphasized 변화 있음</returns>
        public bool myStoneMouseEnter(Stones stone_)
        {
            //게임 상태가 아직 플레이어 위치 고르는 단계일땐, 아예 아무것도 안함.
            if (curGameSession.gameReadyState == ReadyState.SelectPlayerPos) return false;

            //게임 상태가 설정중이라면, 스톤 강조만 해줌
            if (curGameSession.gameReadyState != ReadyState.Gaming)
            {
                myStonesEmphasize(stone_, true);
                return true;
            }


            //내 차례일때
            if (curGameSession.isMyTurn == true)
            {
                //기존에 선택중이었던 돌이 없다면 - emphasized만 키면 됨
                if (curSelectedMyStone == Stones.None)
                {
                    myStonesEmphasize(stone_, true);
                    return true;
                }
                
                //기존에 선택중이었던 돌이랑 마우스 호버링 돌이랑 같다면 - 현상유지
                if (curSelectedMyStone == stone_)
                {
                    return true;
                }
                
                //기존에 선택중이었던 돌이랑 마우스 호버링 돌이 다르다면 - emphasized만 갱신
                myStonesEmphasize(stone_, true);
                return true;
            }

            return true;

        }
        /// <summary>
        /// 내 스톤 목록의 특정한 스톤에 마우스가 호버링하다가 나갔을때, emphasized항목 끄기
        /// </summary>
        /// <param name="stone_"></param>
        /// <returns></returns>
        public bool myStoneMouseLeave(Stones stone_)
        {
            //gamestate 검사해서 플레이어 위치 정하는 단계면 반응 없음
            if (curGameSession.gameReadyState == ReadyState.SelectPlayerPos) return false;

            // 만약 gaming 상태면, 내 차례인지 봐야함
            if (curGameSession.gameReadyState == ReadyState.Gaming)
            {
                //만약 내 차례라면
                if (curGameSession.isMyTurn == true)
                {
                    //기존의 선택했던 돌이 없었다면, emphasized,selected를 다 없애면 없애면 된다.
                    if (curSelectedMyStone == Stones.None)
                    {
                        myStonesEmphasize(stone_, false);
                        myStonesSelected(stone_, false);
                    }//기존의 선택했던 돌이 있는데, 지금 선택한 돌과 다르다면, selected 효과는 유지하면서, emphasized효과를 전부 없앤다
                    else if (curSelectedMyStone != stone_)
                    {
                        myStonesEmphasize(stone_, false);
                    }//기존에 선택한 돌을 또 호버링 했다면, 아무것도 안해도 된다.
                    else
                    {

                    }
                    return true;
                }

                return false;
            }

            //나머지 상태일 경우, 일단 emphasized 효과 끄기
            myStonesEmphasize(stone_, false);
            return false;


        }
        /// <summary>
        /// 내 스톤 목록의 특정한 스톤에 마우스를 클릭했을때, gamestate에 따라서 다르게 반응하기
        /// </summary>
        /// <param name="stone_"></param>
        /// <returns>주변에 effet를 내야 하는가?</returns>
        public bool myStoneMouseLeftButtonClick(Stones stone_)
        {
            //만약 게이밍 상태라면
            if (curGameSession.gameReadyState == ReadyState.Gaming)
            {
                //만약 내 차례라면 - 놓기로 선택중인 돌을 변경, 해당 돌을 놓을 수 있는 곳을 강조.
                if (curGameSession.isMyTurn == true)
                {
                    //이전에 선택중이었던 돌이랑 지금 새로 선택한 돌이랑 안맞는다면? - 새롭게 내 돌 강조 초기화
                    if (curSelectedMyStone != stone_)
                    {
                        myStonesSelected(stone_, true);
                    }

                    curSelectedMyStone = stone_;
                    boardCellEmphasize(stone_, true);
                    return true;

                }
                else
                {
                    return false;
                }
            }// 게이밍 상태는 아니지만, 초기 소지 돌 설정할때 클릭하는것은 해당 돌을 빼는 것으로 간주
            else if ((curGameSession.gameReadyState == ReadyState.SetInitialStones) || (curGameSession.gameReadyState == ReadyState.GameReady))
            {
                removeInitialStone(stone_);
            }
            return true;
        }


        //--------------- 강조 및 선택 효과 관련 refresh
        /// <summary>
        /// 내 돌 목록에서 특정 돌을 강조하는 함수
        /// </summary>
        /// <param name="stone_"></param>
        private bool myStonesEmphasize(Stones stone_, bool isEmphasize)
        {
            if (stone_ == Stones.None) return false;

            foreach (var info in curGameSession.myStones)
            {
                if (info.stone == stone_)
                {
                    info.emphasized = isEmphasize;
                }
                else
                {
                    info.emphasized = false;
                }
            }
            return true;
        }
        /// <summary>
        /// 내 돌 목록에서 특정 돌을 선택했다고 하는 함수
        /// </summary>
        /// <param name="stone_">선택한 돌</param>
        /// <param name="isSelect">어떻게 바꿀지 </param>
        private void myStonesSelected(Stones stone_, bool isSelect)
        {
            foreach (var info in curGameSession.myStones)
            {
                if (info.stone == stone_)
                {
                    info.selected = isSelect;
                }
                else
                {
                    info.selected = false;
                }
            }
        }
        /// <summary>
        /// 보드 셀에서 특정 스톤을 놓을 수 있는 모든 위치의 cell emphasize항목 제어
        /// </summary>
        /// <param name="stone_"></param>
        /// <param name="emphasize_"></param>
        private void boardCellEmphasize(Stones stone_, bool emphasize_)
        {
            foreach (var cell in curGameSession.curBoardData.BoardCellState)
            {
                if (cell.stoneAvailableDatas[(int)stone_] == true)
                {
                    cell.cellEmphasize = emphasize_;
                }
                else
                {
                    cell.cellEmphasize = false;
                }
            }
        }
        /// <summary>
        /// 모든 보드 셀의 emphasize를 제거
        /// </summary>
        private void boardCellClearAllEmphasize()
        {
            foreach (var cell in curGameSession.curBoardData.BoardCellState)
            {
                cell.cellEmphasize = false;
                cell.cellEmphasizeHover = false;
            }
        }
        /// <summary>
        /// 보드 셀에서 아무 돌이나 놓을 수 있는 모든 위치를 emphasize하기
        /// </summary>
        private void boardCellAvailablesEmphasize()
        {
            bool isAvailabled = false;
            foreach (var cell in curGameSession.curBoardData.BoardCellState)
            {
                isAvailabled = false;
                for (int i = 0; i < 6; i++)
                {
                    if (cell.stoneAvailableDatas[i])
                    {
                        isAvailabled = true;
                    }
                }

                if (isAvailabled)
                {
                    cell.cellEmphasize = true;
                }
                else
                {
                    cell.cellEmphasize = false;
                }
            }
        }
        
        /// <summary>
        /// 보드 셀에서 시뮬레이션이 완료된 다음 돌 추정 표시 등록
        /// </summary>
        private void boardCellStoneEstimationEmphasize()
        {
            if(MCTSResult.estimatedPosition.position<=35)
            {
                curGameSession.curBoardData.BoardCellState[MCTSResult.estimatedPosition.position].estimatedStone
                = (Stones)MCTSResult.estimatedPosition.stone;
                curGameSession.curBoardData.BoardCellState[MCTSResult.estimatedPosition.position].cellEstimation = true;
            }
            
        }

        /// <summary>
        /// 보드 셀에서 모든 다음 돌 추정 표시 제거
        /// </summary>
        private void boardCellClearAllEstimationEmphasize()
        {
            foreach(var cell in curGameSession.curBoardData.BoardCellState)
            {
                cell.estimatedStone = Stones.None;
                cell.cellEstimation = false;
            }
        }


    }
}
