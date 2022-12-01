using System;

namespace BitMono.Core.Protecting.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ProtectionNameAttribute : Attribute
    {
        public ProtectionNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}