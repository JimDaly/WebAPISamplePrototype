using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Cds.Metadata.Query
{
  [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
  public class LabelQueryExpression
  {
    public List<int> FilterLanguages { get; set; }
    public int MissingLabelBehavior { get; set; }
  }
}
