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
            foreach (var grid in GetComponentsInChildren<BattleMapIdentifier>())
            {
                var pos = grid.transform.localPosition;
                map.battleGrids.Add(new BattleGrid(idx++, pos, grid.gridIdentifier, grid.cornerBlock));
            }

            // BuildConnection(map);
            battleMapObject.size = size;
            battleMapObject.battleMap = map;
            map.Size = size;
            AssetDatabase.SaveAssets();
            Debug.Log("Map Generated.");
        }
        
    }
}

#endif