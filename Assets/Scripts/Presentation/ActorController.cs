using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace GT.Presentation
{
    public class ActorController : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private Animator _animator;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
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

        private IEnumerator Movement()
        {
            yield return new WaitForSeconds(.1f);
            while (_agent.remainingDistance > .5f)
            {
                yield return null;
            }
            _animator.ResetTrigger("Idle");
            _animator.SetTrigger("Idle");
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