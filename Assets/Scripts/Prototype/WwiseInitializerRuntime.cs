using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    public static class WwiseInitializerRuntime
    {
        public static AkInitializer Ensure()
        {
            AkInitializer initializer = Object.FindFirstObjectByType<AkInitializer>();
            if (initializer != null)
            {
                return initializer;
            }

            GameObject initializerObject = new GameObject("Wwise Initializer");
            return initializerObject.AddComponent<AkInitializer>();
        }
    }
}
