BitMono
=======

How it works?
-------------
Protection modifies the file metadata (PE) to make it unrecognizable for decompilers or other tools such as Detect It Easy, as the result most of the tools will be fooled to think that this is an MS-DOS Executable as Detect It Easy does, decompilers will just not be able to open it up.

Mono doesn't care about the things which decompilers/tools care about, and because of that it does what it does.


.. warning::

    This protection compatible only with Mono.