using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MonsterPyramid.Model.MCTS
{
    /// <summary>
    /// MCST를 진행하는 정적 클래스
    /// </summary>
    public static class MCTS
    {
        /// <summary>a
        /// 현재 계산 중인지 나타내는 변수
        /// </summary>
        public static bool isCalculating = false;

        /// <summary>
        /// 인덱스 컨버터 테이블.[index][mode].0-downLeft/1-downRight/2-upLeft/3-upRight
        /// </summary>
        public static byte[,] indexConvertTable = new byte[,]
        {
            //1층
            {255,255,255,8  },//0
            {255,255,8  ,9  },//1
            {255,255,9  ,10 },//2
            {255,255,10 ,11 },//3
            {255,255,11 ,12 },//4
            {255,255,12 ,13 },//5
            {255,255,13 ,14 },//6
            {255,255,14 ,255},//7
            //2층
            {0  ,1  ,255,15 },//8
            {1  ,2  ,15 ,16 },//9
            {2  ,3  ,16 ,17 },//10
            {3  ,4  ,17 ,18 },//11
            {4  ,5  ,18 ,19 },//12
            {5  ,6  ,19 ,20 },//13
            {6  ,7  ,20 ,255},//14
            //3층
            {8  ,9  ,255,21 },//15
            {9  ,10 ,21 ,22 },//16
            {10 ,11 ,22 ,23 },//17
            {11 ,12 ,23 ,24 },//18
            {12 ,13 ,24 ,25 },//19
            {13 ,14 ,25 ,255},//20
            //4층
            {15 ,16 ,255,26 },//21
            {16 ,17 ,26 ,27 },//22
            {17 ,18 ,27 ,28 },//23
            {18 ,19 ,28 ,29 },//24
            {19 ,20 ,29 ,255},//25
            //5층
            {21 ,22 ,255,30 },//26
            {22 ,23 ,30 ,31 },//27
            {23 ,24 ,31 ,32 },//28
            {24 ,25 ,32 ,255},//29
            //6층
            {26 ,27 ,255,33 },//30
            {27 ,28 ,33 ,34 },//31
            {28 ,29 ,34 ,255},//32
            //7층
            {30 ,31 ,255,35 },//33
            {31 ,32 ,35 ,255},//34
            //8층
            {33 ,34 ,255,255},//35
        };
        /// <summary>
        /// UCT 파라미터
        /// </summary>
        public static float exploationParameter = 1.4142135623f; //root(2)
        /// <summary>
        /// 자식 노드를 생성하기에 필요한 최소 시뮬레이션 횟수 카운트
        /// </summary>
        public static int minimumChildrenSimulationCount = 100;
        /// <summary>
        /// 플레이어들의 배치 정보
        /// </summary>
        public static byte[] playerPosInfos = new byte[3]{0,0,0};
        /// <summary>
        /// 다음 노드들의 결과를 저장함
        /// </summary>
        public static List<NextPosition> NextPositions = new List<NextPosition>();

        /// <summary>
        /// 트리의 최대 깊이
        /// </summary>
        public static int maximumDepth = 0;

        /// <summary>
        /// 정적 랜더마이저 선언
        /// </summary>
        public static Random Randomizer = new Random();

        //---프라이빗 변수들

        private static List<MCTSNode> simulationList = new List<MCTSNode>();
        private static ConcurrentQueue<simResult> simulationResultQueue = new ConcurrentQueue<simResult>();
        private static int MaxMillisec = 4500;

        /// <summary>
        /// 맨 위 최상위 마스터 노드. 현재 게임판의 정보가 그대로 들어감
        /// </summary>
        private static MCTSNode MasterNode = new MCTSNode();

        

        //----------------퍼블릭 함수들

        /// <summary>
        /// 최대 계산시간 지정
        /// </summary>
        /// <param name="maxMillisec"></param>
        public static void setMaxMillsec(int maxMillisec)
        {
            MaxMillisec = maxMillisec;
        }

        /// <summary>
        /// 게임 세션으로부터 mcts node를 만드는 함수
        /// </summary>
        /// <param name="gamesession_">게임 세션</param>
        /// <returns>mcts 노드</returns>
        public static void makeInitialNodeFromGameSession(GameSession gamesession_)
        {
            for (int i = 0; i < 3; i++)
            {
                playerPosInfos[i] = (byte)gamesession_.playerDatas[i].player;
            }

            MCTSNode new_startNode = new MCTSNode();
            new_startNode.setNodeStateFromGameSession(gamesession_);
            MasterNode = new_startNode;
            
        }

        /// <summary>
        /// 하나의 MCTS 싸이클을 돌리는 함수
        /// <paramref name="maxMillisec">최대 계산 밀리초, -1이하시 자체 세팅값으로</paramref>
        /// </summary>
        public static EstimationResult doEstimation(int maxMillisec)
        {
            if (maxMillisec <= -1) maxMillisec = MaxMillisec;

            simulationList = new List<MCTSNode>();

            MCTSNode node;

            getMasterNodeReady();

            var startTime = DateTime.Now;
            TimeSpan elapsedTime;

            int i = 0;

            while (true)
            {
                node = exploration();

                expansion(node,ref simulationList);

                simualation(simulationList, simulationResultQueue);

                backpropagation(simulationResultQueue);

                elapsedTime = DateTime.Now - startTime;
                if (elapsedTime.TotalMilliseconds>= maxMillisec) break;
            }

            

            EstimationResult result = new EstimationResult();
            result.masterNodeTotalCount = MasterNode.simulateCount;
            result.masterNodeTotalWin = MasterNode.winCount;
            result.maximumDepth = maximumDepth;
            getEstimationResult(ref result);

            simulationList = new List<MCTSNode>();
            MasterNode = null;

            //가비지 콜렉션 강제 재생, 메모리 바로 확보

            GC.Collect();
            GC.WaitForFullGCComplete();

            return result;

        }

        //------------------프라이빗 메소드들

        /// <summary>
        /// 시뮬레이션을 진행할 노드 탐색
        /// </summary>
        private static MCTSNode exploration()
        {
            MCTSNode currentNode = MasterNode;
            MCTSNode nextNode;
            while(true)
            {
                nextNode = currentNode.selectBestFittingUCTChild();
                if(nextNode == null)
                {
                    break;
                }

                currentNode = nextNode;
            }

            return currentNode;
        }

        /// <summary>
        /// 해당 노드의 확장 여부 결정. 확장시, 모든 childNode에 대해서 simulation 단계 진행
        /// </summary>
        private static bool expansion(MCTSNode currentNode,ref List<MCTSNode> expandedChildrens)
        {

            if(currentNode.makeChildren() == false)
            {
                expandedChildrens = new List<MCTSNode>();
                expandedChildrens.Add(currentNode);
                return false;
            }

            expandedChildrens = currentNode.childNodes;
            return true;
        }

        /// <summary>
        /// 특정 노드의 시뮬레이션들을 진행하고, 그 승점 결과를 반환하는 단계
        /// </summary>
        private static void simualation(List<MCTSNode> simulationNodes, ConcurrentQueue<simResult> simulationResults)
        {

            clearConcurrentQueue<simResult>(simulationResults);

            Parallel.ForEach(simulationNodes, (node) =>
             {
                 simulationResults.Enqueue(new simResult(node, node.Simulate()));
                 if (maximumDepth < node.depth) maximumDepth = node.depth;
             });



        }

        /// <summary>
        /// 시뮬레이션된 승점 결과를 역전파하여 UCT 값들을 갱신하는 단계
        /// </summary>
        private static bool backpropagation(ConcurrentQueue<simResult> simulationResultQueue)
        {
            MCTSNode currentNode = simulationResultQueue.ElementAt(0).node;
            float winSum = 0;
            int totalSimAdd = simulationResultQueue.Count;
            simResult result = new simResult();

            while(true)
            {
                if (!simulationResultQueue.TryDequeue(out result)) break;

                result.node.addWinCount(result.resultWinCount);
                winSum += result.resultWinCount;
            }

            while (true)
            {
                currentNode = currentNode.parentNode;
                if (currentNode == null) break;
                currentNode.addWinCountWithSimulationCount(winSum, totalSimAdd);       
            }

            return true;
        }

        /// <summary>
        /// 마스터 노드의 칠드런과, 해당 칠드런들이 나타내는 다음 수를 미리 기록하는 메소드
        /// </summary>
        private static void getMasterNodeReady()
        {
            maximumDepth = 0;

            NextPositions.Clear();
            NextPositions = MasterNode.makeChildren_forMasterNode();

            simulationList = MasterNode.childNodes;
            clearConcurrentQueue<simResult>(simulationResultQueue);

            simualation(simulationList, simulationResultQueue);
            backpropagation(simulationResultQueue);

        }

        /// <summary>
        /// 다음 최적수가 무엇이었는지 반환하는 메소드
        /// </summary>
        /// <returns>최적수와 그 위치</returns>
        private static void getEstimationResult(ref EstimationResult estimationResult_)
        {
            var bestChilds = from child in MasterNode.childNodes
                             orderby child.simulateCount descending
                             select child;
            int bestIndex = MasterNode.childNodes.FindIndex(x => x == bestChilds.ElementAt(0));

            if (bestIndex >= 0)
            {
                estimationResult_.resultNodeTotalCount = MasterNode.childNodes[bestIndex].simulateCount;
                estimationResult_.resultNodeTotalWin = MasterNode.childNodes[bestIndex].winCount;
                estimationResult_.estimatedPosition = NextPositions[bestIndex];
            }
            else
            {
                estimationResult_.resultNodeTotalCount = 0;
                estimationResult_.resultNodeTotalWin = 0;
                estimationResult_.estimatedPosition = new NextPosition
                {
                    position = 0xFF,
                    stone = 6
                };
            }
        }


        /// <summary>
        /// 특정 컨커런트 큐를 클리어하는 함수. 자체 clear 메소드가 없기 때문이다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue"></param>
        private static void clearConcurrentQueue<T>(ConcurrentQueue<T> queue)
        {
            T temp;
            while(queue.TryDequeue(out temp))
            {

            }
        }

    }

    /// <summary>
    /// MCTS용 보드셀 정보
    /// </summary>
    public struct cellInfo
    {
        public byte cellNum { get; set; }
        public byte stone { get; set; }
    }

    /// <summary>
    /// MCTS용 리더보드 정보
    /// </summary>
    public struct leaderboard
    {
        public byte player { get; set; }
        public int score { get; set; }
    }

    /// <summary>
    /// MCTS용 적 데이터
    /// </summary>
    public struct EnemyData
    {
        public byte leftStoneCounts { get; set; }
        public byte[] stoneSpecifications { get; set; }
        public byte[] stonesInfo { get; set; }

        /// <summary>
        /// 초기화 생성자
        /// </summary>
        /// <param name="asdf">있으나업으나 ㅋㅋ</param>
        public EnemyData(bool asdf = false)
        {
            leftStoneCounts = 12;
            stoneSpecifications = new byte[] { 0, 0, 0, 0, 0, 0 };
            stonesInfo = new byte[] { 0, 0, 0, 0, 0, 0 };
        }

        /// <summary>
        /// 복사 생성자
        /// </summary>
        /// <param name="original">오리지널</param>
        public EnemyData(EnemyData original)
        {
            leftStoneCounts = original.leftStoneCounts;
            stoneSpecifications = new byte[6];
            original.stoneSpecifications.CopyTo(stoneSpecifications, 0);
            stonesInfo = new byte[6];
            original.stonesInfo.CopyTo(stonesInfo, 0);
        }
    }

    /// <summary>
    /// 다음 위치 후보들 구조체
    /// </summary>
    public struct NextPosition
    {
        public int stone;
        public int position;
    }

    /// <summary>
    /// 추정 결과에 대한 것을 정리하는 구조체
    /// </summary>
    public struct EstimationResult
    {
        public int masterNodeTotalCount { get; set; }
        public float masterNodeTotalWin { get; set; }
        public int resultNodeTotalCount { get; set; }
        public float resultNodeTotalWin { get; set; }
        
        public int maximumDepth { get; set; }

        public NextPosition estimatedPosition { get; set; }
    }

    /// <summary>
    /// 하나의 MCTS 노드 클래스
    /// </summary>
    public class MCTSNode
    {
        /// <summary>
        /// 부모 노드
        /// </summary>
        public MCTSNode parentNode { get;private set; }
        /// <summary>
        /// 자식 노드들
        /// </summary>
        public List<MCTSNode> childNodes { get;private set; }
        /// <summary>
        /// 자식 노드들이 UCT 값에 따라서 자동으로 정렬된 것. LINQ 사용
        /// </summary>
        public IOrderedEnumerable<MCTSNode> orderedChildNodes { get;private set; }
        /// <summary>
        /// 현재 노드의 깊이
        /// </summary>
        public byte depth { get; set; }

        /// <summary>
        /// 시뮬레이션에서 승점 총합
        /// </summary>
        public float winCount { get;private set; }
        /// <summary>
        /// 총 시뮬레이션 횟수
        /// </summary>
        public int simulateCount { get;private set; }
        /// <summary>
        /// 현재 노드의 UCT 값
        /// </summary>
        public double UCT { get;private set; }
        /// <summary>
        /// UCT값의 업데이트가 필요하다고 하는 플래그
        /// </summary>
        public bool needUCTUpdateFlag { get; private set; }

        /// <summary>
        /// 셀 정보들
        /// </summary>
        public cellInfo[] cellInfos { get;private set; }
        /// <summary>
        /// 특정 돌을 놓을 수 있는 칸 번호들
        /// </summary>
        public List<byte>[] stoneAvailabilitys { get;private set; }
        /// <summary>
        /// 지금 플레이어
        /// </summary>
        public byte curPlayer { get;private set; }
        /// <summary>
        /// 지금 플레이어 포지션
        /// </summary>
        public byte curPlayerPos { get;private set; }
        /// <summary>
        /// 내 돌 정보들
        /// </summary>
        public byte[] myStones { get;private set; }
        /// <summary>
        /// 적들의 데이터
        /// </summary>
        public EnemyData[] enemyDatas { get;private set; }
        /// <summary>
        /// 나와 필드 빼고 남은 돌 개수
        /// </summary>
        public byte[] leftStoneCounts { get;private set; }
        /// <summary>
        /// 리더보드
        /// </summary>
        public leaderboard[] leaderboards { get;private set; }
        /// <summary>
        /// 게임 종료를 위한 연속 스킵이 얼마나 일어나는지 카운트
        /// </summary>
        public byte skipCount { get;private set; }

        //---------------------UCT 계산
        /// <summary>
        /// UCT 계산함수. 만약 최상위 노드라면 true 반환
        /// </summary>
        /// <returns></returns>
        private bool updateUCT()
        {
            if (parentNode == null) return true;
            float win = (winCount / simulateCount);
            float visiting = (float)(MCTS.exploationParameter * Math.Sqrt(Math.Log(parentNode.simulateCount) / simulateCount));

            if(this.parentNode.curPlayer== 2)
            {
                this.UCT = win + visiting;
            }
            else
            {
                this.UCT = -win - visiting;
            }

            needUCTUpdateFlag = false;
            return false;
        }


        //----------적이 보유한 말 랜덤 시뮬레이션

        /// <summary>
        /// 랜덤하게 적의 보유 돌을 만들음. 해당 적의 돌 소유 정보 반영됨
        /// </summary>
        /// <returns></returns>
        private bool makeRandomEnemyStones(ref EnemyData curEnemyData_, int selectedEnemyIndex)
        {
            int targetStoneCount = curEnemyData_.leftStoneCounts;
            byte[] modifiedStoneLeftCount = new byte[6];
            leftStoneCounts.CopyTo(modifiedStoneLeftCount, 0);

            modifyStoneLeftCountwithPlayerSpecifications(modifiedStoneLeftCount, curEnemyData_, ref targetStoneCount);

            byte totalLeftStoneCount = 0;
            for (int i = 0; i < 6; i++)
            {
                totalLeftStoneCount += modifiedStoneLeftCount[i];
            }

            selectRandomStonesFromStonesArray(curEnemyData_.stonesInfo, modifiedStoneLeftCount, totalLeftStoneCount, targetStoneCount);
            autoSelectAnotherEnemyStones(ref curEnemyData_, selectedEnemyIndex);


            return false;

        }


        /// <summary>
        /// 플레이어 스페이피케이션으로 플레이어의 남은 돌 정보 등을 랜덤추출에 맞게 변형한다.
        /// </summary>
        /// <param name="StoneLeftCount_">변형할 stone left count</param>
        /// <param name="curEnemyData_">기준으로 삼을 플레이어 데이터</param>
        /// <param name="targetStoneCount">해당 플레이어가 돌을 몇개 가지고 있는것처럼 뽑을것인지 개수</param>
        /// <returns></returns>
        private bool modifyStoneLeftCountwithPlayerSpecifications(byte[] StoneLeftCount_, EnemyData curEnemyData_, ref int targetStoneCount)
        {
            //이미 갖고있음이 확정된 돌들은 제외
            for (int i = 0; i < 6; i++)
            {
                curEnemyData_.stonesInfo[i] = 0;

                if (curEnemyData_.stoneSpecifications[i] == 0xFF)
                {
                    StoneLeftCount_[i] = 0;
                }
                else if (curEnemyData_.stoneSpecifications[i] >= 1)
                {
                    targetStoneCount -= curEnemyData_.stoneSpecifications[i];
                    curEnemyData_.stonesInfo[i] = curEnemyData_.stoneSpecifications[i];
                }
            }

            return true;
        }

        /// <summary>
        /// 남은 돌개수들의 배열로부터 돌 배열 랜덤하게 추출
        /// </summary>
        /// <param name="stonesArray">추출한 결과가 담길 배열</param>
        /// <param name="leftstoneArray">남은 돌 개수 배열</param>
        /// <param name="totalCount">추출할 남은 돌 개수의 총합</param>
        /// <param name="targetStoneCount">그중에 몇개의 돌을 추출할 것인가</param>
        private bool selectRandomStonesFromStonesArray(byte[] stonesArray, byte[] leftstoneArray, int totalCount, int targetStoneCount)
        {
            byte selectedStone = 0;
            for (int i = 0; i < targetStoneCount; i++)
            {

                selectedStone = selectOneRandomStonesFromStonesArray(leftstoneArray, totalCount + 1 - i);
                stonesArray[selectedStone]++;
                leftstoneArray[selectedStone]--;
            }

            return false;
        }

        /// <summary>
        /// 돌 개수들의 배열로부터 하나의 돌을 랜덤하게 추출
        /// </summary>
        /// <param name="stoneArray">돌 배열</param>
        /// <param name="totalCount_">돌 배열의 총 개수</param>
        /// <returns></returns>
        private byte selectOneRandomStonesFromStonesArray(byte[] stoneArray, int totalCount_)
        {
            byte temp_sum = 0;
            byte selectedIndex = (byte)MCTS.Randomizer.Next(1, totalCount_);
            for (int j = 0; j < 6; j++)
            {
                temp_sum += stoneArray[j];
                if (selectedIndex <= temp_sum)
                {
                    return (byte)j;
                }
            }

            return 6;
        }

        /// <summary>
        /// 자동으로 반대쪽 적의 돌 개수를 짜 맞추는 함수
        /// </summary>
        /// <returns></returns>
        private bool autoSelectAnotherEnemyStones(ref EnemyData curEnemyData, int selectedEnemyIndex)
        {
            if (selectedEnemyIndex == 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    enemyDatas[1].stonesInfo[i] = (byte)(leftStoneCounts[i] - curEnemyData.stonesInfo[i]);
                }
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    enemyDatas[0].stonesInfo[i] = (byte)(leftStoneCounts[i] - curEnemyData.stonesInfo[i]);
                }
            }

            return true;
        }

        //-------------현재 플레이어의 착수 관련

        /// <summary>
        /// 현재 플레이어 기반으로 랜덤한 돌과 해당 위치까지 랜덤하게 선택
        /// </summary>
        /// <param name="stone">선택된 돌</param>
        /// <param name="position">선택된 포지션</param>
        /// <returns></returns>
        private bool randomSelectStoneAndPosition(out byte stone, out byte position)
        {
            if (curPlayer == 2)
            {
                stone = randomSelectStonebyPlayer(myStones);
            }
            else
            {
                stone = randomSelectStonebyPlayer(enemyDatas[curPlayer].stonesInfo);
            }

            position = randomSelectPositionOfStone(stone);

            return false;
        }


        /// <summary>
        /// 특정 플레이어의 입장에서 랜덤하게 돌 선택하는 함수
        /// </summary>
        /// <param name="stoneArray">그 플레이어의 돌 배열</param>
        /// <returns>해당 돌</returns>
        private byte randomSelectStonebyPlayer(byte[] stoneArray)
        {

            int totalCount = 0;
            for (int i = 0; i < 6; i++)
            {
                if (stoneAvailabilitys[i].Count == 0) continue;

                totalCount += stoneArray[i];
            }

            byte selectedIndex = (byte)MCTS.Randomizer.Next(1, totalCount+1);
            byte temp_sum = 0;

            for (int j = 0; j < 6; j++)
            {
                if (stoneAvailabilitys[j].Count == 0) continue;

                temp_sum += stoneArray[j];
                if (selectedIndex <= temp_sum)
                {
                    return (byte)j;
                }
            }

            return 6;
        }

        /// <summary>
        /// 특정 돌의 위치를 랜덤하게 정하는 함수
        /// </summary>
        /// <param name="stone">특정 돌</param>
        /// <returns>위치</returns>
        private byte randomSelectPositionOfStone(byte stone)
        {
            if (stone == 6) return 0xFF;

            int totalCount = stoneAvailabilitys[stone].Count;
            int SelectedIndex = MCTS.Randomizer.Next(0, totalCount);
            return stoneAvailabilitys[stone][SelectedIndex];
        }


        /// <summary>
        /// 특정 보드의 칸에 특정 돌을 놓는 행동 수행
        /// </summary>
        /// <param name="stone"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        private bool act(byte stone, int position, bool makingChildren = false)
        {
            if (stone == 6)
            {
                nextPlayer();
                skipCount++;
                return false;
            }

            cellInfos[position].stone = stone;

            if (curPlayer == 2)
            {
                myStones[stone]--;
            }
            else
            {
                if (makingChildren == false)
                { 
                    enemyDatas[curPlayer].stonesInfo[stone]--;
                }
                enemyDatas[curPlayer].leftStoneCounts--;
                leftStoneCounts[stone]--;
            }

            deletePositionInStoneAvailability((byte)position);
            addStoneAviliabilityOfCell(MCTS.indexConvertTable[position, 2]);
            addStoneAviliabilityOfCell(MCTS.indexConvertTable[position, 3]);

            addScoreAndRenewLeaderBoard();
            nextPlayer();
            skipCount = 0;

            return true;
        }


        /// <summary>
        /// 돌 허용하는 칸들 정보에서 특정 셀 포지션 정보를 전부 지우기
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private bool deletePositionInStoneAvailability(byte position)
        {
            if (position == 0xFF) return false;

            int selectedIndex = 0;
            for (int i = 0; i < 6; i++)
            {
                selectedIndex = stoneAvailabilitys[i].FindIndex(x => x == position);
                if (selectedIndex == -1) continue;

                stoneAvailabilitys[i].RemoveAt(selectedIndex);
            }

            return true;
        }

        /// <summary>
        /// 리더보드 갱신
        /// </summary>
        /// <returns></returns>
        private bool addScoreAndRenewLeaderBoard()
        {
            //점수 추가
            for (int i = 0; i < 3; i++)
            {
                if (leaderboards[i].player == curPlayer)
                {
                    leaderboards[i].score += 10;
                    break;
                }
            }

            //리더보드 갱신
            leaderboard temp_leaderboard;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (leaderboards[j].score < leaderboards[j + 1].score)
                    {
                        temp_leaderboard = leaderboards[j + 1];
                        leaderboards[j + 1] = leaderboards[j];
                        leaderboards[j] = temp_leaderboard;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 다음 플레이어 인덱스로 넘어감
        /// </summary>
        /// <returns></returns>
        private bool nextPlayer()
        {
            if (curPlayerPos == 2)
            {
                curPlayerPos = 0;
            }
            else
            {
                curPlayerPos++;
            }

            curPlayer = MCTS.playerPosInfos[curPlayerPos];

            return true;
        }

        /// <summary>
        /// 게임이 끝날 수 있는지 검사하고, 해당 승리점수까지 반환
        /// </summary>
        /// <param name="winCount">승리점수</param>
        /// <returns>끝났는가 안끝났는가</returns>
        private bool isGameFinished(out float winCount)
        {
            if (skipCount < 3)
            {
                winCount = 0;
                return false;
            }


            if (leaderboards[0].player == 2)
            {
                winCount = 1 + 0.01f*(leaderboards[0].score - leaderboards[1].score);
            }
            else
            {
                
                if (leaderboards[1].player == 2)
                {
                    winCount = 0.0f -0.01f * (leaderboards[0].score - leaderboards[1].score);
                }
                else
                {
                    winCount = -1.0f -0.01f * (leaderboards[0].score - leaderboards[2].score);
                }
                
                //winCount = 0;
                
            }

            return true;

        }

        //---------보드정보 업데이트 관련
        /// <summary>
        /// 현재 보드의 특정 셀의 상태를 분석해서, stoneAvilability에 정보를 추가함
        /// </summary>
        /// <param name="pos">그 셀의 포지션값</param>
        /// <returns></returns>
        private bool addStoneAviliabilityOfCell(byte pos)
        {
            if (pos >= 36) return false;
            if (cellInfos[pos].stone != 6) return false;

            byte downLeftIndex = MCTS.indexConvertTable[pos, 0];
            byte downRightIndex = MCTS.indexConvertTable[pos, 1];

            bool isDownLeftEmpty = downLeftIndex == 0xFF;
            bool isDownRightEmpty = downRightIndex == 0xFF;

            if (isDownLeftEmpty && isDownRightEmpty)
            {
                stoneAvailabilitys[0].Add(pos);
                stoneAvailabilitys[1].Add(pos);
                stoneAvailabilitys[2].Add(pos);
                stoneAvailabilitys[3].Add(pos);
                stoneAvailabilitys[4].Add(pos);
                stoneAvailabilitys[5].Add(pos);
                return true;
            }
            else if (!isDownLeftEmpty && !isDownRightEmpty)
            {
                return addStoneAvailbilityFromBaseCells(downLeftIndex, downRightIndex, pos);
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// 해당 셀의 바닥이 멀쩡할 때, 그 정보를 availability에 추가
        /// </summary>
        /// <param name="downLeftIndex_">아래 왼쪽 포지션</param>
        /// <param name="downRightIndex_">아래 오른쪽 포지션</param>
        /// <param name="pos">지금 셀 포지션</param>
        /// <returns></returns>
        private bool addStoneAvailbilityFromBaseCells(byte downLeftIndex_, byte downRightIndex_, byte pos)
        {
            cellInfo downLeftCell = cellInfos[downLeftIndex_];
            cellInfo downRightCell = cellInfos[downRightIndex_];

            if (downLeftCell.stone == 6) return false;
            if (downRightCell.stone == 6) return false;

            if ((downLeftCell.stone != 5) && (downRightCell.stone != 5))
            {
                if (downLeftCell.stone == downRightCell.stone)
                {
                    stoneAvailabilitys[downLeftCell.stone].Add(pos);
                }
                else
                {
                    stoneAvailabilitys[downLeftCell.stone].Add(pos);
                    stoneAvailabilitys[downRightCell.stone].Add(pos);
                }

                stoneAvailabilitys[5].Add(pos);
                return true;
            }

            return false;
        }

        //--------자식 관련

        /// <summary>
        /// 이 노드가 자식을 추가할 만큼 많이 방문 됐었는지 보는 함수. 많이 방문 됐으면 true 반환
        /// </summary>
        /// <returns>자식을 만들려면 true</returns>
        private bool isVisitedEnoughToMakeChildren()
        {
            if (simulateCount >= MCTS.minimumChildrenSimulationCount) return true;

            return false;
        }

        /// <summary>
        /// 이 노드의 자원을 UCT와 노드 연결 관련 빼고 전부 없앰. 가비지 콜렉션이 수거하여 메모리 확보
        /// </summary>
        private void killResourcesOfThisNode()
        {
            cellInfos = null;
            stoneAvailabilitys = null;
            myStones = null;
            enemyDatas = null;
            leftStoneCounts = null;
            leaderboards = null;
        }

        /// <summary>
        /// 이 노드의 자식노드들 중에서 updateflag가 있는 모든 노드의 UCT값 갱신
        /// </summary>
        private void updateUCTsOfChildren()
        {
            foreach(MCTSNode child in this.childNodes)
            {
                if (child.needUCTUpdateFlag) child.updateUCT();
            }
        }
        

        //---------------퍼블릭 메소드들

        /// <summary>
        /// 이 노드에서 자식 노드 생성. 생성에 성공하면 true 반환
        /// </summary>
        /// <returns>자식 생성에 성공할경우 true</returns>
        public bool makeChildren()
        {
            if (isVisitedEnoughToMakeChildren() == false) return false;

            childNodes.Clear();

            float temp = 0;
            if (this.isGameFinished(out temp))
            {
                return false;
            }

            if(curPlayer == 2)
            {
                for(int i=0;i<6;i++)
                {
                    if ((myStones[i] == 0)||(myStones[i]>=8)) continue;
                    if (stoneAvailabilitys[i].Count == 0) continue;

                    foreach(byte pos in stoneAvailabilitys[i])
                    {
                        MCTSNode newChildNode = new MCTSNode(this,true);
                        newChildNode.act((byte)i, pos,true);
                        newChildNode.changeUCTPolicyByTurn();
                        newChildNode.depth = (byte)((int)this.depth + 1);

                        childNodes.Add(newChildNode);

                    }
                }

                if (childNodes.Count == 0)
                {
                    MCTSNode newChildNode = new MCTSNode(this, true);
                    newChildNode.act((byte)6, 0xFF, true);
                    newChildNode.changeUCTPolicyByTurn();
                    newChildNode.depth = (byte)((int)this.depth + 1);

                    childNodes.Add(newChildNode);
                }

            }
            else
            {
                for(int i=0;i<6;i++)
                {
                    if (stoneAvailabilitys[i].Count == 0) continue;
                    if (enemyDatas[curPlayer].stoneSpecifications[i] == 0xFF) continue;
                    if (enemyDatas[curPlayer].stoneSpecifications[i] == 0) continue;

                    foreach (byte pos in stoneAvailabilitys[i])
                    {
                        MCTSNode newChildNode = new MCTSNode(this,true);
                        newChildNode.act((byte)i, pos, true);
                        newChildNode.changeUCTPolicyByTurn();
                        newChildNode.depth = (byte)((int)this.depth + 1);

                        childNodes.Add(newChildNode);

                    }
                }

                if (childNodes.Count == 0)
                {
                    MCTSNode newChildNode = new MCTSNode(this, true);
                    newChildNode.act((byte)6, 0xFF, true);
                    newChildNode.changeUCTPolicyByTurn();
                    newChildNode.depth = (byte)((int)this.depth + 1);

                    childNodes.Add(newChildNode);
                }
            }

            

            killResourcesOfThisNode();
            return true;
           
        }

        /// <summary>
        /// 현재 적이 놓을 차례인경우, UCT를 결정하는 정책을 반전시킴
        /// </summary>
        public void changeUCTPolicyByTurn()
        {
            switch(curPlayer)
            {
                case 0:
                    {
                        orderedChildNodes = from node in childNodes
                                            orderby node.UCT ascending
                                            select node;
                        break;
                    }
                case 1:
                    {
                        orderedChildNodes = from node in childNodes
                                            orderby node.UCT ascending
                                            select node;
                        break;
                    }
                case 2:
                    {
                        orderedChildNodes = from node in childNodes
                                            orderby node.UCT descending
                                            select node;
                        break;
                    }
            }
        }

        /// <summary>
        /// 이 노드에서 자식 노드 생성. 대신 마스터 노드에서 맨 처음에만 한번 사용
        /// </summary>
        /// <returns></returns>
        public List<NextPosition> makeChildren_forMasterNode()
        {
            List<NextPosition> poses = new List<NextPosition>();
            childNodes.Clear();

            for (int i = 0; i < 6; i++)
            {
                if ((myStones[i] == 0) || (myStones[i] >= 8)) continue;
                if (stoneAvailabilitys[i].Count == 0) continue;

                foreach (byte pos in stoneAvailabilitys[i])
                {
                    MCTSNode newChildNode = new MCTSNode(this, true);
                    newChildNode.act((byte)i, pos, true);
                    newChildNode.changeUCTPolicyByTurn();
                    newChildNode.depth = (byte)((int)this.depth + 1);

                    poses.Add(new NextPosition
                    {
                        stone = i,
                        position = pos
                    });
                    childNodes.Add(newChildNode);

                }
            }

            if (childNodes.Count == 0)
            {
                MCTSNode newChildNode = new MCTSNode(this, true);
                newChildNode.act((byte)6, 0xFF, true);
                newChildNode.changeUCTPolicyByTurn();
                newChildNode.depth = (byte)((int)this.depth + 1);

                poses.Add(new NextPosition
                {
                    stone = 6,
                    position = 0xFF
                });
                childNodes.Add(newChildNode);
            }

            return poses;

        }

        /// <summary>
        /// 이 노드의 자식 노드 중에서 제일 UCT 값이 큰 노드 선택
        /// </summary>
        /// <returns></returns>
        public MCTSNode selectBestFittingUCTChild()
        {
            updateUCTsOfChildren();

            try
            {
                return orderedChildNodes.ElementAt(0);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 이 노드의 부모 노드를 설정하는 함수
        /// </summary>
        /// <param name="parentNode"></param>
        public void setParentNode(MCTSNode parentNode)
        {
            this.parentNode = parentNode;
        }

        /// <summary>
        /// 이 노드를 시뮬레이션한다음, 그 승률값 반환
        /// </summary>
        /// <returns>승률값</returns>
        public float Simulate()
        {
            var copiedNode = new MCTSNode(this);
            copiedNode.makeRandomEnemyStones(ref copiedNode.enemyDatas[0], 0);
            float winCount;
            while (true)
            {
                if (copiedNode.RandomNextAct(out winCount)) break;
            }

            return winCount;
        }

        /// <summary>
        /// 이 노드 다음 착수를 랜덤하게 결정하는 함수
        /// </summary>
        /// <param name="winCount">승리점수</param>
        /// <returns></returns>
        public bool RandomNextAct(out float winCount)
        {
            byte stone = 0;
            byte position = 0;
            randomSelectStoneAndPosition(out stone, out position);
            act(stone, position);

            if (isGameFinished(out winCount) == true)
            {
                return true;
            }
            return false;

        }

        /// <summary>
        /// 이 노드에 승리점수를 더하는 함수
        /// </summary>
        /// <param name="winCount">승리점수</param>
        /// <returns></returns>
        public bool addWinCount(float winCount)
        {
            this.winCount += winCount;
            this.simulateCount++;
            this.needUCTUpdateFlag = true;
            return true;
        }
        /// <summary>
        /// 이 노드에 승리점수와 시뮬레이션 횟수까지 같이 더하는 함수
        /// </summary>
        /// <param name="winCount"></param>
        /// <param name="SimulationCount"></param>
        /// <returns></returns>
        public bool addWinCountWithSimulationCount(float winCount, int SimulationCount)
        {
            this.winCount += winCount;
            this.simulateCount += SimulationCount;
            this.needUCTUpdateFlag = true;
            return true;
        }


        //---------------초기조건 생성 관련 퍼블릭 메소드들

        /// <summary>
        /// 이 노드의 정보를 해당 게임 세션에서 옮겨오는 메소드
        /// </summary>
        /// <param name="gameSession_"></param>
        public void setNodeStateFromGameSession(GameSession gameSession_)
        {
            setBoardStateFromGameSession(gameSession_);
            setPlayerInfosFromGameSession(gameSession_);
            setLeftStoneCountsFromGameSession(gameSession_);
            setLeaderBoardFromGameSession(gameSession_);

            curPlayerPos = (byte)gameSession_.curPlayerIndex;
            curPlayer = MCTS.playerPosInfos[curPlayerPos];
        }

        //--------------초기조건 관련 프라이베이트 메소드들

        /// <summary>
        /// 보드 상태를 게임 세션에서 옮겨오는 메소드
        /// </summary>
        /// <param name="gameSession_"></param>
        private void setBoardStateFromGameSession(GameSession gameSession_)
        {
            ObservableCollection<BoardCellInfo> boardcells = gameSession_.curBoardData.BoardCellState;

            int pos = 0;
            foreach (BoardCellInfo cell in boardcells)
            {
                cellInfos[pos].stone = (byte)(cell.curStone);
                pos++;
            }

            for(int i=0;i<36;i++)
            {
                addStoneAviliabilityOfCell((byte)i);
            }

        }

        /// <summary>
        /// 플레이어 돌 소유 정보 및 제한 정보들을 옮기는 함수
        /// </summary>
        /// <param name="gameSession_"></param>
        private void setPlayerInfosFromGameSession(GameSession gameSession_)
        {
            PlayerData curPlayerData = gameSession_.playerDatas[gameSession_.curPlayerIndex];
            curPlayer = (byte)curPlayerData.player;

            PlayerData Player1Data;
            PlayerData Player2Data;
            PlayerData PlayerMeData;
            gameSession_.findSpecificPlayer(out Player1Data, Players.P1);
            gameSession_.findSpecificPlayer(out Player2Data, Players.P2);
            gameSession_.findSpecificPlayer(out PlayerMeData, Players.Me);
            copyPlayerDataToNode(Player1Data);
            copyPlayerDataToNode(Player2Data);
            copyPlayerDataToNode(PlayerMeData);
        }

        /// <summary>
        /// 플레이어 데이터를 이 노드에 복사
        /// </summary>
        /// <param name="playerData"></param>
        private void copyPlayerDataToNode(PlayerData playerData)
        {
            Players player = playerData.player;

            if (player == Players.Me)
            {
                for (int i = 0; i < 6; i++)
                {
                    myStones[i] = (byte)(playerData.StonePossesionInfo[i].count);
                }
            }
            else
            {
                EnemyData newEnemyData = new EnemyData(true);

                newEnemyData.leftStoneCounts = (byte)playerData.leftStoneCount;

                int count = 0;
                for (int i = 0; i < 6; i++)
                {
                    count = (playerData.StonePossesionInfo[i].count);
                    if (count == -1)
                    {
                        newEnemyData.stoneSpecifications[i] = 0xFF;
                    }
                    else
                    {
                        newEnemyData.stoneSpecifications[i] = (byte)count;
                    }

                }

                int enemyNum = (int)player;
                enemyDatas[enemyNum] = newEnemyData;

            }
        }

        /// <summary>
        /// 전체에 남은 돌 개수를 게임 세션에서 옮겨오는 메소드
        /// </summary>
        /// <param name="gameSession"></param>
        private void setLeftStoneCountsFromGameSession(GameSession gameSession)
        {
            for (int i = 0; i < 6; i++)
            {
                leftStoneCounts[i] = (byte)(gameSession.curStonesLeftInfo[i].count);
            }
        }

        /// <summary>
        /// 리더보드 정보를 게임 세션에서 옮겨오는 메소드
        /// </summary>
        /// <param name="gameSession"></param>
        private void setLeaderBoardFromGameSession(GameSession gameSession)
        {
            for (int i = 0; i < 3; i++)
            {
                leaderboards[i].player = (byte)(gameSession.LeaderBoardDatas[i].curPlayerData.player);
                leaderboards[i].score = gameSession.LeaderBoardDatas[i].curPlayerData.score;
            }
        }


        //------------생성자

        public MCTSNode()
        {
            parentNode = null;
            childNodes = new List<MCTSNode>();
            orderedChildNodes = from node in childNodes
                                orderby node.UCT descending
                                select node;
            depth = 0;

            winCount = 0;
            simulateCount = 0;
            UCT = 0;
            needUCTUpdateFlag = false;

            stoneAvailabilitys = new List<byte>[]
                { new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>() };

            cellInfos = new cellInfo[36];
            for (byte i = 0; i < 36; i++)
            {
                cellInfos[i].cellNum = i;
            }

            curPlayer = 0;
            curPlayerPos = 0;
            myStones = new byte[6] { 0, 0, 0, 0, 0, 0 };

            enemyDatas = new EnemyData[2];

            leftStoneCounts = new byte[6] { 7, 7, 7, 7, 7, 1 };

            leaderboards = new leaderboard[3];

            skipCount = 0;
        }

        /// <summary>
        /// deep copy 생성자. parent, childnode는 null로 유지된다.
        /// </summary>
        /// <param name="orig">복사할 노드</param>
        /// <param name="forChild">자식 만들기에 사용되는 복사 생성자인 경우 true</param>
        public MCTSNode(MCTSNode orig,bool forChild = false)
        {
            if (forChild)
            {
                this.parentNode = orig;
                this.childNodes = new List<MCTSNode>();

                this.orderedChildNodes = from node in childNodes
                                         orderby node.UCT descending
                                         select node;

            }
            else
            {
                this.parentNode = null;
                this.childNodes = null;
                this.orderedChildNodes = null;
            }
            

            this.winCount = 0;
            this.simulateCount = 0;
            this.UCT = 0;
            this.needUCTUpdateFlag = false;

            this.stoneAvailabilitys = new List<byte>[6];

            int i = 0;
            foreach(List<byte> positions in orig.stoneAvailabilitys)
            {
                this.stoneAvailabilitys[i] = positions.ConvertAll(x => x);
                i++;
            }

            this.cellInfos = new cellInfo[36];
            orig.cellInfos.CopyTo(this.cellInfos, 0);

            this.curPlayer = orig.curPlayer;
            this.curPlayerPos = orig.curPlayerPos;

            this.myStones = new byte[6];
            orig.myStones.CopyTo(this.myStones, 0);

            this.enemyDatas = new EnemyData[2];
            for (int ii = 0; ii < 2; ii++)
            {
                this.enemyDatas[ii] = new EnemyData(orig.enemyDatas[ii]);
            }

            this.leftStoneCounts = new byte[6];
            orig.leftStoneCounts.CopyTo(this.leftStoneCounts, 0);

            this.leaderboards = new leaderboard[3];
            orig.leaderboards.CopyTo(this.leaderboards, 0);

            this.skipCount = orig.skipCount;

        }
    }


}
