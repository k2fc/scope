
using System;
namespace DGScope.MapImporter.CRC
{
    public class CRCARTCC
    {
        public string id { get; set; }
        public DateTime lastUpdatedAt { get; set; }
        public Facility facility { get; set; }
        public Visibilitycenter1[] visibilityCenters { get; set; }
        public DateTime aliasesLastUpdatedAt { get; set; }
        public Videomap[] videoMaps { get; set; }
    }

    public class Facility
    {
        public string id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public Childfacility[] childFacilities { get; set; }
        public Eramconfiguration eramConfiguration { get; set; }
        public object edstConfiguration { get; set; }
        public object starsConfiguration { get; set; }
        public object towerCabConfiguration { get; set; }
        public object asdexConfiguration { get; set; }
        public object tdlsConfiguration { get; set; }
        public object flightStripsConfiguration { get; set; }
        public Position4[] positions { get; set; }
        public string[] neighboringFacilityIds { get; set; }
        public string[] nonNasFacilityIds { get; set; }
    }

    public class Eramconfiguration
    {
        public string nasId { get; set; }
        public Geomap[] geoMaps { get; set; }
        public string[] emergencyChecklist { get; set; }
        public string[] positionReliefChecklist { get; set; }
        public string[] internalAirports { get; set; }
        public Beaconcodebank[] beaconCodeBanks { get; set; }
        public Neighboringstarsconfiguration[] neighboringStarsConfigurations { get; set; }
        public object[] neighboringCaatsConfigurations { get; set; }
        public object atopHandoffLetter { get; set; }
        public string[] referenceFixes { get; set; }
        public Asrsite[] asrSites { get; set; }
        public int conflictAlertFloor { get; set; }
    }

    public class Geomap
    {
        public string id { get; set; }
        public string name { get; set; }
        public string labelLine1 { get; set; }
        public string labelLine2 { get; set; }
        public Filtermenu[] filterMenu { get; set; }
        public string[] bcgMenu { get; set; }
        public string[] videoMapIds { get; set; }
    }

    public class Filtermenu
    {
        public string id { get; set; }
        public string labelLine1 { get; set; }
        public string labelLine2 { get; set; }
    }

    public class Beaconcodebank
    {
        public string id { get; set; }
        public string category { get; set; }
        public string priority { get; set; }
        public int subset { get; set; }
        public int start { get; set; }
        public int end { get; set; }
    }

    public class Neighboringstarsconfiguration
    {
        public string id { get; set; }
        public string facilityId { get; set; }
        public string starsId { get; set; }
        public string singleCharacterStarsId { get; set; }
        public object twoCharacterStarsId { get; set; }
        public string fieldELetter { get; set; }
        public string fieldEFormat { get; set; }
    }

    public class Asrsite
    {
        public string id { get; set; }
        public string asrId { get; set; }
        public Location location { get; set; }
        public int range { get; set; }
        public int ceiling { get; set; }
    }

    public class Location
    {
        public float lat { get; set; }
        public float lon { get; set; }
    }

    public class Childfacility
    {
        public string id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public Childfacility1[] childFacilities { get; set; }
        public object eramConfiguration { get; set; }
        public object edstConfiguration { get; set; }
        public Starsconfiguration starsConfiguration { get; set; }
        public Towercabconfiguration towerCabConfiguration { get; set; }
        public Asdexconfiguration asdexConfiguration { get; set; }
        public Tdlsconfiguration tdlsConfiguration { get; set; }
        public Flightstripsconfiguration flightStripsConfiguration { get; set; }
        public Position3[] positions { get; set; }
        public string[] neighboringFacilityIds { get; set; }
        public object[] nonNasFacilityIds { get; set; }
    }

    public class Starsconfiguration
    {
        public Area[] areas { get; set; }
        public string[] internalAirports { get; set; }
        public Beaconcodebank1[] beaconCodeBanks { get; set; }
        public object[] rpcs { get; set; }
        public Primaryscratchpadrule[] primaryScratchpadRules { get; set; }
        public object[] secondaryScratchpadRules { get; set; }
        public string[] rnavPatterns { get; set; }
        public bool allow4CharacterScratchpad { get; set; }
        public Starshandoffid[] starsHandoffIds { get; set; }
        public string[] videoMapIds { get; set; }
        public Mapgroup[] mapGroups { get; set; }
    }

