using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Library
{
    [ProtoContract]
    [ProtoInclude(7000, typeof(TrackUpdate))]
    [ProtoInclude(7001, typeof(FlightPlanUpdate))]
    [ProtoInclude(7002, typeof(DeletionUpdate))]
    [ProtoInclude(7003, typeof(WeatherRadarUpdate))]
    public abstract class Update
    {
        [JsonIgnore]
        public IUpdatable Base { get;  protected set; }
        private Guid baseGuid = Guid.NewGuid();
        [ProtoMember(1, IsRequired = true)]
        public Guid Guid
        {
            get
            {
                if (Base != null)
                    return Base.Guid;
                return baseGuid;
            }
            set
            {
                baseGuid = value;
            }
        }
        [ProtoMember(2, IsRequired = true)]
        public DateTime TimeStamp { get; set; } = DateTime.MinValue;
        public abstract UpdateType UpdateType { get; }
        public void SetAllProperties(Update update)
        {
            PropertyInfo[] properties = update.GetType().GetProperties();
            if (update.GetType() == typeof(FlightPlanUpdate))
                ;
            foreach (PropertyInfo property in properties)
            {
                var propertyName = property.Name;
                if (propertyName == "Callsign" )
                    ;
                var baseProperty = update.GetType().GetProperty(propertyName);
                if (baseProperty == null || !property.CanWrite)
                    continue;
                property.SetValue(this, baseProperty.GetValue(update));
            }
        }
        public void SetAllProperties()
        {
            PropertyInfo[] properties = Base.GetType().GetProperties();
            foreach (PropertyInfo baseProperty in properties)
            {

                var propertyName = baseProperty.Name;
                if (propertyName == "Callsign")
                    ;
                var thisProperty = GetType().GetProperty(propertyName);
                var baseValue = baseProperty.GetValue(Base);
                if (baseProperty == null || thisProperty == null || !thisProperty.CanWrite)
                    continue;
                thisProperty.SetValue(this, baseValue);
            }
            TimeStamp = DateTime.Now;
        }
        public void RemoveUnchanged()
        {
            if (Base == null)
                return;
            PropertyInfo[] properties = GetType().GetProperties();
            if (Base.GetType() == typeof(FlightPlan))
                ;
            foreach (PropertyInfo property in properties)
            {
                var propertyName = property.Name;
                var baseProperty = Base.GetType().GetProperty(propertyName);
                if (baseProperty == null)
                    continue;
                var baseValue = baseProperty.GetValue(Base);
                var thisValue = property.GetValue(this);
                if (baseProperty == null)
                    continue;
                if (propertyName == "Guid")
                    continue; 
                if (!property.CanWrite)
                    continue;
                if (property.PropertyType == typeof(string) && thisValue == null && thisValue != baseValue)
                    property.SetValue(this, string.Empty);
                else if ((thisValue == null) != (baseValue == null))
                    continue;
                else if (thisValue != null && !thisValue.Equals(baseValue))
                    continue;
                else
                    property.SetValue(this, null);
                //    property.SetValue(this, string.Empty);
                //else if (baseValue == null && thisValue != null)
                //    continue;
                //else if (thisValue == null || thisValue.Equals(baseValue))
                //    property.SetValue(this, null);
            }

        }
        public static string SerializeToJson(Update update)
        {
            return JsonConvert.SerializeObject(update, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
        public string SerializeToJson()
        {
            return SerializeToJson(this);
        }
        public Update DeserializeFromJson(string json)
        {
            throw new NotImplementedException();
        }
        public static Update DeserializeFromProto(string protoBase64)
        {
            var bytes = Convert.FromBase64String(protoBase64);
            Update upd;
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Seek(0, SeekOrigin.Begin);
                upd = Serializer.Deserialize<Update>(ms);
            }
            return upd;
        }
    }
    public class UpdateConverter : JsonConverter
    {
        static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new BaseSpecifiedConcreteClassConverter() };
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Update));
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            Update update;
            switch (jo["UpdateType"].Value<int>())
            {
                case 0:
                    update = JsonConvert.DeserializeObject<TrackUpdate>(jo.ToString(), SpecifiedSubclassConversion);
                    var track = (update as TrackUpdate).Base as Track;
                    return new TrackUpdate(update as TrackUpdate, track);
                case 1:
                    return JsonConvert.DeserializeObject<FlightPlanUpdate>(jo.ToString(), SpecifiedSubclassConversion);
                default:
                    throw new NotImplementedException("Unknown update type");
            }
            throw new NotImplementedException();
        }
        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
    public class BaseSpecifiedConcreteClassConverter : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(Update).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
            return base.ResolveContractConverter(objectType);
        }
    }

    public class UpdateEventArgs : EventArgs
    {
        public Update Update { get; protected set; }
        public UpdateEventArgs(Update update)
        {
            Update = update;
        }
        public UpdateEventArgs() { }
    }
    public enum UpdateType
    {
        Track = 0,
        Flightplan = 1,
        Deletion = 2,
        WeatherRadar = 3
    }
}
