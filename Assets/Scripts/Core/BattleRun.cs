using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GT.Core
{
    public class BattleRun
    {
        public BattleMap BattleMap;

        public BattleRun(BattleMap map)
        {
            BattleMap = map;
            BuildConnection();
        }

        // private void BuildConnections(BattleMap map)
        // {
        //     foreach (var grid in map.battleGrids)
        //     {
        //         grid.Connections = new Dictionary<BattleGrid, float>();
        //     }
        //     foreach (var connection in map.gridConnections)
        //     {
        //         Debug.Log($"({connection.g1.x},{connection.g1.y}) to ({connection.g2.x},{connection.g2.y})");
        //         connection.g1.Connections ??= new Dictionary<BattleGrid, float>();
        //         connection.g1.Connections.Add(connection.g2, connection.distance);
        //     }
        // }
        public void BuildConnection()
        {
            foreach (var grid in BattleMap.battleGrids)
            {
                grid.Connections ??= new Dictionary<BattleGrid, float>();
                foreach (var other in BattleMap.battleGrids.Where(other =>
                             !other.Equals(grid) && grid.DistanceTo(other) < 1.5f))
                {
                    if (other.x == grid.x || other.y == grid.y)
                    {
                        // map.gridConnections.Add(new Connection(grid, other, 1));
                        grid.Connections.Add(other, 1);
                    }
                    else
                    {
                        var offsetX = other.x - grid.x;
                        var offsetY = other.y - grid.y;
                        if (BattleMap.GetGrid(grid.x + offsetX, grid.y) == null) continue;
                        if (BattleMap.GetGrid(grid.x, grid.y + offsetY) == null) continue;
                        // map.gridConnections.Add(new Connection(grid, other, 1.414f));
                        grid.Connections.Add(other, 1.414f);
                    }
                }
            }
        }
    }
}
