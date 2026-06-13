namespace BitMono.Protections;

public class DotNetHook : Protection
{
    private readonly Renamer _renamer;
    private readonly RandomNext _randomNext;

    public DotNetHook(Renamer renamer, RandomNext randomNext, IBitMonoServiceProvider serviceProvider) : base(serviceProvider)
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
                // newobj allocates and invokes a constructor; it cannot be redirected
                // to a static stub, so leave object construction untouched.
                if (instruction.OpCode.Code == CilCode.Newobj)
                {
                    continue;
                }
                var callingMethod = callingOperandMethod.ResolveOrNull();
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
                // Skip generic methods. The cloned signature carries
                // GenericParameterCount > 0, but we would not attach
                // corresponding GenericParameter definitions to the
                // generated dummyMethod. AsmResolver then rejects the
                // assembly during WriteModule with:
                //   "Method defines 0 generic parameters but its signature
                //   defines N parameters."
                // Fully cloning the GenericParameters (including their
                // constraints) is non-trivial, so we conservatively skip
                // these calls.
                if (callingMethod.Signature.GenericParameterCount > 0)
                {
                    continue;
                }
                if (module.TryLookupMember(callingMethod.MetadataToken, out var callingMethodMetadata) == false)
                {
                    continue;
                }

                // If the original method is an instance method, its signature
                // carries the HasThis flag. Previously callingMethod.Signature
                // was reused by reference while the dummy method was forced to
                // IsStatic = true, which produced inconsistent metadata
                // ("Method is static but its signature has the HasThis flag
                // set."). Under .NET 10 / ASP.NET Core this triggers hundreds
                // of WriteModuleAsync errors. We now rebuild the signature
                // explicitly as static, prepending "this" as a regular first
                // parameter so the IL call stack at the caller stays the same.
                var originalSignature = callingMethod.Signature;
                MethodSignature dummySignature;
                bool prependThis = originalSignature.HasThis;
                if (prependThis)
                {
                    var declaringType = callingMethod.DeclaringType!;
                    // The declaring type becomes the first ("this") parameter of the
                    // static stub. Skip cases we cannot faithfully represent here: an
                    // open generic declaring type (e.g. List<T>) has no concrete
                    // signature, and a value type takes a managed-pointer "this" (&T)
                    // that also interacts with constrained. call prefixes. Both are
                    // conservatively skipped rather than emitting broken metadata.
                    if (declaringType.GenericParameters.Count > 0 || declaringType.IsValueType)
                    {
                        continue;
                    }
                    var paramTypes = new List<TypeSignature>(originalSignature.ParameterTypes.Count + 1)
                    {
                        declaringType.ToTypeSignature()
                    };
                    paramTypes.AddRange(originalSignature.ParameterTypes);
                    dummySignature = MethodSignature.CreateStatic(
                        originalSignature.ReturnType,
                        originalSignature.GenericParameterCount,
                        paramTypes.ToArray());
                }
                else
                {
                    dummySignature = originalSignature;
                }

                // The attributes have to be correct at construction time
                // because AsmResolver verifies them against the signature
                // inside MethodDefinition's constructor. Setting
                // "IsStatic = true" later via an object initializer is too
                // late and would throw "An instance method requires a
                // signature with the HasThis flag set." Strip all
                // visibility/static-related flags from the original method
                // and set Assembly + Static directly.
                const MethodAttributes visibilityMask =
                    MethodAttributes.MemberAccessMask |
                    MethodAttributes.Static |
                    MethodAttributes.Virtual |
                    MethodAttributes.Final |
                    MethodAttributes.Abstract |
                    MethodAttributes.NewSlot;
                var dummyAttributes = (callingMethod.Attributes & ~visibilityMask)
                    | MethodAttributes.Assembly
                    | MethodAttributes.Static;

                var dummyMethod = new MethodDefinition(_renamer.RenameUnsafely(), dummyAttributes, dummySignature)
                    .AssignNextAvailableToken(module);
                moduleType.Methods.Add(dummyMethod);
                if (prependThis)
                {
                    dummyMethod.ParameterDefinitions.Add(new ParameterDefinition(1, "this", 0));
                }
                foreach (var actualParameter in callingMethod.ParameterDefinitions)
                {
                    var parameter = new ParameterDefinition(
                        prependThis ? (ushort)(actualParameter.Sequence + 1) : actualParameter.Sequence,
                        actualParameter.Name, actualParameter.Attributes);
                    dummyMethod.ParameterDefinitions.Add(parameter);
                }
                var dummyMethodBody = dummyMethod.CilMethodBody = new CilMethodBody();
                if (callingMethod.Signature.ReturnsValue)
                {
                    dummyMethodBody.Instructions.Add(new CilInstruction(CilOpCodes.Ldnull));
                }
                dummyMethodBody.Instructions.Add(new CilInstruction(CilOpCodes.Ret));
                var signature = MethodSignature.CreateStatic(systemVoid);
                var initializationMethod = new MethodDefinition( _renamer.RenameUnsafely(), MethodAttributes.Assembly | MethodAttributes.Static, signature)
                {
                    CilMethodBody = new CilMethodBody
                    {
                        Instructions =
                        {
                            new CilInstruction(CilOpCodes.Ldc_I4, dummyMethod.MetadataToken.ToInt32()),
                            new CilInstruction(CilOpCodes.Ldc_I4, callingMethodMetadata.MetadataToken.ToInt32()),
                            new CilInstruction(CilOpCodes.Call, redirectStubMethod),
                            new CilInstruction(CilOpCodes.Ret)
                        }
                    }
                };
                moduleType.Methods.Add(initializationMethod);

                instruction.Operand = dummyMethod;
                // The stub is static; a callvirt at the original instance call site would
                // be invalid IL, so normalize it to a plain call. The hook already binds to
                // a single resolved target, so dispatch semantics are unchanged.
                if (instruction.OpCode.Code == CilCode.Callvirt)
                {
                    instruction.OpCode = CilOpCodes.Call;
                }
                var randomIndex = _randomNext(0, moduleCctor.CilMethodBody!.Instructions.CountWithoutRet());
                moduleCctor.CilMethodBody.Instructions.Insert(randomIndex,
                    new CilInstruction(CilOpCodes.Call, initializationMethod));
            }
        }
        return Task.CompletedTask;
    }
}