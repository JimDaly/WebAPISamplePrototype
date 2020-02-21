using Newtonsoft.Json;

namespace Microsoft.Cds.Metadata.Query
{
  [JsonConverter(typeof(CdsWebApiEnumConverter))]
  public enum MetadataConditionOperator
  {
    Equals,
    NotEquals,
    In,
    NotIn,
    GreaterThan,
    LessThan
  }
}
