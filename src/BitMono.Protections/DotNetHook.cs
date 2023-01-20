namespace BitMono.Protections;

public class DotNetHook : IProtection
{
    private readonly IRenamer m_Renamer;
    private readonly Random m_Random;

    public DotNetHook(IRenamer renamer, RuntimeImplementations runtime)
    {
        m_Renamer = renamer;
        m_Random = runtime.Random;
    }

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters)
    {
        var runtimeHookingType = context.RuntimeModule.ResolveOrThrow<TypeDefinition>(typeof(Hooking));
        var runtimeRedirectStubMethod = runtimeHookingType.Methods.Single(c => c.Name.Equals(nameof(Hooking.RedirectStub)));
        var listener = new ModifyInjectTypeClonerListener(ModifyFlags.All, m_Renamer, context.Module);
        var memberCloneResult = new MemberCloner(context.Module, listener)
            .Include(runtimeHookingType)
            .Clone();
        var redirectStubMethod = memberCloneResult.GetClonedMember(runtimeRedirectStubMethod);

        var factory = context.Module.CorLibTypeFactory;
        var systemVoid = factory.Void;

        var moduleType = context.Module.GetOrCreateModuleType();
        var moduleCctor = moduleType.GetOrCreateStaticConstructor();
        foreach (var method in parameters.Members.OfType<MethodDefinition>())
        {
            if (method.CilMethodBody is { } body)
            {
                for (var i = 0; i < body.Instructions.Count; i++)
                {
                    var instruction = body.Instructions[i];
                    if (instruction.OpCode.FlowControl == CilFlowControl.Call && instruction.Operand is IMethodDescriptor callingOperandMethod)
                    {
                        var callingMethod = callingOperandMethod.Resolve();
                        if (callingMethod is { CilMethodBody: { } } 
                            && callingMethod.ParameterDefinitions.Any(p => p.IsIn || p.IsOut) == false)
                        {
                            if (context.Module.TryLookupMember(callingMethod.MetadataToken, out var callingMethodMetadata))
                            {
                                var dummyMethod = new MethodDefinition(m_Renamer.RenameUnsafely(), callingMethod.Attributes, callingMethod.Signature)
                                {
                                    IsAssembly = true,
                                    IsStatic = true
                                }.AssignNextAvailableToken(context.Module);
                                moduleType.Methods.Add(dummyMethod);
                                var parameterDefinitions = callingMethod.ParameterDefinitions;
                                for (var j = 0; j < parameterDefinitions.Count; j++)
                                {
                                    var parameter = parameterDefinitions[i];
                                    dummyMethod.ParameterDefinitions.Add(new ParameterDefinition(parameter.Sequence, parameter.Name, parameter.Attributes));
                                }
                                var dummyMethodBody = dummyMethod.CilMethodBody = new CilMethodBody(dummyMethod);
                                if (callingMethod.Signature.ReturnsValue)
                                {
                                    dummyMethodBody.Instructions.Add(new CilInstruction(CilOpCodes.Ldnull));
                                }
                                dummyMethodBody.Instructions.Add(new CilInstruction(CilOpCodes.Ret));

                                var signature = MethodSignature.CreateStatic(systemVoid);
                                var attributes = MethodAttributes.Assembly | MethodAttributes.Static;
                                var initializationMethod = new MethodDefinition(m_Renamer.RenameUnsafely(), attributes, signature);
                                initializationMethod.CilMethodBody = new CilMethodBody(initializationMethod)
                                {
                                    Instructions =
                                    {
                                        new CilInstruction(CilOpCodes.Ldc_I4, dummyMethod.MetadataToken.ToInt32()),
                                        new CilInstruction(CilOpCodes.Ldc_I4, callingMethodMetadata.MetadataToken.ToInt32()),
                                        new CilInstruction(CilOpCodes.Call, redirectStubMethod),
                                        new CilInstruction(CilOpCodes.Ret) 
                                    }
                                };
                                moduleType.Methods.Add(initializationMethod);

                                instruction.Operand = dummyMethod;
                                var randomIndex = m_Random.Next(0, moduleCctor.CilMethodBody.Instructions.CountWithoutRet());
                                moduleCctor.CilMethodBody.Instructions.Insert(randomIndex, new CilInstruction(CilOpCodes.Call, initializationMethod));
                            }
                        }
                    }
                }
            }
        }
        return Task.CompletedTask;
    }
}