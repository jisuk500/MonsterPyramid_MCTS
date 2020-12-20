using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MonsterPyramid.Model;
using MonsterPyramid.Command;
using System.Runtime.InteropServices;

using MonsterPyramid.Model.MCTS;
using System.Windows;
using System.ComponentModel;
using System.Windows.Threading;

namespace MonsterPyramid.ViewModel.MainWindow
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {
        

        /// <summary>
        /// 커맨드 관련 일괄 세팅 함수
        /// </summary>
        private void InitializeCommands()
        {
            //보드에 돌 올려놓는거 관련
            PlaceStoneOnBoard_Pepe = new DelegateCommand(PlaceStoneOnBoard_Pepe_Execute, PlaceStoneOnBoard_Pepe_CanExecute);
            PlaceStoneOnBoard_Pink = new DelegateCommand(PlaceStoneOnBoard_Pink_Execute, PlaceStoneOnBoard_Pink_CanExecute);
            PlaceStoneOnBoard_Slime = new DelegateCommand(PlaceStoneOnBoard_Slime_Execute, PlaceStoneOnBoard_Slime_CanExecute);
            PlaceStoneOnBoard_Octo = new DelegateCommand(PlaceStoneOnBoard_Octo_Execute, PlaceStoneOnBoard_Octo_CanExecute);
            PlaceStoneOnBoard_Mush = new DelegateCommand(PlaceStoneOnBoard_Mush_Execute, PlaceStoneOnBoard_Mush_CanExecute);
            PlaceStoneOnBoard_Special = new DelegateCommand(PlaceStoneOnBoard_Special_Execute, PlaceStoneOnBoard_Special_CanExecute);
            PlaceStoneOnBoard_Skipped = new DelegateCommand(PlaceStoneOnBoard_Skipped_Execute, PlaceStoneOnBoard_Skipped_CanExecute);

            //버튼들 관련
            GameStartButtonCommand = new DelegateCommand(GameStartButtonCommand_Execute, GameStartButtonCommand_CanExecute);
            GameNextPhaseCommand = new DelegateCommand(GameNextPhaseCommand_Execute, GameNextPhaseCommand_CanExecute);
            GameResetCommand = new DelegateCommand(GameResetCommand_Execute, GameResetCommand_CanExecute);
            ExpectOptimizedStone = new DelegateCommand(ExpectOptimizedStone_Execute, ExpectOptimizedStone_CanExecute);
        }

        

        //보드에 돌 올려놓는거 관련 커맨드들
        /// <summary>
        /// 보드에 돌 올려놓는거 통합관리
        /// </summary>
        /// <param name="posIndex">해당 포지션</param>
        /// <param name="placedStone">놓은 돌</param>
        private void PlaceStoneOnBoard(int posIndex, Stones placedStone)
        {
            
            //돌 놓는 행동, 이때 만약 다음 플레이어가  me가 아니라면 - 각 칸의 강조점 변경해야함
            if(curGameSession.act(posIndex, placedStone) != Players.Me)
            {
                boardCellAvailablesEmphasize();
            }//다음 플레이어가 me라면 - 각 칸의 강조점을 전부 리셋
            else
            {
                boardCellClearAllEmphasize();
                boardCellClearAllEstimationEmphasize();
            }
        }
        /// <summary>
        /// 보드에 돌 올려놓는거 통합관리 can execute
        /// </summary>
        /// <param name="posIndex">해당 포지션</param>
        /// <param name="placeStone">놓은 돌</param>
        /// <returns>해당 위치에 돌을 놓을 수 있으면 true</returns>
        private bool PlaceStoneOnBoard_CanExecute(int posIndex, Stones placeStone)
        {
            if ((placeStone != Stones.None) && (placeStone != Stones.Skipped))
            {
                //만약 내 차례가 아니라면
                if (curGameSession.isMyTurn == false)
                {
                    //만약 전체 돌의 남은 개수가 충분하지 않으면, false
                    if (curGameSession.curStonesLeftInfo[(int)placeStone].count >= 1)
                    {
                        //만약 해당 위치에 해당 돌을 놓을 수 있으면 true
                        if (curGameSession.curBoardData.BoardCellState[posIndex].stoneAvailableDatas[(int)placeStone] == true)
                        {
                            return true;
                        }
                    }
                }//만약 내 차례라면
                else
                {
                    //내 소유의 돌 중에서 남은 개수가 충분하면 true
                    bool wasExist = false;
                    foreach(var info in curGameSession.myStones)
                    {
                        if(info.stone == placeStone)
                        {
                            wasExist = true;
                            break;
                        }
                    }

                    if(wasExist)
                    {
                        if (curGameSession.curBoardData.BoardCellState[posIndex].stoneAvailableDatas[(int)placeStone] == true)
                        {
                            return true;
                        }
                    }
                    
                }

            }
            else if(placeStone == Stones.Skipped)
            {
                return true;
            }

            return false;

        }

        //보드에 돌 올려놓음 페페
        public DelegateCommand PlaceStoneOnBoard_Pepe { get; set; }
        private void PlaceStoneOnBoard_Pepe_Execute(object obj)
        {
            if (obj != null)
            {
                PlaceStoneOnBoard(Int32.Parse((string)obj), Stones.Pepe);
            }
        }
        private bool PlaceStoneOnBoard_Pepe_CanExecute(object obj)
        {
            if (obj != null)
            {
                return PlaceStoneOnBoard_CanExecute(Int32.Parse((string)obj), Stones.Pepe);
            }
            return false;
        }
        //보드에 돌 올려놓음 핑크빈
        public DelegateCommand PlaceStoneOnBoard_Pink { get; set; }
        private void PlaceStoneOnBoard_Pink_Execute(object obj)
        {
            PlaceStoneOnBoard(Int32.Parse((string)obj), Stones.Pink);
        }
        private bool PlaceStoneOnBoard_Pink_CanExecute(object obj)
        {
            if (obj != null)
            {
                return PlaceStoneOnBoard_CanExecute(Int32.Parse((string)obj), Stones.Pink);
            }
            return false;
        }
        //보드에 돌 올려놓음 슬라임
        public DelegateCommand PlaceStoneOnBoard_Slime { get; set; }
        private void PlaceStoneOnBoard_Slime_Execute(object obj)
        {
            PlaceStoneOnBoard(Int32.Parse((string)obj), Stones.Slime);
        }
        private bool PlaceStoneOnBoard_Slime_CanExecute(object obj)
        {
            if (obj != null)
            {
                return PlaceStoneOnBoard_CanExecute(Int32.Parse((string)obj), Stones.Slime);
            }
            return false;
        }
        //보드에 돌 올려놓음 옥토푸스
        public DelegateCommand PlaceStoneOnBoard_Octo { get; set; }
        private void PlaceStoneOnBoard_Octo_Execute(object obj)
        {
            PlaceStoneOnBoard(Int32.Parse((string)obj), Stones.Octo);
        }
        private bool PlaceStoneOnBoard_Octo_CanExecute(object obj)
        {
            if (obj != null)
            {
                return PlaceStoneOnBoard_CanExecute(Int32.Parse((string)obj), Stones.Octo);
            }
            return false;
        }
        //보드에 돌 올려놓음 주황버섯
        public DelegateCommand PlaceStoneOnBoard_Mush { get; set; }
        private void PlaceStoneOnBoard_Mush_Execute(object obj)
        {
            PlaceStoneOnBoard(Int32.Parse((string)obj), Stones.Mush);
        }
        private bool PlaceStoneOnBoard_Mush_CanExecute(object obj)
        {
            if (obj != null)
            {
                return PlaceStoneOnBoard_CanExecute(Int32.Parse((string)obj), Stones.Mush);
            }
            return false;
        }
        //보드에 돌 올려놓음 스페셜
        public DelegateCommand PlaceStoneOnBoard_Special { get; set; }
        private void PlaceStoneOnBoard_Special_Execute(object obj)
        {
            PlaceStoneOnBoard(Int32.Parse((string)obj), Stones.Special);
        }
        private bool PlaceStoneOnBoard_Special_CanExecute(object obj)
        {
            if (obj != null)
            {
                return PlaceStoneOnBoard_CanExecute(Int32.Parse((string)obj), Stones.Special);
            }
            return false;
        }

        //보드에 돌 올려놓음 - 차례 스킵됨
        public DelegateCommand PlaceStoneOnBoard_Skipped { get; set; }
        private void PlaceStoneOnBoard_Skipped_Execute(object obj)
        {
            PlaceStoneOnBoard(Int32.Parse((string)obj), Stones.Skipped);
        }
        private bool PlaceStoneOnBoard_Skipped_CanExecute(object obj)
        {
            if (obj != null)
            {
                return PlaceStoneOnBoard_CanExecute(Int32.Parse((string)obj), Stones.Skipped);
            }
            return false;
        }

        //게임 스타트 버튼 커맨드
        public DelegateCommand GameStartButtonCommand { get; set; }
        private void GameStartButtonCommand_Execute(object obj)
        {
            if(curGameSession.tryGameStart())
            {
                //만약 게임 시작의 맨 처음이 내가 아니라면
                if(curGameSession.isMyTurn == false)
                {
                    //가능한 보드 셀들 강조
                    boardCellAvailablesEmphasize();
                }
            }
            else
            {

            }
        }
        private bool GameStartButtonCommand_CanExecute(object obj)
        {
            if (curGameSession.gameReadyState == ReadyState.GameReady)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //게임 다음 페이즈 버튼 커맨드
        public DelegateCommand GameNextPhaseCommand { get; set; }
        private void GameNextPhaseCommand_Execute(object obj)
        {
            curGameSession.nextPhase();
        }
        private bool GameNextPhaseCommand_CanExecute(object obj)
        {
            return true;
        }

        //게임 리셋 커맨드
        public DelegateCommand GameResetCommand { get; set; }
        private void GameResetCommand_Execute(object obj)
        {
            GameNum++;
            curSelectedMyStone = Stones.None;
            curGameSession = new GameSession(GameNum);
        }
        private bool GameResetCommand_CanExecute(object obj)
        {
            return true;
        }

        //다음 자신의 최적 수 예측 커맨드
        public DelegateCommand ExpectOptimizedStone { get; set; }
        private void ExpectOptimizedStone_Execute(object obj)
        {
            MCTS.makeInitialNodeFromGameSession(curGameSession);

            progressbarText = "다음 수 계산중..";
            progressbarProcessing = true;

            MCTSBackgrounWorker.RunWorkerAsync();
        }
        private bool ExpectOptimizedStone_CanExecute(object obj)
        {
            if (curGameSession.isMyTurn) return true;

            return false;
        }



        //-----------돌 계산을 위한 백그라운드 워커 쓰레드 이벤트
        private void MCTSBackgrounWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            /**
            MessageBox.Show("현재 상태 총 시뮬레이션 횟수 : " + MCTSResult.masterNodeTotalCount.ToString() + "\n"
                + "현재 상태 총 승률 : " + MCTSResult.masterNodeTotalWin.ToString() + "\n"
                + "다음 상태 총 시뮬레이션 횟수 : " + MCTSResult.resultNodeTotalCount.ToString() + "\n"
                + "다음 상태 총 승률 : " + MCTSResult.resultNodeTotalWin.ToString() + "\n"
                + "최대 탐색 트리 깊이 : " + MCTSResult.maximumDepth.ToString() + "\n"
                + "추정된 돌 : " + MCTSResult.estimatedPosition.stone.ToString() + "\n"
                + "추정된 셀 위치 : " + MCTSResult.estimatedPosition.position.ToString() + "\n", "예측된 것");
            **/
            boardCellStoneEstimationEmphasize();

            float currentProb = MCTSResult.masterNodeTotalWin / MCTSResult.masterNodeTotalCount;
            float nextProb = MCTSResult.resultNodeTotalWin / MCTSResult.resultNodeTotalCount;
            progressbarText = "승률:" + currentProb.ToString("F2") + "->" + nextProb.ToString("F2");
            progressbarProcessing = false;
        }

        private void MCTSBackgrounWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            MCTSResult = MCTS.doEstimation(4500);
        }
    }
}
