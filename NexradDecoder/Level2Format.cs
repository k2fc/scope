



//***********************************************************************
//									*
//  File: Level2Format.java						*
//									*
//  Description: This class provides an interface to NEXRAD Level 2	*
//               format radar data as defined in the RDA/RPG ICD.	*
//									*
//  Developer: David L. Priegnitz					*
//             CIMMS/NSSL						*
//             1313 Halley Circle					*
//             Norman, OK  73069					*
//             (405)-366-0577						*
//             David.Priegnitz@noaa.gov					*
//  History:   10/14/04 - created from old classes Level2Data.java,	*
//                        Level2Record.java.				*
//									*
// ----------------------------------------------------------------------
// Modified by Steve Ansari, National Climatic Data Center (NCDC) to
// use Unidata-produced RandomAccessFile (from MetApps library) that
// uses buffering instead of java.io.RandomAccessFile
//***********************************************************************

//-----------------------------------------------------------------------
//  Level2 Format class definition
//-----------------------------------------------------------------------


using System;
/**
* NOAA's National Climatic Data Center
* NOAA/NESDIS/NCDC
* 151 Patton Ave, Asheville, NC  28801
* 
* THIS SOFTWARE AND ITS DOCUMENTATION ARE CONSIDERED TO BE IN THE 
* PUBLIC DOMAIN AND THUS ARE AVAILABLE FOR UNRESTRICTED PUBLIC USE.  
* THEY ARE FURNISHED "AS IS." THE AUTHORS, THE UNITED STATES GOVERNMENT, ITS
* INSTRUMENTALITIES, OFFICERS, EMPLOYEES, AND AGENTS MAKE NO WARRANTY,
* EXPRESS OR IMPLIED, AS TO THE USEFULNESS OF THE SOFTWARE AND
* DOCUMENTATION FOR ANY PURPOSE. THEY ASSUME NO RESPONSIBILITY (1)
* FOR THE USE OF THE SOFTWARE AND DOCUMENTATION; OR (2) TO PROVIDE
* TECHNICAL SUPPORT TO USERS.
*//**
*  Description of the Class
*
*@author     ncdc.laptop
*@created    March 7, 2005
*/
public class Level2Format
{

    // Constants

    public bool verbose = false;

    /**
     *  Description of the Field
     */
    const int RECORD_HEADER_SIZE = 12;
    /**
     *  Description of the Field
     */
    const int FILE_HEADER_SIZE = 24;
    /**
     *  Description of the Field
     */
    const int RADAR_DATA_MSG_SIZE = 2432;
    /**
     *  Description of the Field
     */
    const int MAX_LEVEL2_RECORDS = 10000;

    /**
     *  Description of the Field
     */
    const int REFLECTIVITY = 0;
    /**
     *  Description of the Field
     */
    const int VELOCITY = 1;
    /**
     *  Description of the Field
     */
    const int SPECTRUM_WIDTH = 2;

    /**
     *  Description of the Field
     */
    const int SIGNAL_BELOW_THRESHOLD = 999;
    /**
     *  Description of the Field
     */
    const int SIGNAL_OVERLAID = 998;
    /**
     *  Description of the Field
     */
    const int DATA_NOT_FOUND = 997;

    /**
     *  Description of the Field
     */
    const int DOPPLER_RESOLUTION_LOW_CODE = 4;
    /**
     *  Description of the Field
     */
    const int DOPPLER_RESOLUTION_HIGH_CODE = 2;
    /**
     *  Description of the Field
     */
    const float DOPPLER_RESOLUTION_LOW = (float)1.0;
    /**
     *  Description of the Field
     */
    const float DOPPLER_RESOLUTION_HIGH = (float)0.5;

    /**
     *  Description of the Field
     */
    const int MAX_RADIALS_IN_CUT = 500;

    /**
     *  Description of the Field
     */
    const float HORIZONTAL_BEAM_WIDTH = (float)1.5;

    /**
     *  Description of the Field
     */
    const float KM_PER_NM = (float)1.85;

    // Global variables

    /**
     *  Description of the Field
     */
    public static int data_lut_init_flag = 0;

    /**
     *  Description of the Field
     */
    public static float[] Reflectivity_LUT = new float[256];
    /**
     *  Description of the Field
     */
    public static float[] Velocity_1km_LUT = new float[256];
    /**
     *  Description of the Field
     */
    public static float[] Velocity_hkm_LUT = new float[256];

    // Initialize moment lookup tables
    /*
    static {

      int i;

    Reflectivity_LUT[0] = (float) SIGNAL_BELOW_THRESHOLD;
    Reflectivity_LUT[1] = (float) SIGNAL_OVERLAID;
    Velocity_1km_LUT[0] = (float) SIGNAL_BELOW_THRESHOLD;
    Velocity_1km_LUT[1] = (float) SIGNAL_OVERLAID;
    Velocity_hkm_LUT[0] = (float) SIGNAL_BELOW_THRESHOLD;
    Velocity_hkm_LUT[1] = (float) SIGNAL_OVERLAID;

      for (i = 2; i< 256; i++) {

         Reflectivity_LUT[i] = (float) (i / 2.0 - 33.0);
         Velocity_1km_LUT[i] = (float) (i - 129.0);
         Velocity_hkm_LUT[i] = (float) (i / 2.0 - 64.5);

      }
   
        */
    int eofFlag = 0;

