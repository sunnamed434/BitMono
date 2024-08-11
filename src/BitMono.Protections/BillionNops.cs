namespace BitMono.Protections;

public class BillionNops : Protection
{
    private readonly Renamer _renamer;

    public BillionNops(Renamer renamer, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _renamer = renamer;
    }

    public override Task ExecuteAsync()
    {
        var module = Context.Module;
        var moduleType = module.GetOrCreateModuleType();
        var factory = module.CorLibTypeFactory;
        var method = new MethodDefinition(_renamer.RenameUnsafely(), MethodAttributes.Public | MethodAttributes.Static,
            MethodSignature.CreateStatic(factory.Void));
        moduleType.Methods.Add(method);
        var body = method.CilMethodBody = new CilMethodBody(method);
        for (var i = 0; i < 100000; i++)
        {
            Context.ThrowIfCancellationTokenRequested();

            body.Instructions.Insert(0, new CilInstruction(CilOpCodes.Nop));
        }
        body.Instructions.Add(new CilInstruction(CilOpCodes.Ret));
        return Task.CompletedTask;
    }
}