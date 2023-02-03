namespace BitMono.Obfuscation;

public class OutputFilePathFactory
{
    public static string Create(BitMonoContext context)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(Path.GetFileNameWithoutExtension(context.FileName));
        if (context.Watermark)
        {
            stringBuilder.Append("_bitmono");
        }
        stringBuilder.Append(Path.GetExtension(context.FileName));
        return Path.Combine(context.OutputDirectoryName, stringBuilder.ToString());
    }
}