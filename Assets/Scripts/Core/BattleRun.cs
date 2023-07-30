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
                             !other.Equals(grid) &&
                             grid.DistanceTo(other) < 1.5f &&
                             grid.gridIdentifier == GridIdentifier.Empty &&
                             grid.height == other.height))
                {
                    if (other.x == grid.x || other.y == grid.y)
                    {
                        grid.Neighbors.Add(new GridNeighbor(other, 1));
                    }
                    else
                    {
                        var offsetX = other.x - grid.x;
                        var offsetY = other.y - grid.y;
                        var neighborX = BattleMap.GetGrid(grid.x + offsetX, grid.height, grid.y);
                        if (neighborX == null || neighborX.cornerBlock) continue;
                        var neighborY = BattleMap.GetGrid(grid.x, grid.height, grid.y + offsetY);
                        if (neighborY == null || neighborY.cornerBlock) continue;
                        grid.Neighbors.Add(new GridNeighbor(other, 1.414f));
                    }
                }
            }
        }
    }
}