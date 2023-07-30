using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace GT.Core
{
    [Serializable]
    public class BattleMap
    {
        public int Size;
        public List<BattleGrid> battleGrids = new List<BattleGrid>();

        // public List<Connection> gridConnections = new List<Connection>();
        public BattleGrid GetGrid(int x, int height, int y)
        {
            return battleGrids.FirstOrDefault(grid =>
                grid.x == x &&
                grid.height == height &&
                grid.y == y);
        }

        public BattleGrid GetGrid(Vector3 coord)
        {
            return battleGrids.FirstOrDefault(grid =>
                grid.x == (int)coord.x &&
                grid.height == (int)coord.y &&
                grid.y == (int)coord.z);
        }

        public List<BattleGrid> GridsInRange(BattleGrid start, float range)
        {
            var num = battleGrids.Count;
            var dist = new float[num];
            var visited = new bool[num];
            var prev = new int[num];

            for (int i = 0; i < num; i++)
            {
                dist[i] = float.MaxValue;
                prev[i] = -1;
                visited[i] = false;
            }

            dist[start.idx] = 0;

            for (int i = 0; i < num - 1; i++)
            {
                var u = MinDistance(dist, visited, num);
                visited[u] = true;
                for (int j = 0; j < battleGrids[u].Neighbors.Count; j++)
                {
                    var neighbor = battleGrids[u].Neighbors[j];
                    if (!visited[neighbor.End] && neighbor.Weight != 0 && dist[u] != float.MaxValue &&
                        dist[u] + neighbor.Weight < dist[neighbor.End]
                       )
                    {
                        dist[neighbor.End] = dist[u] + neighbor.Weight;
                        prev[neighbor.End] = u;
                    }
                }
            }

            var result = new List<BattleGrid>();


            for (int i = 0; i < num; i++)
            {
                if (i != start.idx && dist[i] <= range)
                {
                    result.Add(battleGrids[i]);
                }
            }

            return result;
        }

        public float DistanceOf(BattleGrid g1, BattleGrid g2)
        {
            var num = battleGrids.Count;
            var dist = new float[num];
            var visited = new bool[num];
            var prev = new int[num];

            for (int i = 0; i < num; i++)
            {
                dist[i] = float.MaxValue;
                prev[i] = -1;
                visited[i] = false;
            }

            dist[g1.idx] = 0;

            for (int i = 0; i < num - 1; i++)
            {
                var u = MinDistance(dist, visited, num);
                visited[u] = true;
                for (int j = 0; j < battleGrids[u].Neighbors.Count; j++)
                {
                    var neighbor = battleGrids[u].Neighbors[j];
                    if (!visited[neighbor.End] && neighbor.Weight != 0 && dist[u] != float.MaxValue &&
                        dist[u] + neighbor.Weight < dist[neighbor.End] &&
                        neighbor.BattleGrid.GridState == GridState.Empty
                       )
                    {
                        dist[neighbor.End] = dist[u] + neighbor.Weight;
                        prev[neighbor.End] = u;
                    }
                }
            }

            return dist[g2.idx];
        }


        public BattleMapPath ShortestPath(BattleGrid g1, BattleGrid g2)
        {
            var num = battleGrids.Count;
            var dist = new float[num];
            var visited = new bool[num];
            var prev = new int[num];

            for (int i = 0; i < num; i++)
            {
                dist[i] = float.MaxValue;
                prev[i] = -1;
                visited[i] = false;
            }

            dist[g1.idx] = 0;

            for (int i = 0; i < num - 1; i++)
            {
                var u = MinDistance(dist, visited, num);
                visited[u] = true;
                for (int j = 0; j < battleGrids[u].Neighbors.Count; j++)
                {
                    var neighbor = battleGrids[u].Neighbors[j];
                    if (!visited[neighbor.End] && neighbor.Weight != 0 && dist[u] != float.MaxValue &&
                        dist[u] + neighbor.Weight < dist[neighbor.End] &&
                        neighbor.BattleGrid.GridState == GridState.Empty
                       )
                    {
                        dist[neighbor.End] = dist[u] + neighbor.Weight;
                        prev[neighbor.End] = u;
                    }
                }
            }

            if (dist[g2.idx] != float.MaxValue)
            {
                var result = new List<BattleGrid>();
                var pathIdx = g2.idx;
                while (pathIdx != g1.idx)
                {
                    result.Add(battleGrids[pathIdx]);
                    pathIdx = prev[pathIdx];
                }

                result.Reverse();
                return new BattleMapPath(g1, result, dist[g2.idx]);
            }
            else
            {
                return new BattleMapPath(g1, null, float.MaxValue);
            }
        }

        private int MinDistance(float[] dist, bool[] visited, int num)
        {
            var min = float.MaxValue;
            var minIdx = -1;
            for (int i = 0; i < num; i++)
            {
                if (visited[i] == false && dist[i] <= min)
                {
                    min = dist[i];
                    minIdx = i;
                }
            }

            return minIdx;
        }
    }

    public class BattleGridLink
    {
        public BattleGrid Grid1, Grid2;
        public int ApCost = 1;
    }


    [Serializable]
    public class BattleGrid
    {
        public int idx, x, y, height;
        public List<GridNeighbor> Neighbors = new List<GridNeighbor>();
        public Actor Actor = null;
        public GridIdentifier gridIdentifier;
        public bool cornerBlock = false;

        public GridState GridState
        {
            get
            {
                if (Actor == null) return GridState.Empty;
                return Actor.Type switch
                {
                    ActorType.Ally => GridState.Ally,
                    ActorType.Player => GridState.Player,
                    ActorType.Enemy => GridState.Enemy,
                    _ => GridState.Enemy
                };
            }
        }

        public BattleGrid(int idx, Vector3 coord, GridIdentifier identifier, bool cornerBlock = false)
        {
            this.idx = idx;
            this.x = (int)coord.x;
            this.y = (int)coord.z;
            this.height = Mathf.FloorToInt(coord.y);
            this.gridIdentifier = identifier;
            this.cornerBlock = cornerBlock;
        }

        public float DistanceTo(BattleGrid grid)
        {
            return (new Vector2(x, y) - new Vector2(grid.x, grid.y)).magnitude;
        }

        public string CoordinateToString()
        {
            return $"({x},{y})";
        }

        public Vector3 GetXYCoordinate()
        {
            return new Vector3(x, 0, y);
        }

        public Vector3 GetCoordinate()
        {
            return new Vector3(x, height, y);
        }
    }

    public class GridNeighbor
    {
        public BattleGrid BattleGrid;
        public readonly int End;
        public readonly float Weight;

        public GridNeighbor(BattleGrid grid, float w)
        {
            BattleGrid = grid;
            End = grid.idx;
            Weight = w;
        }
    }

    public struct BattleMapPath
    {
        public BattleGrid StartGrid;
        public List<BattleGrid> PathGrids;
        public float Distance;

        public BattleMapPath(BattleGrid startGrid, List<BattleGrid> path, float distance)
        {
            StartGrid = startGrid;
            PathGrids = path;
            Distance = distance;
        }

        public int Count => PathGrids.Count + 1;
    }


    public enum GridState
    {
        Empty,
        Player,
        Ally,
        Enemy,
    }

    public enum GridIdentifier
    {
        Empty,
        SolidBarricade,
        ShortBarricade,
    }
}