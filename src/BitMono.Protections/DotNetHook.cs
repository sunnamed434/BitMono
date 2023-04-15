namespace BitMono.Protections;

[UsedImplicitly]
public class DotNetHook : Protection
{
    private readonly Renamer _renamer;
    private readonly RandomNext _randomNext;

    public DotNetHook(Renamer renamer, RandomNext randomNext, ProtectionContext context) : base(context)
    {
        _renamer = renamer;
        _randomNext = randomNext;
    }

    [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
    [SuppressMessage("ReSharper", "InvertIf")]
    public override Task ExecuteAsync(ProtectionParameters parameters)
    {
        var runtimeHookingType = Context.RuntimeModule.ResolveOrThrow<TypeDefinition>(typeof(Hooking));
        var runtimeRedirectStubMethod = runtimeHookingType.Methods.Single(c => c.Name.Equals(nameof(Hooking.RedirectStub)));
        var listener = new ModifyInjectTypeClonerListener(ModifyFlags.All, _renamer, Context.Module);
        var memberCloneResult = new MemberCloner(Context.Module, listener)
            .Include(runtimeHookingType)
            .Clone();
        var redirectStubMethod = memberCloneResult.GetClonedMember(runtimeRedirectStubMethod);

        var factory = Context.Module.CorLibTypeFactory;
        var systemVoid = factory.Void;

        var moduleType = Context.Module.GetOrCreateModuleType();
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
                            && callingMethod.ParameterDefinitions.Any(p => p.IsIn || p.IsOut) == false && callingMethod.Managed)
                        {
                            if (Context.Module.TryLookupMember(callingMethod.MetadataToken, out var callingMethodMetadata))
                            {
                                var dummyMethod = new MethodDefinition(_renamer.RenameUnsafely(), callingMethod.Attributes, callingMethod.Signature)
                                {
                                    IsAssembly = true,
                                    IsStatic = true
                                }.AssignNextAvailableToken(Context.Module);
                                moduleType.Methods.Add(dummyMethod);
                                var parameterDefinitions = callingMethod.ParameterDefinitions;
                                for (var j = 0; j < parameterDefinitions.Count; j++)
                                {
                                    var actualParameter = parameterDefinitions[j];
                                    var parameter = new ParameterDefinition(actualParameter.Sequence,
                                        actualParameter.Name, actualParameter.Attributes);
                                    dummyMethod.ParameterDefinitions.Add(parameter);
                                }
                                var dummyMethodBody = dummyMethod.CilMethodBody = new CilMethodBody(dummyMethod);
                                if (callingMethod.Signature.ReturnsValue)
                                {
                                    dummyMethodBody.Instructions.Add(new CilInstruction(CilOpCodes.Ldnull));
                                }
                                dummyMethodBody.Instructions.Add(new CilInstruction(CilOpCodes.Ret));
                                var signature = MethodSignature.CreateStatic(systemVoid);
                                var attributes = MethodAttributes.Assembly | MethodAttributes.Static;
                                var initializationMethod = new MethodDefinition(_renamer.RenameUnsafely(), attributes, signature);
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
                                var randomIndex = _randomNext(0, moduleCctor.CilMethodBody.Instructions.CountWithoutRet());
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