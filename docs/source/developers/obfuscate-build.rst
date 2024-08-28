Automated Build and Obfuscation
===============================

You can make an automatic obfuscation of your build, for example, using ``MSBuild`` (i.e., your ``.csproj`` file) by implementing logic that runs BitMono on your output file before the build ``Task``. This process can be triggered whenever you execute ``dotnet build`` or simply press ``Build`` in your IDE.

BitMono already supports exit codes at the end of the obfuscation process, making it compatible with most tools, including ``MSBuild``. You can retrieve the exit code to determine if the obfuscation was successful or not.

- ``0`` - Obfuscation was successful.
- ``1`` - Obfuscation failed.