using Newtonsoft.Json;
using System;

namespace Microsoft.Cds.Metadata.Query
{
  public class CdsWebApiEnumConverter : JsonConverter
  {
    public override bool CanConvert(Type objectType)
    {
      return objectType.IsEnum;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      //Not expecting to have to Deserialize these objects
      throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      if (value == null)
      {
        writer.WriteNull();
        return;
      }
      Type enumType = ((Enum)value).GetType();

      writer.WriteValue($"Microsoft.Dynamics.CRM.{enumType.Name}'{Enum.GetName(enumType, value)}'");
    }
  }
}
