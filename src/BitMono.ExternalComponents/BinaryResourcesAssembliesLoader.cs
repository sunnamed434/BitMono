using System;
using System.Reflection;

namespace BitMono.ExternalComponents
{
    public static class BinaryResourcesAssembliesLoader
    {
        public static void Load()
        {
            var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (var resource in resources)
            {
                if (resource.StartsWith("bin"))
                {
                    Assembly.Load(ManifestResourcesReader.Read(resource));
                }
            }
        }
    }
}