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

                public CharacterDevice()
                {
                    Id = -1;
                }

                /*
                public bool CanSeek { get; }
                public bool CanTimeout { get; }
                public long Length { get; }
                public long Position { get; set; }
                public void Flush();
                public long Seek(long offset, SeekOrigin origin);
                public void SetLength(long value);
                */
                public virtual bool CanRead { get { return true; } }
                public virtual bool CanWrite { get { return true; } }
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
