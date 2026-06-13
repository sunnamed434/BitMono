namespace BitMono.Core.Analyzing;

[SuppressMessage("ReSharper", "MergeIntoPattern")]
public class RuntimeCriticalAnalyzer : ICriticalAnalyzer<IMetadataMember>
{
    public bool NotCriticalToMakeChanges(IMetadataMember member)
    {
        if (member is TypeDefinition type)
        {
            return !type.IsRuntimeSpecialName;
        }
        if (member is FieldDefinition field)
        {
            return !field.IsRuntimeSpecialName
                && !field.IsLiteral
                && field.DeclaringType?.IsEnum == false;
        }
        if (member is MethodDefinition method)
        {
            return !method.IsRuntimeSpecialName || method.DeclaringType?.IsForwarder == false;
        }
        if (member is EventDefinition @event)
        {
            return !@event.IsRuntimeSpecialName;
        }
        return true;
    }
}