    //  Level2 Record header elements

    short messageSize = 0;
    // message size in halfwords for this segment
    byte channel = 0;
    // RDA channel number
    byte messageType = 0;
    // message type (radar data = 1)
    short idSequence = 0;
    // message sequence number
    short julianDate = 0;
    // julian date (from 1/1/1970)
    int milliseconds = 0;
    // milliseconds from midnight
    short numberSegments = 0;
    // number of segments for radial
    short segNumber = 0;
    // segment number of this message
    int time = 0;
    // Zulu reference time for radial
    short julian = 0;
    // current julian date (from 1/1/1970)
    short unambiguousRange = 0;
    // unambiguous range (kilometers)
    int azimuthAngle = 0;
    // scaled radial azimuth angle (degrees)
    short azimuthNumber = 0;
    // radial azimuth number
    short radialStatus = 0;
    // radial status (0 - start of new elevation
    //                1 - intermediate radial
    //                2 - end of elevation
    //                3 - start of volume
    //                4 - end of volume
    short elevationAngle = 0;
    // scaled radial elevation angle (degrees)
    short elevationNumber = 0;
    // radial elevation number
    short surveillanceRange = 0;
    // scaled range to 1st surv gate center (km)
    short dopplerRange = 0;
    // scaled range to 1st dopl gate center (km)
    short surveillanceInterval = 0;
    // scaled reflectivity sample interval (km)
    short dopplerInterval = 0;
    // scaled doppler sample interval (km)
    short surveillanceBins = 0;
    // number of reflectivity bins
    short dopplerBins = 0;
    // number of doppler bins
    short sectorNumber = 0;
    // sector number within cut
    float calibration = 0;
    // scaling constant for dBZ calc (dB)
    short reflectivityPointer = 0;
    // pointer to first reflectivity bin in
    // this record
    short velocityPointer = 0;
    // pointer to first velocity bin in this
    // record.
    short spectrumWidthPointer = 0;
    // pointer to first spectru width bin in
    // this record
    short resolution = 0;
    // doppler resolution (2 = 0.5 m/s;
    //                     4 = 1.0 m/s)
    short vcp = 0;
    // volume coverage pattern used
    short nyquistVelocity = 0;
    // scaled Nyquist Velocity (m/s)
    short attenuation = 0;
    // atmospheric attenuation factor (dB/km)
    short tover = 0;
    // TOVER (dB)
    short spotBlanking = 0;
    // spot blanking status for current radial;
    //     1 = radial
    //     2 = elevation
    //     4 = volume
    //     0 = none
    int[] spare = new int[32];
    byte[][] bins = new byte[MAX_RADIALS_IN_CUT][2400];

    /**
     *  Description of the Field
     */
    public int oldCut = -1;
    /**
     *  Description of the Field
     */
    public int cut = 0;
    /**
     *  Description of the Field
     */
    public int endOfVolume = 0;

    private float[] azimuth = new float[10000];
    private float[] elevation = new float[10000];
    private float[] trueElevation = new float[100];
    //private int  [] cutStart         = new int   [   20];
    private int[] cutStart = new int[50];
    // s.ansari - changed to 50 in case new vcp with more scans
    /**
     *  Description of the Field
     */
    public int numberOfCuts = 0;
    /**
     *  Description of the Field
     */
    public int numberOfTrueCuts = 0;
    private float[] cutElevation = new float[100];
    private int[] cutIndex = new int[100];
    private float oldElevation = (float)0.0;
    private float deltaElevation = (float)0.0;

    /**
     *  Description of the Field
     */
    public int numberOfRecords = 0;
    private int record = 0;

    /**
     *  Description of the Field
     */
    public int firstRecordInCut = 0;
    /**
     *  Description of the Field
     */
    public int lastRecordInCut = 0;

    //  public  RandomAccessFile din;

    //  public  File level2File = null;
    /**
     *  MetApps HTTP Ready RandomAccessFile
     */
    public ucar.unidata.io.RandomAccessFile din;

    static String file = null;
    static String dir = null;


    //-----------------------------------------------------------------------
    //  Constructor
    //-----------------------------------------------------------------------

    /**
     *  Description of the Method
     */
    public Level2Format()
    {
    }


    //-----------------------------------------------------------------------
    //  Descroption: Method to open a Level2 Format file and build internal
    //               tables.
    //  Input: file      - File object for the universal format data file
    //         startFlag - unused at this time
    //-----------------------------------------------------------------------

