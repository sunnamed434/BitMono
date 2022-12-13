using Rocket.Core.Plugins;
using System;

namespace BitMono.Unturned_RocketMod_Crackme
{
    public class Plugin : RocketPlugin
    {
        protected override void Load()
        {
            Console.WriteLine("CW: Hello world");
            Log("Hello world");
        }

        public static void Log(string message)
        {
            Rocket.Core.Logging.Logger.LogWarning("Log: " + message);
        }
    }
}