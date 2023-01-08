namespace BitMono.Protections;

public class DotNetHook : IProtection
{
    private readonly IRenamer m_Renamer;
    private readonly Random m_Random;

    public DotNetHook(RuntimeImplementations runtime)
    {
        m_Renamer = runtime.Renamer;
        m_Random = runtime.Random;
    }

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters)
    {
        var runtimeHookingType = context.RuntimeModule.ResolveOrThrow<TypeDefinition>(typeof(Hooking));
        var runtimeRedirectStubMethod = runtimeHookingType.Methods.Single(c => c.Name.Equals(nameof(Hooking.RedirectStub)));
        var listener = new ModifyInjectTypeClonerListener(Modifies.All, m_Renamer, context.Module);
        var memberCloneResult = new MemberCloner(context.Module, listener)
            .Include(runtimeHookingType)
            .Clone();
        var hookingType = memberCloneResult.GetClonedMember(runtimeHookingType);
        var redirectStubMethod = memberCloneResult.GetClonedMember(runtimeRedirectStubMethod);

        var factory = context.Module.CorLibTypeFactory;
        var @void = factory.Void;

        var moduleType = context.Module.GetOrCreateModuleType();
        var moduleCctor = moduleType.GetOrCreateStaticConstructor();
        foreach (var method in parameters.Members.OfType<MethodDefinition>())
        {
            if (method.CilMethodBody is { } body)
            {
                for (var i = 0; i < body.Instructions.Count; i++)
                {
                    var instruction = body.Instructions[i];
                    if (instruction.OpCode == CilOpCodes.Call && instruction.Operand is IMethodDescriptor callingOperandMethod)
                    {
                        var callingMethod = callingOperandMethod.Resolve();
                        if (callingMethod != null && callingMethod.CilMethodBody != null
                            && callingMethod.ParameterDefinitions.Any(p => p.IsIn || p.IsOut) == false)
                        {
                            if (context.Module.TryLookupMember(callingMethod.MetadataToken, out var callingMethodMetadata))
                            {
                                var dummyMethod = new MethodDefinition(m_Renamer.RenameUnsafely(), callingMethod.Attributes, callingMethod.Signature);
                                dummyMethod.ImplAttributes = callingMethod.ImplAttributes;
                                dummyMethod.IsAssembly = true;
                                dummyMethod.IsStatic = true;
                                dummyMethod.AssignNextAvaliableToken(context.Module);
                                moduleType.Methods.Add(dummyMethod);
                                foreach (var parameter in callingMethod.ParameterDefinitions)
                                {
                                    dummyMethod.ParameterDefinitions.Add(new ParameterDefinition(parameter.Sequence, parameter.Name, parameter.Attributes));
                                }
                                dummyMethod.MethodBody = new CilMethodBody(dummyMethod);
                                if (callingMethod.Signature.ReturnsValue)
                                {
                                    dummyMethod.CilMethodBody.Instructions.Add(new CilInstruction(CilOpCodes.Ldnull));
                                }
                                dummyMethod.CilMethodBody.Instructions.Add(new CilInstruction(CilOpCodes.Ret));

                                var signature = MethodSignature.CreateStatic(@void);
                                var attributes = MethodAttributes.Assembly | MethodAttributes.Static;
                                var initializatorMethod = new MethodDefinition(m_Renamer.RenameUnsafely(), attributes, signature);
                                initializatorMethod.CilMethodBody = new CilMethodBody(initializatorMethod)
                                {
                                    Instructions =
                                    {
                                        new CilInstruction(CilOpCodes.Ldc_I4, dummyMethod.MetadataToken.ToInt32()),
                                        new CilInstruction(CilOpCodes.Ldc_I4, callingMethodMetadata.MetadataToken.ToInt32()),
                                        new CilInstruction(CilOpCodes.Call, redirectStubMethod),
                                        new CilInstruction(CilOpCodes.Ret) 
                                    }
                                };
                                moduleType.Methods.Add(initializatorMethod);

                                instruction.Operand = dummyMethod;
                                var randomIndex = m_Random.Next(0, moduleCctor.CilMethodBody.Instructions.CountWithoutRet());
                                moduleCctor.CilMethodBody.Instructions.Insert(randomIndex, new CilInstruction(CilOpCodes.Call, initializatorMethod));
                            }
                        }
                    }
                }
            }
        }
        return Task.CompletedTask;
    }
}