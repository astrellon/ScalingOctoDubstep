using System;
using System.IO;

namespace SOD
{
    namespace Nix
    {
        namespace Device
        {
            public abstract class CharacterDevice
            {
                public int Id { get; set; }
                public bool IsOpen { get; protected set; }

                /*
                public bool CanSeek { get; }
                public bool CanTimeout { get; }
                public long Length { get; }
                public long Position { get; set; }
                public void Flush();
                public long Seek(long offset, SeekOrigin origin);
                public void SetLength(long value);
                */
                public abstract bool CanRead { get; }
                public abstract bool CanWrite { get; }
                public abstract void Close(int socketId);
                public abstract int Read(int socketId, byte []buffer, int offset, int count);
                public abstract void Write(int socketid, byte []buffer, int offset, int count);

                public abstract int DeinitDevice();
                public abstract int InitDevice();

                public abstract string DeviceType();
                public abstract string DeviceName();

                private int SocketCounter = 0;
                public DeviceStream CreateStream()
                {
                    return new DeviceStream(this, SocketCounter++);
                }
            }
        }
    }
}
