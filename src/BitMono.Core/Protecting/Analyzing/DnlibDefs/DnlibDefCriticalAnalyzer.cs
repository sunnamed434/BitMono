namespace BitMono.Core.Protecting.Analyzing.DnlibDefs;

public class DnlibDefCriticalAnalyzer : ICriticalAnalyzer<IMemberDefinition>
{
    public bool NotCriticalToMakeChanges(IMemberDefinition memberDefinition)
    {
        if (memberDefinition is TypeDefinition typeDef)
        {
            return typeDef.IsRuntimeSpecialName == false;
        }
        if (memberDefinition is FieldDefinition fieldDef)
        {
            return fieldDef.IsRuntimeSpecialName == false
                && fieldDef.IsLiteral == false
                && fieldDef.DeclaringType.IsEnum == false;
        }
        if (memberDefinition is MethodDefinition methodDefinition)
        {
            return methodDefinition.IsRuntimeSpecialName && methodDefinition.DeclaringType.IsForwarder
                ? false
                : true;
        }
        if (memberDefinition is EventDefinition eventDefinition)
        {
            return eventDefinition.IsRuntimeSpecialName == false;
        }
        return true;
    }
}