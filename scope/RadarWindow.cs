using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Input;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Drawing.Design;
using DGScope.Receivers;

namespace DGScope
{
    public class RadarWindow
    {
        [XmlIgnore]
        [DisplayName("Background Color"),Description("Radar Background Color"), Category("Colors")] 
        public Color BackColor { get; set; } = Color.Black;
        [XmlIgnore]
        [DisplayName("Range Ring Color"), Category("Colors")]
        public Color RangeRingColor { get; set; } = Color.FromArgb(0,25,0);
        [XmlIgnore]
        [DisplayName("Video Map Line Color"), Category("Colors")]
        public Color VideoMapLineColor { get; set; } = Color.Brown;
        [XmlIgnore]
        [DisplayName("Return Color"), Description("Primary Radar return color"), Category("Colors")]
        public Color ReturnColor { get; set; } = Color.Lime;
        [XmlIgnore]
        [DisplayName("Data Block Color"), Description("Color of aircraft data blocks"), Category("Colors")]
        public Color DataBlockColor { get; set; } = Color.Lime;
        [XmlIgnore]
        [DisplayName("Connecting Line Color"), Description("Color of the lines which connect the primary returns to their associated data blocks"), Category("Colors")]
        public Color ConnectingLineColor { get; set; } = Color.Lime;

        [XmlElement("BackColor")]
        [Browsable(false)]
        public int BackColorAsArgb
        {
            get { return BackColor.ToArgb(); }
            set { BackColor = Color.FromArgb(value); }
        }
        [XmlElement("RangeRingColor")]
        [Browsable(false)]
        public int RangeRingColorAsArgb
        {
            get { return RangeRingColor.ToArgb(); }
            set { RangeRingColor = Color.FromArgb(value); }
        }
        [XmlElement("VideoMapLineColor")]
        [Browsable(false)]
        public int VideoMapLineColorAsArgb
        {
            get { return VideoMapLineColor.ToArgb(); }
            set { VideoMapLineColor = Color.FromArgb(value); }
        }
        [XmlElement("ReturnColor")]
        [Browsable(false)]
        public int ReturnColorAsArgb
        {
            get { return ReturnColor.ToArgb(); }
            set { ReturnColor = Color.FromArgb(value); }
        }
        [XmlElement("DataBlockColor")]
        [Browsable(false)]
        public int DataBlockColorAsArgb
        {
            get { return DataBlockColor.ToArgb(); }
            set { DataBlockColor = Color.FromArgb(value); }
        }

        [DisplayName("Fade Reps"), Description("The number of revolutions of the radar the target is faded out over.  A higher number is a slower fade."), Category("Display Properties")]
        public int FadeReps { get; set; } = 6;
        [DisplayName("Lost Target Seconds"), Description("The number of seconds before a target's data block is removed from the scope."), Category("Display Properties")]
        public int LostTargetSeconds { get; set; } = 10;
        [DisplayName("Screen Rotation"), Description("The number of degrees to rotate the image"), Category("Display Properties")]
        public double ScreenRotation { get; set; } = 0;
        [DisplayName("Hide Data Tags"), Category("Display Properties")]
        public bool HideDataTags { get; set; } = false;
        [DisplayName("Window State"), Category("Display Properties")]
        public WindowState WindowState
        {
            get
            {
                return window.WindowState;
            }
            set
            {
                window.WindowState = value;
            }
        }

        [Browsable(false)]
        public Size WindowSize
        {
            get
            {
                return window.Size;
            }
            set
            {
                window.Size = value;
            }
        }

