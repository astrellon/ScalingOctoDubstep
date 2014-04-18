namespace SOD
{
    namespace Nix
    {
        // THERE HAS TO BE A BETTER WAY!!!
        public static class StreamExtensions
        {
            // General Stream extensions
            public static void Write(this System.IO.Stream stream, byte data)
            {
                stream.Write(new byte[] { data }, 0, 1);
            }
            public static void Write(this System.IO.Stream stream, string buffer)
            {
                NixStream nixStream = stream as NixStream;
                if (nixStream != null)
                {
                    nixStream.WriteString(buffer);
                    return;
                }
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(buffer);
                stream.Write(bytes, 0, bytes.Length);
            }
            public static void WriteLine(this System.IO.Stream stream, string buffer)
            {
                NixStream nixStream = stream as NixStream;
                if (nixStream != null)
                {
                    nixStream.WriteStringLine(buffer);
                    return;
                }
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(buffer + "\n");
                stream.Write(bytes, 0, bytes.Length);
            }

            public static int Read(this System.IO.Stream stream, ref string buffer, int maxRead = -1)
            {
                NixStream nixStream = stream as NixStream;
                if (nixStream != null)
                {
                    return nixStream.ReadString(ref buffer, maxRead);
                }
                return 0;
            }

            // NixStream specific
            public static void Write(this NixStream stream, string buffer)
            {
                stream.WriteString(buffer);
            }
            public static void WriteLine(this NixStream stream, string buffer)
            {
                stream.WriteStringLine(buffer);
            }

            public static bool GetEchoStream(this System.IO.Stream stream)
            {
                NixStream nixStream = stream as NixStream;
                if (nixStream != null)
                {
                    return nixStream.EchoStream;
                }
                return true;
            }
            public static void SetEchoStream(this System.IO.Stream stream, bool value)
            {
                NixStream nixStream = stream as NixStream;
                if (nixStream != null)
                {
                    nixStream.EchoStream = value;
                }
            }
        }
    }
}
