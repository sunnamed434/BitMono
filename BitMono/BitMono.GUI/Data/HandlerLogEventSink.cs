using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using System.Collections.Concurrent;

namespace BitMono.GUI.Data
{
    internal class HandlerLogEventSink : ILogEventSink
    {
        private readonly ITextFormatter m_TextFormatter;
        public event Action OnEnqueued;

        public HandlerLogEventSink()
        {
            m_TextFormatter = new MessageTemplateTextFormatter("{Timestamp} [{Level}] {Message}{Exception}");
            Queue = new ConcurrentQueue<string>();
        }

        public ConcurrentQueue<string> Queue { get; }


        void ILogEventSink.Emit(LogEvent logEvent)
        {
            var renderSpace = new StringWriter();
            m_TextFormatter.Format(logEvent, renderSpace);
            Queue.Enqueue(renderSpace.ToString());
            OnEnqueued?.Invoke();
        }
    }
}