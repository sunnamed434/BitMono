using dnlib.DotNet;

namespace BitMono.ExternalComponents
{
    public static class MetadataCustomStreamsResolver
    {
        private static readonly ModuleDefMD m_SelfModuleDefMD;

        static MetadataCustomStreamsResolver()
        {
            m_SelfModuleDefMD = ModuleDefMD.Load(typeof(MetadataCustomStreamsResolver).Module);
        }

        public static byte[] Resolve(int index)
        {
            return m_SelfModuleDefMD.Metadata.AllStreams[index].CreateReader().ReadRemainingBytes();
        }
    }
}