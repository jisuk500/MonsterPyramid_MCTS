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
    /// 현재 게임의 데이터
    /// </summary>
    public class GameSession : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 현재 게임 번호
        /// </summary>
        public int gameNum { get; private set; }
        /// <summary>
        /// 현재 3번중 몇번째 페이즈인지
        /// </summary>
        public int curPhase { get; private set; }
        /// <summary>
        /// 현재 플레이어가 어디칸에 있는 플레이어인지
        /// </summary>
        public int curPlayerIndex { get; private set; }

        /// <summary>
        /// 현재 플레이어 정보. 각 칸의 위치에 맞는 원소3개 컬렉션
        /// </summary>
        public ObservableCollection<PlayerData> playerDatas { get; set; }
        /// <summary>
        /// 현재 보드의 데이터
        /// </summary>
        public BoardData curBoardData { get; set; }
        /// <summary>
        /// 현재 나오지 않은 말의 개수 표시
        /// </summary>
        public ObservableCollection<StoneCount> curStonesLeftInfo { get; set; }
        /// <summary>
        /// 플레이어들의 행동 로그
        /// </summary>
        public ObservableCollection<PlayerActLog> playerActLogs { get; set; }
        /// <summary>
        /// 리더보드 점수 데이터들
        /// </summary>
        public ObservableCollection<LeaderBoardData> LeaderBoardDatas { get; set; }
        /// <summary>
        /// 현재 내가 보유한 돌
        /// </summary>
        public ObservableCollection<StoneCount> myStones { get; set; }

        /// <summary>
        /// 이 게임 세션의 전체의 로그
        /// </summary>
        public GameLog curGameLog { get; set; }

        /// <summary>
        /// 이 게임의 현재 준비 단계 상태
        /// </summary>
        public ReadyState gameReadyState { get; private set; }
        /// <summary>
        /// 이 게임세션에서 내 차례 위치가 정해졌는가
        /// </summary>
        public bool myPositionSet
        {
            get { return myPositionSet_internal; }
            set
            {
                myPositionSet_internal = value;
                if (value == true)
                {
                    gameReadyState = ReadyState.SetInitialStones;
                }
            }
        }
        private bool myPositionSet_internal;
        /// <summary>
        /// 이 게임세션에서 내 돌의 초기상태가 정해졌는가
        /// </summary>
        public bool myStonesSet
        {
            get { return myStonesSet_internal; }
            set
            {
                myStonesSet_internal = value;
                if ((value == true) && (gameReadyState == ReadyState.SetInitialStones))
                {
                    gameAble = true;
                }
                else if (value == false)
                {
                    gameAble = false;
                }
            }
        }
        private bool myStonesSet_internal;
       
        
        /// <summary>
        /// 이 게임세션의 게임을 시작할 수 있는가
        /// </summary>
        public bool gameAble 
        {
            get { return gameAble_internal; }
            set
            {
                gameAble_internal = value;
                if((value == true)&&(gameReadyState == ReadyState.SetInitialStones))
                {
                    gameReadyState = ReadyState.GameReady;
                }
                else if ((value == false) && (gameReadyState == ReadyState.GameReady))
                {
                    gameReadyState = ReadyState.SetInitialStones;
                }
            }
        }
        private bool gameAble_internal;

        /// <summary>
        /// 지금이 내 차례인지
        /// </summary>
        public bool isMyTurn { get; private set; }

        //------------- 초기상태 세팅 관련
        /// <summary>
        /// 초기 내 플레이어 포지션을 선택하는 함수
        /// </summary>
        /// <param name="pos">0~2로 결정</param>
        public void selectInitialMePlayerPos(int pos)
        {
            int MePassed = 0;
            for(int i=0;i<3;i++)
            {
                if(i == pos)
                {
                    playerDatas[pos].player = Players.Me;
                    playerDatas[pos].isSelected = false;
                    MePassed++;
                }
                else
                {
                    playerDatas[i].player = (Players)(i - MePassed);
                }
            }

            myPositionSet = true;
        }

        /// <summary>
        /// 내 초기상태 돌 추가 함수
        /// </summary>
        /// <param name="selectedStone_">추가할 돌 정보</param>
        public bool addInitialStone(Stones selectedStone_)
        {
            PlayerData MeData;
            if(findSpecificPlayer(out MeData,Players.Me))
            {
                int total = 0;
                foreach(var stonecount in MeData.StonePossesionInfo)
                {
                    total += stonecount.count;
                }

                if((total<=11)&&(curStonesLeftInfo[(int)selectedStone_].count>=1))
                {
                    MeData.StonePossesionInfo[(int)selectedStone_].count += 1;
                    curStonesLeftInfo[(int)selectedStone_].count -= 1;
                    syncMyStonePossesionInfo();

                    if(total==11)
                    {
                        myStonesSet = true;
                    }
                    return true;
                }
            }
            myStonesSet = false;
            return false;
        }
        /// <summary>
        /// 내 초기상태 돌 제거 함수
        /// </summary>
        /// <param name="selectedStone_">선택한 돌</param>
        /// <returns>제거 성공했는가</returns>
        public bool removeIntialStone(Stones selectedStone_)
        {
            PlayerData MeData;
            if (findSpecificPlayer(out MeData,Players.Me))
            {
                if(MeData.StonePossesionInfo[(int)selectedStone_].count>0)
                {
                    MeData.StonePossesionInfo[(int)selectedStone_].count -= 1;
                    curStonesLeftInfo[(int)selectedStone_].count += 1;
                    syncMyStonePossesionInfo();

                    myStonesSet = false;
                }
            }

            return false;

        }

        //------------ 게임 흐름 관련
        /// <summary>
        /// 현재 게임을 시작하는것을 시도
        /// </summary>
        /// <returns>시도 성공</returns>
        public bool tryGameStart()
        {
            if((myStonesSet == true)&&(myPositionSet==true)&&(gameReadyState == ReadyState.GameReady))
            {
                gameReadyState = ReadyState.Gaming;

                curPlayerIndex = 2;
                //마지막 플레이어에서 다음 플레이어로 넘어가도록 해서, 마치 처음부터 시작인것처럼 함
                nextPlayer();

                return true;
            }
            return false;
        }

        /// <summary>
        /// 다음 페이즈로 넘기는 함수
        /// </summary>
        /// <returns>3페이즈라서 게임이 끝날경우</returns>
        public bool nextPhase()
        {
            //현재 페이즈 저장
            saveGamePhaseLogs();

            //페이즈 번호 바꾸기
            if (curPhase == 3)
            {
                return false;
            }
            else
            {
                curPhase += 1;
            }
            //기존 로그 클리어
            playerActLogs.Clear();
            //플레이어들 돌 소유 상황 리셋
            resetStonePossesionInfos();
            syncMyStonePossesionInfo();
            //플레이어들 번호 바꾸기
            changeNextPhasePlayersOrder();
            //보드 셀 상황 리셋
            resetBoardCellDatas();
            //남은 돌 개수 리셋
            resetLeftStoneCounts();
            foreach(var data in playerDatas)
            {
                data.leftStoneCount = 12;
            }
            //다시 돌을 고르는 쪽으로 넘어감
            gameReadyState = ReadyState.GameReady;
            myStonesSet = false;

            return true;
        }

        /// <summary>
        /// 다음 플레이어로 넘기는 함수
        /// </summary>
        /// <returns>다음 플레이어</returns>
        public Players nextPlayer()
        {
            //이전의 플레이어 차례 표시 제거
            playerDatas[curPlayerIndex].nowTurn = false;

            //플레이어 인덱스 수정
            if (curPlayerIndex == 2)
            {
                curPlayerIndex = 0;
            }
            else
            {
                curPlayerIndex += 1;
            }
            //다음의 플레이어 차례 표시 
            playerDatas[curPlayerIndex].nowTurn = true;

            //지금 내 차례인지 표시
            if (playerDatas[curPlayerIndex].player == Players.Me)
            {
                isMyTurn = true;
            }
            else
            {
                isMyTurn = false;
            }

            return playerDatas[curPlayerIndex].player;

        }

        //----------- 게임 행동 관련
        /// <summary>
        /// 보드에 대한 행동 개시
        /// </summary>
        /// <param name="stonePos_">행동한 돌의 위치</param>
        /// <param name="stone_">놓아진 돌</param>
        /// <returns>다음 플레이어</returns>
        public Players act(int stonePos_, Stones stone_)
        {
            if ((stone_ != Stones.None)&&(stone_ != Stones.Skipped))
            {
                curBoardData.BoardCellState[stonePos_].curStone = stone_;
                curBoardData.BoardCellState[stonePos_].owner = playerDatas[curPlayerIndex].player;
                playerActLogs.Add(new PlayerActLog(playerActLogs.Count + 1, curBoardData.BoardCellState[stonePos_]));

                playerDatas[curPlayerIndex].leftStoneCount--;

                if (playerDatas[curPlayerIndex].player == Players.Me)
                {
                    playerDatas[curPlayerIndex].StonePossesionInfo[(int)stone_].count--;
                    syncMyStonePossesionInfo();
                }
                else
                {
                    curStonesLeftInfo[(int)stone_].count--;

                    if(playerDatas[curPlayerIndex].StonePossesionInfo[(int)stone_].count>=1)
                    {
                        playerDatas[curPlayerIndex].StonePossesionInfo[(int)stone_].count--;
                    }
                }

                addScoreRenewLeaderboard();
                curBoardData.refreshAllStoneAvailable();
                var nextplayer = nextPlayer();
                cutOffMaximumCountStonesAvailability(nextplayer);

                return nextplayer;
            }
            else if(stone_ == Stones.Skipped)
            {
                playerActLogs.Add(new PlayerActLog(playerActLogs.Count + 1,null,true,playerDatas[curPlayerIndex].player));

                //적의 스킵 행동에서 정보 추출
                if (playerDatas[curPlayerIndex].player != Players.Me)
                {
                    examineSkippingAction();
                }
                curBoardData.refreshAllStoneAvailable();
                var nextplayer = nextPlayer();
                cutOffMaximumCountStonesAvailability(nextplayer);

                return nextplayer;
            }
            else
            {
                return Players.None;
            }
                

            
        }

        //----------- 데이터 연동 및 데이터 처리 관련

        /// <summary>
        /// 특정 돌을 놓을때 점수 더하기, 및 스코어보드 갱신
        /// </summary>
        private void addScoreRenewLeaderboard()
        {
            playerDatas[curPlayerIndex].score += 10;

            LeaderBoardData temp;
            for(int i=0;i<3;i++)
            {
                for(int j=0;j<2;j++)
                {
                    if(LeaderBoardDatas[j].curPlayerData.score < LeaderBoardDatas[j+1].curPlayerData.score)
                    {
                        temp = LeaderBoardDatas[j + 1];
                        LeaderBoardDatas[j+1] = LeaderBoardDatas[j];
                        LeaderBoardDatas[j] = temp;
                    }
                }
            }
            
        }

        /// <summary>
        /// 내 돌 소유 정보를 아래 줄과도 동기화시킨다.
        /// </summary>
        private void syncMyStonePossesionInfo()
        {
            myStones.Clear();
            PlayerData curPlayerData;
            if (findSpecificPlayer(out curPlayerData,Players.Me))
            {
                foreach (var stoneinfo in curPlayerData.StonePossesionInfo)
                {
                    for (int i = 0; i < stoneinfo.count; i++)
                    {
                        myStones.Add(new StoneCount(stoneinfo.stone));
                    }
                }
            }

        }

        /// <summary>
        /// 돌이 필드에 나올만큼 나온거를 골라서 availability를 없앤다.
        /// </summary>
        /// <param name="player_">해당 상황을 적용할 player</param>
        private void cutOffMaximumCountStonesAvailability(Players player_)
        {
            bool[] availability = { true, true, true, true, true, true };

            if (player_ == Players.Me)
            {
                PlayerData meData;
                if(findSpecificPlayer(out meData,Players.Me))
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if ((curStonesLeftInfo[i].count + meData.StonePossesionInfo[i].count)<= 0)
                        {
                            availability[i] = false;
                        }
                    }
                }
                
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    if (curStonesLeftInfo[i].count <= 0)
                    {
                        availability[i] = false;
                    }
                }

                for(int i=0;i<6;i++)
                {
                    if(playerDatas[curPlayerIndex].StonePossesionInfo[i].count == -1)
                    {
                        availability[i] = false;
                    }
                }
            }

            foreach(var cell in curBoardData.BoardCellState)
            {
                for(int i=0;i<6;i++)
                {
                    cell.stoneAvailableDatas[i] = cell.stoneAvailableDatas[i] & availability[i];
                }
            }
        }

        /// <summary>
        /// Me 플레이어를 찾아오는 함수
        /// </summary>
        /// <param name="MePlayerData">내 플레이어 데이터로 나가질것</param>
        /// <returns>찾으면 true</returns>
        public bool findSpecificPlayer(out PlayerData MePlayerData,Players player)
        {
            if (player == Players.None)
            {
                MePlayerData = new PlayerData(Ranking.Rank1, Players.None, -1);
                return false;
            }
            

            try
            {
                MePlayerData = playerDatas.Single(x => x.player == player);
                return true;
            }
            catch
            {
                MePlayerData = new PlayerData(Ranking.Rank1, Players.None, -1);
                return false;
            }
        }

        /// <summary>
        /// 적이 스킵하는 행동에서 정보 추출
        /// </summary>
        /// <returns></returns>
        private bool examineSkippingAction()
        {
            //현재 적과 적2 찾아오기
            PlayerData curEnemy = playerDatas[curPlayerIndex];
            PlayerData oppositeEnemy = new PlayerData(Ranking.Rank1,Players.None);

            foreach(var data in playerDatas)
            {
                if((data.player!=Players.Me)&(data.player!=curEnemy.player))
                {
                    oppositeEnemy = data;
                    break;
                }
            }

            //현재 보드 판을 돌면서, 냈어야 하는 돌들 감지해서 그만큼 패널티
            foreach(var cell in curBoardData.BoardCellState)
            {
                for(int i=0;i<6;i++)
                {
                    if(cell.stoneAvailableDatas[i])
                    {
                        curEnemy.StonePossesionInfo[i].count = -1;
                        oppositeEnemy.StonePossesionInfo[i].count = curStonesLeftInfo[i].count;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 현재 게임 페이즈를 저장하는 함수
        /// </summary>
        /// <returns></returns>
        private bool saveGamePhaseLogs()
        {
            return false;
        }

        /// <summary>
        /// 다음 페이즈에 맞게 현재 플레이어들의 상태를 변경하는 함수
        /// </summary>
        /// <returns></returns>
        private bool changeNextPhasePlayersOrder()
        {
            playerDatas.Add(playerDatas[0]);
            playerDatas.RemoveAt(0);

            return true;
        }

        /// <summary>
        /// 모든 플레이어들의 돌 소유 상황을 리셋
        /// </summary>
        /// <returns></returns>
        private bool resetStonePossesionInfos()
        {
            foreach(var data in playerDatas)
            {
                for(int i=0;i<6;i++)
                {
                    data.StonePossesionInfo[i].count = 0;
                }
            }
            return true;
        }

        /// <summary>
        /// 모든 보드 셀의 돌 배치 상황을 리셋
        /// </summary>
        /// <returns></returns>
        private bool resetBoardCellDatas()
        {
            foreach(var cell in curBoardData.BoardCellState)
            {
                cell.curStone = Stones.None;
                cell.owner = Players.None;
                cell.cellEmphasize = false;
                cell.cellEmphasizeHover = false;
                cell.stoneEmphasize = false;
            }

            curBoardData.refreshAllStoneAvailable();

            return true;
        }

        /// <summary>
        /// 남은 돌 상황들을 리셋
        /// </summary>
        /// <returns></returns>
        private bool resetLeftStoneCounts()
        {
            for(int i=0;i<5;i++)
            {
                curStonesLeftInfo[i].count = 7;
            }
            curStonesLeftInfo[5].count = 1;

            return true;
        }



        /// <summary>
        /// 게임 데이터 생성자
        /// <paramref name="gameNum_">현재 게임 번호</paramref>
        /// </summary>
        public GameSession(int gameNum_)
        {
            gameNum = gameNum_;

            curPhase = 1;
            curPlayerIndex = 0;

            playerDatas = new ObservableCollection<PlayerData>();
            for (int i = 0; i < 3; i++)
            {
                playerDatas.Add(new PlayerData((Ranking)i, Players.None, 0));
            }

            curBoardData = new BoardData();

            curStonesLeftInfo = new ObservableCollection<StoneCount>();
            for (int i = 0; i < 6; i++)
            {
                if ((Stones)i != Stones.Special)
                {
                    curStonesLeftInfo.Add(new StoneCount((Stones)i, 7));
                }
                else
                {
                    curStonesLeftInfo.Add(new StoneCount((Stones)i, 1));
                }
            }

            playerActLogs = new ObservableCollection<PlayerActLog>();

            LeaderBoardDatas = new ObservableCollection<LeaderBoardData>();
            for (int i = 0; i < 3; i++)
            {
                LeaderBoardDatas.Add(new LeaderBoardData(playerDatas[i]));
            }

            myStones = new ObservableCollection<StoneCount>();

            gameReadyState = ReadyState.SelectPlayerPos;
            myStonesSet = false;
            myPositionSet = false;
            gameAble = false;

        }
    }
}
