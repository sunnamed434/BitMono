using NullGuard;
using System;
using System.Runtime.CompilerServices;

namespace BitMono.Core.Logging
{
    public interface ILogger
    {
        void Debug(string message, [CallerMemberName] string caller = null);
        void Info(string message, [CallerMemberName] string caller = null);
        void Warn(string message, [CallerMemberName] string caller = null);
        void Error(Exception ex, string message = null, [CallerMemberName] string caller = null);
    }
}