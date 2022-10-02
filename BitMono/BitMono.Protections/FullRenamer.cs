using BitMono.API.Protecting;
using BitMono.API.Protecting.Renaming;
using BitMono.Core.Protecting.Analyzing;
using BitMono.Utilities.Extensions.Dnlib;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    public class FullRenamer : IProtection
    {
        private readonly TypeDefCriticalAnalyzer m_TypeDefCriticalAnalyzer;
        private readonly MethodDefCriticalAnalyzer m_MethodDefCriticalAnalyzer;
        private readonly FieldDefCriticalAnalyzer m_FieldDefCriticalAnalyzer;
        private readonly TypeDefModelCriticalAnalyzer m_TypeDefModelCriticalAnalyzer;
        private readonly IRenamer m_Renamer;

        public FullRenamer(
            TypeDefCriticalAnalyzer typeDefCriticalAnalyzer, 
            MethodDefCriticalAnalyzer methodDefCriticalAnalyzer, 
            FieldDefCriticalAnalyzer fieldDefCriticalAnalyzer, 
            TypeDefModelCriticalAnalyzer typeDefModelCriticalAnalyzer, 
            IRenamer renamer)
        {
            m_TypeDefCriticalAnalyzer = typeDefCriticalAnalyzer;
            m_MethodDefCriticalAnalyzer = methodDefCriticalAnalyzer;
            m_FieldDefCriticalAnalyzer = fieldDefCriticalAnalyzer;
            m_TypeDefModelCriticalAnalyzer = typeDefModelCriticalAnalyzer;
            m_Renamer = renamer;
        }


        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                if (typeDef.IsGlobalModuleType == false
                    && m_TypeDefCriticalAnalyzer.NotCriticalToMakeChanges(context, typeDef)
                    && m_TypeDefModelCriticalAnalyzer.NotCriticalToMakeChanges(context, typeDef))
                {
                    m_Renamer.Rename(context, typeDef);

                    if (typeDef.HasFields)
                    {
                        foreach (var fieldDef in typeDef.Fields.ToArray())
                        {
                            if (m_FieldDefCriticalAnalyzer.NotCriticalToMakeChanges(context, fieldDef))
                            {
                                m_Renamer.Rename(context, fieldDef);
                            }
                        }
                    }

                    if (typeDef.HasMethods)
                    {
                        foreach (var methodDef in typeDef.Methods.ToArray())
                        {
                            if (methodDef.IsConstructor == false
                                && methodDef.IsVirtual == false
                                && m_MethodDefCriticalAnalyzer.NotCriticalToMakeChanges(context, methodDef))
                            {
                                m_Renamer.Rename(context, methodDef);
                                if (methodDef.HasParameters())
                                {
                                    foreach (var parameter in methodDef.Parameters.ToArray())
                                    {
                                        m_Renamer.Rename(context, parameter);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}