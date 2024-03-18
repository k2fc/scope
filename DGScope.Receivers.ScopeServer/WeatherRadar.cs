using DGScope.Library;
using DGScope;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System;

public class WeatherRadar : IUpdatable
{
    public DateTime LastMessageTime { get; private set; }
    public Dictionary<PropertyInfo, DateTime> PropertyUpdatedTimes { get; } = new Dictionary<PropertyInfo, DateTime>();
    public Guid Guid { get; set; } = Guid.NewGuid();
    public string RadarID { get; set; }
    public GeoPoint ReferencePoint { get; set; }
    public int Rows { get; set; }
    public int Columns { get; set; }
    public Vector2 OffsetToOrigin { get; set; }
    public Vector2 BoxSize { get; set; }
    public byte[] Levels { get; set; }

    public event EventHandler<UpdateEventArgs> Updated;
    public event EventHandler<UpdateEventArgs> Created;

    public Update GetCompleteUpdate()
    {
        var newUpdate = new WeatherRadarUpdate(this);
        newUpdate.SetAllProperties();
        newUpdate.TimeStamp = LastMessageTime;
        return newUpdate;
    }

    public void Update(Update update)
    {
        LastMessageTime = update.TimeStamp;
        var wxUpdate = update as WeatherRadarUpdate;
        bool changed = false;
        if (wxUpdate != null)
        {
            foreach (var updateProperty in update.GetType().GetProperties())
            {
                PropertyInfo thisProperty = GetType().GetProperty(updateProperty.Name);
                object updateValue = updateProperty.GetValue(update);
                if (updateValue is null || thisProperty is null)
                {
                    continue;
                }
                if (!PropertyUpdatedTimes.TryGetValue(thisProperty, out DateTime lastUpdatedTime))
                    PropertyUpdatedTimes.Add(thisProperty, update.TimeStamp);
                if (update.TimeStamp > lastUpdatedTime)
                {
                    if (thisProperty.CanWrite)
                    {
                        thisProperty.SetValue(this, updateValue);
                        PropertyUpdatedTimes[thisProperty] = update.TimeStamp;
                        changed = true;
                    }
                }
            }
            if (changed)
            {
                Updated?.Invoke(this, new TrackUpdatedEventArgs(update));
            }
            return;
        }
    }
}

public class WeatherRadarUpdate : Update
{
    public string RadarID { get; set; }
    public GeoPoint ReferencePoint { get; set; }
    public int? Rows { get; set; }
    public int? Columns { get; set; }
    public Vector2? OffsetToOrigin { get; set; }
    public Vector2? BoxSize { get; set; }
    public byte[] Levels { get; set; }
    public override UpdateType UpdateType => UpdateType.WeatherRadar;
    public WeatherRadarUpdate(WeatherRadar radar)
    {
        Base = radar;
    }
    public WeatherRadarUpdate(WeatherRadarUpdate update, WeatherRadar radar)
    {
        SetAllProperties(update);
        Base = radar;
    }
    public WeatherRadarUpdate() { }
}