namespace UnityEngine
{
    // Minimal stand-ins for the Unity types the analyzer matches by full name, so the fixture needs
    // no UnityEngine reference. The analyzer is name-based, so these are indistinguishable from real.
    public class MonoBehaviour { }
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
}
