using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GT.Core;
using UnityEngine;
using UnityEngine.AI;

namespace GT.Presentation
{
    public class ActorController : MonoBehaviour
    {
        public BattleRun BattleRun;
        public Actor Actor;
        private NavMeshAgent _agent;
        private Animator _animator;

        [SerializeField] private float runningSpeed = 3.8f;
        [SerializeField] private float turningAngularSpeed = 3.8f;
        [SerializeField] private float endLength = .75f;
        [SerializeField] private float turningRadius = .5f;


        private bool _running = false;

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

        public async Task MoveTo(List<BattleGrid> queue)
        {
            _path = queue;
            _running = true;
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
            _running = false;
            _path = null;
        }

        private async Task Turning(Vector3 target)
        {
            _animator.ResetTrigger("Turn");
            _animator.SetTrigger("Turn");
            var t = Vector3.Angle(transform.forward, target - transform.position) / turningAngularSpeed;
            transform.DOLookAt(target, t).SetEase(Ease.InSine);
            await UniTask.WaitForSeconds(t);
        }

        private async Task MoveToGrid(BattleGrid grid)
        {
            _agent.destination = new Vector3(grid.x, 0, grid.y);
            if (_pathIdx != _path.Count - 1)
            {
                await UniTask.WaitUntil(() => _agent.remainingDistance < turningRadius);
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
                transform.position = _agent.destination;

                // await UniTask.WaitUntil(() => _agent.remainingDistance < 0.01f);
            }
        }
    }
}