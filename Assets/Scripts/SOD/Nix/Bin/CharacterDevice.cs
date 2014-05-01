using System;

namespace SOD
{
    namespace Nix
    {
        namespace Bin
        {
            public abstract class CharacterDevice
            {
                public abstract int Read(byte []buffer, int offset, int count);
                public abstract void Write(byte []buffer, int offset, int count);
                public abstract int Open();
                public abstract int Release();
                public abstract int Init();
                public abstract void Close();

                public abstract string DeviceType();
                public abstract string DeviceName();
                public abstract int GetId();
                public abstract void SetId(int id);
            }
        }
    }
}
