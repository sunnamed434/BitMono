using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Pipeline;
using dnlib.DotNet.Writer;
using System.Threading;
using System.Threading.Tasks;

namespace BitMono.Protections.Metadata
{
    public class InvalidMetadata : IStageProtection
    {
        private ModuleWriterOptions _moduleWriterOptions;

        public PipelineStages Stage => PipelineStages.ModuleWrite;

        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            _moduleWriterOptions = context.ModuleWriterOptions;
            context.ModuleWriterOptions.WriterEvent += onWriterEventHandle;
            return Task.CompletedTask;
        }

        private void onWriterEventHandle(object sender, ModuleWriterEventArgs e)
        {
            if (e.Event == ModuleWriterEvent.MDEndCreateTables)
            {
                e.Writer.TheOptions.MetadataOptions.CustomHeaps.Add(new CustomHeap("#"));
            }

            if (e.Event == ModuleWriterEvent.End)
            {
                _moduleWriterOptions.WriterEvent -= onWriterEventHandle;
            }
        }
    }
}