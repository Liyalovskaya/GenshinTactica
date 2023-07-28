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
            if (Input.GetKeyDown(KeyCode.A))
            {
                BattleIndicatorManager.Instance.ShowInds(
                    BattleRun.BattleMap.GridsInRange(BattleRun.Actors[0].BattleGrid, BattleRun.Actors[0].MoveRange));
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