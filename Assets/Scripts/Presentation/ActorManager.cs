using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GT.Core;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace GT.Presentation
{
    public class ActorManager : MonoBehaviour
    {
        public BattleRun BattleRun;
        public Actor Actor;
        private NavMeshAgent _agent;
        private Animator _animator;

        [SerializeField] private float joggingSpeed = 1.5f;
        [SerializeField] private float runningSpeed = 3.8f;
        [SerializeField] private float turningAngularSpeed = 3.8f;
        [SerializeField] private float runningEndLength = .75f;
        [SerializeField] private float joggingEndLength = .25f;
        [SerializeField] private float turningRadius = .5f;

        private SkinnedMeshRenderer[] _skinnedMeshRenderers;
        private MeshRenderer[] _meshRenderers;

        public bool Highlight
        {
            set
            {
                if (value)
                {
                    foreach (var mesh in _skinnedMeshRenderers)
                    {
                        if (Actor.Type == ActorType.Player)
                        {
                            mesh.gameObject.layer = LayerMask.NameToLayer("PlayerHighlight");
                        }

                        if (Actor.Type == ActorType.Enemy)
                        {
                            mesh.gameObject.layer = LayerMask.NameToLayer("EnemyHighlight");
                        }
                    }

                    foreach (var mesh in _meshRenderers)
                    {
                        if (Actor.Type == ActorType.Player)
                        {
                            mesh.gameObject.layer = LayerMask.NameToLayer("PlayerHighlight");
                        }

                        if (Actor.Type == ActorType.Enemy)
                        {
                            mesh.gameObject.layer = LayerMask.NameToLayer("EnemyHighlight");
                        }
                    }
                }
                else
                {
                    foreach (var mesh in _skinnedMeshRenderers)
                    {
                        mesh.gameObject.layer = LayerMask.NameToLayer("Character");
                    }

                    foreach (var mesh in _meshRenderers)
                    {
                        mesh.gameObject.layer = LayerMask.NameToLayer("Character");
                    }
                }
            }
        }


        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _agent.speed = runningSpeed;
            _skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
        }


        public void Initialize(BattleRun battleRun)
        {
            BattleRun = battleRun;
        }

        private BattleMapPath _path;
        private int _pathIdx;

        private LineRenderer _pathLine;
        private Indicator _targetInd;
        private bool _running;
        private static readonly int StopRun = Animator.StringToHash("StopRun");
        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int TurnLeft = Animator.StringToHash("TurnLeft");
        private static readonly int TurnRight = Animator.StringToHash("TurnRight");
        private static readonly int Jog = Animator.StringToHash("Jog");
        private static readonly int Run = Animator.StringToHash("Run");

        private void Update()
        {
            if (_running && _pathLine != null)
            {
                _pathLine.SetPosition(0, transform.position + new Vector3(0, .06f, 0));
            }
        }

        public void SetPath(LineRenderer line, Indicator targetInd)
        {
            _pathLine = line;
            _targetInd = targetInd;
            _targetInd.SetTarget();
        }

        public async Task MoveTo(BattleMapPath path)
        {
            BattleRunManager.InputLock = true;
            _running = true;
            _path = path;
            _pathIdx = 0;

            var prevGrid = Actor.BattleGrid;
            Actor.BattleGrid = null;
            BattleMapManager.Instance.RefreshIndicator(prevGrid);

            await Turning(_path.PathGrids[0].GetXYCoordinate());


            if (_path.Count > 1)
            {
                _agent.speed = runningSpeed;
                _animator.ResetTrigger(Run);
                _animator.SetTrigger(Run);
                for (var i = 0; i < _path.Count - 1; i++)
                {
                    await RunToGrid(_path.PathGrids[i]);
                }
            }
            else
            {
                _agent.speed = joggingSpeed;
                _animator.ResetTrigger(Jog);
                _animator.SetTrigger(Jog);
                await JogToGrid(_path.PathGrids[0]);
            }

            ReachEnd(_path.PathGrids[^1]);
        }

        private async Task Turning(Vector3 target)
        {
            var forward = transform.forward;
            var tar = (target - transform.position).normalized;
            var ccw = Mathf.Sign(forward.x * tar.z - forward.z * tar.x);
            var angle = Vector3.Angle(forward, tar);
            if (angle < 25f)
            {
                return;
            }

            if (ccw > 0)
            {
                _animator.ResetTrigger(TurnLeft);
                _animator.SetTrigger(TurnLeft);
            }
            else
            {
                _animator.ResetTrigger(TurnRight);
                _animator.SetTrigger(TurnRight);
            }


            var t = angle / turningAngularSpeed;
            transform.DOLookAt(target, t).SetEase(Ease.Linear);
            await UniTask.WaitForSeconds(Mathf.Clamp01(t - .05f));
        }

        private async Task RunToGrid(BattleGrid grid)
        {
            _agent.destination = grid.GetCoordinate();
            if (_pathIdx != _path.Count - 2)
            {
                await UniTask.WaitUntil(() => _agent.remainingDistance < turningRadius);
                if (_pathLine != null)
                {
                    var tmp = new Vector3[_pathLine.positionCount];
                    _pathLine.GetPositions(tmp);
                    _pathLine.SetPositions(tmp.Skip(1).ToArray());
                }

                _pathIdx++;
            }
            else
            {
                await UniTask.WaitUntil(() => _agent.remainingDistance < runningEndLength);
                var t = 2 * runningEndLength / _agent.speed;

                _animator.ResetTrigger(StopRun);
                _animator.SetTrigger(StopRun);

                DOVirtual.Float(_agent.speed, 1f, t, x => _agent.speed = x).SetEase(Ease.Linear);
                await UniTask.WaitForSeconds(t);
                Actor.BattleGrid = _path.PathGrids[^1];
                _targetInd.ReachTarget();
                // _agent.ResetPath();
                // await UniTask.WaitUntil(() => _agent.remainingDistance < 0.01f);
            }
        }


        private async Task JogToGrid(BattleGrid grid)
        {
            _agent.destination = new Vector3(grid.x, 0, grid.y);
            await UniTask.WaitUntil(() => _agent.remainingDistance < joggingEndLength);
            _animator.ResetTrigger(Idle);
            _animator.SetTrigger(Idle);
            var t = 2 * joggingEndLength / _agent.speed;
            DOVirtual.Float(_agent.speed, 0f, t, x => _agent.speed = x).SetEase(Ease.Linear);
            await UniTask.WaitForSeconds(t);
            Actor.BattleGrid = _path.PathGrids[^1];
            _targetInd.ReachTarget();
            _agent.ResetPath();
        }

        private void ReachEnd(BattleGrid grid)
        {
            _path = new BattleMapPath();
            _running = false;
            _pathLine.enabled = false;
            _pathLine = null;
            BattleRunManager.InputLock = false;
        }
    }
}