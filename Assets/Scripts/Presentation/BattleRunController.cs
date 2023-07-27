using GT.Core;
using UnityEngine;

namespace GT.Presentation
{
    public class BattleRunController : MonoBehaviour
    {
        [SerializeField] private ActorController actor;
        [SerializeField] private Transform camFollowTarget;
        [SerializeField] private BattleMapObject battleMapObject;

        public BattleRun BattleRun;

        private void Awake()
        {
            Application.targetFrameRate = 170;
            BattleRun = new BattleRun(battleMapObject.battleMap);
            BattleIndicatorManager.Instance.Initialize(BattleRun.BattleMap);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 100))
                {
                    actor.MoveTo(hit.point);
                }
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                BattleIndicatorManager.Instance.ShowInds(BattleRun.BattleMap.battleGrids);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                BattleIndicatorManager.Instance.HideInds();
            }
            
        }

        private void LateUpdate()
        {
            camFollowTarget.transform.position = actor.transform.position;
        }
    }
}
