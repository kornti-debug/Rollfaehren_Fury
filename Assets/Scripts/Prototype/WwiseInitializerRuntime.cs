using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    public static class WwiseInitializerRuntime
    {
        private static GameObject host;

        public static GameObject Emitter
        {
            get
            {
                Ensure();
                RegisterEmitterIfReady();
                return host;
            }
        }

        public static AkInitializer Ensure()
        {
            AkInitializer initializer = host != null
                ? host.GetComponent<AkInitializer>()
                : null;
            initializer ??= Object.FindFirstObjectByType<AkInitializer>();

            if (initializer != null)
            {
                host = initializer.gameObject;
                EnsureEmitterComponent();
                Object.DontDestroyOnLoad(host);
                return initializer;
            }

            host = new GameObject("Wwise Runtime");
            host.AddComponent<AkGameObj>();
            initializer = host.AddComponent<AkInitializer>();
            Object.DontDestroyOnLoad(host);
            return initializer;
        }

        public static bool IsEmitterRegistered()
        {
            AkGameObj emitter = host != null ? host.GetComponent<AkGameObj>() : null;
            return emitter != null && emitter.GameObjIsRegistered();
        }

        private static void EnsureEmitterComponent()
        {
            if (host != null && host.GetComponent<AkGameObj>() == null)
            {
                host.AddComponent<AkGameObj>();
            }
        }

        private static void RegisterEmitterIfReady()
        {
            if (host == null || !AkUnitySoundEngine.IsInitialized())
            {
                return;
            }

            AkGameObj emitter = host.GetComponent<AkGameObj>();
            if (emitter != null && !emitter.GameObjIsRegistered())
            {
                emitter.Register();
            }
        }
    }
}
