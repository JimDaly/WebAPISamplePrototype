using Newtonsoft.Json;

namespace Microsoft.Cds.Metadata.Query
{
  [JsonConverter(typeof(CdsWebApiEnumConverter))]
  public enum DeletedMetadataFilters
  {
    Default = 1,
    Attribute = 2,
    Relationship = 4,
    Label = 8,
    OptionSet = 16,
    All = 31
  }
}

