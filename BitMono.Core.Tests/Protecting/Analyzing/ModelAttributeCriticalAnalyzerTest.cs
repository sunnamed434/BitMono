using BitMono.Core.Protecting.Analyzing;
using BitMono.Core.Protecting.Injection;
 using System;
 using System.Collections.Generic;
 using System.IO;
 using System.Text;
 using System.Xml.Serialization;
 using Xunit;
 using AsmResolver.DotNet;
 using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
 using BitMono.API.Configuration;
 using BitMono.Core.Protecting.Resolvers;
 using BitMono.Core.Tests.Properties;
 using BitMono.Shared.Models;
 using Microsoft.Extensions.Configuration;
 using Newtonsoft.Json;

namespace BitMono.Core.Tests.Protecting.Analyzing;

public class TestBitMonoCriticalsConfiguration : IBitMonoCriticalsConfiguration
{
    public TestBitMonoCriticalsConfiguration(string json)
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json)))
            .Build();
    }

    public IConfiguration Configuration { get; }
}

public class ModelAttributeCriticalAnalyzerTest
{
    public static IEnumerable<object[]> GetAttributes()
    {
        yield return new object[] { nameof(SerializableAttribute), typeof(SerializableAttribute).Namespace };
        yield return new object[] { nameof(XmlAttributeAttribute), typeof(XmlAttributeAttribute).Namespace };
        yield return new object[] { nameof(XmlArrayItemAttribute), typeof(XmlArrayItemAttribute).Namespace };
        yield return new object[] { nameof(JsonPropertyAttribute), typeof(JsonPropertyAttribute).Namespace };
    }

    [Theory]
    [MemberData(nameof(GetAttributes))]
    public void WhenModelCriticalAnalyzing_AndModelIsCritical_ThenShouldBeFalse(string name, string @namespace)
    {
        var module = ModuleDefinition.FromBytes(Resources.HelloWorldLib);

        var criticals = new Criticals()
        {
            UseCriticalModelAttributes = true,
            CriticalModelAttributes = new List<CriticalAttribute>
            {
                new CriticalAttribute
                {
                    Namespace = @namespace,
                    Name = name
                },
            }
        };

        var json = JsonConvert.SerializeObject(criticals);
        var configuration = new TestBitMonoCriticalsConfiguration(json);
        var attemptAttributeResolver = new AttemptAttributeResolver(new CustomAttributeResolver());
        var criticalAnalyzer = new ModelAttributeCriticalAnalyzer(configuration, attemptAttributeResolver);
        
        var type = new TypeDefinition(string.Empty, Guid.NewGuid().ToString(), TypeAttributes.Public);
        module.TopLevelTypes.Add(type);
        var injector = new MscorlibInjector();
        injector.InjectAttribute(module, @namespace, name, type);

        var value = criticalAnalyzer.NotCriticalToMakeChanges(type);
        Assert.False(value);
    }
}