ObjectReturnType
================

How it works?
-------------

Protection changes the nonvoid method return types to object return types.

Protection Type
---------------

The protection type is `Protection`.

IL2CPP
------

Not supported on IL2CPP builds. It changes return types to ``System.Object`` without boxing the value, which ``il2cpp.exe``'s static IL-to-C++ converter rejects. BitMono skips it automatically when building for IL2CPP (see :doc:`../protection-list/unity`).