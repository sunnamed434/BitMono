namespace BitMono.CLI.Modules;

internal class CLIBitMonoModuleFileResolver
{
    public static string? Resolve(string[] args)
    {
        string? file = null;
        if (args.IsEmpty() == false)
        {
            file = PathFormatterUtility.Format(args[0]);
        }
        return file;
    }
}