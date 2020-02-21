﻿using Newtonsoft.Json;
using System;

namespace Microsoft.Cds.Metadata
{
  [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
  public abstract class MetadataBase : crmmodelbaseentity
  {
    /// <summary>
    /// Indicates whether the item of metadata has changed.
    /// </summary>
    public bool? HasChanged { get; set; }
    /// <summary>
    /// A unique identifier for the metadata item.
    /// </summary>
    public Guid? MetadataId { get; set; }
  }
}
