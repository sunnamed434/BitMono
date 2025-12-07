Unity Integration Testing
=========================

This guide explains how to test the Unity integration during development. 
Use this when you're working on Unity-related features or fixing bugs in the integration.

The Unity integration automatically obfuscates your game code during Unity builds. 
This testing setup lets you verify that changes work correctly without creating full Unity packages or your own Unity project.

Scripts
-------

Located in ``src/BitMono.Unity/scripts/``:

- ``copy-to-test-project.bat/ps1`` - Copies files to test project & Generates .meta to disable CLI plugin import under Assets

Testing
-------

1. Build CLI: ``dotnet build src/BitMono.CLI/BitMono.CLI.csproj -c Release -f net462``
2. Run: ``src/BitMono.Unity/scripts/copy-to-test-project.bat``
3. Open Unity: ``test/BitMono.Unity.TestProject/``
4. Build to test

Testing Package Export
----------------------

To verify that the ``.unitypackage`` export includes all required DLLs:

1. Follow steps 1-3 above to set up the test project
2. In Unity, go to **BitMono > Test Export Package**
3. Check the Console for output showing included files and DLL count
4. The package will be created at the repository root: ``BitMono-Unity-vtest-Unity<version>.unitypackage``

Expected output::

    === BitMono Package Export Test ===
    Including 192 files from BitMono.CLI folder (164 DLLs)
    Total assets to export: 198
    Package exported successfully!
      Size: ~8 MB

This menu item is only available in the source code (not in exported packages) since ``PackageExporter.cs`` is excluded from the ``.unitypackage``.