        [Browsable(false)]
        public Point WindowLocation
        {
            get
            {
                return window.Location;
            }
            set
            {
                window.Location = value;
                if (value == new Point(-32000, -32000))
                {
                    window.Location = new Point(0, 0);
                }
            }
        }
        [DisplayName("Video Maps")]
        [Editor(typeof(VideoMapCollectionEditor), typeof(UITypeEditor))]
        public List<VideoMap> VideoMaps { get; set; } = new List<VideoMap>();
        float scale => (float)(radar.Range / Math.Sqrt(2));
        float xPixelScale => 2f / window.ClientRectangle.Width;
        float yPixelScale => 2f / window.ClientRectangle.Height;
        float aspect_ratio => (float)window.ClientRectangle.Width / (float)window.ClientRectangle.Height;
        float oldar;
        [DisplayName("Range Ring Interval"), Category("Display Properties")]
        public int RangeRingInterval { get; set; } = 5;
        private Radar radar = new Radar();
        [Browsable(false)]
        public bool isRadarRunning  => radar.isRunning;
        [Editor(typeof(ReceiverCollectionEditor), typeof(UITypeEditor))]
        [DisplayName("Receivers"), Description("The collection of data receivers for the Radar scope")]
        public ListOfIReceiver Receivers
        {
            get => radar.Receivers; 
            set
            {
                radar.Receivers = value;
            }
        }
        [DisplayName("Screen Center Point"), Category("Radar Properties")]
        public GeoPoint ScreenCenterPoint
        {
            get => radar.Location;
            set
            {
                radar.Location = value;
            }
        }
        [DisplayName("Radar Range"), Category("Radar Properties"), Description("About two more minutes, chief!")]
        public double Range
        {
            get => radar.Range;
            set
            {
                radar.Range = value;
            }
        }
        [DisplayName("Rotation Period"), Category("Radar Properties"), Description("The number of seconds the radar takes to make one revolution")]
        public double RotationPeriod
        {
            get => radar.RotationPeriod;
            set
            {
                radar.RotationPeriod = value;
            }
        }

        [DisplayName("Max Altitude"), Category("Radar Properties"), Description("The maximum altitude of displayed aircraft.")]
        public int MaxAltitude
        {
            get => radar.MaxAltitude;
            set
            {
                radar.MaxAltitude = value;
            }
        }

        [DisplayName("Vertical Sync"),Description("Limit FPS to refresh rate of monitor"), Category("Display Properties")]
        public VSyncMode VSync
        { 
            get
            {
                return window.VSync;
            }
            set
            {
                window.VSync = value;
            }
        }

        List<PrimaryReturn> PrimaryReturns = new List<PrimaryReturn>();

        private GameWindow window;
        private bool isScreenSaver = false;
        
        public RadarWindow(GameWindow Window)
        {
            window = Window;
            window.Load += Window_Load;
            window.Closing += Window_Closing;
            window.RenderFrame += Window_RenderFrame;
            window.UpdateFrame += Window_UpdateFrame;
            window.Resize += Window_Resize;
            window.WindowStateChanged += Window_WindowStateChanged;
            window.KeyDown += Window_KeyDown;
            window.MouseWheel += Window_MouseWheel;
            window.MouseMove += Window_MouseMove;
            aircraftGCTimer.Start();
            aircraftGCTimer.Elapsed += AircraftGCTimer_Elapsed;
            GL.ClearColor(BackColor);
            string settingsstring = XmlSerializer<RadarWindow>.Serialize(this);
            using (MD5 md5 = MD5.Create())
            {
                md5.Initialize();
                md5.ComputeHash(Encoding.UTF8.GetBytes(settingsstring));
                settingshash = md5.Hash;
            }
        }
        public RadarWindow()
        {
            window = new GameWindow(1000, 1000);
            window.Load += Window_Load;
            window.Closing += Window_Closing;
            window.RenderFrame += Window_RenderFrame;
            window.UpdateFrame += Window_UpdateFrame;
            window.Resize += Window_Resize;
            window.WindowStateChanged += Window_WindowStateChanged;
            window.KeyDown += Window_KeyDown;
            window.MouseWheel += Window_MouseWheel;
            window.MouseMove += Window_MouseMove;
            aircraftGCTimer.Start();
            aircraftGCTimer.Elapsed += AircraftGCTimer_Elapsed;
            GL.ClearColor(BackColor);
            string settingsstring = XmlSerializer<RadarWindow>.Serialize(this);
            using (MD5 md5 = MD5.Create())
            {
                md5.Initialize();
                md5.ComputeHash(Encoding.UTF8.GetBytes(settingsstring));
                settingshash = md5.Hash;
            }
        }
        byte[] settingshash;
        public void Run(bool isScreenSaver)
        {
            this.isScreenSaver = isScreenSaver;
            window.Run();
        }

