using GT.Core;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
namespace GT.DevTool
{
    public class BattleMapIdentifier : MonoBehaviour
    {
        public GridIdentifier gridIdentifier;
        public bool cornerBlock = false;
    }
    
}
#endif
