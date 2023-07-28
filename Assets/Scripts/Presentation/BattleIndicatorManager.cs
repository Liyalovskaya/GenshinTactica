using System;
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

        private readonly Dictionary<BattleGrid, Indicator> _gIndicators = new Dictionary<BattleGrid, Indicator>();
        private readonly Dictionary<Collider, Indicator> _cIndicators = new Dictionary<Collider, Indicator>();

        public BattleRun BattleRun => BattleRunController.Instance.BattleRun;

        public void Initialize(BattleMap map)
        {
            foreach (var grid in map.battleGrids)
            {
                var ind = Instantiate(gridIndTemplate, gridIndRoot);
                ind.transform.localPosition = new Vector3(grid.x, 0, grid.y);
                ind.gameObject.SetActive(false);
                ind.BattleGrid = grid;
                _gIndicators.Add(grid, ind);
                _cIndicators.Add(ind.GetComponentInChildren<Collider>(), ind);
            }
            _pathLine = Instantiate(lineTemplate, gridIndRoot);
        }

        private Indicator _selectedInd;

        public Indicator SelectedInd
        {
            get => _selectedInd;
            set
            {
                if (_selectedInd == null)
                {
                    if (value == null) return;
                    _selectedInd = value;
                    _selectedInd.OnSelected();
                }
                else
                {
                    if (value == null)
                    {
                        _selectedInd.OnDeselected();
                        _selectedInd = null;
                    }
                    else
                    {
                        _selectedInd.OnDeselected();
                        _selectedInd = value;
                        _selectedInd.OnSelected();
                    }
                }

            }
        }


        private void Update()
        {
            if (BattleRunController.Instance.moveMode)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 100))
                {
                    if (_cIndicators.TryGetValue(hit.collider, out var target))
                    {
                        var path = BattleRun.BattleMap.ShortestPath(BattleRun.Actors[0].BattleGrid, target.BattleGrid);
                        SelectedInd = target;
                        ShowPath(BattleRun.Actors[0].BattleGrid, path);
                        if (Input.GetMouseButtonDown(0))
                        {
                            _ = BattleRunController.Instance.actor.MoveTo(path);
                        }
                    }
                    else
                    {
                        _pathLine.enabled = false;
                    }
                }
            }
        }

        private LineRenderer _pathLine;
        public void ShowPath(BattleGrid start, List<BattleGrid> grids)
        {
            _pathLine.enabled = true;
            _pathLine.positionCount = grids.Count + 1;
            for (int i = 0; i < _pathLine.positionCount; i++)
            {
                if (i == 0)
                {
                    _pathLine.SetPosition(i, new Vector3(start.x, .06f, start.y));
                }
                else
                {
                    _pathLine.SetPosition(i, new Vector3(grids[i - 1].x, .06f, grids[i - 1].y));
                }
            }
        }

        public void ShowInds(List<BattleGrid> grids)
        {
            foreach (var grid in grids)
            {
                if (_gIndicators.TryGetValue(grid, out var ind))
                {
                    ind.gameObject.SetActive(true);
                }
            }
        }

        public void HideInds()
        {
            foreach (var ind in _gIndicators.Values)
            {
                ind.gameObject.SetActive(false);
            }
        }
    }
}