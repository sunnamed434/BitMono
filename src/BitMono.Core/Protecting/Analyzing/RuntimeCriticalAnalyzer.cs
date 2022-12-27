namespace BitMono.Core.Protecting.Analyzing;

public class RuntimeCriticalAnalyzer : ICriticalAnalyzer<IMemberDefinition>
{
    public bool NotCriticalToMakeChanges(IMemberDefinition memberDefinition)
    {
        if (memberDefinition is TypeDefinition type)
        {
            return type.IsRuntimeSpecialName == false;
        }
        if (memberDefinition is FieldDefinition field)
        {
            return field.IsRuntimeSpecialName == false
                && field.IsLiteral == false
                && field.DeclaringType.IsEnum == false;
        }
        if (memberDefinition is MethodDefinition method)
        {
            return method.IsRuntimeSpecialName && method.DeclaringType.IsForwarder
                ? false
                : true;
        }
        if (memberDefinition is EventDefinition @event)
        {
            return @event.IsRuntimeSpecialName == false;
        }
        return true;
    }
}