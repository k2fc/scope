namespace DGScope
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRoot(Namespace = "", IsNullable = false)]
    public partial class response
    {

        private uint request_indexField;

        private responseData_source data_sourceField;

        private responseRequest requestField;

        private object errorsField;

        private object warningsField;

        private byte time_taken_msField;

        private responseData dataField;

        private decimal versionField;

        /// <remarks/>
        public uint request_index
        {
            get
            {
                return this.request_indexField;
            }
            set
            {
                this.request_indexField = value;
            }
        }

        /// <remarks/>
        public responseData_source data_source
        {
            get
            {
                return this.data_sourceField;
            }
            set
            {
                this.data_sourceField = value;
            }
        }

        /// <remarks/>
        public responseRequest request
        {
            get
            {
                return this.requestField;
            }
            set
            {
                this.requestField = value;
            }
        }

        /// <remarks/>
        public object errors
        {
            get
            {
                return this.errorsField;
            }
            set
            {
                this.errorsField = value;
            }
        }

        /// <remarks/>
        public object warnings
        {
            get
            {
                return this.warningsField;
            }
            set
            {
                this.warningsField = value;
            }
        }

        /// <remarks/>
        public byte time_taken_ms
        {
            get
            {
                return this.time_taken_msField;
            }
            set
            {
                this.time_taken_msField = value;
            }
        }

        /// <remarks/>
        public responseData data
        {
            get
            {
                return this.dataField;
            }
            set
            {
                this.dataField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseData_source
    {

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseRequest
    {

        private string typeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseData
    {

        private responseDataMETAR[] mETARField;

        private ushort num_resultsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("METAR")]
        public responseDataMETAR[] METAR
        {
            get
            {
                return this.mETARField;
            }
            set
            {
                this.mETARField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort num_results
        {
            get
            {
                return this.num_resultsField;
            }
            set
            {
                this.num_resultsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseDataMETAR
    {

        private object[] itemsField;

        private ItemsChoiceType1[] itemsElementNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("altim_in_hg", typeof(decimal))]
        [System.Xml.Serialization.XmlElementAttribute("dewpoint_c", typeof(decimal))]
        [System.Xml.Serialization.XmlElementAttribute("elevation_m", typeof(decimal))]
        [System.Xml.Serialization.XmlElementAttribute("flight_category", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("latitude", typeof(decimal))]
        [System.Xml.Serialization.XmlElementAttribute("longitude", typeof(decimal))]
        [System.Xml.Serialization.XmlElementAttribute("maxT24hr_c", typeof(decimal))]
        [System.Xml.Serialization.XmlElementAttribute("maxT_c", typeof(decimal))]
        [System.Xml.Serialization.XmlElementAttribute("metar_type", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("minT24hr_c", typeof(decimal))]
        [System.Xml.Serialization.XmlElementAttribute("minT_c", typeof(decimal))]
        [System.Xml.Serialization.XmlElementAttribute("observation_time", typeof(System.DateTime))]
        [System.Xml.Serialization.XmlElementAttribute("pcp24hr_in", typeof(decimal))]
        [System.Xml.Serialization.XmlElementAttribute("pcp6hr_in", typeof(decimal))]
        [System.Xml.Serialization.XmlElementAttribute("precip_in", typeof(decimal))]
        [System.Xml.Serialization.XmlElementAttribute("quality_control_flags", typeof(responseDataMETARQuality_control_flags))]
        [System.Xml.Serialization.XmlElementAttribute("raw_text", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("sea_level_pressure_mb", typeof(decimal))]
        [System.Xml.Serialization.XmlElementAttribute("sky_condition", typeof(responseDataMETARSky_condition))]
        [System.Xml.Serialization.XmlElementAttribute("station_id", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("temp_c", typeof(decimal))]
        [System.Xml.Serialization.XmlElementAttribute("three_hr_pressure_tendency_mb", typeof(decimal))]
        [System.Xml.Serialization.XmlElementAttribute("vert_vis_ft", typeof(ushort))]
        [System.Xml.Serialization.XmlElementAttribute("visibility_statute_mi", typeof(decimal))]
        [System.Xml.Serialization.XmlElementAttribute("wind_dir_degrees", typeof(ushort))]
        [System.Xml.Serialization.XmlElementAttribute("wind_gust_kt", typeof(byte))]
        [System.Xml.Serialization.XmlElementAttribute("wind_speed_kt", typeof(byte))]
        [System.Xml.Serialization.XmlElementAttribute("wx_string", typeof(string))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemsElementName")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemsChoiceType1[] ItemsElementName
        {
            get
            {
                return this.itemsElementNameField;
            }
            set
            {
                this.itemsElementNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseDataMETARQuality_control_flags
    {

        private string[] itemsField;

        private ItemsChoiceType[] itemsElementNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("auto", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("auto_station", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("corrected", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("freezing_rain_sensor_off", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("lightning_sensor_off", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("maintenance_indicator_on", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("no_signal", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("present_weather_sensor_off", typeof(string))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public string[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemsElementName")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemsChoiceType[] ItemsElementName
        {
            get
            {
                return this.itemsElementNameField;
            }
            set
            {
                this.itemsElementNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum ItemsChoiceType
    {

        /// <remarks/>
        auto,

        /// <remarks/>
        auto_station,

        /// <remarks/>
        corrected,

        /// <remarks/>
        freezing_rain_sensor_off,

        /// <remarks/>
        lightning_sensor_off,

        /// <remarks/>
        maintenance_indicator_on,

        /// <remarks/>
        no_signal,

        /// <remarks/>
        present_weather_sensor_off,
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseDataMETARSky_condition
    {

        private string sky_coverField;

        private ushort cloud_base_ft_aglField;

        private bool cloud_base_ft_aglFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string sky_cover
        {
            get
            {
                return this.sky_coverField;
            }
            set
            {
                this.sky_coverField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort cloud_base_ft_agl
        {
            get
            {
                return this.cloud_base_ft_aglField;
            }
            set
            {
                this.cloud_base_ft_aglField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cloud_base_ft_aglSpecified
        {
            get
            {
                return this.cloud_base_ft_aglFieldSpecified;
            }
            set
            {
                this.cloud_base_ft_aglFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum ItemsChoiceType1
    {

        /// <remarks/>
        altim_in_hg,

        /// <remarks/>
        dewpoint_c,

        /// <remarks/>
        elevation_m,

        /// <remarks/>
        flight_category,

        /// <remarks/>
        latitude,

        /// <remarks/>
        longitude,

        /// <remarks/>
        maxT24hr_c,

        /// <remarks/>
        maxT_c,

        /// <remarks/>
        metar_type,

        /// <remarks/>
        minT24hr_c,

        /// <remarks/>
        minT_c,

        /// <remarks/>
        observation_time,

        /// <remarks/>
        pcp24hr_in,

        /// <remarks/>
        pcp6hr_in,

        /// <remarks/>
        precip_in,

        /// <remarks/>
        quality_control_flags,

        /// <remarks/>
        raw_text,

        /// <remarks/>
        sea_level_pressure_mb,

        /// <remarks/>
        sky_condition,

        /// <remarks/>
        station_id,

        /// <remarks/>
        temp_c,

        /// <remarks/>
        three_hr_pressure_tendency_mb,

        /// <remarks/>
        vert_vis_ft,

        /// <remarks/>
        visibility_statute_mi,

        /// <remarks/>
        wind_dir_degrees,

        /// <remarks/>
        wind_gust_kt,

        /// <remarks/>
        wind_speed_kt,

        /// <remarks/>
        wx_string,
    }


}
