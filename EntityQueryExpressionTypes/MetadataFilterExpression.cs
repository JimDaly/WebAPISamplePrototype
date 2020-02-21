using Microsoft.Cds.QueryTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Cds.Metadata.Query
{
  [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
  public class MetadataFilterExpression
  {
    public List<MetadataConditionExpression> Conditions { get; set; }
    public LogicalOperator FilterOperator { get; set;}
    public List<MetadataFilterExpression> Filters { get; set; }
  }
}
