
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:us:gov:dot:faa:atm:terminal:entities:v4-0:tais:terminalautomationinformation")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:us:gov:dot:faa:atm:terminal:entities:v4-0:tais:terminalautomationinformation", IsNullable = false)]
public partial class TATrackAndFlightPlan
{

    private string srcField;

    private record[] recordField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "")]
    public string src
    {
        get
        {
            return this.srcField;
        }
        set
        {
            this.srcField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("record", Namespace = "")]
    public record[] record
    {
        get
        {
            return this.recordField;
        }
        set
        {
            this.recordField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class record
{

    private recordTrack trackField;

    private recordFlightPlan flightPlanField;

    private recordEnhancedData enhancedDataField;

    /// <remarks/>
    public recordTrack track
    {
        get
        {
            return this.trackField;
        }
        set
        {
            this.trackField = value;
        }
    }

    /// <remarks/>
    public recordFlightPlan flightPlan
    {
        get
        {
            return this.flightPlanField;
        }
        set
        {
            this.flightPlanField = value;
        }
    }

    /// <remarks/>
    public recordEnhancedData enhancedData
    {
        get
        {
            return this.enhancedDataField;
        }
        set
        {
            this.enhancedDataField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class recordTrack
{

    private ushort trackNumField;

    private System.DateTime mrtTimeField;

    private string statusField;

    private string acAddressField;

    private uint xPosField;

    private uint yPosField;

    private decimal latField;

    private decimal lonField;

    private short vVertField;

    private short vxField;

    private short vyField;

    private short vVertRawField;

    private short vxRawField;

    private short vyRawField;

    private byte frozenField;

    private byte newField;

    private byte pseudoField;

    private byte adsbField;

    private ushort reportedBeaconCodeField;

    private ushort reportedAltitudeField;

    /// <remarks/>
    public ushort trackNum
    {
        get
        {
            return this.trackNumField;
        }
        set
        {
            this.trackNumField = value;
        }
    }

    /// <remarks/>
    public System.DateTime mrtTime
    {
        get
        {
            return this.mrtTimeField;
        }
        set
        {
            this.mrtTimeField = value;
        }
    }

    /// <remarks/>
    public string status
    {
        get
        {
            return this.statusField;
        }
        set
        {
            this.statusField = value;
        }
    }

    /// <remarks/>
    public string acAddress
    {
        get
        {
            return this.acAddressField;
        }
        set
        {
            this.acAddressField = value;
        }
    }

    /// <remarks/>
    public uint xPos
    {
        get
        {
            return this.xPosField;
        }
        set
        {
            this.xPosField = value;
        }
    }

    /// <remarks/>
    public uint yPos
    {
        get
        {
            return this.yPosField;
        }
        set
        {
            this.yPosField = value;
        }
    }

    /// <remarks/>
    public decimal lat
    {
        get
        {
            return this.latField;
        }
        set
        {
            this.latField = value;
        }
    }

    /// <remarks/>
    public decimal lon
    {
        get
        {
            return this.lonField;
        }
        set
        {
            this.lonField = value;
        }
    }

    /// <remarks/>
    public short vVert
    {
        get
        {
            return this.vVertField;
        }
        set
        {
            this.vVertField = value;
        }
    }

    /// <remarks/>
    public short vx
    {
        get
        {
            return this.vxField;
        }
        set
        {
            this.vxField = value;
        }
    }

    /// <remarks/>
    public short vy
    {
        get
        {
            return this.vyField;
        }
        set
        {
            this.vyField = value;
        }
    }

    /// <remarks/>
    public short vVertRaw
    {
        get
        {
            return this.vVertRawField;
        }
        set
        {
            this.vVertRawField = value;
        }
    }

    /// <remarks/>
    public short vxRaw
    {
        get
        {
            return this.vxRawField;
        }
        set
        {
            this.vxRawField = value;
        }
    }

    /// <remarks/>
    public short vyRaw
    {
        get
        {
            return this.vyRawField;
        }
        set
        {
            this.vyRawField = value;
        }
    }

    /// <remarks/>
    public byte frozen
    {
        get
        {
            return this.frozenField;
        }
        set
        {
            this.frozenField = value;
        }
    }

    /// <remarks/>
    public byte @new
    {
        get
        {
            return this.newField;
        }
        set
        {
            this.newField = value;
        }
    }

    /// <remarks/>
    public byte pseudo
    {
        get
        {
            return this.pseudoField;
        }
        set
        {
            this.pseudoField = value;
        }
    }

    /// <remarks/>
    public byte adsb
    {
        get
        {
            return this.adsbField;
        }
        set
        {
            this.adsbField = value;
        }
    }

    /// <remarks/>
    public ushort reportedBeaconCode
    {
        get
        {
            return this.reportedBeaconCodeField;
        }
        set
        {
            this.reportedBeaconCodeField = value;
        }
    }

    /// <remarks/>
    public ushort reportedAltitude
    {
        get
        {
            return this.reportedAltitudeField;
        }
        set
        {
            this.reportedAltitudeField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class recordFlightPlan
{

    private ushort sfpnField;

    private string ocrField;

    private byte rnavField;

    private string cpsField;

    private string runwayField;
    private string scratchPad1Field;
    private string scratchPad2Field;

    private ushort assignedBeaconCodeField;

    private ushort requestedAltitudeField;

    private string categoryField;

    private object dbiField;

    private string acidField;

    private string acTypeField;

    private string entryFixField;

    private string exitFixField;

    private string airportField;

    private string flightRulesField;

    private string typeField;

    private ushort ptdTimeField;

    private string statusField;

    private byte deleteField;

    private byte suspendedField;

    private string lldField;

    private string eCIDField;

    /// <remarks/>
    public ushort sfpn
    {
        get
        {
            return this.sfpnField;
        }
        set
        {
            this.sfpnField = value;
        }
    }

    /// <remarks/>
    public string ocr
    {
        get
        {
            return this.ocrField;
        }
        set
        {
            this.ocrField = value;
        }
    }

    /// <remarks/>
    public byte rnav
    {
        get
        {
            return this.rnavField;
        }
        set
        {
            this.rnavField = value;
        }
    }

    /// <remarks/>
    public string cps
    {
        get
        {
            return this.cpsField;
        }
        set
        {
            this.cpsField = value;
        }
    }

    /// <remarks/>
    public string runway
    {
        get
        {
            return this.runwayField;
        }
        set
        {
            this.runwayField = value;
        }
    }
    public string scratchPad1
    {
        get
        {
            return this.scratchPad1Field;
        }
        set
        {
            this.scratchPad1Field = value;
        }
    }
    public string scratchPad2
    {
        get
        {
            return this.scratchPad2Field;
        }
        set
        {
            this.scratchPad2Field = value;
        }
    }

    /// <remarks/>
    public ushort assignedBeaconCode
    {
        get
        {
            return this.assignedBeaconCodeField;
        }
        set
        {
            this.assignedBeaconCodeField = value;
        }
    }

    /// <remarks/>
    public ushort requestedAltitude
    {
        get
        {
            return this.requestedAltitudeField;
        }
        set
        {
            this.requestedAltitudeField = value;
        }
    }

    /// <remarks/>
    public string category
    {
        get
        {
            return this.categoryField;
        }
        set
        {
            this.categoryField = value;
        }
    }

    /// <remarks/>
    public object dbi
    {
        get
        {
            return this.dbiField;
        }
        set
        {
            this.dbiField = value;
        }
    }

    /// <remarks/>
    public string acid
    {
        get
        {
            return this.acidField;
        }
        set
        {
            this.acidField = value;
        }
    }

    /// <remarks/>
    public string acType
    {
        get
        {
            return this.acTypeField;
        }
        set
        {
            this.acTypeField = value;
        }
    }

    /// <remarks/>
    public string entryFix
    {
        get
        {
            return this.entryFixField;
        }
        set
        {
            this.entryFixField = value;
        }
    }

    /// <remarks/>
    public string exitFix
    {
        get
        {
            return this.exitFixField;
        }
        set
        {
            this.exitFixField = value;
        }
    }

    /// <remarks/>
    public string airport
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

    /// <remarks/>
    public string flightRules
    {
        get
        {
            return this.flightRulesField;
        }
        set
        {
            this.flightRulesField = value;
        }
    }

    /// <remarks/>
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

    /// <remarks/>
    public ushort ptdTime
    {
        get
        {
            return this.ptdTimeField;
        }
        set
        {
            this.ptdTimeField = value;
        }
    }

    /// <remarks/>
    public string status
    {
        get
        {
            return this.statusField;
        }
        set
        {
            this.statusField = value;
        }
    }

    /// <remarks/>
    public byte delete
    {
        get
        {
            return this.deleteField;
        }
        set
        {
            this.deleteField = value;
        }
    }

    /// <remarks/>
    public byte suspended
    {
        get
        {
            return this.suspendedField;
        }
        set
        {
            this.suspendedField = value;
        }
    }

    /// <remarks/>
    public string lld
    {
        get
        {
            return this.lldField;
        }
        set
        {
            this.lldField = value;
        }
    }

    /// <remarks/>
    public string ECID
    {
        get
        {
            return this.eCIDField;
        }
        set
        {
            this.eCIDField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class recordEnhancedData
{

    private string eramGufiField;

    private string sfdpsGufiField;

    private string departureAirportField;

    private string destinationAirportField;

    /// <remarks/>
    public string eramGufi
    {
        get
        {
            return this.eramGufiField;
        }
        set
        {
            this.eramGufiField = value;
        }
    }

    /// <remarks/>
    public string sfdpsGufi
    {
        get
        {
            return this.sfdpsGufiField;
        }
        set
        {
            this.sfdpsGufiField = value;
        }
    }

    /// <remarks/>
    public string departureAirport
    {
        get
        {
            return this.departureAirportField;
        }
        set
        {
            this.departureAirportField = value;
        }
    }

    /// <remarks/>
    public string destinationAirport
    {
        get
        {
            return this.destinationAirportField;
        }
        set
        {
            this.destinationAirportField = value;
        }
    }
}

