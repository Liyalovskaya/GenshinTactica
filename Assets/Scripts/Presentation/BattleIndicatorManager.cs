using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GT.Core;
using UnityEngine;

namespace GT.Presentation
{
    public class BattleIndicatorManager : Singleton<BattleIndicatorManager>
    {
        [SerializeField] private Transform gridIndRoot;
        [SerializeField] private Indicator gridIndTemplate;
        [SerializeField] private LineRenderer lineTemplate;

        private readonly Dictionary<BattleGrid, Indicator> _indicators = new Dictionary<BattleGrid, Indicator>();

        public void Initialize(BattleMap map)
        {
            foreach (var grid in map.battleGrids)
            {
                var ind = Instantiate(gridIndTemplate, gridIndRoot);
                ind.transform.localPosition = new Vector3(grid.x, 0, grid.y);
                ind.gameObject.SetActive(false);
                _indicators.Add(grid, ind);
                // foreach (var neighbor in grid.Connections.Keys)
                // {
                //     var line = Instantiate(lineTemplate, gridIndRoot);
                //     line.positionCount = 2;
                //     line.SetPosition(0, new Vector3(grid.x , .05f, grid.y ));
                //     line.SetPosition(1, new Vector3(neighbor.x, .05f, neighbor.y ));
                // }

            }

            // foreach (var connection in map.GridConnections)
            // {
            //     var line = Instantiate(lineTemplate, gridIndRoot);
            //     line.positionCount = 2;
            //     line.SetPosition(0, new Vector3(connection.g1.x , .05f, connection.g1.y ));
            //     line.SetPosition(1, new Vector3(connection.g2.x, .05f, connection.g2.y ));
            // }
        }


        public void ShowInds(List<BattleGrid> grids)
        {
            foreach (var grid in grids)
            {
                if (_indicators.TryGetValue(grid, out var ind))
                {
                    ind.gameObject.SetActive(true);
                }
            }
        }

        public void HideInds()
        {
            foreach (var ind in _indicators.Values)
            {
                ind.gameObject.SetActive(false);
            }
        }
    }
}