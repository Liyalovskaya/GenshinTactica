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

namespace GT.Presentation
{
    public class ActorManager : MonoBehaviour
    {
        public BattleRun BattleRun;
        public Actor Actor;
        private NavMeshAgent _agent;
        private Animator _animator;

        [SerializeField] private float runningSpeed = 3.8f;
        [SerializeField] private float turningAngularSpeed = 3.8f;
        [SerializeField] private float endLength = .75f;
        [SerializeField] private float turningRadius = .5f;

        

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _agent.speed = runningSpeed;
        }


        public void Initialize(BattleRun battleRun)
        {
            BattleRun = battleRun;
        }

        private List<BattleGrid> _path = new List<BattleGrid>();
        private int _pathIdx;

        public LineRenderer pathLine;
        private bool _running;

        private void Update()
        {
            if (_running && pathLine != null)
            {
                pathLine.SetPosition(0, transform.position + new Vector3(0, .06f, 0));
            }
        }

        public async Task MoveTo(List<BattleGrid> queue)
        {
            BattleRunManager.InputLock = true;
            _running = true;
            _path = queue;
            _pathIdx = 0;
            _agent.speed = runningSpeed;

            await Turning(_path[0].GetXYPosition());

            _animator.ResetTrigger("Run");
            _animator.SetTrigger("Run");
            for (var i = 0; i < _path.Count; i++)
            {
                await MoveToGrid(_path[i]);
            }

            Actor.MoveTo(_path[^1]);
            _path = null;
            _running = false;
            pathLine.enabled = false;
            pathLine = null;
            BattleRunManager.InputLock = false;
        }

        private async Task Turning(Vector3 target)
        {
            _animator.ResetTrigger("Turn");
            _animator.SetTrigger("Turn");
            var t = Vector3.Angle(transform.forward, target - transform.position) / turningAngularSpeed;
            transform.DOLookAt(target, t).SetEase(Ease.Linear);
            await UniTask.WaitForSeconds(Mathf.Clamp01(t - .05f));
        }

        private async Task MoveToGrid(BattleGrid grid)
        {
            _agent.destination = new Vector3(grid.x, 0, grid.y);
            if (_pathIdx != _path.Count - 1)
            {
                await UniTask.WaitUntil(() => _agent.remainingDistance < turningRadius);
                if (pathLine != null)
                {
                    var tmp = new Vector3[pathLine.positionCount];
                    pathLine.GetPositions(tmp);
                    pathLine.SetPositions(tmp.Skip(1).ToArray());
                }
                _pathIdx++;
            }
            else
            {
                await UniTask.WaitUntil(() => _agent.remainingDistance < endLength);
                _animator.ResetTrigger("Idle");
                _animator.SetTrigger("Idle");
                var t = 2 * endLength / runningSpeed;
                DOVirtual.Float(runningSpeed, 0f, t, x => _agent.speed = x).SetEase(Ease.Linear);
                await UniTask.WaitForSeconds(t);
                _agent.ResetPath();

                // await UniTask.WaitUntil(() => _agent.remainingDistance < 0.01f);
            }
        }
    }
}