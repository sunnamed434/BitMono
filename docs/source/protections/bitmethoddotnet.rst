BitMethodDotnet
===============

How it works?
-------------

Protection adds invalid IL code in the file, as the result in the old dnSpy version it's going to be harder to see the C# code of the method body.

Protection Type
---------------

The protection type is `Protection`.


.. warning::

    This protection compatible only with Mono, but, as far as is known this protection can be used in old versions of .NET Framework, but literally not with .NET Core, etc.

IL2CPP
------

Not supported on IL2CPP builds. It inserts dead IL prefix opcodes that aren't attached to a valid instruction, which ``il2cpp.exe`` rejects as malformed IL. BitMono skips it automatically when building for IL2CPP (see :doc:`../protection-list/unity`).