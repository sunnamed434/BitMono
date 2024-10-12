namespace BitMono.Protections;

public class DotNetHook : Protection
{
    private readonly Renamer _renamer;
    private readonly RandomNext _randomNext;

    public DotNetHook(Renamer renamer, RandomNext randomNext, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _renamer = renamer;
        _randomNext = randomNext;
    }

    public override Task ExecuteAsync()
    {
        var module = Context.Module;
        var runtimeModule = Context.RuntimeModule;
        var runtimeHookingType = runtimeModule.ResolveOrThrow<TypeDefinition>(typeof(Hooking));
        var runtimeRedirectStubMethod = runtimeHookingType.Methods.Single(x => x.Name!.Equals(nameof(Hooking.RedirectStub)));
        var listener = new ModifyInjectTypeClonerListener(ModifyFlags.All, _renamer, module);
        var memberCloneResult = new MemberCloner(module, listener)
            .Include(runtimeHookingType)
            .CloneSafely(module, runtimeModule);
        var redirectStubMethod = memberCloneResult.GetClonedMember(runtimeRedirectStubMethod);

        var factory = module.CorLibTypeFactory;
        var systemVoid = factory.Void;

        var moduleType = module.GetOrCreateModuleType();
        var moduleCctor = moduleType.GetOrCreateStaticConstructor();
        foreach (var method in Context.Parameters.Members.OfType<MethodDefinition>())
        {
            if (method.CilMethodBody is not { } body)
            {
                continue;
            }

            for (var i = 0; i < body.Instructions.Count; i++)
            {
                var instruction = body.Instructions[i];
                if (instruction.OpCode.FlowControl != CilFlowControl.Call)
                {
                    continue;
                }
                if (instruction.Operand is not IMethodDescriptor callingOperandMethod)
                {
                    continue;
                }
                var callingMethod = callingOperandMethod.Resolve();
                if (callingMethod == null)
                {
                    continue;
                }
                if (callingMethod.CilMethodBody == null || callingMethod.Signature == null || callingMethod.Managed == false)
                {
                    continue;
                }
                if (callingMethod.ParameterDefinitions.Any(p => p.IsIn || p.IsOut))
                {
                    continue;
                }
                if (module.TryLookupMember(callingMethod.MetadataToken, out var callingMethodMetadata) == false)
                {
                    continue;
                }

                var dummyMethod = new MethodDefinition(_renamer.RenameUnsafely(), callingMethod.Attributes, callingMethod.Signature)
                {
                    IsAssembly = true,
                    IsStatic = true
                }.AssignNextAvailableToken(module);
                moduleType.Methods.Add(dummyMethod);
                foreach (var actualParameter in callingMethod.ParameterDefinitions)
                {
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
                var initializationMethod = new MethodDefinition(
                    _renamer.RenameUnsafely(),
                    MethodAttributes.Assembly | MethodAttributes.Static,
                    signature);
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
                var randomIndex = _randomNext(0, moduleCctor.CilMethodBody!.Instructions.CountWithoutRet());
                moduleCctor.CilMethodBody.Instructions.Insert(randomIndex,
                    new CilInstruction(CilOpCodes.Call, initializationMethod));
            }
        }
        return Task.CompletedTask;
    }
}