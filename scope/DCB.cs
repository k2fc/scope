using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Drawing;

namespace DGScope
{
    internal class DCB
    {
        public DCBLocation Location { get; set; } = DCBLocation.Top;
        public int Size { get; set; } = 80;
        public bool Vertical
        {
            get
            {
                return (Location == DCBLocation.Left || Location == DCBLocation.Right);
            }
        }
        
        public bool Visible { get; set; }
        public DCBMenu ActiveMenu { get; set; }
        public List<DCBMenuItem> Items { get; set; } = new List<DCBMenuItem>();
        public void Draw(int width, int height, ref Matrix4 pixelTransform)
        {
            if (!Visible) return;
            Vector4 vec = new Vector4(0, 0, 0, 1);
            vec *= pixelTransform;
            GL.PushMatrix();
            GL.MultMatrix(ref pixelTransform);
            if (Location == DCBLocation.Bottom)
            {
                GL.Translate(0, height - Size, 0);
            }
            else if (Location == DCBLocation.Right)
            {
                GL.Translate(width - Size, 0, 0);
            }
            GL.Begin(PrimitiveType.Polygon);
            GL.Color4(Color.FromArgb(0, 35, 15)) ;
            if (Vertical)
            {
                GL.Vertex2(0, 0);
                GL.Vertex2(Size, 0);
                GL.Vertex2(Size, height);
                GL.Vertex2(0, height);
            }
            else
            {
                GL.Vertex2(0, 0);
                GL.Vertex2(0, Size);
                GL.Vertex2(width, Size);
                GL.Vertex2(width, 0);
            }
            GL.End();
            if (Vertical)
            {
                ActiveMenu.Width = Size;
            }
            else
            {
                ActiveMenu.Height = Size;
            }
            ActiveMenu.Draw(Vertical);
            GL.PopMatrix();
        }
        
    }

    enum DCBLocation
    {
        Top, Bottom, Left, Right
    }
}
