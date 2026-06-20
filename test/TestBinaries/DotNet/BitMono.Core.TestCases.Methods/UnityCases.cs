using System.Collections;

namespace UnityEngine
{
    // Minimal stand-ins for the Unity types the analyzers match by full name, so the fixture needs no
    // UnityEngine reference. The analyzers are name-based, so these are indistinguishable from real,
    // and the hierarchy (MonoBehaviour : Behaviour : Component) mirrors Unity's.
    public class Component
    {
        public void SendMessage(string methodName) { }
        public void SendMessageUpwards(string methodName) { }
        public void BroadcastMessage(string methodName) { }
    }

    public class Behaviour : Component { }

    public class MonoBehaviour : Behaviour
    {
        protected void Invoke(string methodName, float time) { }
        protected void InvokeRepeating(string methodName, float time, float repeatRate) { }
        protected void CancelInvoke(string methodName) { }
        public void StartCoroutine(string methodName) { }
    }

    public class ScriptableObject { }

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class SerializeFieldAttribute : System.Attribute { }
}

namespace BitMono.Core.TestCases.Methods
{
    public class UnityPlayer : UnityEngine.MonoBehaviour
    {
        public int UnityPublicField;                              // serialized by default -> critical
        [UnityEngine.SerializeField] private int unitySerialized = 0; // [SerializeField] -> critical
        private int unityPrivatePlain = 0;                        // not serialized -> renamable
        public static int UnityStaticField;                       // static -> renamable
        public readonly int UnityReadonlyField;                   // readonly -> renamable
        [System.NonSerialized] public int UnityNonSerialized;     // [NonSerialized] -> renamable
    }

    public class UnityNonContainer
    {
        public int PublicFieldOutsideUnity;                       // not a Unity container -> renamable
    }

    public class UnityInvoker : UnityEngine.MonoBehaviour
    {
        public void Trigger()
        {
            Invoke("DelayedSpawn", 1f);
            StartCoroutine("RunRoutine");
            SendMessage("OnPing");
        }

        private void DelayedSpawn() { }                           // Invoke target -> critical
        private IEnumerator RunRoutine() { yield break; }         // StartCoroutine target -> critical
        public void OnPing() { }                                  // SendMessage target -> critical
        public void NotInvokedByString() { }                      // never named in a string -> renamable
    }
}
