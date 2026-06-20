using System;
using System.Collections.Generic;

namespace BitMono.IL2CPP;

/// <summary>
/// Names that must never be renamed in <c>global-metadata.dat</c> because IL2CPP/Unity resolves them by
/// name at runtime - renaming them breaks the build. This is the part of the rename policy you can decide
/// from the string alone. Names that are unsafe for *contextual* reasons - classes referenced by scenes/
/// assets, reflection targets, <c>[SerializeField]</c> members - can't be told apart from a bare string and
/// are NOT covered here; they need separate analysis.
/// </summary>
public static class ReservedNames
{
    // MonoBehaviour "magic" methods the engine invokes by name. Not exhaustive - add as you hit more.
    // Source: Unity MonoBehaviour message docs + Tulach's IsInternalType filter (tulach.cc).
    private static readonly HashSet<string> MagicMethods = new(StringComparer.Ordinal)
    {
        "Awake", "Start", "OnEnable", "OnDisable", "OnDestroy",
        "Update", "FixedUpdate", "LateUpdate",
        "OnGUI", "OnValidate", "Reset",
        "OnApplicationQuit", "OnApplicationPause", "OnApplicationFocus",
        "OnBecameVisible", "OnBecameInvisible",
        "OnCollisionEnter", "OnCollisionStay", "OnCollisionExit",
        "OnCollisionEnter2D", "OnCollisionStay2D", "OnCollisionExit2D",
        "OnTriggerEnter", "OnTriggerStay", "OnTriggerExit",
        "OnTriggerEnter2D", "OnTriggerStay2D", "OnTriggerExit2D",
        "OnMouseEnter", "OnMouseOver", "OnMouseExit", "OnMouseDown",
        "OnMouseUp", "OnMouseUpAsButton", "OnMouseDrag",
        "OnDrawGizmos", "OnDrawGizmosSelected",
        "OnPreCull", "OnPreRender", "OnPostRender", "OnRenderObject", "OnWillRenderObject", "OnRenderImage",
        "OnParticleCollision", "OnParticleTrigger", "OnParticleSystemStopped",
        "OnAnimatorMove", "OnAnimatorIK",
        "OnControllerColliderHit", "OnJointBreak", "OnJointBreak2D",
        "OnTransformChildrenChanged", "OnTransformParentChanged",
        "OnLevelWasLoaded", "OnAudioFilterRead", "OnBeforeTransformParentChanged",
    };

    /// <summary>True when <paramref name="name"/> must be left alone (never renamed).</summary>
    public static bool IsReserved(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return true; // index 0 is the empty string, and blank entries carry no rename value anyway
        }
        // Constructors (.ctor/.cctor), explicit-interface impls (Namespace.IFoo.Bar) and namespace strings all
        // carry a '.' and are resolved textually - the engine and dumpers both key on the literal text. Tulach
        // skips any name containing '.' for the same reason; conservative (it also catches namespace strings).
        if (name.IndexOf('.') >= 0)
        {
            return true;
        }
        return MagicMethods.Contains(name);
    }
}