    //public void open (File file, int startFlag) {
    /**
     *  Description of the Method
     *
     *@param  nexrad_url  Description of the Parameter
     *@param  start_flag  Description of the Parameter
     */
    public void read(string nexrad_url, int start_flag)
    {

        /*
         *  Read the file until we get an I/O exception (i.e., EOF)
         */
        try
        {

            float azi;
            float ele;
            int i;
            int julianDate;
            int seconds;

            byte[] bins = new byte[2332];

            //if (file != level2File) {

            //level2File = file;
            _numberOfRecords = 0;

            //din = new RandomAccessFile (file,"r");
            //Console.WriteLine ("-----"+file+"-----");
            //Console.WriteLine ("\nLoading Data starting with record"+
            //               numberOfRecords+" Be patient!");

            try
            {
                if (nexrad_url.getProtocol().equals("file"))
                {
                    din = new ucar.unidata.io.RandomAccessFile(nexrad_url.getFile().replaceAll("%20", " "), "r");
                }
                else
                {
                    din = new ucar.unidata.io.http.HTTPRandomAccessFile(nexrad_url.toString());
                }
                din.order(ucar.unidata.io.RandomAccessFile.BIG_ENDIAN);

                Console.WriteLine("**** OPENING: " + nexrad_url + " ::: " + din);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("ERROR WITH URL: " + nexrad_url);
                return;
            }

            //Console.WriteLine("-----" + nexrad_url + "-----");
            //Console.WriteLine("\nLoading Data starting with record" +
            //      numberOfRecords + " Be patient!");

            //      Skip over the 12 byte record header which is not used

            din.skipBytes(RECORD_HEADER_SIZE);

            //      Read the date (julian from 1/1/1970) and time (milliseconds
            //      past midnight

            julianDate = din.readInt();
            seconds = din.readInt();

            //Console.WriteLine("File header julian date [" + julianDate +
            //      "] - seconds [" + seconds + "]");

            //      }

            //    Initialize the cut fields.  Remember that the number of cuts will
            //    probably be larger than the number of true cuts since the lowest
            //    few cuts a split (surveillance and Doppler).  A true cut is
            //    basically a unique elevation.

            numberOfCuts = 0;
            numberOfTrueCuts = 0;

            //for (cut=0;cut<20;cut++)  // s.ansari - changed to 50 in case new vcp with more scans
            for (cut = 0; cut < 50; cut++)
            {
                cutStart[cut] = -1;
            }

            //    Read the entire file and return when we get an end-of-file
            //    exception.  We assume that all level2 files contain less than
            //    MAX_LEVEL2_RECORDS.

            for (record = _numberOfRecords; record < MAX_LEVEL2_RECORDS; record++)
            {

                readHeader(record);

                //      Check for and end-of-file condition

                if (eof() != 0)
                {

                    cutStart[oldCut + 1] = record;
                    return;
                }

                //      Check to see if this is the last radial in the volume.  If so,
                //      set the end of volume flag to 1.

                if (radialStatus == 4)
                {
                    endOfVolume = 1;
                }
                else
                {
                    endOfVolume = 0;
                }

                //      Only process digital radar data messages.  Ignore the rest for
                //	now.

                if (getMessageType() == 1)
                {

                    //        Extract the azimuth and elevation angles of the radial.

                    azimuth[record] = getAzimuth();
                    elevation[record] = getElevation();

                    //        If a new elevation cut has started, update the cut LUT

                    if (oldCut != getElevationNum())
                    {

                        if (verbose)
                        {
                            Console.WriteLine("messageSize [" + messageSize + "]");
                        }

                        if (getElevationNum() > 0)
                        {

                            if (verbose)
                            {
                                Console.WriteLine("New cut --> " + getElevationNum() +
                                   " at record " + record);
                            }
                            oldCut = getElevationNum();
                            cutStart[getElevationNum() - 1] = record;
                            cutElevation[numberOfCuts] = elevation[record];

                            //            We also need to maintain a table of unique elevation
                            //            cuts to account for split cuts.

                            deltaElevation = Math.Abs(elevation[record] - oldElevation);

                            if (deltaElevation > 0.1)
                            {

                                trueElevation[numberOfTrueCuts] = elevation[record];
                                numberOfTrueCuts++;

                            }

                            oldElevation = elevation[record];
                            cutIndex[numberOfCuts] = numberOfTrueCuts;
                            numberOfCuts++;

                            if (verbose)
                            {
                                Console.WriteLine("dBZ bins [" + getBinNum(REFLECTIVITY) + "]");
                                Console.WriteLine("Vel bins [" + getBinNum(VELOCITY) + "]");
                            }

                        }
                        else
                        {

                            return;
                        }
                    }

                    _numberOfRecords++;

                }
                else
                {

                    //        Since this message doesn't contain radar data, set the
                    //        azimuth/elevation LUT table entries to -1 to indicate this
                    //        record doesn't contain radar data.

                    if (verbose)
                    {
                        Console.WriteLine("Message type " + getMessageType() + " detected");
                    }
                    azimuth[record] = -1;
                    elevation[record] = -1;

                }
            }
            //  For EOF exceptions just print a message.
        }
        catch (Exception e)
        {

            //      Console.WriteLine (e);

        }
    }


