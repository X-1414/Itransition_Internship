namespace CV_mng_sys.Core.Entities;

public class PositionAccessRule
{
    public int Id {get; set;}
    public int PositionId {get; set;}
    public Position Position {get; set;} = null!;
    public int AttributeDefinitionId {get; set;}
    public AttributeDefinition AttributeDefinition {get; set;} = null!;
    public AccessRuleOperator Operator {get; set;}
    public string? ComparisonValue {get; set;}
}