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
