using System;
using System.IO;

namespace SOD
{
    namespace Nix
    {
        namespace Bin
        {
            public abstract class CharacterDevice : Stream
            {
                public int Id { get; set; }
                public bool IsOpen { get; protected set; }

                /*
                public bool CanRead { get; }
                public bool CanSeek { get; }
                public bool CanTimeout { get; }
                public bool CanWrite { get; }
                public long Length { get; }
                public long Position { get; set; }
                public void Close();
                public void Flush();
                public long Seek(long offset, SeekOrigin origin);
                public void SetLength(long value);
                */

                public abstract int DeinitDevice();
                public abstract int InitDevice();

                public abstract string DeviceType();
                public abstract string DeviceName();
            }
        }
    }
}
