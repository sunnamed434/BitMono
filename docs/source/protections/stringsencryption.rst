StringsEncryption
=================

How it works?
-------------

Protection encrypts strings using basic AES encryption, but not everyone like it because it makes the worse performance of application, but can be used with AntiDecompiler to crash dnSpy while analyzing the used class, also makes the RVA of the byte[] 0

Protection Type
---------------

The protection type is `Protection`.


.. warning::

    This protection slows down the application a lot.

IL2CPP
------

Works on IL2CPP builds: it runs before ``il2cpp.exe``, so plaintext strings are removed from ``global-metadata.dat`` and the managed decryptor is AOT-compiled to C++ (see :doc:`../protection-list/unity`).