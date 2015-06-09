using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D9;
using System.Runtime.InteropServices;

namespace BBitsCore.Renderers
{
    class Direct3D9 : Renderer
    {
        public Device Device { get; private set; }

        public util.VertexBuffer<Vertex, VertexBuffer> PrimitiveBuffer { get; private set; }
        public util.VertexBuffer<FontVertex, VertexBuffer> FontBuffer { get; private set; }

        protected Dictionary<Font, D3D9FontData> FontLibrary { get; private set; }
        

        public Direct3D9(Device device)
        {
            if (device == null)
                throw new ArgumentNullException("device");

            Device = device;
            FontLibrary = new Dictionary<Font, D3D9FontData>();

            PrimitiveBuffer = new D3D9VertexBuffer<Vertex>(VertexFormat.PositionRhw | VertexFormat.Diffuse);
            FontBuffer = new D3D9VertexBuffer<FontVertex>(VertexFormat.PositionRhw | VertexFormat.Diffuse | VertexFormat.Texture0);
        }

        public override void Initialize()
        {

        }

        public override void Dispose()
        {
            foreach(var pair in FontLibrary)
            {
                pair.Value.Dispose();
            }
            PrimitiveBuffer.Dispose();
            FontBuffer.Dispose();
        }

        public override void Begin(int fps)
        {
            PrimitiveBuffer.Begin(Device);
            FontBuffer.Begin(Device);

        }

        public override void End()
        {
            PrimitiveBuffer.End();
            FontBuffer.End();
        }

        public override void Present()
        {
            Device.SetStreamSource(0, PrimitiveBuffer.Buffer, 0, Marshal.SizeOf(typeof(Vertex)));
            Device.DrawPrimitives(PrimitiveType.TriangleList, 0, -1);
        }

        public override void FillRect(Rectangle rect)
        {
            Vertex[] v = new Vertex[]
            {
                new Vertex(new Vector4(rect.Left, rect.Bottom, 0.5f, 0.5f), Color),
                new Vertex(new Vector4(rect.Left, rect.Top, 0.5f, 0.5f), Color),
                new Vertex(new Vector4(rect.Right, rect.Bottom, 0.5f, 0.5f), Color),

                new Vertex(new Vector4(rect.Right, rect.Top, 0.5f, 0.5f), Color),
                new Vertex(new Vector4(rect.Right, rect.Bottom, 0.5f, 0.5f), Color),
                new Vertex(new Vector4(rect.Left, rect.Top, 0.5f, 0.5f), Color)
            };
            PrimitiveBuffer.Stream.WriteRange<Vertex>(v);
        }

        public override void AddLine(Rectangle rect)
        {

        }

        public override void LoadFont(Font font)
        {
            D3D9FontData data = null;
            if (!FontLibrary.TryGetValue(font, out data))
            {
                FontLibrary.Add(font, new D3D9FontData(this, font));
            }
        }

        public override void FreeFont(Font font)
        {            
            D3D9FontData data = null;
            if (FontLibrary.TryGetValue(font, out data))
            {
                data.Dispose();
            }
        }

        public override void AddText(Font font, Rectangle rect, string strText, FontRenderFlags dwFlag = FontRenderFlags.Left)
        {

        }

