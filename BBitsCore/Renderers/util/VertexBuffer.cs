using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBitsCore.Renderers.util
{
    abstract class VertexBuffer<T, B> : IDisposable where B : class
    {
        public B Buffer { get; protected set; }

        public int MaxVertices { get; protected set; }

        public int NumberOfVertices { get; protected set; }

        public bool IsBufferResizing { get; protected set; }

        public bool IsOpen { get; protected set; }

        private DataStream _stream;

        public DataStream Stream
        {
            get
            {
                if (_stream == null || !IsOpen)
                    throw new InvalidOperationException("Stream not initialize!");

                return Stream;
            }

            protected set { _stream = value; }
        }

        public VertexBuffer(int maxVertices = 1)
        {
            IsBufferResizing = true;
            NumberOfVertices = 0;
            MaxVertices = maxVertices;
            IsOpen = false;
            Buffer = null;
            Stream = null;
        }

        public virtual void Dispose()
        {
            if (IsOpen)
                End();

            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
        }

        public abstract void Begin(object obj);
        
        public abstract void End();

    }
}