    //---------------------------------------------------------------------
    //  Description: This method returns the number of unique cuts in the
    //               file.
    //---------------------------------------------------------------------

    /**
     *  Gets the numberOfTrueCuts attribute of the Level2Format object
     *
     *@return    The numberOfTrueCuts value
     */
    public int getNumberOfTrueCuts()
    {

        return (numberOfTrueCuts);
    }


    //---------------------------------------------------------------------
    //  Description: This method returns the unique cut for the specified
    //               elevation cut.
    //  Input: cut_num - The volume cut (elevation) number
    //---------------------------------------------------------------------

    /**
     *  Gets the cutIndex attribute of the Level2Format object
     *
     *@param  cut_num  Description of the Parameter
     *@return          The cutIndex value
     */
    public int getCutIndex(int cut_num)
    {

        if (cut_num <= numberOfCuts)
        {
            return (0);
        }
        else
        {
            return (cutIndex[cut_num]);
        }

    }


    //---------------------------------------------------------------------
    //  Description: This method returns the number of cuts in the file.
    //---------------------------------------------------------------------

    /**
     *  Gets the numberOfCuts attribute of the Level2Format object
     *
     *@return    The numberOfCuts value
     */
    public int getNumberOfCuts()
    {

        return (numberOfCuts);
    }


    //---------------------------------------------------------------------
    //  Description: This method returns the elevation angle (degrees)
    //               of the specified cut (elevation number).
    //  Input: cut_num - The volume cut (elevation) number
    //---------------------------------------------------------------------

    /**
     *  Gets the cutElevation attribute of the Level2Format object
     *
     *@param  cut_num  Description of the Parameter
     *@return          The cutElevation value
     */
    public float getCutElevation(int cut_num)
    {

        if (cut_num >= numberOfCuts)
        {
            return ((float)-99.9);
        }
        else
        {
            return (cutElevation[cut_num]);
        }

    }


    //---------------------------------------------------------------------
    //  Description: This method returns the record number of the first
    //               radial in the specified cut (elevation number).
    //  Input: cut_num - The volume cut (elevation) number
    //---------------------------------------------------------------------

    /**
     *  Gets the cutStart attribute of the Level2Format object
     *
     *@param  cut_num  Description of the Parameter
     *@return          The cutStart value
     */
    public int getCutStart(int cut_num)
    {

        return (cutStart[cut_num]);
    }


    //---------------------------------------------------------------------
    //  Description: This method returns the azimuth angle for the specified
    //               record.
    //  Input: record - The record of the radial from the beginning of the
    //                  file.
    //---------------------------------------------------------------------

    /**
     *  Gets the azimuth attribute of the Level2Format object
     *
     *@param  record  Description of the Parameter
     *@return         The azimuth value
     */
    public float getAzimuth(int record)
    {

        return (azimuth[record]);
    }


    //---------------------------------------------------------------------
    //  Description: This method returns the elevation angle for the specified
    //               record.
    //  Input: record - The record of the radial from the beginning of the
    //                  file.
    //---------------------------------------------------------------------

    /**
     *  Gets the elevation attribute of the Level2Format object
     *
     *@param  record  Description of the Parameter
     *@return         The elevation value
     */
    public float getElevation(int record)
    {

        return (elevation[record]);
    }


    //---------------------------------------------------------------------
    //  Description: This method returns the true elevation angle for the
    //               specified elevation cut.
    //  Input: cut - The cut number for the unique elevation.
    //---------------------------------------------------------------------

    /**
     *  Gets the trueElevation attribute of the Level2Format object
     *
     *@param  cut  Description of the Parameter
     *@return      The trueElevation value
     */
    public float getTrueElevation(int cut)
    {

        return (trueElevation[cut]);
    }


    //---------------------------------------------------------------------
    //  Description: This method returns the number of records read in from
    //               the beginning of the level 2 file.
    //---------------------------------------------------------------------

    /**
     *  Description of the Method
     *
     *@return    Description of the Return Value
     */
    int _numberOfRecords;
    public int numberOfRecords()
    {

        return (_numberOfRecords);
    }


    //---------------------------------------------------------------------
    //  Description: The following method reads the azimuth and elevation
    //               angles from from a specified level2 record.
    //  Input: record - record number from the beginning of the file.
    //---------------------------------------------------------------------

    /**
     *  Description of the Method
     *
     *@param  record  Description of the Parameter
     */
    public void readAngles(int record)
    {

        try
        {

            int count = 0;
            long offset = 0;
            int i;

            //    Determine the byte offset in the file to the start of the
            //    specified record and the skip the bytes.

            offset = record * RADAR_DATA_MSG_SIZE + FILE_HEADER_SIZE;
            din.seek(offset);

            din.skipBytes(RECORD_HEADER_SIZE);

            messageSize = din.readShort();
            channel = din.readByte();
            messageType = din.readByte();

            //    Since we aren't interested in anything but angles skip over
            //    everything before the azimuth element.

            din.skipBytes(20);

            azimuthAngle = (int)din.readUnsignedShort();

            //    Skip over everything before the elevation angle element

            din.skipBytes(4);

            elevationAngle = din.readShort();
            elevationNumber = din.readShort();

        }
        catch (System.IO.EndOfStreamException e)
        {

            eofFlag = 1;

            //      Console.WriteLine ("End-of-file exception caught");

        }
        catch (Exception e)
        {

            Console.WriteLine(e);

        }
    }


