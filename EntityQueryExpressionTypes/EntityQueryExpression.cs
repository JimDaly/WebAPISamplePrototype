using Newtonsoft.Json;

namespace Microsoft.Cds.Metadata.Query
{
  [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
  public class EntityQueryExpression
  {
    public AttributeQueryExpression AttributeQuery { get; set; }
    public MetadataFilterExpression Criteria { get; set; }
    public EntityKeyQueryExpression KeyQuery { get; set; }
    public LabelQueryExpression LabelQuery { get; set; }
    public MetadataPropertiesExpression Properties { get; set; }
    public RelationshipQueryExpression RelationshipQuery { get; set; }
  }
}