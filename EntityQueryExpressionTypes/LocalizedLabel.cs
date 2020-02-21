using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Cds.Metadata
{
  public class LocalizedLabel : MetadataBase
  {
    public LocalizedLabel(string label, int languagecode) {
      Label = label;
      LanguageCode = languagecode;
    }
    [JsonProperty("@odata.type")]
    public string ODataType { get; } = "Microsoft.Dynamics.CRM.LocalizedLabel";
    public string Label { get; set; }
    public int LanguageCode { get; set; }
    public bool IsManaged { get; set; }
  }
}
