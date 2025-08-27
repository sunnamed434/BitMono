using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsmResolver.DotNet;
using BitMono.Core;
using BitMono.Core.Attributes;
using BitMono.Core.Contexts;

namespace BitMono.Unity;

[ProtectionName("Unity")]
[ProtectionDescription("Unity Engine obfuscation with IL2CPP support")]
public class UnityProtection : Protection
{
    public static readonly HashSet<string> UnityMethods = new HashSet<string>
    {
        "Awake", "Start", "Update", "FixedUpdate", "LateUpdate",
        "OnEnable", "OnDisable", "OnDestroy", "OnGUI",
        "OnCollisionEnter", "OnCollisionExit", "OnCollisionStay",
        "OnTriggerEnter", "OnTriggerExit", "OnTriggerStay",
        "OnMouseDown", "OnMouseUp", "OnMouseEnter", "OnMouseExit"
    };

    public UnityProtection(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override async Task ExecuteAsync()
    {
        var module = Context.Module;
        
        if (!IsUnityAssembly(module))
        {
            await Task.CompletedTask;
            return;
        }

        var integrationModule = new UnityIntegrationModule(Context);
        integrationModule.PrepareForIL2CPP(module);

        foreach (var type in module.GetAllTypes())
        {
            if (IsMonoBehaviour(type))
            {
                PreserveUnityMethods(type);
            }
        }

        await Task.CompletedTask;
    }

    private bool IsUnityAssembly(ModuleDefinition module)
    {
        return module.AssemblyReferences.Any(a => 
            a.Name?.ToString()?.Contains("UnityEngine") == true || 
            a.Name?.ToString()?.Contains("Unity") == true);
    }

    private bool IsMonoBehaviour(TypeDefinition type)
    {
        var current = type.BaseType;
        while (current != null)
        {
            if (current.FullName == "UnityEngine.MonoBehaviour")
                return true;
                
            try
            {
                current = current.Resolve()?.BaseType;
            }
            catch
            {
                break;
            }
        }
        return false;
    }

    private void PreserveUnityMethods(TypeDefinition type)
    {
        foreach (var method in type.Methods)
        {
            var methodName = method.Name?.ToString();
            if (!string.IsNullOrEmpty(methodName) && UnityMethods.Contains(methodName))
            {
                method.IsRuntimeSpecialName = true;
            }
        }
    }
}

public static class UnityProtectionExtensions
{
    public static bool IsUnityProject(this ModuleDefinition module)
    {
        return module.AssemblyReferences.Any(a => 
            a.Name?.ToString()?.Contains("UnityEngine") == true);
    }
    
    public static bool ShouldPreserveForUnity(this MethodDefinition method)
    {
        var methodName = method.Name?.ToString();
        return !string.IsNullOrEmpty(methodName) && UnityProtection.UnityMethods.Contains(methodName);
    }
}