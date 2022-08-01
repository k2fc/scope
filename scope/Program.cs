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
            if (screensaver)
                facilityConfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DGScope.xml");
            else if (facilityConfig == null)
            {
                using (OpenFileDialog open = new OpenFileDialog())
                {
                    open.Filter = "Facility Config File (*.xml)|*.xml|All files (*.*)|*.*";
                    open.FilterIndex = 1;
                    open.CheckFileExists = false;
                    if (open.ShowDialog() == DialogResult.OK)
                    {
                        facilityConfig = open.FileName;
                    }
                    else
                    {
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
                    MessageBox.Show("No config file found. Starting a new config.");
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
            gitVersion = stream == null ? "1.0.0" : new StreamReader(stream).ReadToEnd();
            /*
            //Here's your old code in case you decide to delete mine above
            using (StreamReader reader = new StreamReader(stream))
            {
                gitVersion = reader.ReadToEnd();
            }
            Console.WriteLine(gitVersion);
            */
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            LoadReceiverPlugins();
            bool screensaver = false;
            bool inhibit = false;
            string facilityConfig = null;
            if (args.Length > 0)
            {
                foreach (var arg in args)
                {
                    //Screensaver commands:
                    
                    //Start screensaver in foreground mode
                    if (arg.Contains("--c") || arg.Contains("--screensaver_foreground") || arg.Contains("/C"))
                    {
                        facilityConfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DGScope.xml");
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
                    else if (arg.Contains("--s") || arg.Contains("--screensaver") || arg.Contains("/S"))
                    {
                        screensaver = true;
                    }
                    //Start screensaver in child mode
                    else if (arg.Contains("--p") || arg.Contains("--screensaver_child") || arg.Contains("/P"))
                    {
                        inhibit = true;
                    }
                    
                    //Selecting which facility config file to use:
                    
                    //Argument format: "--f=PATH" or "--file=PATH" (quotes need not be included if running via the command line, but must be included if passed as arguments via Visual Studio's Debug Config)
                    if (arg.Contains("--f") || arg.Contains("--file"))
                    {
                        facilityConfig = arg.Split('=')[1].Trim();
                    }
                    else
                    {
                        facilityConfig = arg;
                    }
                    
                }
                
                if (!inhibit)
                {
                    Start(screensaver, facilityConfig);
                }
            }
            else
            {
                Start(false);
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
                var mboxresult = MessageBox.Show("Error reading settings file "+ facilityConfig + "\n" + 
                    ex.Message + "\nPress Abort to exit, Retry to try again, or Ignore to destroy " +
                    "the file and start a new config.", "Error reading settings file", 
                    MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                if (mboxresult == DialogResult.Abort)
                    Environment.Exit(1);
                else if (mboxresult == DialogResult.Retry)
                    return TryLoad(facilityConfig);
                else
                {
                    radarWindow = new RadarWindow();
                    PropertyForm propertyForm = new PropertyForm(radarWindow);
                    propertyForm.ShowDialog();
                    return radarWindow;
                }
            }
            return new RadarWindow();
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
