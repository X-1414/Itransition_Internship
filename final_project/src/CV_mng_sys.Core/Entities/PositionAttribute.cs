namespace CV_mng_sys.Core.Entities
{
    public class PositionAttribute
    {
        public int Id {get; set;}
        public int PositionId {get; set;}
        public Position Position {get; set;} = null!;
        public int AttributeDefinitionId {get; set;}
        public AttributeDefinition AttributeDefinition {get; set;} = null!;
        public bool IsRequired{get; set;}
        public int SortOrder{get; set;}
    }
}