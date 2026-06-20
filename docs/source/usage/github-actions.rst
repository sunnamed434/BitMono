GitHub Actions (CI/CD)
======================

Obfuscate in your CI pipeline without touching your source or your ``.csproj``. You build your project
as usual, then point the action at the compiled assembly and it comes back obfuscated. Nothing about
obfuscation lives in your repo, no package reference, no MSBuild properties, no config files unless you
actually want them.

This is the opposite trade-off from :doc:`msbuild-integration`. The MSBuild package obfuscates *inside*
your build and travels with the project; the action obfuscates *outside* it, in the pipeline only. Pick
whichever fits, if you'd rather keep your project clean and only protect what ships from CI, this is it.

Under the hood the action just installs ``BitMono.GlobalTool`` from nuget.org and runs it, so you can do
the exact same thing by hand (see `Without the action`_ below). The action is only there to save you the
boilerplate.

Requirements
------------

The .NET SDK has to be on the runner. If you built your project in the same job you already have it,
otherwise add ``actions/setup-dotnet`` before the BitMono step. BitMono's tool runs on .NET 6–9; on a
.NET 10-only runner it still works, the action rolls forward automatically.

Quick start
-----------

.. code-block:: yaml

   jobs:
     build:
       runs-on: ubuntu-latest
       steps:
         - uses: actions/checkout@v7
         - uses: actions/setup-dotnet@v5
           with:
             dotnet-version: 9.x
         - run: dotnet build -c Release

         - uses: sunnamed434/BitMono@0.43.0   # pin the latest release tag
           with:
             file: bin/Release/net9.0/MyApp.dll
             preset: Maximum

That's it. The obfuscated assembly lands in ``bin/Release/net9.0/output/MyApp.dll`` (or set ``output:``
to put it elsewhere). Upload it with ``actions/upload-artifact``, ship it, whatever you need.

Inputs
------

Every input maps one-to-one to a CLI option, so anything ``BitMono.CLI`` can do you can do here. Only
``file`` is required.

.. list-table::
   :header-rows: 1
   :widths: 25 75

   * - Input
     - Description
   * - ``file``
     - **Required.** Path to the compiled assembly to obfuscate, e.g. ``bin/Release/net9.0/MyApp.dll``.
   * - ``output``
     - Output directory. Default: an ``output`` folder next to ``file``.
   * - ``output-name``
     - Output file name. Default: keeps the original name.
   * - ``libraries``
     - Space-separated dependency directories, e.g. ``bin/Release/net9.0``.
   * - ``protections``
     - Space-separated protections, e.g. ``FullRenamer StringsEncryption``. Overrides preset/JSON.
   * - ``preset``
     - Protection preset: ``Minimal``, ``Balanced`` or ``Maximum``.
   * - ``obfuscation-file`` / ``protections-file`` / ``criticals-file`` / ``logging-file``
     - Paths to the JSON config files (same formats as the CLI). Used only when you pass them.
   * - ``no-watermark``
     - Set to ``true`` to disable the BitMono watermark.
   * - ``no-logo``
     - Set to ``true`` to suppress the BitMono ASCII banner on startup (``--nologo``).
   * - ``strong-name-key``
     - Path to a ``.snk`` to re-sign the obfuscated assembly. See :doc:`assembly-signing`.
   * - ``extra-args``
     - Anything else passed straight to the CLI verbatim, for options not listed above.
   * - ``version``
     - ``BitMono.GlobalTool`` version to install. Default: latest. See `Versioning`_.

Prefer config files? It works the same as everywhere else, commit your ``protections.json`` /
``criticals.json`` / ``obfuscation.json`` and point the matching inputs at them. See :doc:`how-to-use`
for the schemas.

Versioning
----------

The action lives in the BitMono repo, so it doesn't have a version of its own, you reference it by a
BitMono release tag. ``sunnamed434/BitMono@0.43.0`` reads the action from the repo at tag ``0.43.0``.
Every release you'd normally cut is automatically a usable action version, nothing extra to publish.

There are two things you can pin, and they're independent:

- the **tag** after ``@`` decides which version of the action wrapper you run.
- the **``version``** input decides which version of the obfuscator (the nuget tool) it installs.
  Defaults to latest.

For a build that produces the same bytes months from now, pin both to the same number:

.. code-block:: yaml

   - uses: sunnamed434/BitMono@0.43.0
     with:
       file: bin/Release/net9.0/MyApp.dll
       version: 0.43.0

Use the newest tag from the `releases page <https://github.com/sunnamed434/BitMono/releases>`_.

Without the action
------------------

You don't actually need the action, it's a thin wrapper over the global tool. If you'd rather not pull a
third-party action into your workflow, two ``run`` steps do the same job:

.. code-block:: yaml

   - run: dotnet tool install --global BitMono.GlobalTool
   - run: bitmono.console -f bin/Release/net9.0/MyApp.dll --preset Maximum

The action just saves you the install dance, the PATH handling and the argument mapping.

Troubleshooting
---------------

The CLI exits non-zero when obfuscation fails, so a broken run fails the job, no silent pass. Read the
log under the ``BitMono obfuscation`` group in the step output, it has the full command and the
obfuscator's messages. See :doc:`troubleshooting`.
