## Imporving obfuscation process for everyone
`criticals.json` - something which cannot be protected because this is a critical thing in Third-Party or somewhere else
* CriticalMethods - add the methods which are critical, eg Unity Methods (Awake, Update etc) which couldn`t be renamed or protected.
* CriticalInterfaces - add the interfaces which are critical, eg Third-Party interfaces which couldn`t be renamed or protected.
* CriticalBaseTypes - add the base types which are critical, eg Third-Party base types which couldn`t be renamed or protected.

`protections.json` - depends how protections will execute, in what order
* Sort protections as it should be better, to improve BitMono usage experience for others.

`obfuscation.json` - config that used while obfuscating