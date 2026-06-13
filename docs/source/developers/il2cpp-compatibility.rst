IL2CPP Compatibility
====================

On a Unity **IL2CPP** build the managed ``Assembly-CSharp.dll`` is not shipped: ``il2cpp.exe`` consumes it
and converts it to C++ (``GameAssembly.dll``). BitMono obfuscates the assembly *before* that conversion, so
pure managed/metadata protections (renaming, string encryption, ...) carry through into
``global-metadata.dat``. Protections that emit native code, ``calli``, pack the PE, or otherwise produce
output ``il2cpp.exe`` can't handle would break the build (or only affect the managed PE that IL2CPP throws
away), so they must be skipped on IL2CPP builds.

You mark such a protection with ``[IL2CPPIncompatible]``. When BitMono runs in IL2CPP mode (the Unity
integration sets it automatically when the scripting backend is IL2CPP, or you pass ``--il2cpp`` /
``"IL2CPP": true``), every protection marked this way is skipped and the reason is logged.

.. code-block:: csharp

    [IL2CPPIncompatible("Emits calli, which IL2CPP's AOT compiler does not support")]
    public class CallToCalli : Protection

The reason you pass is shown to the user so they understand why the protection was skipped:

.. code-block:: text

    [IL2CPP] CallToCalli - Emits calli, which IL2CPP's AOT compiler does not support

Under the hood a protection is treated as IL2CPP-incompatible when it is marked with
``[IL2CPPIncompatible]`` **or** with :doc:`native-code` (``[ConfigureForNativeCode]``) - native method
bodies can never be converted to C++, so they are always excluded even without an explicit attribute.

.. code-block:: csharp

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class IL2CPPIncompatibleAttribute : Attribute
    {
        public IL2CPPIncompatibleAttribute(string reason = "")
        {
            Reason = reason;
        }

        public string Reason { get; }

        public string GetMessage()
        {
            return string.IsNullOrWhiteSpace(Reason)
                ? "Not supported on IL2CPP builds"
                : Reason;
        }
    }

.. note::

   The attribute is read automatically by reflection, the same way as
   :doc:`protection-runtime-moniker` - you don't register anything. Add it on top of the protection (a
   built-in one or a :doc:`plugin <plugins>`) and BitMono skips it on IL2CPP builds while still running it
   normally on Mono / standalone .NET builds.

If a protection only does pure managed metadata/IL edits that ``il2cpp.exe`` can parse (renaming, clearing
namespaces, replacing ``ldstr`` with a managed decryptor call, ...), **don't** mark it - it should keep
running on IL2CPP so its effect lands in ``global-metadata.dat``.
