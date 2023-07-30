using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using GT.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GT.Presentation
{
    public class BattleMapManager : Singleton<BattleMapManager>
    {
        [SerializeField] private Transform gridIndRoot;
        [SerializeField] private Indicator gridIndTemplate;
        [SerializeField] private LineRenderer lineTemplate;
        [SerializeField] private MeshRenderer gridBackground;

        [SerializeField] private BattleMapLinkObject[] links;

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
                    BattleRunManager.Instance.actor.Highlight = true;
                    _backgroundTween?.Kill();
                    _backgroundTween = gridBackground.material.DOFloat(.5f, "_Alpha", .2f).SetEase(Ease.OutCubic);
                }
                else
                {
                    BattleRunManager.Instance.actor.Highlight = false;
                    _backgroundTween?.Kill();
                    _backgroundTween = gridBackground.material.DOFloat(.25f, "_Alpha", .2f).SetEase(Ease.OutCubic);
                }
            }
        }

        private readonly Dictionary<BattleGrid, Indicator> _gIndicators = new Dictionary<BattleGrid, Indicator>();
        private readonly Dictionary<Collider, Indicator> _cIndicators = new Dictionary<Collider, Indicator>();

        private readonly Dictionary<Collider, InteractiveObject> _interactiveObjects =
            new Dictionary<Collider, InteractiveObject>();

        public BattleRun BattleRun => BattleRunManager.Instance.BattleRun;
        public BattleMap BattleMap => BattleRun.BattleMap;

        public Actor CurrentActor => BattleRun.CurrentActor;
        public BattleGrid CurrentActorGird => BattleRun.CurrentActor.BattleGrid;
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

            foreach (var obj in FindObjectsOfType<InteractiveObject>())
            {
                _interactiveObjects.Add(obj.GetComponent<Collider>(), obj);
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
                        if (SelectedObject == null)
                        {
                            _pathLine.enabled = false;
                        }
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

        private InteractiveObject _selectedObject;

        public InteractiveObject SelectedObject
        {
            get => _selectedObject;
            set
            {
                if (value == null)
                {
                    if (_selectedObject == null) return;
                    _selectedObject.ExitHovering();
                    _selectedObject = null;
                    if (SelectedInd == null)
                    {
                        _pathLine.enabled = false;
                    }
                }
                else
                {
                    if (_selectedObject != null)
                        _selectedObject.ExitHovering();
                    _selectedObject = value;
                    _selectedObject.OnHovering();
                }
            }
        }


        private void Update()
        {
            if (MoveMode)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 100,
                        LayerMask.GetMask("Interactive", "ObjectHighlight")))
                {
                    if (_cIndicators.TryGetValue(hit.collider, out var target))
                    {
                        if (target.BattleGrid.GridState == GridState.Empty)
                        {
                            var path = BattleRun.BattleMap.ShortestPath(CurrentActorGird,
                                target.BattleGrid);
                            SelectedInd = target;
                            ShowPath(path);
                            if (Input.GetMouseButtonDown(0))
                            {
                                MovementInstruction(BattleRunManager.Instance.actor, path);
                                HideIndicators();
                            }
                        }
                        else
                        {
                            SelectedInd = null;
                        }
                    }
                    else
                    {
                        SelectedInd = null;
                    }

                    if (_interactiveObjects.TryGetValue(hit.collider, out var obj))
                    {
                        SelectedObject = obj;
                        var interactGrid = BattleMap.GetGrid(SelectedObject.interactGrids.FirstOrDefault(grid =>
                            (int)grid.y == CurrentActorGird.height));
                        var path = BattleMap.ShortestPath(CurrentActorGird, interactGrid);
                        ShowPath(path, CurrentActor.MoveAbility);
                        if (Input.GetMouseButtonDown(0))
                        {
                            SelectedObject.Interact();
                        }
                    }
                    else
                    {
                        SelectedObject = null;
                    }
                }
                else
                {
                    SelectedInd = null;
                    SelectedObject = null;
                }
            }
        }

        private bool _moving;

        private async void MovementInstruction(ActorManager actor, BattleMapPath path)
        {
            _moving = true;
            actor.SetPath(_pathLine, SelectedInd);
            await actor.MoveTo(path);
            _moving = false;
        }


        private LineRenderer _pathLine;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public void ShowPath(BattleMapPath path, float range = 0)
        {
            _pathLine.enabled = true;
            _pathLine.positionCount = path.Count;
            if (range == 0 || range > path.Distance)
            {
                _pathLine.material.SetColor(BaseColor, new Color(182 / 255f, 216 / 255f, 255 / 255f));
            }
            else
            {
                _pathLine.material.SetColor(BaseColor, new Color(255 / 255f, 0, 0));
            }

            for (int i = 0; i < _pathLine.positionCount; i++)
            {
                if (i == 0)
                {
                    _pathLine.SetPosition(i, new Vector3(path.StartGrid.x, .06f, path.StartGrid.y));
                }
                else
                {
                    _pathLine.SetPosition(i, new Vector3(path.PathGrids[i - 1].x, .06f, path.PathGrids[i - 1].y));
                }
            }
        }

        public void ShowIndicators(List<BattleGrid> grids, bool outline = false)
        {
            foreach (var grid in grids)
            {
                if (_gIndicators.TryGetValue(grid, out var ind))
                {
                    if (grid.gridIdentifier != GridIdentifier.Empty) continue;
                    ind.gameObject.SetActive(true);
                    ind.Color = IndicatorColor.Default;
                }
            }

            RefreshIndicators();
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

        public void RefreshIndicators()
        {
            foreach (var grid in BattleMap.battleGrids)
            {
                if (_gIndicators.TryGetValue(grid, out var ind))
                {
                    RefreshIndicator(ind);
                    if (!ind.setTarget) ind.State = IndicatorState.Default;
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
    }
}