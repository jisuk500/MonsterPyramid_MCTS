using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterPyramid.Model.MCTS
{
    /// <summary>
    /// MCTS 시뮬레이션을 멀티 쓰레드를 사용하여 병렬 코어를 사용하도록 하는 풀 클래스
    /// </summary>
    static class MCTSSimulationPool
    {

    }

    /// <summary>
    /// 시뮬레이션 결과 구조체
    /// </summary>
    struct simResult
    {
        public MCTSNode node { get; set; }
        public float resultWinCount { get; set; }

        public simResult(MCTSNode node,float resultWinCount)
        {
            this.node = node;
            this.resultWinCount = resultWinCount;
        }
    }
}
