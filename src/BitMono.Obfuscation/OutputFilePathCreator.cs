namespace BitMono.Obfuscation;

public class OutputFilePathCreator
{
    public BitMonoContext Create(BitMonoContext context)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(Path.GetFileNameWithoutExtension(context.FileName));
        if (context.Watermark)
        {
            stringBuilder.Append("_bitmono");
        }
        stringBuilder.Append(Path.GetExtension(context.FileName));
        var outputFile = Path.Combine(context.OutputDirectoryName, stringBuilder.ToString());
        context.OutputFile = outputFile;
        return context;
    }
}