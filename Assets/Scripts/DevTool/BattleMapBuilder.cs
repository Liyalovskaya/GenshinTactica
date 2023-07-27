#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using GT.Core;
using UnityEditor;
using UnityEngine;

namespace GT.DevTool
{
    public class BattleMapBuilder : MonoBehaviour
    {
        public BattleMapObject battleMapObject;
        [SerializeField] private int size;

        public void BuildMap()
        {
            EditorUtility.SetDirty(battleMapObject);
            var map = new BattleMap
            {
                battleGrids = new List<BattleGrid>(),
                // gridConnections = new List<Connection>()
            };
            var idx = 0;
            foreach (var grid in GetComponentsInChildren<MeshRenderer>())
            {
                var pos = grid.transform.localPosition;
                var x = (int)pos.x;
                var y = (int)pos.z;
                var height = Mathf.FloorToInt(pos.y);
                map.battleGrids.Add(new BattleGrid(idx++, x, y, height));
            }

            // BuildConnection(map);
            battleMapObject.size = size;
            battleMapObject.battleMap = map;
            AssetDatabase.SaveAssets();
            Debug.Log("Map Generated.");
        }
        
    }
}

#endif