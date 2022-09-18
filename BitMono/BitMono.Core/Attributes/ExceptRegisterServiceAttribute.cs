using System;

namespace BitMono.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class ExceptRegisterServiceAttribute : Attribute
    {
    }
}