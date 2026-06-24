using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    public sealed class UiLayoutMarker : MonoBehaviour
    {
        [SerializeField] private int layoutVersion = 1;

        public int LayoutVersion => layoutVersion;
    }
}
