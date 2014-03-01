using System;
using System.IO;
using System.Threading;
using UnityEngine;

public class NixStream : INixStream {

    public Stream InternalStream {get; set;}
    public bool IsFileStream {get; private set;}

    public NixStream(Stream baseStream) {
        InternalStream = baseStream;
        IsFileStream = baseStream is FileStream;
        Enabled = true;
    }

    public bool Enabled {get; set;}

    public int Length() {
        return (int)InternalStream.Length;
    }
    public int Read(byte []dest, int offset = 0, int maxRead = -1) {
        if (maxRead < 0) {
            maxRead = dest.Length;
        }

        int result = 0;
        lock (InternalStream) {
            if (IsFileStream) {
                result = InternalStream.Read(dest, offset, maxRead);
            }
            else {
                while (InternalStream.Length == 0) {
                    Monitor.Wait(InternalStream);
                }
                InternalStream.Seek(0, SeekOrigin.Begin);
                result = InternalStream.Read(dest, offset, maxRead);
                InternalStream.SetLength(0);
            }
        }
        string read = System.Text.Encoding.Default.GetString(dest, 0, maxRead);
        return result;
    }
    public int Read(ref string dest, int maxRead = -1) {
		byte []buffer;
        int result = 0;
		lock (InternalStream) {
            if (IsFileStream) {
                while (InternalStream.Length == 0) {
                    Monitor.Wait(InternalStream);
                }
            }
            if (maxRead < 0) {
                maxRead = Length(); 
            }
			buffer = new byte[maxRead];
            if (IsFileStream) {
                result = InternalStream.Read(buffer, 0, maxRead);
            }
            else {
                InternalStream.Seek(0, SeekOrigin.Begin);
                result = InternalStream.Read(buffer, 0, maxRead);
                InternalStream.SetLength(0);
            }
        }
        if (result > 0) {
            dest = System.Text.Encoding.Default.GetString(buffer, 0, maxRead);
        }

        return result;
    }

    public void Write(byte []buffer, int offset = 0, int count = -1) {
        if (!Enabled) {
            return;
        }
        if (count < 0) {
            count = buffer.Length;
        }
		lock (InternalStream) {
            InternalStream.Write(buffer, offset, count);
			Monitor.PulseAll(InternalStream);
        }
    } 
    public void Write(byte data) { 
        if (!Enabled) {
            return;
        }
		lock (InternalStream) {
            InternalStream.Write(new byte[]{data}, 0, 1);
			Monitor.PulseAll(InternalStream);
        }
    } 
    public void Write(string buffer) {
        if (!Enabled) {
            return;
        }
        byte[] bytes = new byte[buffer.Length];
        for (int i = 0; i < buffer.Length; i++) {
            bytes[i] = (byte)buffer[i];
        }
		lock (InternalStream) {
            InternalStream.Write(bytes, 0, bytes.Length);
			Monitor.PulseAll(InternalStream);
        }
    }
}