        private void AircraftGCTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            List<Aircraft> delplane;
            lock(radar.Aircraft)
                delplane = radar.Aircraft.Where(x => x.LastMessageTime < DateTime.UtcNow.AddMinutes(-1)).ToList();
            foreach (var plane in delplane)
            {
                GL.DeleteTexture(plane.DataBlock.TextureID);
                plane.DataBlock.TextureID = 0;
                //plane.Dispose();
                lock(radar.Aircraft)
                    radar.Aircraft.Remove(plane);
                Debug.WriteLine("Deleted airplane " + plane.ModeSCode.ToString("X"));
            }
        }

        private void Window_WindowStateChanged(object sender, EventArgs e)
        {
            window.CursorVisible = window.WindowState != WindowState.Fullscreen;
        }

        System.Drawing.Point mouseLocation;
        
        private void Window_MouseMove(object sender, MouseMoveEventArgs e)
        {
            if (mouseLocation.X == 0 && mouseLocation.Y == 0)
                mouseLocation = e.Position;
            
            double move = Math.Sqrt(((mouseLocation.X - e.Position.X) * (mouseLocation.X - e.Position.X)) + ((mouseLocation.Y - e.Position.Y) * (mouseLocation.Y - e.Position.Y)));
            
            if (move > 10 && isScreenSaver)
                Environment.Exit(0);
            mouseLocation = e.Position;
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var oldscale = scale;
            if (e.Delta > 0 && radar.Range > 5)
                radar.Range -= 5;
            else if (e.Delta < 0)
                radar.Range += 5;
            RescaleTargets((oldscale / scale), (oldar / aspect_ratio));
        }

