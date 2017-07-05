using System;
using Newtonsoft.Json;
using lukascoding.TelegramBotApiClient.Types.InlineQueryResults;

namespace lukascoding.TelegramBotApiClient.Converters
{
    internal class InlineQueryResultTypeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var resultType = (InlineQueryResultType)value;

            writer.WriteValue(resultType.ToTypeString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value.ToString().Replace("_", "");

            Enum.TryParse(value, true, out InlineQueryResultType resultType);

            return resultType;
        }

        public override bool CanConvert(Type objectType)
            => objectType == typeof (InlineQueryResultType);

    }
}
