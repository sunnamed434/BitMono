BitTimeDateStamp
================

How it works?
-------------

Protection modifies the file metadata (PE) and erases the TimeDateStamp, as the result no one will be able to know when this file was compiled.

Protection Type
---------------

The protection type is `Packer`.

IL2CPP
------

Not supported on IL2CPP builds. It only zeroes the managed PE's timestamp, which IL2CPP discards, so it has no effect on the IL2CPP output. BitMono skips it automatically when building for IL2CPP (see :doc:`../protection-list/unity`).