        private void Window_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            var oldscale = scale;
            switch (e.Key)
            {
                case Key.D:
                    HideDataTags = !HideDataTags;
                    break;
                case Key.F11:
                    if (isScreenSaver)
                        Environment.Exit(0);
                    else
                        window.WindowState = window.WindowState == WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen;
                    break;
                case Key.KeypadPlus:
                    if (radar.Range > 5)
                        radar.Range -= 5;
                    RescaleTargets((oldscale / scale), (oldar / aspect_ratio));
                    break;
                case Key.KeypadMinus:
                    radar.Range += 5;
                    RescaleTargets((oldscale / scale), (oldar / aspect_ratio));
                    break;
                case Key.R:
                    if (e.Modifiers == KeyModifiers.Shift && RangeRingInterval < radar.Range)
                        RangeRingInterval += 5;
                    else if (RangeRingInterval > 0)
                        RangeRingInterval -= 5;
                    else
                        RangeRingInterval = (int)radar.Range;
                    break;
                case Key.C:
                    dataBlocks.Clear();
                    radar.Aircraft.Clear();
                    break;
                case Key.P:
                    PropertyForm properties = new PropertyForm(this);
                    properties.Show();
                    break;
                case Key.Escape:
                    if (window.WindowState == WindowState.Fullscreen)
                        window.Close();
                    break;
                case Key.ShiftLeft:
                    break;
                case Key.ShiftRight:
                    break;
                case Key.M:
                    VideoMapSelector selector = new VideoMapSelector(VideoMaps);
                    selector.Show();
                    selector.BringToFront();
                    selector.Focus();
                    break;
                default:
                    if (isScreenSaver)
                        Environment.Exit(0);
                    break;
            }
        }


        private void Window_Resize(object sender, EventArgs e)
        {
            var oldscale = scale;
            GL.Viewport(0, 0, window.Width, window.Height);
            RescaleTargets((oldscale / scale), (oldar/aspect_ratio));
            oldar = aspect_ratio;
        }

        private void Window_UpdateFrame(object sender, FrameEventArgs e)
        {
        }

        private void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            GL.ClearColor(BackColor);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.DstAlpha);
            DrawRangeRings();
            DrawReceiverLocations();
            DrawLines();
            GenerateTargets();
            DrawTargets();
            GL.Flush();
            window.SwapBuffers();
            window.Title = $"(Vsync: {window.VSync}) FPS: {1f / e.Time:0} Aircraft: {radar.Aircraft.Count}";
        }

        private void Window_Load(object sender, EventArgs e)
        {
            if (isScreenSaver)
            {
                window.WindowState = WindowState.Fullscreen;
                window.CursorVisible = false;
            }
            radar.Start();
            oldar = aspect_ratio;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(this.GetType());
            byte[] newhash;
            if (isScreenSaver)
                return;
            using (MD5 md5 = MD5.Create())
            {
                md5.Initialize();
                md5.ComputeHash(Encoding.UTF8.GetBytes(XmlSerializer<RadarWindow>.Serialize(this)));
                newhash = md5.Hash;
            }
            bool changed = false;
            for (int i = 0; i < settingshash.Length; i++)
            {
                if (newhash[i] != settingshash[i])
                {
                    changed = true;
                    break;
                }
            }
            if (changed && !isScreenSaver)
            {
                SaveSettings(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DGScope.xml"));
            }
        }

        public void SaveSettings(string path)
        {
            XmlSerializer<RadarWindow>.SerializeToFile(this, path);
        }
        
        private void DrawRangeRings()
        {
            for (int i = RangeRingInterval; i <= radar.Range && RangeRingInterval > 0; i += RangeRingInterval)
            {
                DrawCircle(0, 0, (float)(i / scale), aspect_ratio, 1000, RangeRingColor);
            }
        }

        private void DrawReceiverLocations()
        {
            foreach (IReceiver receiver in radar.Receivers)
            {
                double bearing = radar.Location.BearingTo(receiver.Location) - ScreenRotation;
                double distance = radar.Location.DistanceTo(receiver.Location);
                float x = (float)(Math.Sin(bearing * (Math.PI / 180)) * (distance / scale));
                float y = (float)(Math.Cos(bearing * (Math.PI / 180)) * (distance / scale) * aspect_ratio);
                DrawCircle(x, y, .0025f, aspect_ratio, 100, VideoMapLineColor, true);
            }
        }

        private void DrawCircle (float cx, float cy, float r, double aspect_ratio, int num_segments, Color color, bool fill = false)
        {
            GL.Begin(fill ? PrimitiveType.Polygon : PrimitiveType.LineLoop);
            GL.Color4(color);
            for (int ii = 0; ii < num_segments; ii++)
            {
                float theta = 2.0f * (float)Math.PI * ii / num_segments;
                float x = r * (float)Math.Cos(theta);
                float y = r * (float)Math.Sin(theta) * (float)aspect_ratio;

                GL.Vertex2(x + cx, y + cy);
            }
            GL.End();
        }

        private void DrawLines()
        {
            List<Line> lines = new List<Line>();
            foreach (var map in VideoMaps)
            {
                if (map.Visible)
                    lines.AddRange(map.Lines);
            }
            DrawLines(lines);
        }

        private void DrawLines (List<Line> lines)
        {
            double scale = radar.Range / Math.Sqrt(2); 
            double yscale = (double)window.Width / (double)window.Height;
            foreach (Line line in lines)
            {
                double bearing1 = radar.Location.BearingTo(line.End1) - ScreenRotation;
                double distance1 = radar.Location.DistanceTo(line.End1);
                float x1 = (float)(Math.Sin(bearing1 * (Math.PI / 180)) * (distance1 / scale));
                float y1 = (float)(Math.Cos(bearing1 * (Math.PI / 180)) * (distance1 / scale) * yscale);

                double bearing2 = radar.Location.BearingTo(line.End2) - ScreenRotation;
                double distance2 = radar.Location.DistanceTo(line.End2);
                float x2 = (float)(Math.Sin(bearing2 * (Math.PI / 180)) * (distance2 / scale));
                float y2 = (float)(Math.Cos(bearing2 * (Math.PI / 180)) * (distance2 / scale) * yscale);

                DrawLine(x1, y1, x2, y2, VideoMapLineColor);
            }
        }

        private void DrawLine (float x1, float y1, float x2, float y2, Color color, float width = 1)
        {
            GL.Begin(PrimitiveType.Lines);
            GL.LineWidth(width);
            GL.Color4(color);
            GL.Vertex2(x1, y1);
            GL.Vertex2(x2, y2);
            GL.End();
        }
        private List<TransparentLabel> dataBlocks = new List<TransparentLabel>();
        private void GenerateTargets()
        {
            List<Aircraft> targets = radar.Scan();
            foreach (Aircraft aircraft in targets)
            {
                double bearing = radar.Location.BearingTo(aircraft.Location) - ScreenRotation; 
                double distance = radar.Location.DistanceTo(aircraft.Location);
                float x = (float)(Math.Sin(bearing * (Math.PI / 180)) * (distance / scale));
                float y = (float)(Math.Cos(bearing * (Math.PI / 180)) * (distance / scale) * aspect_ratio);
                var location = new PointF(x, y);
                if (aircraft.LastPositionTime >= DateTime.UtcNow.AddSeconds(-radar.RotationPeriod) && aircraft.Altitude <= radar.MaxAltitude)
                {
                    PrimaryReturn newreturn = new PrimaryReturn();
                    aircraft.TargetReturn = newreturn;
                    newreturn.ParentAircraft = aircraft;
                    newreturn.FadeTime = radar.RotationPeriod * FadeReps;
                    aircraft.RedrawTarget(location);
                    
                    
                    //newreturn.LocationF = location;
                    //aircraft.LocationF = location;
                    
                    newreturn.Intensity = 1;
                    newreturn.ForeColor = ReturnColor;
                    aircraft.DataBlock.ForeColor = DataBlockColor;
                    PrimaryReturns.Add(newreturn);
                    Bitmap text_bmp = aircraft.DataBlock.TextBitmap();
                    var realWidth = (float)text_bmp.Width * xPixelScale;
                    var realHeight = (float)text_bmp.Height * yPixelScale;
                    aircraft.DataBlock.SizeF = new SizeF(realWidth, realHeight);
                    aircraft.DataBlock.LocationF = ShiftedLabelLocation(aircraft.LocationF, aircraft.DataBlock.SizeF.Width /2, Math.PI/4, aircraft.DataBlock.SizeF);
                    
                    aircraft.DataBlock.ParentAircraft = aircraft;
                    Deconflict(aircraft.DataBlock);
                    if (!dataBlocks.Contains(aircraft.DataBlock))
                        dataBlocks.Add(aircraft.DataBlock);
                    
                }
                
                else if (aircraft.LastPositionTime < DateTime.UtcNow.AddSeconds(-LostTargetSeconds))
                {
                    dataBlocks.Remove(aircraft.DataBlock);
                }
                if (aircraft.Altitude > radar.MaxAltitude)
                {
                    dataBlocks.Remove(aircraft.DataBlock);
                }
            }
            foreach (PrimaryReturn target in PrimaryReturns.ToList())
            {
                if (target.Intensity < .001)
                    PrimaryReturns.Remove(target);
            }
            lock (radar.Aircraft)
            {
                foreach (Aircraft plane in radar.Aircraft)
                {
                    
                }
            }
            
        }

        System.Timers.Timer aircraftGCTimer = new System.Timers.Timer(60000);

        private void RescaleTargets(float scalechange, float ar_change)
        {
            
            foreach (PrimaryReturn target in PrimaryReturns.ToList())
            {
                PointF newLocation = new PointF(target.LocationF.X * scalechange, (target.LocationF.Y * scalechange) / ar_change);
                target.LocationF = newLocation;
            }
            foreach (TransparentLabel block in dataBlocks.ToList())
            {
                PointF newLocation = new PointF(block.LocationF.X * scalechange, (block.LocationF.Y * scalechange) / ar_change);
                block.LocationF = newLocation;
            }
            lock (radar.Aircraft)
            {
                foreach (Aircraft plane in radar.Aircraft)
                {
                    PointF newLocation = new PointF(plane.LocationF.X * scalechange, (plane.LocationF.Y * scalechange) / ar_change);
                    plane.LocationF = newLocation;
                }
            }
            
        }

        private void DrawTarget(PrimaryReturn target)
        {
            float targetHeight = 15f * xPixelScale;// (window.ClientRectangle.Height/2);
            float targetWidth = 5f * xPixelScale;// (window.ClientRectangle.Width/2);
            float atan = (float)Math.Atan(targetHeight / targetWidth);
            float targetHypotenuse = (float)(Math.Sqrt((targetHeight*targetHeight) + (targetWidth * targetWidth))/2);
            float x1 = (float)(Math.Sin(atan) * targetHypotenuse);
            float y1 = (float)(Math.Cos(atan) * targetHypotenuse);
            float circleradius = 4f * xPixelScale;
            //target.SizeF = new SizeF(2 * circleradius, 2 * circleradius * aspect_ratio);
            //DrawCircle(target.LocationF.X, target.LocationF.Y, circleradius, aspect_ratio, 25, target.ForeColor, true);
            
            target.SizeF = new SizeF(targetHypotenuse * 2, targetHypotenuse * 2 * aspect_ratio);
            
            GL.LoadIdentity();
            GL.PushMatrix();

            float angle = (float)(-(target.Angle + 360) % 360) + (float)ScreenRotation;

            GL.Translate(target.LocationF.X, target.LocationF.Y, 0.0f);
            GL.Rotate(angle, 0.0f, 0.0f, 1.0f);
            GL.Begin(PrimitiveType.Polygon);
            GL.Scale(aspect_ratio, 0.0f, 0.0f);
            GL.Color4(target.ForeColor);
            GL.Vertex2(x1, y1);
            GL.Vertex2(-x1, y1);
            GL.Vertex2(-x1, -y1);
            GL.Vertex2(x1, -y1);
            

            GL.End();
            GL.Translate(-target.LocationF.X, -target.LocationF.Y , 0.0f);
            

            GL.PopMatrix();
            
            /*
            GL.Begin(PrimitiveType.Lines);
            GL.Color4(Color.Red);
            GL.Vertex2(target.BoundsF.Left, target.BoundsF.Top);
            GL.Vertex2(target.BoundsF.Right, target.BoundsF.Top);
            GL.Vertex2(target.BoundsF.Right, target.BoundsF.Top);
            GL.Vertex2(target.BoundsF.Right, target.BoundsF.Bottom);
            GL.Vertex2(target.BoundsF.Right, target.BoundsF.Bottom);
            GL.Vertex2(target.BoundsF.Left, target.BoundsF.Bottom);
            GL.Vertex2(target.BoundsF.Left, target.BoundsF.Bottom);
            GL.Vertex2(target.BoundsF.Left, target.BoundsF.Top);
            GL.End();
            */
        }

        private void DrawTargets()
        {
            foreach (var target in PrimaryReturns.OrderBy(x => x.Intensity).ToList())
            {
                DrawTarget(target);
                if(!HideDataTags)
                    Deconflict(target);
            }
            foreach (var block in dataBlocks.ToList().OrderBy(x=>x.ParentAircraft.ModeSCode))
            {
                //Deconflict(block);
                if (!HideDataTags)
                    DrawLabel(block);
            }
        }

        private void DrawLabel(TransparentLabel Label)
        {
            if (!radar.Aircraft.Contains(Label.ParentAircraft))
                return;
            if (Label.TextureID == 0)
                Label.TextureID = GL.GenTexture();
            var text_texture = Label.TextureID;
            if (Label.Redraw && Label.Text.Trim() != "")
            {
                Bitmap text_bmp = Label.TextBitmap();
                var realWidth = (float)text_bmp.Width * xPixelScale;
                var realHeight = (float)text_bmp.Height * yPixelScale;
                Label.SizeF = new SizeF(realWidth, realHeight);
                GL.BindTexture(TextureTarget.Texture2D, text_texture);
                BitmapData data = text_bmp.LockBits(new Rectangle(0, 0, text_bmp.Width, text_bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, text_bmp.Width, text_bmp.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero); // just allocate me
                text_bmp.UnlockBits(data);
                Label.Redraw = false;
                //text_bmp.Save($"{text_texture}.bmp");
            }
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, text_texture);
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(Label.ForeColor);
            
            //Deconflict(Label);
            var Location = Label.LocationF;
            GL.TexCoord2(0,0);
            GL.Vertex2(Location.X, Label.SizeF.Height + Location.Y);
            GL.TexCoord2(1, 0); 
            GL.Vertex2(Label.SizeF.Width + Location.X, Label.SizeF.Height + Location.Y);
            GL.TexCoord2(1, 1); 
            GL.Vertex2(Label.SizeF.Width + Location.X, Location.Y);
            GL.TexCoord2(0, 1); 
            GL.Vertex2(Location.X, Location.Y);
            GL.End();
            GL.Disable(EnableCap.Texture2D);

            
            GL.Begin(PrimitiveType.Lines);
            GL.Color4(ConnectingLineColor);
            
            ConnectingLineF line = new ConnectingLineF();
            
            line.End = ConnectingLinePoint(Label.ParentAircraft.LocationF, Label.BoundsF);
            line.Start = ConnectingLinePoint(line.End, Label.ParentAircraft.TargetReturn.BoundsF);
            GL.Vertex2(line.Start.X, line.Start.Y);
            GL.Vertex2(line.End.X, line.End.Y);
            GL.End();
            
            /*
            GL.Begin(PrimitiveType.Lines);
            GL.Color4(Color.Red);
            GL.Vertex2(Label.BoundsF.Left, Label.BoundsF.Top);
            GL.Vertex2(Label.BoundsF.Right, Label.BoundsF.Top);
            GL.Vertex2(Label.BoundsF.Right, Label.BoundsF.Top);
            GL.Vertex2(Label.BoundsF.Right, Label.BoundsF.Bottom);
            GL.Vertex2(Label.BoundsF.Right, Label.BoundsF.Bottom);
            GL.Vertex2(Label.BoundsF.Left, Label.BoundsF.Bottom);
            GL.Vertex2(Label.BoundsF.Left, Label.BoundsF.Bottom);
            GL.Vertex2(Label.BoundsF.Left, Label.BoundsF.Top);
            GL.End();

            /*
            ConnectingLineF connectingLineF = new ConnectingLineF();
            connectingLineF.Start = Label.ParentAircraft.LocationF;
            connectingLineF.End = ConnectingLinePoint(Label.ParentAircraft.LocationF, Label.BoundsF); 
            GL.Begin(PrimitiveType.Lines);
            GL.Color4(Color.Red);
            GL.Vertex2(connectingLineF.BoundsF.Left, connectingLineF.BoundsF.Top);
            GL.Vertex2(connectingLineF.BoundsF.Right, connectingLineF.BoundsF.Top);
            //GL.Vertex2(connectingLineF.BoundsF.Right, connectingLineF.BoundsF.Top);
            //GL.Vertex2(connectingLineF.BoundsF.Right, connectingLineF.BoundsF.Bottom);
            //GL.Vertex2(connectingLineF.BoundsF.Right, connectingLineF.BoundsF.Bottom);
            //GL.Vertex2(connectingLineF.BoundsF.Left, connectingLineF.BoundsF.Bottom);
            GL.Vertex2(connectingLineF.BoundsF.Left, connectingLineF.BoundsF.Bottom);
            GL.Vertex2(connectingLineF.BoundsF.Left, connectingLineF.BoundsF.Top);
            GL.End();
            */
        }
        private void Deconflict(PrimaryReturn Return)
        {
            List<TransparentLabel> blocks = new List<TransparentLabel>();
            blocks.AddRange(dataBlocks.ToList());
            foreach (TransparentLabel block in blocks)
            {
                if (block.BoundsF.IntersectsWith(Return.BoundsF))
                    Deconflict(block);
            }
        }
        private void Deconflict(TransparentLabel Label)
        {
            if (window.WindowState == WindowState.Minimized || window.Width == 0 || window.Height == 0)
                return;
            double circlespeed = 5 * (Math.PI / 180);
            float growsize = (10/72f)*xPixelScale;// * (float)circlespeed;
            ConnectingLineF connectingLine = new ConnectingLineF();
            connectingLine.Start = ConnectingLinePoint(Label.ParentAircraft.LocationF, Label.BoundsF);
            connectingLine.End = ConnectingLinePoint(connectingLine.Start, Label.ParentAircraft.TargetReturn.BoundsF);
            connectingLine.ParentAircraft = Label.ParentAircraft;
            int loopcount = 0;
            
            int conflictcount;
            float circleSize = Label.SizeF.Width / 2;
            double angle = 0;
            List<IScreenObject> screenObjects = new List<IScreenObject>();
            screenObjects.AddRange(PrimaryReturns.ToList());
            screenObjects.AddRange(dataBlocks.ToList());
            do
            {
                conflictcount = 0;
                loopcount++;
                foreach (IScreenObject screenObject in screenObjects)
                {
                    bool crashesWithLabel = Label.BoundsF.IntersectsWith(screenObject.BoundsF) && screenObject != Label;
                    bool crashesWithLine = (connectingLine.IntersectsWith(screenObject.BoundsF) && screenObject != Label || connectingLine.IntersectsWith(screenObject.ParentAircraft.ConnectingLine)) && !screenObject.BoundsF.IntersectsWith(Label.ParentAircraft.TargetReturn.BoundsF);
                    if (crashesWithLabel || crashesWithLine)
                    {
                        conflictcount++;
                        //Debug.WriteLine("{0}'s {1} has a conflict with {2}'s {3}.", Label.ParentAircraft, crashesWithLabel ? "label" : "line", screenObject.ParentAircraft, screenObject.GetType());
                    }

                }
                //if (conflictcount > 0)
                //    Debug.WriteLine("Shifting label for {0} for the {1} time because it has {2} conflicts. Angle: {3} Radius: {4}", Label.ParentAircraft, loopcount, conflictcount, (int)(angle * (180 / Math.PI)) % 360, circleSize);
                Label.LocationF = ShiftedLabelLocation(Label.ParentAircraft.LocationF, circleSize, angle, Label.SizeF); 
                connectingLine.Start = ConnectingLinePoint(Label.ParentAircraft.LocationF, Label.BoundsF);
                connectingLine.End = ConnectingLinePoint(connectingLine.Start, Label.ParentAircraft.TargetReturn.BoundsF);
                Label.ParentAircraft.ConnectingLine = connectingLine;
                angle += circlespeed;
                //angle = 180 * (Math.PI / 180);
                circleSize += growsize;
                if (loopcount >720)
                    return;
            } while (conflictcount > 0 && conflictcount < screenObjects.Count) ;
        }

        public void Storage()
        {

        }
        public PointF ConnectingLinePoint(PointF StartPoint, RectangleF Label)
        {
            PointF EndPoint = new PointF();
            if (Label.Right < StartPoint.X)
                EndPoint.X = Label.Right;
            else if (Label.Left > StartPoint.X)
                EndPoint.X = Label.Left;
            else
                EndPoint.X = Label.Left + (Label.Width / 2);

            if (Label.Top > StartPoint.Y)
                EndPoint.Y = Label.Top;
            else if (Label.Bottom < StartPoint.Y)
                EndPoint.Y = Label.Bottom;
            else
                EndPoint.Y = Label.Bottom - (Label.Height / 2);
            return EndPoint;
        }
        public PointF ShiftedLabelLocation(PointF StartPoint, float radius, double angle, SizeF Label)
        {
            PointF EndPoint = new PointF();
            double degrees = ((angle * (180 / Math.PI)) + 360) % 360;
            
            if (degrees > 135 && degrees < 225)
            {
                EndPoint.X = StartPoint.X + ((float)Math.Cos(angle) * radius) - Label.Width; //X bound to right
                EndPoint.Y = StartPoint.Y + ((float)Math.Sin(angle) * radius) + (Label.Height / 2); //Y bound to mid
            }
            else if (degrees > 45 && degrees < 135)
            {
                EndPoint.X = StartPoint.X + ((float)Math.Cos(angle) * radius) - (Label.Width / 2);//X bound to mid
                EndPoint.Y = StartPoint.Y + (float)Math.Sin(angle) * radius; //Y bound to bottom
            }
            else if (degrees > 225 && degrees < 315)
            {
                EndPoint.X = StartPoint.X + ((float)Math.Cos(angle) * radius) - (Label.Width / 2); //X bound to mid
                EndPoint.Y = StartPoint.Y + ((float)Math.Sin(angle) * radius) - Label.Height; //Y bound to top
            }
            else
            {
                EndPoint.X = StartPoint.X + (float)Math.Cos(angle) * radius; //X bound to left
                EndPoint.Y = (StartPoint.Y + (float)Math.Sin(angle) * radius) + (Label.Height / 2); //Y bound to mid
            }
            return EndPoint;
        }
    }

    

    
    public interface IScreenObject
    {
        PointF LocationF { get;  }
        SizeF SizeF { get;  }
        RectangleF BoundsF { get; }
        Aircraft ParentAircraft { get; set; }
    }
}
