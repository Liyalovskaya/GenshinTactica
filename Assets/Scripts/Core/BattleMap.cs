using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Vector3 = System.Numerics.Vector3;

namespace GT.Core
{
    [Serializable]
    public class BattleMap
    {
        public List<BattleGrid> battleGrids = new List<BattleGrid>();

        // public List<Connection> gridConnections = new List<Connection>();
        public BattleGrid GetGrid(int x, int y)
        {
            return battleGrids.FirstOrDefault(grid => grid.x == x && grid.y == y);
        }
        
        public List<BattleGrid> Dijkstra(BattleGrid g1, BattleGrid g2)
        {
            var num = battleGrids.Count;
            
            
            
            
            
            
            var result = new List<BattleGrid>();
            
            
            
            
            return result;
        }
    }

    [Serializable]
    public class BattleGrid
    {
        private int _idx;
        public int x, y, height;
        
        public Dictionary<BattleGrid, float> Connections = new Dictionary<BattleGrid, float>();

        public BattleGrid(int idx, int x, int y, int height)
        {
            _idx = idx;
            this.x = x;
            this.y = y;
            this.height = height;
        }

        public float DistanceTo(BattleGrid grid)
        {
            return (new Vector2(x, y) - new Vector2(grid.x, grid.y)).magnitude;
        }
    }
    
    

}
