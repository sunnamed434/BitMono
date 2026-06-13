DotNetHook
==========

How it works?
-------------

Protection hooks methods, as a result, will call empty methods but, in fact, a completely different method will be called (the original one).

Protection Type
---------------

The protection type is `Protection`.

IL2CPP
------

Not supported on IL2CPP builds. It installs runtime method detours by overwriting JIT-compiled native code, and IL2CPP is AOT-compiled with no JIT to hook. BitMono skips it automatically when building for IL2CPP (see :doc:`../protection-list/unity`).