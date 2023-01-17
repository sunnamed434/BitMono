namespace BitMono.CLI.Modules;

internal class CLIBitMonoModuleFileResolver
{
    [return: AllowNull]
    public string Resolve(string[] args)
    {
        string file = null;
        if (args?.Any() == true)
        {
            file = args[0];
        }
        return file;
    }
}