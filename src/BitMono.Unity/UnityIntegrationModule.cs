using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;
using BitMono.Core.Contexts;

namespace BitMono.Unity;

public class UnityIntegrationModule
{
    private readonly ProtectionContext _context;
    private readonly UnityMethodDetector _methodDetector;
    
    public UnityIntegrationModule(ProtectionContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _methodDetector = new UnityMethodDetector();
    }
    
    public bool IsUnityAssembly(ModuleDefinition module)
    {
        var hasUnityEngine = module.AssemblyReferences.Any(a => 
            a.Name?.ToString()?.IndexOf("UnityEngine", StringComparison.OrdinalIgnoreCase) >= 0);
        
        var isAssemblyCSharp = module.Assembly?.Name?.ToString()?.IndexOf("Assembly-CSharp", StringComparison.OrdinalIgnoreCase) >= 0;
        
        var hasMonoBehaviour = module.GetAllTypes().Any(t => 
            t.BaseType?.FullName?.Contains("UnityEngine.MonoBehaviour") == true);
        
        return hasUnityEngine || isAssemblyCSharp || hasMonoBehaviour;
    }
    
    public void PrepareForIL2CPP(ModuleDefinition module)
    {
        foreach (var type in module.GetAllTypes())
        {
            if (IsUnityType(type))
            {
                PreserveUnityMetadata(type);
            }
        }
    }
    
    private bool IsUnityType(TypeDefinition type)
    {
        var unityBaseTypes = new[]
        {
            "UnityEngine.MonoBehaviour",
            "UnityEngine.ScriptableObject",
            "UnityEngine.Component",
            "UnityEditor.Editor",
            "UnityEditor.EditorWindow"
        };
        
        var currentType = type;
        while (currentType != null)
        {
            if (unityBaseTypes.Contains(currentType.BaseType?.FullName))
                return true;
            
            currentType = currentType.BaseType?.Resolve();
        }
        
        return false;
    }
    
    private void PreserveUnityMetadata(TypeDefinition type)
    {
        type.IsPublic = true;
    }
    
    public bool ShouldExcludeFromRenaming(MethodDefinition method)
    {
        return _methodDetector.IsUnityCallback(method) || 
               _methodDetector.IsSerializationCallback(method) ||
               _methodDetector.IsCoroutine(method);
    }
}

public class UnityMethodDetector
{
    private readonly HashSet<string> _unityCallbacks = new()
    {
        "Awake", "Start", "Update", "FixedUpdate", "LateUpdate",
        "OnEnable", "OnDisable", "OnDestroy", "OnApplicationPause",
        "OnApplicationFocus", "OnApplicationQuit",
        "OnCollisionEnter", "OnCollisionStay", "OnCollisionExit",
        "OnCollisionEnter2D", "OnCollisionStay2D", "OnCollisionExit2D",
        "OnTriggerEnter", "OnTriggerStay", "OnTriggerExit",
        "OnTriggerEnter2D", "OnTriggerStay2D", "OnTriggerExit2D",
        "OnBecameVisible", "OnBecameInvisible",
        "OnPreCull", "OnPreRender", "OnPostRender",
        "OnRenderObject", "OnWillRenderObject",
        "OnGUI", "OnMouseDown", "OnMouseUp", "OnMouseOver",
        "OnMouseExit", "OnMouseEnter", "OnMouseDrag",
        "OnAnimatorMove", "OnAnimatorIK",
        "OnStartServer", "OnStopServer", "OnStartClient", "OnStopClient",
        "OnServerConnect", "OnServerDisconnect", "OnClientConnect", "OnClientDisconnect"
    };
    
    private readonly HashSet<string> _serializationCallbacks = new()
    {
        "OnBeforeSerialize", "OnAfterDeserialize",
        "OnValidate", "Reset"
    };
    
    public bool IsUnityCallback(MethodDefinition method)
    {
        var methodName = method.Name?.ToString();
        return !string.IsNullOrEmpty(methodName) && _unityCallbacks.Contains(methodName);
    }
    
    public bool IsSerializationCallback(MethodDefinition method)
    {
        var methodName = method.Name?.ToString();
        return !string.IsNullOrEmpty(methodName) && _serializationCallbacks.Contains(methodName);
    }
    
    public bool IsCoroutine(MethodDefinition method)
    {
        return method.Signature?.ReturnType?.FullName?.IndexOf("System.Collections.IEnumerator") >= 0;
    }
}