    //---------------------------------------------------------------------
    //  Description: The following method reads the header portion of a
    //               specified level2 record.
    //  Input - record - Record number from the beginning of the file.
    //---------------------------------------------------------------------

    /**
     *  Description of the Method
     *
     *@param  record  Description of the Parameter
     */
    public void readHeader(int record)
    {

        try
        {

            int count = 0;
            long offset = 0;
            int i;

            //    Determine the byte offset in the file to the start of the
            //    specified record and the skip the bytes.

            offset = record * RADAR_DATA_MSG_SIZE + FILE_HEADER_SIZE;
            din.seek(offset);

            din.skipBytes(RECORD_HEADER_SIZE);

            messageSize = din.readShort();
            channel = din.readByte();
            messageType = din.readByte();
            idSequence = din.readShort();
            julianDate = din.readShort();
            milliseconds = din.readInt();
            numberSegments = din.readShort();
            segNumber = din.readShort();
            time = din.readInt();
            julian = din.readShort();
            unambiguousRange = din.readShort();
            azimuthAngle = (int)din.readUnsignedShort();
            azimuthNumber = din.readShort();
            radialStatus = din.readShort();
            elevationAngle = din.readShort();
            elevationNumber = din.readShort();
            surveillanceRange = din.readShort();
            dopplerRange = din.readShort();
            surveillanceInterval = din.readShort();
            dopplerInterval = din.readShort();
            surveillanceBins = din.readShort();
            dopplerBins = din.readShort();
            sectorNumber = din.readShort();
            calibration = din.readFloat();
            reflectivityPointer = din.readShort();
            velocityPointer = din.readShort();
            spectrumWidthPointer = din.readShort();
            resolution = din.readShort();
            vcp = din.readShort();

            //    Skip over the archive II header fields since we don't use them.

            din.skipBytes(14);

            nyquistVelocity = din.readShort();
            attenuation = din.readShort();
            tover = din.readShort();
            spotBlanking = din.readShort();

            din.skipBytes(2332);

        }
        catch (System.IO.EndOfStreamException e)
        {

            eofFlag = 1;

            //Console.WriteLine("End-of-file exception caught");

        }
        catch (Exception e)
        {

            Console.WriteLine(e);

        }
    }


    //---------------------------------------------------------------------
    // Description: Method to read a selected record from a specified level
    //              II file.
    //  Input - record - Record number from the beginning of the file.
    //---------------------------------------------------------------------

    /**
     *  Description of the Method
     *
     *@param  record  Description of the Parameter
     */
    public void readRecord(int record)
    {

        try
        {

            int count = 0;
            long offset = 0;
            int i;

            //    Determine the byte offset in the file to the start of the
            //    specified record and the skip the bytes.

            offset = record * RADAR_DATA_MSG_SIZE + FILE_HEADER_SIZE;
            din.seek(offset);

            din.skipBytes(RECORD_HEADER_SIZE);

            messageSize = din.readShort();
            channel = din.readByte();
            messageType = din.readByte();
            idSequence = din.readShort();
            julianDate = din.readShort();
            milliseconds = din.readInt();
            numberSegments = din.readShort();
            segNumber = din.readShort();
            time = din.readInt();
            julian = din.readShort();
            unambiguousRange = din.readShort();
            azimuthAngle = (int)din.readUnsignedShort();
            azimuthNumber = din.readShort();
            radialStatus = din.readShort();
            elevationAngle = din.readShort();
            elevationNumber = din.readShort();
            surveillanceRange = din.readShort();
            dopplerRange = din.readShort();
            surveillanceInterval = din.readShort();
            dopplerInterval = din.readShort();
            surveillanceBins = din.readShort();
            dopplerBins = din.readShort();
            sectorNumber = din.readShort();
            calibration = din.readFloat();
            reflectivityPointer = din.readShort();
            velocityPointer = din.readShort();
            spectrumWidthPointer = din.readShort();
            resolution = din.readShort();
            vcp = din.readShort();

            //    Skip over the archive II header fields since we don't use them.

            din.skipBytes(14);

            nyquistVelocity = din.readShort();
            attenuation = din.readShort();
            tover = din.readShort();
            spotBlanking = din.readShort();

            //    Skip over the trailing unused portion of the header to get to the
            //    start of the radar data.

            din.skipBytes(32);

            //    Read the radar data into a data buffer.

            din.readFully(bins[0], 0, 2304);

        }
        catch (System.IO.EndOfStreamException e)
        {

            eofFlag = 1;

            //    Console.WriteLine ("End-of-file exception caught");

        }
        catch (Exception e)
        {

            Console.WriteLine(e);

        }
    }


