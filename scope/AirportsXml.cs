namespace DGScope
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Airports
    {

        private Airport[] airportField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Airport")]
        public Airport[] Airport
        {
            get
            {
                return this.airportField;
            }
            set
            {
                this.airportField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Airport
    {

        private AirportLocation locationField;

        private Runway[] runwaysField;

        private string idField;

        private string nameField;

        private short elevationField;

        private decimal magVarField;

        private byte frequencyField;

        /// <remarks/>
        public AirportLocation Location
        {
            get
            {
                return this.locationField;
            }
            set
            {
                this.locationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Runway", IsNullable = false)]
        public Runway[] Runways
        {
            get
            {
                return this.runwaysField;
            }
            set
            {
                this.runwaysField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
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

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public short Elevation
        {
            get
            {
                return this.elevationField;
            }
            set
            {
                this.elevationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal MagVar
        {
            get
            {
                return this.magVarField;
            }
            set
            {
                this.magVarField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte Frequency
        {
            get
            {
                return this.frequencyField;
            }
            set
            {
                this.frequencyField = value;
            }
        }
        public override string ToString()
        {
            return string.Format("{ 0}: {1}", ID, Name);
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class AirportLocation : GeoPoint
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double Lat
        {
            get
            {
                return this.Latitude;
            }
            set
            {
                this.Latitude = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double Lon
        {
            get
            {
                return this.Longitude;
            }
            set
            {
                this.Longitude = value;
            }
        }

        public AirportLocation() : base() { }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Runway
    {

        private AirportLocation startLocField;

        private AirportLocation endLocField;

        private string idField;

        private decimal headingField;

        private ushort lengthField;

        private ushort widthField;

        /// <remarks/>
        public AirportLocation StartLoc
        {
            get
            {
                return this.startLocField;
            }
            set
            {
                this.startLocField = value;
            }
        }

        /// <remarks/>
        public AirportLocation EndLoc
        {
            get
            {
                return this.endLocField;
            }
            set
            {
                this.endLocField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Heading
        {
            get
            {
                return this.headingField;
            }
            set
            {
                this.headingField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort Length
        {
            get
            {
                return this.lengthField;
            }
            set
            {
                this.lengthField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort Width
        {
            get
            {
                return this.widthField;
            }
            set
            {
                this.widthField = value;
            }
        }
        public override string ToString()
        {
            return ID;
        }
    }


}
