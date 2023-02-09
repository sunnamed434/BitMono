namespace BitMono.Core.Protecting.Analyzing;

public class RuntimeCriticalAnalyzer : ICriticalAnalyzer<IMetadataMember>
{
    public bool NotCriticalToMakeChanges(IMetadataMember member)
    {
        if (member is TypeDefinition type)
        {
            return type.IsRuntimeSpecialName == false;
        }
        if (member is FieldDefinition field)
        {
            return field.IsRuntimeSpecialName == false
                && field.IsLiteral == false
                && field.DeclaringType.IsEnum == false;
        }
        if (member is MethodDefinition method)
        {
            return method.IsRuntimeSpecialName == false || method.DeclaringType.IsForwarder == false;
        }
        if (member is EventDefinition @event)
        {
            return @event.IsRuntimeSpecialName == false;
        }
        return true;
    }
}