    public class Area
    {
        public string id { get; set; }
        public string name { get; set; }
        public Visibilitycenter visibilityCenter { get; set; }
        public int surveillanceRange { get; set; }
        public string[] ssaAirports { get; set; }
        public Towerlistconfiguration[] towerListConfigurations { get; set; }
        public bool ldbBeaconCodesInhibited { get; set; }
        public bool pdbGroundSpeedInhibited { get; set; }
        public bool displayRequestedAltInFdb { get; set; }
        public bool useVfrPositionSymbol { get; set; }
        public bool showDestinationDepartures { get; set; }
        public bool showDestinationSatelliteArrivals { get; set; }
        public bool showDestinationPrimaryArrivals { get; set; }
    }

    public class Visibilitycenter
    {
        public float lat { get; set; }
        public float lon { get; set; }
    }

    public class Towerlistconfiguration
    {
        public string id { get; set; }
        public string airportId { get; set; }
        public int range { get; set; }
    }

    public class Beaconcodebank1
    {
        public string id { get; set; }
        public string type { get; set; }
        public int? subset { get; set; }
        public int start { get; set; }
        public int end { get; set; }
    }

    public class Primaryscratchpadrule
    {
        public string id { get; set; }
        public string[] airportIds { get; set; }
        public string searchPattern { get; set; }
        public int? minAltitude { get; set; }
        public int? maxAltitude { get; set; }
        public string template { get; set; }
    }

    public class Starshandoffid
    {
        public string id { get; set; }
        public string facilityId { get; set; }
        public int handoffNumber { get; set; }
    }

    public class Mapgroup
    {
        public string id { get; set; }
        public int?[] mapIds { get; set; }
        public string[] tcps { get; set; }
    }

    public class Towercabconfiguration
    {
        public string videoMapId { get; set; }
        public int defaultRotation { get; set; }
        public int defaultZoomRange { get; set; }
        public int aircraftVisibilityCeiling { get; set; }
        public Towerlocation towerLocation { get; set; }
    }

    public class Towerlocation
    {
        public float lat { get; set; }
        public float lon { get; set; }
    }

    public class Asdexconfiguration
    {
        public string videoMapId { get; set; }
        public int defaultRotation { get; set; }
        public int defaultZoomRange { get; set; }
        public int targetVisibilityRange { get; set; }
        public int targetVisibilityCeiling { get; set; }
        public Fixrule[] fixRules { get; set; }
        public bool useDestinationIdAsFix { get; set; }
        public object[] runwayConfigurations { get; set; }
        public Position[] positions { get; set; }
        public string defaultPositionId { get; set; }
        public Towerlocation1 towerLocation { get; set; }
    }

    public class Towerlocation1
    {
        public float lat { get; set; }
        public float lon { get; set; }
    }

    public class Fixrule
    {
        public string id { get; set; }
        public string searchPattern { get; set; }
        public string fixId { get; set; }
    }

    public class Position
    {
        public string id { get; set; }
        public string name { get; set; }
        public object[] runwayIds { get; set; }
    }

    public class Tdlsconfiguration
    {
        public bool mandatorySid { get; set; }
        public bool mandatoryClimbout { get; set; }
        public bool mandatoryClimbvia { get; set; }
        public bool mandatoryInitialAlt { get; set; }
        public bool mandatoryDepFreq { get; set; }
        public bool mandatoryExpect { get; set; }
        public bool mandatoryContactInfo { get; set; }
        public bool mandatoryLocalInfo { get; set; }
        public Sid[] sids { get; set; }
        public Climbout[] climbouts { get; set; }
        public object[] climbvias { get; set; }
        public Initialalt[] initialAlts { get; set; }
        public Depfreq[] depFreqs { get; set; }
        public Expect[] expects { get; set; }
        public Contactinfo[] contactInfos { get; set; }
        public Localinfo[] localInfos { get; set; }
        public string defaultSidId { get; set; }
        public object defaultTransitionId { get; set; }
    }

    public class Sid
    {
        public string name { get; set; }
        public string id { get; set; }
        public Transition[] transitions { get; set; }
    }

