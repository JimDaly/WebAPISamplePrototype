using Newtonsoft.Json;

namespace Microsoft.Cds.Metadata.Query
{
  [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
  public class Object
  { public string Type { get; set; }
    public string Value { get; set; }
  }
}
