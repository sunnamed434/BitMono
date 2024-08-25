BitDotNet
=========

How it works?
-------------

The protection uses dnlib exploit and modifies the file metadata (PE) to make it unrecognizable for dnSpy, as the result, at first sight, it will look like not a .NET file, for example, a C++ file.

Mono doesn't care about the thing which dnlib care about, and because of that it does what it does

Protection Type
---------------

The protection type is `Packer`.


.. warning::

    This protection compatible only with Mono.