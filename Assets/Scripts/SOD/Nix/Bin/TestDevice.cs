using System;
using System.IO;

namespace SOD
{
    namespace Nix
    {
        namespace Bin
        {

            public class TestDevice : CharacterDevice
            {
                public string Message = "no message";
                private byte []FullMessage = null;
                private bool ReadFlag = false;
                public TestDevice()
                {
                    MakeFullMessage();
                }
                private void MakeFullMessage()
                {
                    FullMessage = System.Text.Encoding.UTF8.GetBytes("Test device!: >" + Message + "<");
                }

                public override bool CanRead { get { return true; } }
                public override bool CanSeek { get { return true; } }
                public override bool CanTimeout { get { return true; } }
                public override bool CanWrite { get { return true; } }

                public override long Length { get { return FullMessage.Length; } }
                public override long Position { get { return 0; } set { } }
                public override int Read(byte []buffer, int offset, int count)
                {
                    if (ReadFlag)
                    {
                        return 0;
                    }
                    ReadFlag = true;
                    Session.BaseStdOut.WriteLine("Attempting to read from Test device!"); 
                    int numRead = Math.Min(count, FullMessage.Length);
                    Array.Copy(FullMessage, 0, buffer, offset, numRead);
                    return numRead;
                }
                public override void Write(byte []buffer, int offset, int count)
                {
                    ReadFlag = false;
                    Message = System.Text.Encoding.UTF8.GetString(buffer, offset, count);
                    Session.BaseStdOut.WriteLine("Writing to device >" + Message + "<");
                    MakeFullMessage();
                }
                public override void Close()
                {

                }
                public override void Flush()
                {

                }
                public override long Seek(long offset, SeekOrigin origin)
                {
                    return 0;
                }
                public override void SetLength(long value)
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
