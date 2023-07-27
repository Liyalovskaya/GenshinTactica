using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GT.Core
{
    [CreateAssetMenu(menuName = "GT/BattleMap",fileName = "map")]
    public class BattleMapObject : ScriptableObject
    {
        public int size;
        public int minX, minY;
        public BattleMap battleMap;
    }
}
