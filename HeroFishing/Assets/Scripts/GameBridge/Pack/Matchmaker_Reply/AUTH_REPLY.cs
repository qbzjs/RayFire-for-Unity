﻿using System.Collections;
using UnityEngine;

namespace HeroFishing.Socket.Matchmaker {
    public class AUTH_REPLY : SocketContent {
        //class名稱就是封包的CMD名稱

        public bool IsAuth { get; private set; }

        public AUTH_REPLY() {
        }
    }
}