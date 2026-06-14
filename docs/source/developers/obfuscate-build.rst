Automated Build and Obfuscation
===============================

Want your assembly obfuscated automatically every time you build, instead of running BitMono by hand?
There are two ways to do it.

The easy way: the NuGet package
-------------------------------

Add the ``BitMono.Integration`` package to your project and it hooks into the build for you, no extra
tool run, no scripting. Build in ``Release`` and the output assembly comes out obfuscated.

.. code-block:: xml

   <ItemGroup>
     <PackageReference Include="BitMono.Integration" Version="0.41.1">
       <PrivateAssets>all</PrivateAssets>
       <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
     </PackageReference>
   </ItemGroup>

This is the recommended option for most projects. See :doc:`../usage/msbuild-integration` for the full
guide, configuration, properties, signing, and so on.

The manual way: run the CLI yourself
-------------------------------------

If you'd rather wire it up by hand (a custom ``MSBuild`` ``Task`` in your ``.csproj``, a CI step, a shell
script, whatever), you can call the BitMono CLI on your output file before the rest of your build runs.
This triggers whenever you run ``dotnet build`` or press ``Build`` in your IDE.

BitMono returns an exit code when it finishes, so any tool can tell whether obfuscation worked:

- ``0`` — obfuscation was successful.
- ``1`` — obfuscation failed.

Check that exit code in your build step and fail the build if it's non-zero.
