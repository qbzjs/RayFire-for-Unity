//using System;
//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Mathematics;
//using Unity.Transforms;
//using UnityEngine;
//using UnityEngine.UIElements;

//namespace HeroFishing.Battle {
//    /// <summary>
//    /// 怪物可以被子彈擊中的區域網格設定
//    /// </summary>
//    public struct MapGridData : IComponentData {
//        public float CellSize; // 每個網格大小
//        public int GridWidth; // 網格的寬度
//        public int GridHeight; // 網格的高度
//        public int2 BoundaryX;// 網格X軸範圍
//        public int2 BoundaryY;// 網格Y軸範圍
//        public NativeParallelMultiHashMap<int2, MonsterValue> GridMap;
//    }
//    /// <summary>
//    /// 怪物超出邊界會被移除的區域
//    /// </summary>
//    public struct RemoveMonsterBoundaryData : IComponentData {
//        public int2 BoundaryX;// X軸範圍
//        public int2 BoundaryZ;// Z軸範圍
//    }
//}
