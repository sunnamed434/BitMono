using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BitMono.ExternalComponents
{
    public static class Hooking
    {
        public static void RedirectStub(int from, int to)
        {
            var fromHandle = typeof(Module).Module.ResolveMethod(from).MethodHandle;
            var toHandle = typeof(Module).Module.ResolveMethod(to).MethodHandle;
            RuntimeHelpers.PrepareMethod(fromHandle);
            RuntimeHelpers.PrepareMethod(toHandle);

            var fromPtr = fromHandle.GetFunctionPointer();
            var toPtr = toHandle.GetFunctionPointer();

            VirtualProtect(fromPtr, (IntPtr)5, 0x40, out uint oldProtect);

            if (IntPtr.Size == 8)
            {
                Marshal.WriteByte(fromPtr, 0, 0x49);
                Marshal.WriteByte(fromPtr, 1, 0xBB);

                Marshal.WriteInt64(fromPtr, 2, toPtr.ToInt64());

                Marshal.WriteByte(fromPtr, 10, 0x41);
                Marshal.WriteByte(fromPtr, 11, 0xFF);
                Marshal.WriteByte(fromPtr, 12, 0xE3);
            }
            else if (IntPtr.Size == 4)
            {
                Marshal.WriteByte(fromPtr, 0, 0xE9);
                Marshal.WriteInt32(fromPtr, 1, toPtr.ToInt32() - fromPtr.ToInt32() - 5);
                Marshal.WriteByte(fromPtr, 5, 0xC3);
            }

            VirtualProtect(fromPtr, (IntPtr)5, oldProtect, out _);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern bool VirtualProtect(IntPtr address, IntPtr size, uint newProtect, out uint oldProtect);
    }
}