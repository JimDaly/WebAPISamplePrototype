using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Cds.Metadata
{
public class BooleanManagedProperty
  {
    public bool Value { get; set; }
    public bool CanBeChanged { get; set; }
    public string ManagedPropertyLogicalName { get; set; }
    public BooleanManagedProperty CanModifyAdditionalSettings { get; set; }
    public int ColumnNumber { get; set; }
    public string DeprecatedVersion { get; set; }

  }
}
