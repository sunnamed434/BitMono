namespace BitMono.Protections;

public class DotNetHook : IStageProtection
{
    private readonly IInjector m_Injector;
    private readonly IRenamer m_Renamer;
    private readonly Random m_Random;

    public DotNetHook(IInjector injector, IRenamer renamer)
    {
        m_Injector = injector;
        m_Renamer = renamer;
        m_Random = new Random();
    }

    public PipelineStages Stage => PipelineStages.ModuleWritten;

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
    {
        var runtimeHookingType = context.RuntimeModule.ResolveOrThrow<TypeDefinition>(typeof(Hooking));
        var memberCloneResult = new MemberCloner(context.Module, new InjectTypeClonerListener(context.Module))
            .Include(runtimeHookingType)
            .Clone();
        var hookingType = memberCloneResult.GetClonedMember(runtimeHookingType);
        var redirectStubMethod = memberCloneResult.ClonedMembers.Single(c => c.Name.Equals(nameof(Hooking.RedirectStub)));

        memberCloneResult
            .RenameClonedMembers(m_Renamer)
            .RemoveNamespaceOfClonedMembers(m_Renamer);

        var moduleType = context.Module.GetOrCreateModuleType();
        foreach (var method in parameters.Targets.OfType<MethodDefinition>())
        {
            if (method.HasMethodBody)
            {
                for (var i = 0; i < method.CilMethodBody.Instructions.Count; i++)
                {
                    if (method.CilMethodBody.Instructions[i].OpCode == CilOpCodes.Call
                        && method.CilMethodBody.Instructions[i].Operand is IMethodDescriptor methodDescriptor)
                    {
                        var callingMethod = methodDescriptor.Resolve();
                        if (callingMethod != null && callingMethod.HasMethodBody
                            && callingMethod.ParameterDefinitions.Any(p => p.IsIn || p.IsOut) == false)
                        {
                            if (context.Module.TryLookupMember(callingMethod.MetadataToken, out var callingMethodMetadata))
                            {
                                var dummyMethod = new MethodDefinition(m_Renamer.RenameUnsafely(), callingMethod.Attributes, callingMethod.Signature);
                                dummyMethod.ImplAttributes = callingMethod.ImplAttributes;
                                dummyMethod.IsStatic = true;
                                dummyMethod.AssignNextAvaliableToken(context.Module);
                                moduleType.Methods.Add(dummyMethod);
                                foreach (var parameterDefinition in callingMethod.ParameterDefinitions)
                                {
                                    dummyMethod.ParameterDefinitions.Add(new ParameterDefinition(parameterDefinition.Sequence, parameterDefinition.Name, parameterDefinition.Attributes));
                                }
                                dummyMethod.MethodBody = new CilMethodBody(dummyMethod);
                                if (callingMethod.Signature.ReturnsValue)
                                {
                                    dummyMethod.CilMethodBody.Instructions.Add(new CilInstruction(CilOpCodes.Ldnull));
                                }
                                dummyMethod.CilMethodBody.Instructions.Add(new CilInstruction(CilOpCodes.Ret));

                                var initializatorMethodDef = new MethodDefinition(m_Renamer.RenameUnsafely(), MethodAttributes.Assembly | MethodAttributes.Static,
                                    MethodSignature.CreateStatic(context.Module.CorLibTypeFactory.Void));
                                initializatorMethodDef.CilMethodBody = new CilMethodBody(initializatorMethodDef)
                                {
                                    Instructions =
                                    {
                                        new CilInstruction(CilOpCodes.Ldc_I4, dummyMethod.MetadataToken.ToInt32()),
                                        new CilInstruction(CilOpCodes.Ldc_I4, callingMethodMetadata.MetadataToken.ToInt32()),
                                        new CilInstruction(CilOpCodes.Call, redirectStubMethod),
                                        new CilInstruction(CilOpCodes.Ret) 
                                    }
                                };
                                moduleType.Methods.Add(initializatorMethodDef);

                                method.CilMethodBody.Instructions[i].Operand = dummyMethod;
                                var globalTypeCctor = moduleType.GetOrCreateStaticConstructor();
                                var randomIndex = m_Random.Next(0, globalTypeCctor.CilMethodBody.Instructions.CountWithoutRet());
                                globalTypeCctor.CilMethodBody.Instructions.Insert(randomIndex, new CilInstruction(CilOpCodes.Call, initializatorMethodDef));
                            }
                        }
                    }
                }
            }
        }
        return Task.CompletedTask;
    }
}