    //---------------------------------------------------------------------
    // Description: Method to read all radials in a selected cut from a
    //              specified levela2 file.
    //  Input - record - Record number from the beginning of the file where
    //                   the cut starts.
    //---------------------------------------------------------------------

    /**
     *  Description of the Method
     *
     *@param  record  Description of the Parameter
     *@return         Description of the Return Value
     */
    public int readCut(int record)
    {

        int i = 0;
        int j = 0;
        int range;
        int max;

        firstRecordInCut = record;
        lastRecordInCut = record;

        try
        {

            int count = 0;
            long offset = 0;
            short oldElevationNumber = 99;

            //    Read the header of the first radial in the cut so we can update
            //    the data pointers.

            readRecord(record);

            oldElevationNumber = 999;

            for (i = 0; i < MAX_RADIALS_IN_CUT; i++)
            {

                offset = (record + i) * RADAR_DATA_MSG_SIZE + FILE_HEADER_SIZE;
                din.seek(offset);

                din.skipBytes(RECORD_HEADER_SIZE + 3);

                messageType = din.readByte();

                if (messageType == 1)
                {

                    din.skipBytes(28);
                    elevationNumber = din.readShort();

                    if (elevationNumber > oldElevationNumber)
                    {

                        Console.WriteLine(i + " radials read in cut " + oldElevationNumber);
                        break;
                    }
                    else
                    {

                        lastRecordInCut = firstRecordInCut + i;
                        oldElevationNumber = elevationNumber;
                        din.skipBytes(82);

                        din.readFully(bins[i], 0, 2304);

                    }

                }
                else
                {

                    if (verbose)
                    {
                        Console.WriteLine("Message type " + messageType + " found");
                    }

                }
            }

            return (i);
        }
        catch (System.IO.EndOfStreamException e)
        {

            eofFlag = 1;

            //    Console.WriteLine ("End-of-file exception caught");

            return (i);
        }
        catch (Exception e)
        {

            Console.WriteLine(e);
            return (i);
        }
    }


    //---------------------------------------------------------------------
    // Description: This method  returns the azimuth angle (degrees) from
    //              the latest read record.
    //---------------------------------------------------------------------

    /**
     *  Gets the azimuth attribute of the Level2Format object
     *
     *@return    The azimuth value
     */
    public float getAzimuth()
    {

        if (messageType != 1)
        {

            return ((float)-1);
        }
        else
        {

            return (((float)180.0) * ((float)azimuthAngle) / ((float)32768));
        }
    }


    //---------------------------------------------------------------------
    // Description: This method  returns the elevation angle (degrees) from
    //              the latest read record.
    //---------------------------------------------------------------------

    /**
     *  Gets the elevation attribute of the Level2Format object
     *
     *@return    The elevation value
     */
    public float getElevation()
    {

        return (((float)180.0) * ((float)elevationAngle) / ((float)32768));
    }


    //---------------------------------------------------------------------
    // Description: This method  returns the range to the first bin (km)
    //              from the latest read record.
    // Input - moment - Moment index (REFLECTIVITY, VELOCITY, SPECTRUM_WIDTH)
    //---------------------------------------------------------------------

    /**
     *  Gets the rangeToFirstBin attribute of the Level2Format object
     *
     *@param  moment  Description of the Parameter
     *@return         The rangeToFirstBin value
     */
    public float getRangeToFirstBin(int moment)
    {

        if (moment == REFLECTIVITY)
        {

            return (((float)surveillanceRange) / ((float)1000));
        }
        else
        {

            return (((float)dopplerRange) / ((float)1000));
        }
    }


    //---------------------------------------------------------------------
    // Description: This method  returns the elevation number
    //              from the latest read record.
    //---------------------------------------------------------------------

    /**
     *  Gets the elevationNum attribute of the Level2Format object
     *
     *@return    The elevationNum value
     */
    public short getElevationNum()
    {

        return (elevationNumber);
    }


    //---------------------------------------------------------------------
    // Description: This method  returns the message type
    //              from the latest read record.
    //---------------------------------------------------------------------

    /**
     *  Gets the messageType attribute of the Level2Format object
     *
     *@return    The messageType value
     */
    public short getMessageType()
    {

        return (messageType);
    }


    //---------------------------------------------------------------------
    // Description: This method  returns the Volume Coverage Pattern (VCP)
    //              from the latest read record.
    //---------------------------------------------------------------------

    /**
     *  Gets the vCP attribute of the Level2Format object
     *
     *@return    The vCP value
     */
    public short getVCP()
    {

        return (vcp);
    }


    //---------------------------------------------------------------------
    //  Description: This method returns the state of the end of file flag.
    //               If 0 is returned, the end of file condition has not
    //               been met.  If 1 is returned, the end of file has been
    //               reached.
    //---------------------------------------------------------------------

