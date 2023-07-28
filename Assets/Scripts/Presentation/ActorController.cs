using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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

        private List<BattleGrid> path = new List<BattleGrid>();

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
        }

        public void Initialize(BattleRun battleRun)
        {
            BattleRun = battleRun;
        }
        
        
        
        private IEnumerator _moveCoroutine;

        public void MoveTo(Vector3 dst)
        {
            _animator.ResetTrigger("Run");
            _animator.SetTrigger("Run");
            _agent.destination = dst; 
            _moveCoroutine = Movement();
            StartCoroutine(_moveCoroutine);

        }

        public async Task MoveTo(List<BattleGrid> queue)
        {
            for (var i = 0; i < queue.Count; i++)
            {
                await MoveToGrid(queue[i]);
            }

            Actor.MoveTo(queue[^1]);
        }

        private async Task MoveToGrid(BattleGrid grid)
        {
            _agent.destination = new Vector3(grid.x, 0, grid.y);
            await UniTask.WaitUntil(() => _agent.remainingDistance < 0.25f);
        }

        private IEnumerator Movement()
        {
            // yield return new WaitForSeconds(.1f);
            while (_agent.remainingDistance > .01f)
            {
                yield return null;
            }
            // _animator.ResetTrigger("Idle");
            // _animator.SetTrigger("Idle");
            //
            // while (_agent.remainingDistance > .01f)
            // {
            //     Debug.Log($"{_agent.remainingDistance}");
            //     yield return null;
            // }
            // Debug.Log("stop");


        }
    }
}