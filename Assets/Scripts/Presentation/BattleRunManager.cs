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

        public static bool InputLock = false;

        private void Awake()
        {
            Application.targetFrameRate = 170;
            BattleRun = new BattleRun(battleMapObject.battleMap);
            BattleRun.BattleMap.Size = battleMapObject.size;
            var klee = new Actor("Klee", BattleRun.BattleMap.GetGrid(8, 0, 5));
            var bucket = new Actor("bucket", BattleRun.BattleMap.GetGrid(11, 0, 8));
            klee.Type = ActorType.Player;
            bucket.Type = ActorType.Enemy;
            BattleRun.SetActor(klee);
            BattleRun.SetActor(bucket);
            actor.Actor = klee;
            BattleRun.CurrentActor = klee;
            BattleMapManager.Instance.Initialize(BattleRun.BattleMap);
        }

        private void Update()
        {
            if (InputLock) return;
            if (Input.GetKeyDown(KeyCode.A))
            {
                BattleMapManager.Instance.ShowIndicators(
                    BattleRun.BattleMap.GridsInRange(BattleRun.CurrentActor.BattleGrid,
                        BattleRun.CurrentActor.MoveAbility));
                BattleMapManager.Instance.MoveMode = true;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                BattleMapManager.Instance.HideIndicators();
                BattleMapManager.Instance.MoveMode = false;
            }
        }

        private void LateUpdate()
        {
            camFollowTarget.transform.position = actor.transform.position;
        }
    }
}