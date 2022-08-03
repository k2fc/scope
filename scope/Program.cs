using System;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Linq;
using DGScope.Receivers;
using System.Security.Principal;

namespace DGScope
{
    static class Program
    {
        static void Start(bool screensaver = false, string facilityConfig = null)
        {

            //Only prompt the user for a facilityConfig file if they are not using screensaver mode, otherwise, open the default passed file
            if (screensaver && facilityConfig==null)
                facilityConfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DGScope.xml");
            else if (facilityConfig == null)
            {
                using (OpenFileDialog open = new OpenFileDialog())
                {
                    open.Filter = "Facility Config File (*.xml)|*.xml|All files (*.*)|*.*";
                    open.FilterIndex = 1;
                    open.CheckFileExists = true;
                    if (open.ShowDialog() == DialogResult.OK)
                    {
                        facilityConfig = open.FileName;
                    }
                    else
                    {
                        var mboxresult = MessageBox.Show("Would you like to create a new config file?","No Config File Selected", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                        if (mboxresult == DialogResult.Yes)
                        {
                            using (SaveFileDialog save = new SaveFileDialog())
                            {
                                save.Filter = "Facility Config File (*.xml)|*.xml|All files (*.*)|*.*";
                                save.FilterIndex = 1;
                                save.CheckFileExists = false;
                                if (save.ShowDialog() == DialogResult.OK)
                                {
                                    var newfile = save.FileName;
                                    try
                                    {
                                        using (_ = File.Create(newfile))
                                        facilityConfig = newfile;
                                        File.Delete(newfile);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message);
                                    }
                                    if (facilityConfig != null)
                                        Start(false, facilityConfig);
                                }
                            }
                        }
                        else
                            Environment.Exit(0);
                    }
                }
            }
            RadarWindow radarWindow;
            if (File.Exists(facilityConfig))
            {
                radarWindow = TryLoad(facilityConfig);
            }
            else
            {
                radarWindow = new RadarWindow();
                if (!screensaver)
                {
                    //MessageBox.Show("No config file found. Starting a new config.");
                    PropertyForm propertyForm = new PropertyForm(radarWindow);
                    propertyForm.ShowDialog();
                    radarWindow.SaveSettings(facilityConfig);
                }
            }
            radarWindow.Run(screensaver, facilityConfig);
        }
        [STAThread]
        static void Main(string[] args)
        {
            string gitVersion = string.Empty;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DGScope.version.txt"))
            //Don't get mad at me... This is the only way I could get it to stop throwing a "not found" error on the version file... Even though the version file exists
            gitVersion = stream == null ? "No git version description" : new StreamReader(stream).ReadToEnd();
            /*
            //Here's your old code in case you decide to delete mine above
            using (StreamReader reader = new StreamReader(stream))
            {
                gitVersion = reader.ReadToEnd();
            }
            */
            Console.WriteLine(gitVersion);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            LoadReceiverPlugins();
            bool screensaver = false;
            bool inhibit = false;
            string facilityConfig = null;
            if (args.Length > 0)
            {
                foreach (var argument in args)
                {
                    String arg = argument.ToLower();
                    //Selecting which facility config file to use:
                    //Either take a filepath as a parameter with the file argument, or accept it as a sole argument.  If neither is a valid file path, leave as null to be handled upon Start()

                    //Argument format: "--f=PATH" or "--file=PATH" (quotes need not be included if running via the command line, but must be included if passed as arguments via Visual Studio's Debug Config)
                    if (arg.StartsWith("--f") || arg.StartsWith("--file"))
                    {
                        string paramFileName = arg.Split('=')[1].Trim();
                        if (File.Exists(paramFileName))
                            facilityConfig = paramFileName;
                    }
                    //Allow for drag 'n drop or "open with" functionality
                    else if (File.Exists(arg))
                    {
                        facilityConfig = arg;
                    }

                    //Screensaver commands:

                    //Start in config mode
                    if (arg.StartsWith("--c") || arg.StartsWith("--config") || arg.StartsWith("/c"))
                    {
                        if(facilityConfig == null)
                        {
                            facilityConfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DGScope.xml");
                        }
                        RadarWindow radarWindow;
                        if (File.Exists(facilityConfig))
                        {
                            radarWindow = TryLoad(facilityConfig);
                        }
                        else
                        {
                            radarWindow = new RadarWindow();
                        }
                        PropertyForm propertyForm = new PropertyForm(radarWindow);
                        propertyForm.ShowDialog();
                        radarWindow.SaveSettings(facilityConfig);
                        inhibit = true;
                    }
                    //Start screensaver in normal mode
                    else if (arg.StartsWith("--s") || arg.StartsWith("--screensaver") || arg.StartsWith("/c"))
                    {
                        screensaver = true;
                    }
                    //Start screensaver in child mode
                    else if (arg.StartsWith("--p") || arg.StartsWith("--screensaver_child") || arg.StartsWith("/p"))
                    {
                        inhibit = true;
                    }
                    
                }
                
                if (!inhibit)
                {
                    Start(screensaver, facilityConfig);
                }
            }
            else
            {
                Start(screensaver,facilityConfig);
            }
        }

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        static RadarWindow TryLoad(string facilityConfig)
        {
            try
            {
                return XmlSerializer<RadarWindow>.DeserializeFromFile(facilityConfig);
            }
            catch (Exception ex)
            {
                RadarWindow radarWindow = new RadarWindow();
                Console.WriteLine(ex.StackTrace);
                var mboxresult = MessageBox.Show(facilityConfig + "\n" + ex.Message, "Error reading facility config", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
                return null;
            }
        }

        static void LoadReceiverPlugins()
        {
            string path = Application.StartupPath;
            string[] pluginFiles = Directory.GetFiles(path, "DGScope.*.dll");
            var ipi = (from file in pluginFiles let asm = Assembly.LoadFile(file)
                      from plugintype in asm.GetExportedTypes()
                      where typeof(Receiver).IsAssignableFrom(plugintype)
                      select (Receiver)Activator.CreateInstance(plugintype)).ToArray();
        }
        
    }
    
    
    

    
    
    
}