    public class Transition
    {
        public string name { get; set; }
        public string id { get; set; }
        public object firstRoutePoint { get; set; }
        public string defaultExpect { get; set; }
        public object defaultClimbout { get; set; }
        public object defaultClimbvia { get; set; }
        public string defaultInitialAlt { get; set; }
        public object defaultDepFreq { get; set; }
        public string defaultContactInfo { get; set; }
        public string defaultLocalInfo { get; set; }
    }

    public class Climbout
    {
        public string id { get; set; }
        public string value { get; set; }
    }

    public class Initialalt
    {
        public string id { get; set; }
        public string value { get; set; }
    }

    public class Depfreq
    {
        public string id { get; set; }
        public string value { get; set; }
    }

    public class Expect
    {
        public string id { get; set; }
        public string value { get; set; }
    }

    public class Contactinfo
    {
        public string id { get; set; }
        public string value { get; set; }
    }

    public class Localinfo
    {
        public string id { get; set; }
        public string value { get; set; }
    }

    public class Flightstripsconfiguration
    {
        public Stripbay[] stripBays { get; set; }
        public object[] externalBays { get; set; }
        public bool displayDestinationAirportIds { get; set; }
        public bool displayBarcodes { get; set; }
        public bool enableArrivalStrips { get; set; }
        public bool enableSeparateArrDepPrinters { get; set; }
    }

    public class Stripbay
    {
        public string id { get; set; }
        public string name { get; set; }
        public int numberOfRacks { get; set; }
    }

    public class Childfacility1
    {
        public string id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public object[] childFacilities { get; set; }
        public object eramConfiguration { get; set; }
        public object edstConfiguration { get; set; }
        public object starsConfiguration { get; set; }
        public Towercabconfiguration1 towerCabConfiguration { get; set; }
        public Asdexconfiguration1 asdexConfiguration { get; set; }
        public Tdlsconfiguration1 tdlsConfiguration { get; set; }
        public Flightstripsconfiguration1 flightStripsConfiguration { get; set; }
        public Position2[] positions { get; set; }
        public string[] neighboringFacilityIds { get; set; }
        public object[] nonNasFacilityIds { get; set; }
    }

    public class Towercabconfiguration1
    {
        public string videoMapId { get; set; }
        public int defaultRotation { get; set; }
        public int defaultZoomRange { get; set; }
        public int aircraftVisibilityCeiling { get; set; }
        public Towerlocation2 towerLocation { get; set; }
    }

    public class Towerlocation2
    {
        public float lat { get; set; }
        public float lon { get; set; }
    }

    public class Asdexconfiguration1
    {
        public string videoMapId { get; set; }
        public int defaultRotation { get; set; }
        public int defaultZoomRange { get; set; }
        public int targetVisibilityRange { get; set; }
        public int targetVisibilityCeiling { get; set; }
        public Fixrule1[] fixRules { get; set; }
        public bool useDestinationIdAsFix { get; set; }
        public Runwayconfiguration[] runwayConfigurations { get; set; }
        public Position1[] positions { get; set; }
        public string defaultPositionId { get; set; }
        public Towerlocation3 towerLocation { get; set; }
    }

    public class Towerlocation3
    {
        public float lat { get; set; }
        public float lon { get; set; }
    }

    public class Fixrule1
    {
        public string id { get; set; }
        public string searchPattern { get; set; }
        public string fixId { get; set; }
    }

    public class Runwayconfiguration
    {
        public string id { get; set; }
        public string name { get; set; }
        public string[] arrivalRunwayIds { get; set; }
        public string[] departureRunwayIds { get; set; }
        public object[] holdShortRunwayPairs { get; set; }
    }

    public class Position1
    {
        public string id { get; set; }
        public string name { get; set; }
        public string[] runwayIds { get; set; }
    }

    public class Tdlsconfiguration1
    {
        public bool mandatorySid { get; set; }
        public bool mandatoryClimbout { get; set; }
        public bool mandatoryClimbvia { get; set; }
        public bool mandatoryInitialAlt { get; set; }
        public bool mandatoryDepFreq { get; set; }
        public bool mandatoryExpect { get; set; }
        public bool mandatoryContactInfo { get; set; }
        public bool mandatoryLocalInfo { get; set; }
        public Sid1[] sids { get; set; }
        public Climbout1[] climbouts { get; set; }
        public Climbvia[] climbvias { get; set; }
        public Initialalt1[] initialAlts { get; set; }
        public Depfreq1[] depFreqs { get; set; }
        public Expect1[] expects { get; set; }
        public Contactinfo1[] contactInfos { get; set; }
        public Localinfo1[] localInfos { get; set; }
        public string defaultSidId { get; set; }
        public object defaultTransitionId { get; set; }
    }

