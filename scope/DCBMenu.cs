using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using DGScope.MapImporter.CRC;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.ComponentModel;
using OpenTK.Input;

namespace DGScope
{
    internal class DCBMenu : DCBMenuItem
    {
        bool laidoutvertical;
        bool laidouthorizontal;
        Font font;
        protected List<DCBMenuItem> Buttons { get; set; } = new List<DCBMenuItem>();
        public void AddButton(DCBMenuItem button)
        {
            Buttons.Add(button);
            button.ParentMenu = this;
            laidouthorizontal = false;
            laidoutvertical = false;
            button.Font = font;
        }
        public void RemoveButton(DCBMenuItem button)
        {
            Buttons.Remove(button);
            button.ParentMenu = null;
            laidouthorizontal = false;
            laidoutvertical = false;
        }
        public new bool RotateIfVertical { get => true; }
        public new bool Enabled
        {
            get => base.Enabled;
            set
            {
                if (base.Enabled == value)
                {
                    return;
                }
                base.Enabled = value;
                Buttons.ForEach(x => x.Enabled = value);
            }
        }
        public override Font Font
        {
            get => font;
            set
            {
                Buttons.ForEach(x => x.Font = value);
                font = value;
            }
        }
        public override void Draw(Point location, bool vertical, int brightness)
        {
            LayoutButtons(vertical);
            GL.PushMatrix();
            GL.Translate(Left, Top, 0);
            var newloc = new Point(location.X + Left, location.Y + Top);
            DrawnBounds = new Rectangle(newloc.X, newloc.Y, Width, Height);
            for (int i = Buttons.Count - 1; i >= 0; i--)
            {
                var button = Buttons[i];
                button.Draw(newloc, vertical, brightness);
            }
            GL.PopMatrix();
        }
        protected void OnClick(Point clickedPoint)
        {
            var clicked = Buttons.Where(x => x.DrawnBounds.Contains(clickedPoint)).ToList();
            if (clicked.Count == 0)
                return;
            //else
            //    clicked.ForEach(x => x.OnClick(diff));
        }
        public void LayoutButtons(bool vertical)
        {
            if (laidoutvertical == vertical && laidouthorizontal != vertical)
                return;
            int top = 0;
            int left = 0;
            foreach (var button in Buttons)
            {
                var rotate = button.RotateIfVertical && vertical;
                int width = rotate ? button.Height : button.Width;
                int height = rotate ? button.Width : button.Height;
                button.Top = top;
                button.Left = left;
                top += height;
                if (!vertical && button.Bottom >= Height)
                {
                    left = button.Right;
                    Width = button.Right;
                    top = 0;
                }
                else if (button.Bottom >= Height)
                {
                    Height = button.Bottom;
                }
            }

            laidoutvertical = vertical;
            laidouthorizontal = !vertical;
        }
        public override void MouseMove(Point position)
        {
            var diff = new Point(position.X - DrawnBounds.X, position.Y - DrawnBounds.Y);
            Buttons.ForEach(x=> x.MouseMove(position));
        }
        public override void MouseDown()
        {
            Buttons.ForEach(x => x.MouseDown());
        }
        public override void MouseUp()
        {
            Buttons.ForEach(x => x.MouseUp());
        }
    }
    internal abstract class DCBMenuItem
    {
        public int Top 
        {
            get => Location.Y;
            set
            {
                Location = new Point(Location.X, value);
            }
        }
        public int Left
        {
            get => Location.X;
            set
            {
                Location = new Point(value, Location.Y);
            }
        }
        public int Bottom { get => Bounds.Bottom; }
        public int Right { get => Bounds.Right; }
        public int Width
        {
            get => Size.Width;
            set
            {
                Size = new Size(value, Size.Height);
            }
        }
        public int Height
        {
            get => Size.Height;
            set
            {
                Size = new Size(Size.Width, value);
            }
        }
        public Point Location { get; set; }
        public Rectangle Bounds 
        { 
            get
            {
                return new Rectangle(Location, Size);
            }
        }
        public Size Size { get; set;}
        public bool Enabled { get; set; } = true;
        public DCBMenu ParentMenu { get; set; }
        public bool RotateIfVertical { get; set; }
        public abstract Font Font { get; set; }
        public Rectangle DrawnBounds { get; set; }
        public abstract void Draw(Point location, bool vertical, int brightness);
        public abstract void MouseMove(Point position);
        public virtual void MouseDown() { }
        public virtual void MouseUp() { }

    }
}
