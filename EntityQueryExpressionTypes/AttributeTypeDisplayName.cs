using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Microsoft.Cds.Metadata
{
  public class AttributeTypeDisplayName
  {
    public AttributeTypeDisplayName(AttributeTypeDisplayNameValues value) {
      Value = value;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public AttributeTypeDisplayNameValues Value { get; set; }

  }

  public enum AttributeTypeDisplayNameValues
  {
    BigIntType,
    BooleanType,
    CalendarRulesType,
    CustomerType,
    DateTimeType,
    DecimalType,
    DoubleType,
    EntityNameType,
    FileType,
    ImageType,
    IntegerType,
    LookupType,
    ManagedPropertyType,
    MemoType,
    MoneyType,
    MultiSelectPicklistType,
    OwnerType,
    PartyListType,
    PicklistType,
    StateType,
    StatusType,
    StringType,
    UniqueidentifierType,
    VirtualType
  }
}