        public override void AddText(Font font, Rectangle rect, float scale, string strText, FontRenderFlags dwFlag = FontRenderFlags.Left)
        {
            LoadFont(font);

            D3D9FontData data = null;
            if (!FontLibrary.TryGetValue(font, out data))
                throw new NullReferenceException("Font does not exist");

            float x = rect.X;
            float y = rect.Y;

            foreach(var c in strText)
            {
                if (c == ' ')
                    x += data.SpaceWidth;
                else if (c == '\n')
                {
                    x = rect.X;
                    y += data.CharacterHeight;
                }
                else
                {
                    Rectangle charRect = data.CharacterRects[c];

                    int width = charRect.Right - charRect.Left;
                    int height = charRect.Bottom - charRect.Top;

                    FontVertex[] v = new FontVertex[]
                    {
                        new FontVertex(new Vector4(x, y + height, 0.5f, 0.5f), this.FontColor, new Vector2((float)charRect.Left / data.TextureSize.Width,(float)charRect.Bottom / data.TextureSize.Height)),
                        new FontVertex(new Vector4(x, y, 0.5f, 0.5f), this.FontColor, new Vector2((float)charRect.Left / data.TextureSize.Width,(float)charRect.Top / data.TextureSize.Height)),
                        new FontVertex(new Vector4(x + width, y + height, 0.5f, 0.5f), this.FontColor, new Vector2((float)charRect.Right / data.TextureSize.Width,(float)charRect.Bottom / data.TextureSize.Height)),

                        new FontVertex(new Vector4(x + width, y, 0.5f, 0.5f), this.FontColor, new Vector2((float)charRect.Right / data.TextureSize.Width,(float)charRect.Top / data.TextureSize.Height)),
                        new FontVertex(new Vector4(x + width, y + height, 0.5f, 0.5f), this.FontColor, new Vector2((float)charRect.Right / data.TextureSize.Width,(float)charRect.Bottom / data.TextureSize.Height)),
                        new FontVertex(new Vector4(x, y, 0.5f, 0.5f), this.FontColor, new Vector2((float)charRect.Left / data.TextureSize.Width,(float)charRect.Top / data.TextureSize.Height))
                    };

                    FontBuffer.Stream.Write<FontVertex>(v[0]);

                    x += width;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Size=20)]
        public struct Vertex
        {
            public Vector4 Vector;
            public ColorBGRA Color;

            public Vertex(Vector4 vector, ColorBGRA color)
            {
                Vector = vector;
                Color = color;
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 28)]
        public struct FontVertex
        {
            public Vertex Vertex;
            public Vector2 TexCoords;

            public FontVertex(Vector4 vector, ColorBGRA color, Vector2 texCoords)
            {
                Vertex = new Vertex(vector, color);
                TexCoords = texCoords;
            }
        }

        protected class D3D9FontData : FontData
        {
            public Direct3D9 Renderer { get; private set; }

            public Texture Texture { get; private set; }

            public D3D9FontData(Direct3D9 renderer,Font font)
                : base(renderer, font)
            {
                Renderer = renderer;
            }
            
            protected override bool BuildFontTexture(Renderer _renderer, System.Drawing.Bitmap fontBitmap)
            {
                Direct3D9 renderer = _renderer as Direct3D9;
                if (renderer == null)
                    return false;
                 
                System.Drawing.Imaging.BitmapData bmData = fontBitmap.LockBits(new System.Drawing.Rectangle(0, 0, TextureSize.Width, TextureSize.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                Texture = new Texture(renderer.Device, TextureSize.Width, TextureSize.Height, 1, Usage.None, Format.A8B8G8R8, Pool.Managed);

                if (Texture == null)
                    return false;


                DataStream stream;
                var rect = Texture.LockRectangle(0, LockFlags.Discard, out stream);
                stream.Write(bmData.Scan0, 0, bmData.Stride * bmData.Height);

                stream.Close();
                fontBitmap.UnlockBits(bmData);

                Texture.UnlockRectangle(0);
                return true;
            }

            public override void Dispose()
            {
                if (Texture != null)
                    Texture.Dispose();

                Texture = null;
            }
        }

        private class D3D9VertexBuffer<T> : util.VertexBuffer<T, VertexBuffer>
        {
            VertexFormat Flag;

            public D3D9VertexBuffer(VertexFormat flag, int maxVertices=1)
                : base(maxVertices)
            {
                Flag = flag;
            }

            public override void Dispose()
            {
                if (IsOpen)
                    End();
                if (Buffer != null)
                    Buffer.Dispose();

                if (Stream != null)
                    Stream.Dispose();
                Stream = null;
            }

            public override void Begin(object obj)
            {
                Device device = obj as Device;
                if (device == null)
                    throw new ArgumentNullException("device");

                if (IsBufferResizing || Buffer == null)
                {
                    if (Buffer != null)
                        Buffer.Dispose();

                    Buffer = new VertexBuffer(device, MaxVertices * Marshal.SizeOf(typeof(T)), Usage.WriteOnly | Usage.Dynamic, Flag, Pool.Managed);

                    IsBufferResizing = false;
                }

                Stream = Buffer.Lock(0, 0, LockFlags.Discard);
                IsOpen = true;
            }

            public override void End()
            {
                if (Stream != null)
                    Stream.Close();

                Buffer.Unlock();
                Stream = null;
            }
        }

    }
}