    public class Sid1
    {
        public string name { get; set; }
        public string id { get; set; }
        public Transition1[] transitions { get; set; }
    }

    public class Transition1
    {
        public string name { get; set; }
        public string id { get; set; }
        public string firstRoutePoint { get; set; }
        public string defaultExpect { get; set; }
        public object defaultClimbout { get; set; }
        public string defaultClimbvia { get; set; }
        public string defaultInitialAlt { get; set; }
        public string defaultDepFreq { get; set; }
        public string defaultContactInfo { get; set; }
        public string defaultLocalInfo { get; set; }
    }

    public class Climbout1
    {
        public string id { get; set; }
        public string value { get; set; }
    }

    public class Climbvia
    {
        public string id { get; set; }
        public string value { get; set; }
    }

    public class Initialalt1
    {
        public string id { get; set; }
        public string value { get; set; }
    }

    public class Depfreq1
    {
        public string id { get; set; }
        public string value { get; set; }
    }

    public class Expect1
    {
        public string id { get; set; }
        public string value { get; set; }
    }

    public class Contactinfo1
    {
        public string id { get; set; }
        public string value { get; set; }
    }

    public class Localinfo1
    {
        public string id { get; set; }
        public string value { get; set; }
    }

    public class Flightstripsconfiguration1
    {
        public Stripbay1[] stripBays { get; set; }
        public Externalbay[] externalBays { get; set; }
        public bool displayDestinationAirportIds { get; set; }
        public bool displayBarcodes { get; set; }
        public bool enableArrivalStrips { get; set; }
        public bool enableSeparateArrDepPrinters { get; set; }
    }

    public class Stripbay1
    {
        public string id { get; set; }
        public string name { get; set; }
        public int numberOfRacks { get; set; }
    }

    public class Externalbay
    {
        public string facilityId { get; set; }
        public string bayId { get; set; }
    }

    public class Position2
    {
        public string id { get; set; }
        public string name { get; set; }
        public bool starred { get; set; }
        public string radioName { get; set; }
        public string callsign { get; set; }
        public int frequency { get; set; }
        public object eramConfiguration { get; set; }
        public Starsconfiguration1 starsConfiguration { get; set; }
    }

    public class Starsconfiguration1
    {
        public int subset { get; set; }
        public string sectorId { get; set; }
        public string areaId { get; set; }
        public string colorSet { get; set; }
    }

    public class Position3
    {
        public string id { get; set; }
        public string name { get; set; }
        public bool starred { get; set; }
        public string radioName { get; set; }
        public string callsign { get; set; }
        public int frequency { get; set; }
        public object eramConfiguration { get; set; }
        public Starsconfiguration2 starsConfiguration { get; set; }
    }

    public class Starsconfiguration2
    {
        public int subset { get; set; }
        public string sectorId { get; set; }
        public string areaId { get; set; }
        public string colorSet { get; set; }
    }

    public class Position4
    {
        public string id { get; set; }
        public string name { get; set; }
        public bool starred { get; set; }
        public string radioName { get; set; }
        public string callsign { get; set; }
        public int frequency { get; set; }
        public Eramconfiguration1 eramConfiguration { get; set; }
        public object starsConfiguration { get; set; }
    }

    public class Eramconfiguration1
    {
        public string sectorId { get; set; }
    }

    public class Visibilitycenter1
    {
        public float lat { get; set; }
        public float lon { get; set; }
    }

    public class Videomap
    {
        public string id { get; set; }
        public string name { get; set; }
        public string[] tags { get; set; }
        public string shortName { get; set; }
        public string sourceFileName { get; set; }
        public DateTime lastUpdatedAt { get; set; }
        public string starsBrightnessCategory { get; set; }
        public int? starsId { get; set; }
        public bool starsAlwaysVisible { get; set; }
        public bool tdmOnly { get; set; }
    }

}

