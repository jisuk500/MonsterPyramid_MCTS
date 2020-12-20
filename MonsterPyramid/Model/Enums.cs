using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterPyramid.Model
{
    /// <summary>
    /// 플레이어 enum 형식
    /// </summary>
    public enum Players { P1, P2, Me, None };

    /// <summary>
    /// 놓는 돌 enum 형식
    /// </summary>
    public enum Stones { Pepe, Pink, Slime, Mush, Octo, Special, None, Skipped };

    /// <summary>
    /// 현재 순위 enum 형식
    /// </summary>
    public enum Ranking { Rank1, Rank2, Rank3 };

    /// <summary>
    /// 현재 게임의 준비 상태 enum 형식
    /// </summary>
    public enum ReadyState { SelectPlayerPos, SetInitialStones, GameReady, Gaming}
}
