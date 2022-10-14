using System.IO;

namespace BitMono.GUI.API
{
    public interface ILogTextContainer
    {
        StringWriter StringWriter { get; }
    }
}