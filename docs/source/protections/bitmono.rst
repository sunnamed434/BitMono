BitMono
=======

How it works?
-------------

Protection modifies the file metadata (PE) to make it unrecognizable for decompilers or other tools such as Detect It Easy, as the result most of the tools will be fooled to think that this is an MS-DOS Executable as Detect It Easy does, decompilers will just not be able to open it up.

Mono doesn't care about the things which decompilers/tools care about, and because of that it does what it does.

Protection Type
---------------

The protection type is `Packer`.


.. warning::

    This protection compatible only with Mono.

IL2CPP
------

Not supported on IL2CPP builds. It packs the managed PE so only the Mono loader can read it, but ``il2cpp.exe`` must parse the assembly. BitMono skips it automatically when building for IL2CPP (see :doc:`../protection-list/unity`).