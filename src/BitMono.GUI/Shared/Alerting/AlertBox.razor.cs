using Microsoft.AspNetCore.Components;

namespace BitMono.GUI.Shared.Alerting
{
    public partial class AlertBox : IAsyncDisposable
    {
        [Parameter] public string Id { get; set; }
        [Parameter] public string Group { get; set; }
        [Inject] public AlertsContainer AlertsContainer { get; set; }
        public bool ShouldShow { get; protected set; }
        public MarkupString Text { get; set; }
        public Alerts Alert { get; set; }


        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                AlertsContainer.AddAlertBoxAsync(this);
            }
        }

        public Task ShowAsync(string text, Alerts alert)
        {
            Alert = alert;
            Text = new MarkupString(text);
            ShouldShow = true;
            StateHasChanged();
            return Task.CompletedTask;
        }
        public ValueTask DisposeAsync()
        {
            ShouldShow = false;
            StateHasChanged();
            return new ValueTask();
        }

        private string getBackgroundClass()
        {
            return Alert switch
            {
                Alerts.Primary => "bg-primary",
                Alerts.Warning => "bg-warning",
                Alerts.Danger  => "bg-danger",
                Alerts.Info    => "bg-info",
                Alerts.Success => "bg-success",
                _ => throw new ArgumentOutOfRangeException(Alert.ToString()),
            };
        }
        private string getIcon()
        {
            return Alert switch
            {
                Alerts.Primary => "fas fa-info",
                Alerts.Warning => "fas fa-exclamation-triangle",
                Alerts.Danger  => "fas fa-exclamation-circle",
                Alerts.Info    => "fas fa-info-circle",
                Alerts.Success => "fas fa-check-circle",
                _ => throw new ArgumentOutOfRangeException(Alert.ToString()),
            };
        }
    }
}
