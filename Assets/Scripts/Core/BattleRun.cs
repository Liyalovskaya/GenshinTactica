using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GT.Core
{
    public class BattleRun
    {
        public BattleMap BattleMap;
        public List<Actor> Actors = new List<Actor>();
        public Actor CurrentActor;

        public BattleRun(BattleMap map)
        {
            BattleMap = map;
            BuildConnection();
        }

        public void SetActor(Actor actor)
        {
            Actors.Add(actor);
        }


        
        public void BuildConnection()
        {
            foreach (var grid in BattleMap.battleGrids)
            {
                grid.Neighbors?.Clear();
                grid.Neighbors = new List<GridNeighbor>();
                foreach (var other in BattleMap.battleGrids.Where(other =>
                             !other.Equals(grid) && grid.DistanceTo(other) < 1.5f))
                {
                    if (other.x == grid.x || other.y == grid.y)
                    {
                        // map.gridConnections.Add(new Connection(grid, other, 1));
                        grid.Neighbors.Add(new GridNeighbor(other,100));
                    }
                    else
                    {
                        var offsetX = other.x - grid.x;
                        var offsetY = other.y - grid.y;
                        if (BattleMap.GetGrid(grid.x + offsetX, grid.y) == null) continue;
                        if (BattleMap.GetGrid(grid.x, grid.y + offsetY) == null) continue;
                        // map.gridConnections.Add(new Connection(grid, other, 1.414f));
                        grid.Neighbors.Add(new GridNeighbor(other,141));
                    }
                }
            }
        }
    }
}
