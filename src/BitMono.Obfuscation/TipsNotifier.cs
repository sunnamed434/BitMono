using BitMono.API.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BitMono.Obfuscation
{
    public class TipsNotifier
    {
        private readonly IConfiguration m_Configuration;
        private readonly ILogger m_Logger;

        public TipsNotifier(IBitMonoAppSettingsConfiguration configuration, ILogger logger)
        {
            m_Configuration = configuration.Configuration;
            m_Logger = logger.ForContext<TipsNotifier>();
        }

        public Task NotifyAsync()
        {
            var tips = m_Configuration.GetSection("Tips").Get<string[]>();
            Random random = new Random();
            var tip = tips.Reverse().ToArray()[random.Next(0, tips.Length)];
            m_Logger.Information("Today is your day! Generating helpful tip for you - see it a bit down!");
            m_Logger.Information(tip);
            return Task.CompletedTask;
        }
    }
}