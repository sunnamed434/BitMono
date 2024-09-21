ReadyToRun
==========

If you're encountering an error from BitMono regarding ReadyToRun, or your application is built using ReadyToRun, it means that your application is "protected" by being compiled into native code. However, BitMono cannot obfuscate it because it requires "managed" code.

To solve this issue, you can do the following:

Disable ReadyToRun
------------------

Simply go to your `.csproj` file, add the `PublishReadyToRun` option and set it to `false`. This will look like the following:

.. code-block:: xml

    <Project Sdk="Microsoft.NET.Sdk">
        <PropertyGroup>
            <OutputType>Exe</OutputType>
            <TargetFramework>net8.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
            <PublishReadyToRun>false</PublishReadyToRun> <!-- Add this option and set to false -->
        </PropertyGroup>
    </Project>


Now, build the project and use `.dll` file that has a managed code, instead of native code.

`For more information, visit: <https://learn.microsoft.com/en-us/dotnet/core/deploying/ready-to-run>`_

Use .dll File with Managed Code Instead of .exe File
----------------------------------------------------

Usually, in the output folder of your project (if it's a `.exe`), there should be a `.dll` file with managed code. Use this `.dll` file for obfuscation instead, if there's no such file or it has a native code then use upper solution.

Didn't Help?
------------

If none of the solutions worked, it likely means your file is either protected or broken. If this isn't the case, please open an issue on the BitMono GitHub repository or reach out to us on Discord for further assistance.