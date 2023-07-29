using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using GT.Core;
using UnityEngine;

namespace GT.Presentation
{
    public class BattleMapManager : Singleton<BattleMapManager>
    {
        [SerializeField] private Transform gridIndRoot;
        [SerializeField] private Indicator gridIndTemplate;
        [SerializeField] private LineRenderer lineTemplate;
        [SerializeField] private MeshRenderer gridBackground;

        private Tween _backgroundTween;

        private bool _moveMode;
        public bool MoveMode
        {
            get => _moveMode;
            set
            {
                _moveMode = value;
                if (_moveMode)
                {
                    _backgroundTween?.Kill();
                    _backgroundTween = gridBackground.material.DOFloat(.5f, "_Alpha", .2f).SetEase(Ease.OutCubic);
                }
                else
                {
                    _backgroundTween?.Kill();
                    _backgroundTween = gridBackground.material.DOFloat(.25f, "_Alpha", .2f).SetEase(Ease.OutCubic);
                }

            }
        }

        private readonly Dictionary<BattleGrid, Indicator> _gIndicators = new Dictionary<BattleGrid, Indicator>();
        private readonly Dictionary<Collider, Indicator> _cIndicators = new Dictionary<Collider, Indicator>();

        public BattleRun BattleRun => BattleRunManager.Instance.BattleRun;
        public BattleMap BattleMap => BattleRun.BattleMap;
        public int Size => BattleMap.Size;

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
            if (MoveMode)
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
                            var actor = BattleRunManager.Instance.actor;
                            _ = actor.MoveTo(path);
                            actor.SetPath(_pathLine, target);
                            HideIndicators();
                        }
                    }
                    else
                    {
                        SelectedInd = null;
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

        public void ShowIndicators(List<BattleGrid> grids, bool outline = false)
        {
            var matrix = new int[Size][];
            for (int i = 0; i < Size; i++)
            {
                matrix[i] = new int[Size];
            }

            foreach (var grid in grids)
            {
                if (_gIndicators.TryGetValue(grid, out var ind))
                {
                    ind.gameObject.SetActive(true);
                    matrix[grid.x][grid.y] = -1;
                }
            }
            //
            // if (outline)
            // {
            //     for (int i = 0; i < Size; i++)
            //     {
            //         for (int j = 0; j < Size; j++)
            //         {
            //             if (matrix[i][j] == -1) continue;
            //             _gIndicators[BattleMap.GetGrid(i, j)]
            //                 .SetOutline(BattleMapIndicatorHelper.GetGridBit(Size, matrix, i, j));
            //         }
            //     }
            // }
        }

        public void HideIndicators()
        {
            foreach (var ind in _gIndicators.Values.Where(ind => !ind.setTarget))
            {
                ind.gameObject.SetActive(false);
            }

            MoveMode = false;
        }
    }
}