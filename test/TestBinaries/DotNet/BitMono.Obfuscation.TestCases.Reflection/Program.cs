using System;
using System.Reflection;

namespace BitMono.Obfuscation.TestCases.Reflection
{
    // A deliberately reflection-heavy program. Its reflection targets are private/internal members
    // and an internal type that renaming WOULD scramble - so if BitMono's reflection analysis fails to
    // exclude them, the lookups return null and Main exits non-zero. The obfuscation integration test
    // obfuscates this exe (renaming + IL protections on) and asserts it still exits 0.
    public static class Program
    {
        public static int Main()
        {
            var failed = 0;
            void Check(string name, bool ok)
            {
                if (!ok)
                {
                    failed++;
                    Console.Error.WriteLine("FAIL: " + name);
                }
            }

            var type = typeof(SecretBox);
            var instance = new SecretBox();

            // GetMethod + Invoke on a private instance method.
            var compute = type.GetMethod("Compute", BindingFlags.NonPublic | BindingFlags.Instance);
            Check("GetMethod(Compute)", compute != null);
            Check("Invoke(Compute)", compute != null && (int)compute.Invoke(instance, new object[] { 20 }) == 42);

            // GetField + GetValue on a private field.
            var seed = type.GetField("_seed", BindingFlags.NonPublic | BindingFlags.Instance);
            Check("GetField(_seed)", seed != null && (int)seed.GetValue(instance) == 22);

            // GetProperty on a private property.
            var label = type.GetProperty("Label", BindingFlags.NonPublic | BindingFlags.Instance);
            Check("GetProperty(Label)", label != null && (string)label.GetValue(instance) == "box");

            // Type.GetType(string) + Activator on an internal type (its name must survive renaming).
            var pluginType = Type.GetType("BitMono.Obfuscation.TestCases.Reflection.Plugin");
            Check("GetType(Plugin)", pluginType != null);
            Check("Activator(Plugin)", pluginType != null && Activator.CreateInstance(pluginType) is Plugin);

            // Delegate.CreateDelegate by method name (static).
            var doubler = Delegate.CreateDelegate(typeof(Func<int, int>), typeof(SecretBox), "Double") as Func<int, int>;
            Check("CreateDelegate(Double)", doubler != null && doubler(21) == 42);

            // Enum.Parse / Enum.GetName rely on enum field names.
            Check("Enum.Parse(Fast)", (Mode)Enum.Parse(typeof(Mode), "Fast") == Mode.Fast);
            Check("Enum.GetName(Slow)", Enum.GetName(typeof(Mode), Mode.Slow) == "Slow");

            // RuntimeReflectionExtensions on a public method.
            var runtimeMethod = type.GetRuntimeMethod("PublicCompute", new[] { typeof(int) });
            Check("GetRuntimeMethod(PublicCompute)", runtimeMethod != null);

            // Nested type lookup by name.
            var inner = type.GetNestedType("Inner", BindingFlags.NonPublic);
            Check("GetNestedType(Inner)", inner != null);

            // Reading raw IL back via reflection must keep working (and stay valid under IL protections).
            var il = compute?.GetMethodBody()?.GetILAsByteArray();
            Check("GetILAsByteArray", il != null && il.Length > 0);

            if (failed == 0)
            {
                Console.WriteLine("OK");
                return 0;
            }
            Console.Error.WriteLine(failed + " reflection check(s) failed");
            return 1;
        }
    }

    public class SecretBox
    {
        private int _seed = 22;
        private string Label => "box";
        private int Compute(int x) => x + _seed;
        public int PublicCompute(int x) => x + _seed;
        public static int Double(int x) => x * 2;

        private class Inner
        {
        }
    }

    internal class Plugin
    {
    }

    public enum Mode
    {
        Slow,
        Fast
    }
}
