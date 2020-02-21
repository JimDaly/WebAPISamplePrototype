using Microsoft.Cds.Metadata.Query;
using Newtonsoft.Json;

namespace Microsoft.Cds.QueryTypes
{
  [JsonConverter(typeof(CdsWebApiEnumConverter))]
  public enum LogicalOperator
  {
    And,
    Or
  }
}
