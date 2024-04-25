### Troubleshooting

Here we describe most known issues and its solution, something like Faq.

#### Access Denied:

Try to set `OpenFileDestinationInFileExplorer` to `false` in `obfuscation.json`

#### Output file changed its bits:

If your output file is changed from x32 to x64 or something similar, and it cause issues for you then set `AllowPotentialBreakingChangesToModule` to `false` in `obfuscation.json`, however be careful changing it, because some protections may be dependent on this configurations (for example UnmanagedString).