    /**
     *  Description of the Method
     *
     *@return    Description of the Return Value
     */
    public int eof()
    {

        return (eofFlag);
    }


    //-----------------------------------------------------------------------
    //  Description: This method returns the radial bin size (meters) for
    //               the specified moment in a read level II record.
    //  Input: moment - Moment ID
    //	   bin    - bin index
    //-----------------------------------------------------------------------

    /**
     *  Gets the binSize attribute of the Level2Format object
     *
     *@param  moment  Description of the Parameter
     *@return         The binSize value
     */
    public int getBinSize(int moment)
    {

        switch (moment)
        {

            case REFLECTIVITY:

                return ((int)surveillanceInterval);
            case VELOCITY:
            case SPECTRUM_WIDTH:

                return ((int)dopplerInterval);
        }

        return (0);
    }


    //-----------------------------------------------------------------------
    //  Description: This method returns the number of bins defined for
    //               the specified moment in a read level II record.
    //  Input: moment - Moment ID
    //         bin    - bin index
    //-----------------------------------------------------------------------

    /**
     *  Gets the binNum attribute of the Level2Format object
     *
     *@param  moment  Description of the Parameter
     *@return         The binNum value
     */
    public int getBinNum(int moment)
    {

        switch (moment)
        {

            case REFLECTIVITY:

                return ((int)surveillanceBins);
            case VELOCITY:
            case SPECTRUM_WIDTH:

                return ((int)dopplerBins);
        }

        return (0);
    }


    //-----------------------------------------------------------------------
    //  Description: This method returns a bin data element from the
    //               specified read level II record.
    //  Input: moment - Moment ID
    //         radial - radial index
    //	   bin    - bin index
    //-----------------------------------------------------------------------

    /**
     *  Gets the binData attribute of the Level2Format object
     *
     *@param  moment  Description of the Parameter
     *@param  radial  Description of the Parameter
     *@param  bin     Description of the Parameter
     *@return         The binData value
     */
    public int getBinData(int moment, int radial, int bin)
    {

        int value;

        value = DATA_NOT_FOUND;

        switch (moment)
        {

            case REFLECTIVITY:

                value = bins[radial][(int)(reflectivityPointer - 100 + bin)] >= 0 ?
                      bins[radial][(int)(reflectivityPointer - 100 + bin)] :
                      256 + bins[radial][(int)(reflectivityPointer - 100 + bin)];
                break;
            case VELOCITY:

                value = bins[radial][(int)(velocityPointer - 100 + bin)] >= 0 ?
                      bins[radial][(int)(velocityPointer - 100 + bin)] :
                      256 + bins[radial][(int)(velocityPointer - 100 + bin)];
                break;
            case SPECTRUM_WIDTH:

                value = bins[radial][(int)(spectrumWidthPointer - 100 + bin)] >= 0 ?
                      bins[radial][(int)(spectrumWidthPointer - 100 + bin)] :
                      256 + bins[radial][(int)(spectrumWidthPointer - 100 + bin)];
                break;
        }

        return (value);
    }


    //-----------------------------------------------------------------------
    //  Description: This method returns a bin data value from the specified
    //               read level II record.
    //  Input: moment - Moment ID
    //         radial - radial index
    //	   bin    - bin index
    //-----------------------------------------------------------------------

    /**
     *  Gets the binValue attribute of the Level2Format object
     *
     *@param  moment  Description of the Parameter
     *@param  radial  Description of the Parameter
     *@param  bin     Description of the Parameter
     *@return         The binValue value
     */
    public float getBinValue(int moment, int radial, int bin)
    {

        int value;

        switch (moment)
        {

            case REFLECTIVITY:

                value = bins[radial][(int)(reflectivityPointer - 100 + bin)] >= 0 ?
                      bins[radial][(int)(reflectivityPointer - 100 + bin)] :
                      256 + bins[radial][(int)(reflectivityPointer - 100 + bin)];

                return (Reflectivity_LUT[value]);
            case VELOCITY:

                value = bins[radial][(int)(velocityPointer - 100 + bin)] >= 0 ?
                      bins[radial][(int)(velocityPointer - 100 + bin)] :
                      256 + bins[radial][(int)(velocityPointer - 100 + bin)];

                if (resolution == DOPPLER_RESOLUTION_LOW)
                {

                    return (Velocity_1km_LUT[value]);
                }
                else
                {

                    return (Velocity_hkm_LUT[value]);
                }

            case SPECTRUM_WIDTH:

                value = bins[radial][(int)(spectrumWidthPointer - 100 + bin)] >= 0 ?
                      bins[radial][(int)(spectrumWidthPointer - 100 + bin)] :
                      256 + bins[radial][(int)(spectrumWidthPointer - 100 + bin)];

                return (Velocity_hkm_LUT[value]);
        }

        return ((float)DATA_NOT_FOUND);
    }


