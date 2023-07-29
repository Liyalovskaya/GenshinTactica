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
                ind.BattleGrid = grid;
                ind.gameObject.SetActive(false);
                _gIndicators.Add(grid, ind);
                _cIndicators.Add(ind.GetComponentInChildren<Collider>(), ind);
            }
            _pathLine = Instantiate(lineTemplate, gridIndRoot);
            RefreshIndicators();
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
                    _selectedInd.State = IndicatorState.Selected;
                }
                else
                {
                    if (value == null)
                    {
                        _selectedInd.State = IndicatorState.Default;
                        _selectedInd = null;
                    }
                    else
                    {
                        _selectedInd.State = IndicatorState.Default;
                        _selectedInd = value;
                        _selectedInd.State = IndicatorState.Selected;
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
                        if (target.BattleGrid.GridState == GridState.Empty)
                        {
                            var path = BattleRun.BattleMap.ShortestPath(BattleRun.Actors[0].BattleGrid,
                                target.BattleGrid);
                            SelectedInd = target;
                            ShowPath(BattleRun.Actors[0].BattleGrid, path);
                            if (Input.GetMouseButtonDown(0))
                            {
                                MovementInstruction(BattleRunManager.Instance.actor, path);
                                HideIndicators();
                            }
                        }
                        else
                        {
                            SelectedInd = null;
                            _pathLine.enabled = false;
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

        private bool _moving;

        private async void MovementInstruction(ActorManager actor, List<BattleGrid> path)
        {
            _moving = true;
            actor.SetPath(_pathLine, SelectedInd);
            await actor.MoveTo(path);
            _moving = false;
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
            foreach (var grid in grids)
            {
                if (_gIndicators.TryGetValue(grid, out var ind))
                {
                    ind.gameObject.SetActive(true);
                    ind.Color = IndicatorColor.Default;
                }
            }

            RefreshIndicators();
        }

        public void RefreshIndicators()
        {
            foreach (var grid in BattleMap.battleGrids)
            {
                if (_gIndicators.TryGetValue(grid, out var ind))
                {
                    RefreshIndicator(ind);
                    if(!ind.setTarget) ind.State = IndicatorState.Default;
                    if (grid.GridState != GridState.Empty)
                    {
                        ind.gameObject.SetActive(true);
                    }
                }
            }
        }

        public void RefreshIndicator(BattleGrid grid)
        {
            var ind = _gIndicators[grid];
            ind.Color = grid.GridState switch
            {
                GridState.Empty => IndicatorColor.Default,
                GridState.Player => IndicatorColor.Player,
                GridState.Ally => IndicatorColor.Ally,
                GridState.Enemy => IndicatorColor.Enemy,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        public void RefreshIndicator(Indicator ind)
        {
            ind.Color = ind.BattleGrid.GridState switch
            {
                GridState.Empty => IndicatorColor.Default,
                GridState.Player => IndicatorColor.Player,
                GridState.Ally => IndicatorColor.Ally,
                GridState.Enemy => IndicatorColor.Enemy,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void HideIndicators()
        {
            foreach (var ind in _gIndicators.Values.Where(ind =>
                         !ind.setTarget || ind.BattleGrid.GridState == GridState.Enemy))
            {
                ind.gameObject.SetActive(false);
            }

            if (!_moving)
            {
                _pathLine.enabled = false;
            }

            MoveMode = false;
            RefreshIndicators();
        }
    }
}