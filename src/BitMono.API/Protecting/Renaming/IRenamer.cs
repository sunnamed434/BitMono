namespace BitMono.API.Protecting.Renaming;

public interface IRenamer
{
    string RenameUnsafely();
    void Rename(IDnlibDef dnlibDef);
    void Rename(params IDnlibDef[] dnlibDefs);
    void Rename(IFullName fullName);
    void Rename(params IFullName[] fullNames);
    void Rename(IVariable variable);
}