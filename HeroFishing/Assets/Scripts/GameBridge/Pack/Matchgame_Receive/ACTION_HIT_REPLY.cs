using System;
using System.Collections;
using UnityEngine;

namespace HeroFishing.Socket.Matchgame {
    public class ACTION_HIT_REPLY : SocketContent {
        //class名稱就是封包的CMD名稱

        /// <summary>
        /// 擊殺怪物索引清單
        /// KillMonsterIdxs與GainGolds是對應的, 例如KillMonsterIdxs為[0,3,6]而GainGolds是[30,0,120], 就是此次攻擊擊殺了索引為0,3,6的怪物並分別獲得30,0,120金幣
        /// </summary>
        public int[] KillMonsterIdx { get; private set; }
        /// <summary>
        /// 獲得金幣清單
        /// </summary>
        public long[] GainGolds { get; private set; }

        public ACTION_HIT_REPLY() {
        }
    }
}