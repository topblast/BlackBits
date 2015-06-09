using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBitsCore.Renderers
{
    [Flags]
    public enum FontRenderFlags
    {
        Left,
        Right,
        Center
    }

    public abstract class Renderer : IDisposable
    {
        public ColorBGRA Color { get; set; }

        public ColorBGRA FontColor { get; set; }

        public abstract void Initialize();
        public abstract void Dispose();

	    public abstract void Begin(int fps);
	    public abstract void End();
	    public abstract void Present();

	    public abstract void FillRect(Rectangle rect);
	    public abstract void AddLine(Rectangle rect);

	    public abstract void LoadFont(Font font);
	    public abstract void FreeFont(Font font);

        public abstract void AddText(Font font, Rectangle rect, string strText, FontRenderFlags dwFlag = 0);
        public abstract void AddText(Font font, Rectangle rect, float scale, string strText, FontRenderFlags dwFlag = 0);
    }
}
