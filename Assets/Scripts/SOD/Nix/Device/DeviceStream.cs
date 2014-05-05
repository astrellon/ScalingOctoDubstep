using System;
using System.IO;

namespace SOD
{
    namespace Nix
    {
        namespace Device
        {
            public class DeviceStream : Stream
            {
                public CharacterDevice Device { get; protected set; }
                public int SocketId { get; protected set; }

                public DeviceStream(CharacterDevice device, int id)
                {
                    Device = device;
                    SocketId = id;
                }

                public override bool CanSeek { get { return false; } }
                public override bool CanTimeout { get { return false; } }
                public override long Length { get { return 0; } }
                public override long Position { get { return 0; } set { } }
                public override void Flush() { }
                public override long Seek(long offset, SeekOrigin origin) { return 0; }
                public override void SetLength(long value) { }

                public override bool CanRead
                {
                    get
                    {
                        return Device.CanRead;
                    }
                }
                public override bool CanWrite
                {
                    get
                    {
                        return Device.CanWrite;
                    }
                }
                public override void Close()
                {
                    Device.Close(SocketId);
                }
                public override int Read(byte []buffer, int offset, int count)
                {
                    return Device.Read(SocketId, buffer, offset, count);
                }
                public override void Write(byte []buffer, int offset, int count)
                {
                    Device.Write(SocketId, buffer, offset, count);
                }
            }

        }
    }
}
