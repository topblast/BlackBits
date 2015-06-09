using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace BBitsCore.Renderers
{
    public class Font
    {
        public System.Drawing.Font InnerFont { get; private set; }

        public System.Drawing.Text.TextRenderingHint RenderingHint { get; private set; }

        public Font(string fontName, float emSize, System.Drawing.FontStyle style, bool antiAliased=true)
        {
            InnerFont = new System.Drawing.Font(fontName, emSize, style, System.Drawing.GraphicsUnit.Pixel);
           
            System.Drawing.Text.TextRenderingHint hint = antiAliased ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.SystemDefault;

        }

        public override int GetHashCode()
        {
            return base.GetHashCode() * InnerFont.GetHashCode() % RenderingHint.GetHashCode() * 8;
        }
    }

    public abstract class FontData : IDisposable
    {
        const char START_CHARACTER = (char)33;
        const char END_CHARACTER = (char)127;
        const char NUMBER_OF_CHARACTERS = (char)(END_CHARACTER - START_CHARACTER);
        
        public float CharacterHeight { get; private set; }

        public float SpaceWidth { get; private set; }

        public System.Drawing.Size TextureSize { get; private set; }

        public Rectangle[] CharacterRects { get; private set; }

        public FontData(Renderer renderer, Font font)
        {
            int tempSize = Convert.ToInt32(Math.Round(font.InnerFont.Size * 2));

            TextureSize = new System.Drawing.Size(tempSize, tempSize);

            CharacterRects = new Rectangle[NUMBER_OF_CHARACTERS];
            
            using (System.Drawing.Bitmap charBitmap = new System.Drawing.Bitmap(TextureSize.Width, TextureSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var charGraphics = System.Drawing.Graphics.FromImage(charBitmap))
                {
                    charGraphics.PageUnit = System.Drawing.GraphicsUnit.Pixel;
                    charGraphics.TextRenderingHint = font.RenderingHint;

                    MeasureChars(font.InnerFont, charGraphics);

                    using (var fontBitmap = new System.Drawing.Bitmap(TextureSize.Width, TextureSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        using (var fontGraphics = System.Drawing.Graphics.FromImage(fontBitmap))
                        {
                            fontGraphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                            fontGraphics.Clear(System.Drawing.Color.FromArgb(0, System.Drawing.Color.Black));

                            BuildFontBitmap(font.InnerFont, charGraphics, charBitmap, fontGraphics);

                            BuildFontTexture(renderer, fontBitmap);
                        }
                    }
                }
            }
        }

        private void MeasureChars(System.Drawing.Font font, System.Drawing.Graphics charGraphics)
        {
            char[] allChars = new char[NUMBER_OF_CHARACTERS];

            for (char i = (char)0; i < NUMBER_OF_CHARACTERS; i++)
            {
                allChars[i] = (char)(START_CHARACTER + i);
            }

            System.Drawing.SizeF size = charGraphics.MeasureString(new String(allChars), font, new System.Drawing.PointF(0, 0), System.Drawing.StringFormat.GenericDefault);

            CharacterHeight = size.Height + 0.5f;

            int numRows = (int)Math.Round(size.Width / TextureSize.Width);
            TextureSize = new System.Drawing.Size(TextureSize.Width,  numRows * (int)CharacterHeight + 1);

            System.Drawing.StringFormat strFormat = System.Drawing.StringFormat.GenericDefault;
            strFormat.FormatFlags |= System.Drawing.StringFormatFlags.MeasureTrailingSpaces;
            size = charGraphics.MeasureString(" ", font, 0, strFormat);

            SpaceWidth = size.Width + 0.5f;
        }

        private void BuildFontBitmap(System.Drawing.Font font, System.Drawing.Graphics charGraphics, System.Drawing.Bitmap charBitmap, System.Drawing.Graphics fontGraphics)
        {
            System.Drawing.Brush whiteBrush = System.Drawing.Brushes.White;
            int fontX = 0;
            int fontY = 0;


            for (int i = 0; i < NUMBER_OF_CHARACTERS; i++)
            {
                charGraphics.Clear(System.Drawing.Color.FromArgb(0, System.Drawing.Color.Black));
                charGraphics.DrawString(((char)(START_CHARACTER + i)).ToString(), font, whiteBrush, new System.Drawing.PointF(0.0f, 0.0f));

                int minX = GetCharMinX(charBitmap);
                int maxX = GetCharMaxX(charBitmap);
                int charWidth = maxX - minX + 1;

                if (fontX + charWidth >= TextureSize.Width)
                {
                    fontX = 0;
                    fontY += (int)(CharacterHeight) + 1;
                }

                CharacterRects[i] = new Rectangle(fontX, fontY, fontX + charWidth, fontY + (int)CharacterHeight);

                fontGraphics.DrawImage(charBitmap, fontX, fontY, new System.Drawing.Rectangle(minX, 0, charWidth, (int)CharacterHeight), System.Drawing.GraphicsUnit.Pixel);

                fontY += charWidth + 1;
            }
        }

        private int GetCharMaxX(System.Drawing.Bitmap charBitmap)
        {
            int width = charBitmap.Width;
            int height = charBitmap.Height;

            for (int x = width - 1; x >= 0; --x)
            {
                for (int y = 0; y < height; ++y)
                {
                    System.Drawing.Color color;

                    color = charBitmap.GetPixel(x, y);
                    if (color.A > 0)
                        return x;
                }
            }

            return width - 1;
        }

        private int GetCharMinX(System.Drawing.Bitmap charBitmap)
        {
            int width = charBitmap.Width;
            int height = charBitmap.Height;

            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    System.Drawing.Color color;

                    color = charBitmap.GetPixel(x, y);
                    if (color.A > 0)
                        return x;
                }
            }

            return 0;
        }

        protected abstract bool BuildFontTexture(Renderer renderer, System.Drawing.Bitmap fontBitmap);

        public abstract void Dispose();
    }
}
