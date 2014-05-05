using System;
using System.IO;
using System.Collections.Generic;

namespace SOD
{
    namespace Nix
    {
        namespace Device
        {
            public class TestDevice : CharacterDevice
            {
                public string Message = "no message";
                private byte []FullMessage = null;
                private Dictionary<int, bool> ReadFlags = new Dictionary<int, bool>();
                public TestDevice()
                {
                    MakeFullMessage();
                }
                private void MakeFullMessage()
                {
                    FullMessage = System.Text.Encoding.UTF8.GetBytes("Test device!: >" + Message + "<");
                }

                public override bool CanRead { get { return true; } }
                public override bool CanWrite { get { return true; } }
                public override int Read(int socketId, byte []buffer, int offset, int count)
                {
                    Session.BaseStdOut.WriteLine("Attempting to read from Test device! " + socketId + " | " + count); 
                    if (ReadFlags.ContainsKey(socketId) && ReadFlags[socketId])
                    {
                        return 0;
                    }
                    ReadFlags[socketId] = true;
                    int numRead = Math.Min(count, FullMessage.Length);
                    Array.Copy(FullMessage, 0, buffer, offset, numRead);
                    return numRead;
                }
                public override void Write(int socketId, byte []buffer, int offset, int count)
                {
                    ReadFlags[socketId] = false;
                    Message = System.Text.Encoding.UTF8.GetString(buffer, offset, count);
                    Session.BaseStdOut.WriteLine("Writing to device >" + Message + "<");
                    MakeFullMessage();
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
                    return "string";
                }
                public override string DeviceName()
                {
                    return "test";
                }
            }

        }
    }
}
