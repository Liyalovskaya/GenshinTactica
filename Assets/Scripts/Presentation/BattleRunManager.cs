using GT.Core;
using UnityEngine;

namespace GT.Presentation
{
    public class BattleRunManager : Singleton<BattleRunManager>
    {
        public ActorManager actor;
        [SerializeField] private Transform camFollowTarget;
        [SerializeField] private BattleMapObject battleMapObject;

        public BattleRun BattleRun;
        public bool moveMode = false;

        public static bool InputLock = false;

        private void Awake()
        {
            Application.targetFrameRate = 170;
            BattleRun = new BattleRun(battleMapObject.battleMap);
            BattleRun.BattleMap.Size = battleMapObject.size;
            var klee = new Actor("Klee", BattleRun.BattleMap.GetGrid(8, 5));
            BattleRun.SetActor(klee);
            actor.Actor = klee;
            BattleRun.CurrentActor = klee;
            BattleMapManager.Instance.Initialize(BattleRun.BattleMap);
        }

        private void Update()
        {
            if(InputLock) return;
            if (Input.GetKeyDown(KeyCode.A))
            {
                BattleMapManager.Instance.ShowIndicators(
                    BattleRun.BattleMap.GridsInRange(BattleRun.CurrentActor.BattleGrid, BattleRun.CurrentActor.MoveRange));
                moveMode = true;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                BattleMapManager.Instance.HideIndicators();
                moveMode = false;
            }
        }

        private void LateUpdate()
        {
            camFollowTarget.transform.position = actor.transform.position;
        }
    }
}