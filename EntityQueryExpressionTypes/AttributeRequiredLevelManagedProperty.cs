using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Cds.Metadata
{
  public class AttributeRequiredLevelManagedProperty
  {
    public AttributeRequiredLevelManagedProperty(AttributeRequiredLevel value) {
      Value = value;
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public AttributeRequiredLevel Value { get; set; }

    public bool CanBeChanged { get; set; }
    public string ManagedPropertyLogicalName { get; } = "canmodifyrequirementlevelsettings";
  }
}
