using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Microsoft.Cds.Metadata.Query
{
  [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
  public class ComplexEntityKeyMetadata
  {
    public Label DisplayName { get; set; }
    public EntityKeyIndexStatus EntityKeyIndexStatus { get; set; }
    public string EntityLogicalName { get; set; }
    public bool HasChanged { get; set; }
    public string IntroducedVersion { get; set; }
    public BooleanManagedProperty IsCustomizable { get; set; }
    public bool IsManaged { get; set; }
    public List<string> KeyAttributes { get; set; }
    public string LogicalName { get; set; }
    public Guid MetadataId { get; set; }
    public string SchemaName { get; set; }
  }
}
