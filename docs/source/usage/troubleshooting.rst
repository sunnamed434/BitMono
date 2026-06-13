Troubleshooting
===============

A few things that commonly go wrong, and how to fix them. If none of this helps, open an issue on
`GitHub <https://github.com/sunnamed434/BitMono/issues>`_ or ping us on Discord.

"Access Denied" when opening the output folder
----------------------------------------------

When BitMono is done it tries to open the output directory in your file explorer. If that throws an
"Access Denied", just turn it off, set ``OpenFileDestinationInFileExplorer`` to ``false`` in
``obfuscation.json``.

Missing dependencies
--------------------

BitMono needs your app's dependencies to understand the code. If it can't find them:

- Put every ``.dll`` your app references in the ``libs`` folder, next to the file you're obfuscating.
- Make sure the files are actually there and not locked by another process.
- If some dependency is genuinely optional and you can't get it, set ``FailOnNoRequiredDependency`` to
  ``false`` in ``obfuscation.json`` to continue anyway. Be careful though, skipping a real dependency
  can break the obfuscated app.

Framework mismatch
------------------

Use the BitMono build that matches your app's framework, otherwise the obfuscated file won't run:

- .NET app → .NET build of BitMono
- .NET Framework app → .NET Framework build
- Unity/Mono → .NET Framework build

More on this in :doc:`../obfuscationissues/compatibility`.

No protections enabled
----------------------

If BitMono says nothing is enabled, you didn't turn anything on. Enable at least one protection in
``protections.json`` (set ``Enabled`` to ``true``), or pass them on the command line with
``-p Protection1,Protection2``, or pick a preset with ``--preset``.

Can't write to the output directory
-----------------------------------

Make sure BitMono can actually write there: the path exists, it's not read-only, and nothing else has
the file open. A normal user folder is usually fine, you rarely need to run as administrator.

Still stuck?
------------

- Read the console output, BitMono usually tells you exactly what went wrong.
- Try the interactive mode, it shows more detailed prompts.
- Open an issue `here <https://github.com/sunnamed434/BitMono/issues>`_ and bring the log with you.
