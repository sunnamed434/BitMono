namespace BitMono.GUI.Data;

internal class HandlerLogEventSink : ILogEventSink
{
    public event Action OnEnqueued;
    private readonly ITextFormatter m_TextFormatter;

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