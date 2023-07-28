using GT.Core;
using UnityEngine;

namespace GT.Presentation
{
    public class BattleRunController : Singleton<BattleRunController>
    {
        public ActorController actor;
        [SerializeField] private Transform camFollowTarget;
        [SerializeField] private BattleMapObject battleMapObject;

        public BattleRun BattleRun;
        public bool moveMode = false;
        
        
        private void Awake()
        {
            Application.targetFrameRate = 170;
            BattleRun = new BattleRun(battleMapObject.battleMap);
            var klee = new Actor("Klee", BattleRun.BattleMap.GetGrid(8, 5));
            BattleRun.SetActor(klee);
            actor.Actor = klee;
            BattleIndicatorManager.Instance.Initialize(BattleRun.BattleMap);
        }

        private void Update()
        {
            // if (Input.GetMouseButtonDown(0))
            // {
            //     if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 100))
            //     {
            //         actor.MoveTo(hit.point);
            //     }
            // }

            if (Input.GetKeyDown(KeyCode.A))
            {
                BattleIndicatorManager.Instance.ShowInds(BattleRun.BattleMap.battleGrids);
                moveMode = true;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                BattleIndicatorManager.Instance.HideInds();
                moveMode = false;
            }
            
        }

        private void LateUpdate()
        {
            camFollowTarget.transform.position = actor.transform.position;
        }
    }
}
