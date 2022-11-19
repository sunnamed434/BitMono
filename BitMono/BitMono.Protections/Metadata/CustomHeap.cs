using dnlib.DotNet.Writer;
using dnlib.IO;
using dnlib.PE;
using System;
using System.Text;

namespace BitMono.Protections.Metadata
{
    public class CustomHeap : IHeap
    {
        private string _name;
        private FileOffset _offset;
        private RVA _rva;
        private byte[] _data;

        public CustomHeap(string name) : this(name, Array.Empty<byte>())
        {
        }
        public CustomHeap(string name, string data) : this(name, Encoding.ASCII.GetBytes(data))
        {
        }
        public CustomHeap(string name, byte[] data)
        {
            _name = name;
            _data = data;
        }

        public string Name => _name;
        public bool IsEmpty => false;
        public FileOffset FileOffset => _offset;
        public RVA RVA => _rva;

        public void SetReadOnly()
        {
            throw new NotImplementedException();
        }
        public void SetOffset(FileOffset offset, RVA rva)
        {
            _offset = offset;
            _rva = rva;
        }
        public uint GetFileLength()
        {
            return (uint)_data.Length;
        }
        public uint GetVirtualSize()
        {
            return GetFileLength();
        }
        public void WriteTo(DataWriter writer)
        {
            writer.WriteBytes(_data);
        }
    }
}