namespace BitMono.Core.Configuration;

public class JsonConfigurationProviderEx : FileConfigurationProvider
{
    private readonly JsonConfigurationSourceEx _source;
    private const string ParseMethodName = "Parse";
    private const string ParserTypeName = "Microsoft.Extensions.Configuration.Json.JsonConfigurationFileParser";
    private static readonly Type ParserType = typeof(JsonConfigurationProvider).Assembly.GetType(ParserTypeName);
    private static readonly MethodInfo ParseMethodInfo = ParserType.GetMethod(ParseMethodName,
        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

    public JsonConfigurationProviderEx(JsonConfigurationSourceEx source) : base(source)
    {
        _source = source;
    }

    public override void Load(Stream stream)
    {
        using (var streamReader = new StreamReader(stream, true))
        {
            var text = streamReader.ReadToEnd();
            PreProcessJson(ref text);
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream, streamReader.CurrentEncoding))
            {
                streamWriter.Write(text);
                streamWriter.Flush();
                memoryStream.Seek(0L, SeekOrigin.Begin);
                try
                {
                    var parseMethod = (Func<MemoryStream, IDictionary<string, string>>)Delegate.CreateDelegate(
                        typeof(Func<MemoryStream, IDictionary<string, string>>), ParseMethodInfo);
                    Data = parseMethod.Invoke(memoryStream);
                }
                catch (JsonException ex)
                {
                    throw new FormatException("Could not parse the JSON file: " + ex.Message + ".", ex);
                }
            }
        }
    }
    private void PreProcessJson(ref string json)
    {
        if (_source.Variables == null)
        {
            return;
        }
        json = _source.Variables.Aggregate(json,
            (current, keyValuePair) => current.Replace("{{" + keyValuePair.Key + "}}", keyValuePair.Value));
    }
}