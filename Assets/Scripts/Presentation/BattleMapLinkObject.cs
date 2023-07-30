using UnityEngine;

namespace GT.Presentation
{
    public class BattleMapLinkObject : InteractiveObject
    {
        public Vector3 grid1, grid2;

        public override void Interact()
        {
            base.Interact();
            Debug.Log("Interact");
        }

        public override void OnHovering()
        {
            base.OnHovering();
        }

        public override void ExitHovering()
        {
            base.ExitHovering();
        }
    }
}