    //-----------------------------------------------------------------------
    //  Description: This method returns the moment value from the radial that
    //               most closely matches the the specified radial and range.
    //  Input: moment  - Moment ID (REFLECTIVITY, VELOCITY, SPECTRUM_WIDTH)
    //         azimuth - azimuth angle (degrees)
    //	   range   - radial range (kilometers)
    //-----------------------------------------------------------------------

    /**
     *  Gets the binMoment attribute of the Level2Format object
     *
     *@param  moment  Description of the Parameter
     *@param  azi     Description of the Parameter
     *@param  ran     Description of the Parameter
     *@return         The binMoment value
     */
    public float getBinMoment(int moment, float azi, float ran)
    {

        float diff = 0;
        float oldDiff = (float)9999.9;
        float value;
        int radial;
        int scale;
        int bin;

        value = (float)-99.9;
        oldDiff = (float)999.9;
        radial = -1;

        Console.WriteLine("Getting value for moment " + moment);

        switch (moment)
        {

            case REFLECTIVITY:

                //      We need to find the radial that is closest to the specified azimuth.
                //      If the closest radial is more than one beamwidth away then we want
                //      to return -99.9.

                for (int i = firstRecordInCut; i <= lastRecordInCut; i++)
                {

                    diff = Math.Abs(azimuth[i] - azi);

                    if (diff < oldDiff)
                    {
                        oldDiff = diff;
                        radial = i;
                    }
                }

                radial = radial - firstRecordInCut;

                if (radial < 0)
                {
                    return ((float)-99.9);
                }

                //      Now that we found the closest radial we need to convert the
                //      input range to a bin index.

                bin = (int)(ran - getRangeToFirstBin(moment) /
                      ((float)surveillanceInterval) / ((float)1000));

                if ((bin < 0) || (bin > surveillanceBins))
                {
                    return ((float)-99.9);
                }
                else
                {

                    scale = bins[radial][(int)(reflectivityPointer - 100 + bin)] >= 0 ?
                          bins[radial][(int)(reflectivityPointer - 100 + bin)] :
                          256 + bins[radial][(int)(reflectivityPointer - 100 + bin)];
                    value = scale / ((float)2.0) - (float)33.0;

                }
                break;
            case VELOCITY:

                //      We need to find the radial that is closest to the specified azimuth.
                //      If the closest radial is more than one beamwidth away then we want
                //      to return -99.9.

                for (int i = firstRecordInCut; i <= lastRecordInCut; i++)
                {

                    diff = Math.Abs(azimuth[i] - azi);

                    if (diff < oldDiff)
                    {
                        oldDiff = diff;
                        radial = i;
                    }
                }

                radial = radial - firstRecordInCut;

                if (radial < 0)
                {
                    return ((float)-99.9);
                }

                //      Now that we found the closest radial we need to convert the
                //      input range to a bin index.

                bin = (int)((ran - getRangeToFirstBin(moment)) /
                      (((float)dopplerInterval) / ((float)1000)));

                if ((bin < 0) || (bin > dopplerBins))
                {
                    return ((float)-99.9);
                }
                else
                {

                    scale = bins[radial][(int)(velocityPointer - 100 + bin)] >= 0 ?
                          bins[radial][(int)(velocityPointer - 100 + bin)] :
                          256 + bins[radial][(int)(velocityPointer - 100 + bin)];

                    if (resolution == DOPPLER_RESOLUTION_LOW_CODE)
                    {
                        value = scale - (float)129.0;
                    }
                    else
                    {
                        value = scale / ((float)2.0) - (float)64.5;
                    }

                }

                break;
            case SPECTRUM_WIDTH:

                //      We need to find the radial that is closest to the specified azimuth.
                //      If the closest radial is more than one beamwidth away then we want
                //      to return -99.9.

                for (int i = firstRecordInCut; i <= lastRecordInCut; i++)
                {

                    diff = Math.Abs(azimuth[i] - azi);

                    if (diff < oldDiff)
                    {
                        oldDiff = diff;
                        radial = i;
                    }
                }

                radial = radial - firstRecordInCut;

                if (radial < 0)
                {
                    return ((float)-99.9);
                }

                //      Now that we found the closest radial we need to convert the
                //      input range to a bin index.

                bin = (int)((ran - getRangeToFirstBin(moment)) /
                      (((float)dopplerInterval) / ((float)1000)));

                if ((bin < 0) || (bin > dopplerBins))
                {
                    return ((float)-99.9);
                }
                else
                {

                    scale = bins[radial][(int)(spectrumWidthPointer - 100 + bin)] >= 0 ?
                          bins[radial][(int)(spectrumWidthPointer - 100 + bin)] :
                          256 + bins[radial][(int)(spectrumWidthPointer - 100 + bin)];

                    value = scale / ((float)2.0) - (float)64.5;

                }

                break;
        }

        return (value);
    }



    /**
     *  Description of the Method
     */
    public void close()
    {
        try
        {
            if (din != null)
            {
                din.close();
                //Console.WriteLine("**** CLOSED: ::: " + din);
                din = null;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);
        }
    }

}

