using UnityEngine;

namespace AmesGame
{
    // Simple component that marks an object as carrying a key.
    // Other systems can set HasKey = true to indicate the object currently has the key.
    public class KeyHolder : MonoBehaviour
    {
        [Tooltip("Set to true when this object currently has the key")] 
        public bool HasKey = false;
    }
}
