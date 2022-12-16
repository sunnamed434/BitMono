namespace BitMono.GUI.Shared.Alerting;

public class AlertsContainer
{
    private readonly ILogger m_Logger;
    private readonly List<AlertBox> m_AlertBoxes;

    public AlertsContainer(ILogger logger)
    {
        m_Logger = logger;
        m_AlertBoxes = new List<AlertBox>();
    }

    internal Task AddAlertBoxAsync(AlertBox box)
    {
        m_AlertBoxes.RemoveAll(x => x.Id == box.Id);
        m_AlertBoxes.Add(box);
        return Task.CompletedTask;
    }
    public async Task<AlertBox> GetAlertBoxAsync(string boxId)
    {
        var box = m_AlertBoxes.FirstOrDefault(x => x.Id == boxId);
        if (box == null)
        {
            return null;
        }
        return await Task.FromResult(box);
    }
    public async Task ShowAlertAsync(string boxId, string text, Alerts alert = Alerts.Primary)
    {
        var box = await GetAlertBoxAsync(boxId);
        if (box == null)
        {
            m_Logger.Warning("Box with Id {0} not found to be shown!", boxId);
            return;
        }

        var hideBox = m_AlertBoxes.FirstOrDefault(x => x.Group == box.Group && x.ShouldShow);
        if (hideBox != null)
        {
            await hideBox.DisposeAsync();
        }
        await box.ShowAsync(text, alert);
    }
    public async Task HideAlertAsync(string boxId)
    {
        var box = await GetAlertBoxAsync(boxId);
        if (box == null)
        {
            m_Logger.Warning("Box with Id {0} not found to be hiden!", boxId);
            return;
        }
        await box.DisposeAsync();
    }
}