UnmanagedString
===============

How it works?
-------------

Protection creates native method with assembly code and protects strings only that can be encoded with `Windows-1252` encoding. 

Protection Type
---------------

The protection type is `Protection`.


.. warning::

    This protection is only compatible with .NET Framework and .NET Core, and doesn't work with Mono.

IL2CPP
------

Not supported on IL2CPP builds. It emits unmanaged (native) method bodies, which IL2CPP can't convert to C++. BitMono skips it automatically when building for IL2CPP (see :doc:`../protection-list/unity`).