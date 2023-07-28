using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        public List<BattleGrid> GridsInRange(BattleGrid start, float range)
        {
            var num = battleGrids.Count;
            var dist = new int[num];
            var visited = new bool[num];
            var prev = new int[num];

            for (int i = 0; i < num; i++)
            {
                dist[i] = 0x3f3f3f3f;
                prev[i] = -1;
                visited[i] = false;
            }

            dist[start.idx] = 0;

            for (int i = 0; i < num - 1; i++)
            {
                var u = MinDistance(dist, visited, num);
                visited[u] = true;
                for (int j = 0; j < battleGrids[u].neighbors.Count; j++)
                {
                    var neigbor = battleGrids[u].neighbors[j];
                    if (!visited[neigbor.end] && neigbor.weight != 0 && dist[u] != 0x3f3f3f3f &&
                        dist[u] + neigbor.weight < dist[neigbor.end]
                       )
                    {
                        dist[neigbor.end] = dist[u] + neigbor.weight;
                        prev[neigbor.end] = u;
                    }
                }
            }

            var result = new List<BattleGrid>();


            for (int i = 0; i < num; i++)
            {
                if (i != start.idx && dist[i] / 100f < range)
                {
                    result.Add(battleGrids[i]);
                }
            }

            return result;
        }

        public List<BattleGrid> ShortestPath(BattleGrid g1, BattleGrid g2)
        {
            var num = battleGrids.Count;
            var dist = new int[num];
            var visited = new bool[num];
            var prev = new int[num];

            for (int i = 0; i < num; i++)
            {
                dist[i] = 0x3f3f3f3f;
                prev[i] = -1;
                visited[i] = false;
            }

            dist[g1.idx] = 0;

            for (int i = 0; i < num - 1; i++)
            {
                var u = MinDistance(dist, visited, num);
                visited[u] = true;
                for (int j = 0; j < battleGrids[u].neighbors.Count; j++)
                {
                    var neigbor = battleGrids[u].neighbors[j];
                    if (!visited[neigbor.end] && neigbor.weight != 0 && dist[u] != 0x3f3f3f3f &&
                        dist[u] + neigbor.weight < dist[neigbor.end]
                       )
                    {
                        dist[neigbor.end] = dist[u] + neigbor.weight;
                        prev[neigbor.end] = u;
                    }
                }
            }

            if (dist[g2.idx] != 0x3f3f3f3f)
            {
                var result = new List<BattleGrid>();
                var pathIdx = g2.idx;
                while (pathIdx != g1.idx)
                {
                    result.Add(battleGrids[pathIdx]);
                    pathIdx = prev[pathIdx];
                }

                result.Reverse();
                return result;
            }
            else
            {
                return null;
            }
        }

        private int MinDistance(int[] dist, bool[] visited, int num)
        {
            float min = 0x3f3f3f3f;
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

    [Serializable]
    public class BattleGrid
    {
        public int idx, x, y, height;
        public List<GridNeighbor> neighbors = new List<GridNeighbor>();

        public BattleGrid(int idx, int x, int y, int height)
        {
            this.idx = idx;
            this.x = x;
            this.y = y;
            this.height = height;
        }

        public float DistanceTo(BattleGrid grid)
        {
            return (new Vector2(x, y) - new Vector2(grid.x, grid.y)).magnitude;
        }

        public string CoordinateToString()
        {
            return $"({x},{y})";
        }

        public Vector3 GetXYPosition()
        {
            return new Vector3(x, 0, y);
        }
    }

    [Serializable]
    public class GridNeighbor
    {
        public int end;
        public int weight;

        public GridNeighbor(int e, int w)
        {
            end = e;
            weight = w;
        }
    }
}