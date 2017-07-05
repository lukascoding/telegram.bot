using System;
using System.Linq;
using Newtonsoft.Json;
using lukascoding.TelegramBotApiClient.Types.Enums;

namespace lukascoding.TelegramBotApiClient.Converters
{
    internal class ParseModeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => writer.WriteValue(ParseModeExtensions.StringMap[(ParseMode)value]);
        

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            =>ParseModeExtensions.StringMap.FirstOrDefault(mode => mode.Value == reader.ReadAsString());
        

        public override bool CanConvert(Type objectType) => objectType == typeof (ParseMode);
    }
}
