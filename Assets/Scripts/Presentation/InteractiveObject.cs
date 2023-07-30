using UnityEngine;

namespace GT.Presentation
{
    public class InteractiveObject : MonoBehaviour
    {
        public Vector3[] interactGrids;

        public bool Highlight
        {
            set => gameObject.layer = LayerMask.NameToLayer(value ? "ObjectHighlight" : "Interactive");
        }
        public virtual void Interact()
        {
            
        }

        public virtual void OnHovering()
        {
            Highlight = true;
        }
        public virtual void ExitHovering()
        {
            Highlight = false;
        }
    }
}
