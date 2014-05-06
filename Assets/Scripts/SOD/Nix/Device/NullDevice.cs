using System;

namespace SOD
{
    namespace Nix
    {
        namespace Device
        {
            public class NullDevice : CharacterDevice
            {
                public override int Read(int socketId, byte []buffer, int offset, int count)
                {
                    return 0;
                }
                public override void Write(int socketId, byte []buffer, int offset, int count)
                {
                    return;
                }
                public override void Close(int socketId)
                {

                }
                public override int DeinitDevice()
                {
                    return 1;
                }
                public override int InitDevice()
                {
                    return 1;
                }
                public override string DeviceType()
                {
                    return "character";
                }
                public override string DeviceName()
                {
                    return "null";
                }
            }
        }
    }
}
