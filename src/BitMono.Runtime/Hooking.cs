namespace BitMono.Runtime;

internal struct Hooking
{
    internal static void RedirectStub(int from, int to)
    {
        var fromMethodHandle = typeof(Hooking).Module.ResolveMethod(from).MethodHandle;
        var toMethodHandle = typeof(Hooking).Module.ResolveMethod(to).MethodHandle;
        RuntimeHelpers.PrepareMethod(fromMethodHandle);
        RuntimeHelpers.PrepareMethod(toMethodHandle);

        var fromPtr = fromMethodHandle.GetFunctionPointer();
        var toPtr = toMethodHandle.GetFunctionPointer();

        if (Environment.OSVersion.Platform is PlatformID.Unix or PlatformID.MacOSX)
        {
            const int PROT_EXEC = 0x04;
            const int PROT_READ = 0x01;
            const int PROT_WRITE = 0x02;

            mprotect(fromPtr, 5, PROT_READ | PROT_WRITE);

            MakeHook(fromPtr, toPtr);

            mprotect(fromPtr, 5, PROT_READ | PROT_EXEC);
        }
        else
        {
            VirtualProtect(fromPtr, (IntPtr)5, 0x40, out var oldProtect);

            MakeHook(fromPtr, toPtr);

            VirtualProtect(fromPtr, (IntPtr)5, oldProtect, out _);
        }
    }

    private static void MakeHook(IntPtr fromPtr, IntPtr toPtr)
    {
        const int x64BitProcess = 8;
        const int x32BitProcess = 4;
        if (IntPtr.Size == x64BitProcess)
        {
            Marshal.WriteByte(fromPtr, 0, 0x49);
            Marshal.WriteByte(fromPtr, 1, 0xBB);
            Marshal.WriteInt64(fromPtr, 2, toPtr.ToInt64());
            Marshal.WriteByte(fromPtr, 10, 0x41);
            Marshal.WriteByte(fromPtr, 11, 0xFF);
            Marshal.WriteByte(fromPtr, 12, 0xE3);
        }
        else if (IntPtr.Size == x32BitProcess)
        {
            Marshal.WriteByte(fromPtr, 0, 0xE9);
            Marshal.WriteInt32(fromPtr, 1, toPtr.ToInt32() - fromPtr.ToInt32() - 5);
            Marshal.WriteByte(fromPtr, 5, 0xC3);
        }
    }

    [DllImport("libc.so.6", EntryPoint = nameof(mprotect))]
    internal static extern int mprotect(IntPtr start, ulong len, int prot);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi, EntryPoint = nameof(VirtualProtect))]
    internal static extern bool VirtualProtect(IntPtr address, IntPtr size, uint newProtect, out uint oldProtect);
}