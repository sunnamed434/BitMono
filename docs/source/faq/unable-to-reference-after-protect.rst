Unable to Reference After Protection
====================================

If you're having trouble referencing your ``.dll`` file after protecting it with BitMono, follow these steps:

1. **Keep an Original Copy**: Always keep an original, unprotected copy of your ``.dll`` file. This will be used as a reference in your IDE or other tools.

2. **Protect the DLL**: Use BitMono to protect your ``.dll`` file.

3. **Set Up Output Folder**: In your output folder (e.g., ``Release\...``), place the protected version of your ``.dll`` file.

By following these steps, you can ensure that your project references the original ``.dll`` while deploying the protected version.