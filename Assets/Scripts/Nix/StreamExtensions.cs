public static class StreamExtensions {
    public static void Write(this System.IO.Stream stream, string buffer) {
        byte []bytes = System.Text.Encoding.UTF8.GetBytes(buffer);
        stream.Write(bytes, 0, bytes.Length);
    }
    public static void WriteLine(this System.IO.Stream stream, string buffer) {
        byte []bytes = System.Text.Encoding.UTF8.GetBytes(buffer + "\n");
        stream.Write(bytes, 0, bytes.Length);
    }
}
