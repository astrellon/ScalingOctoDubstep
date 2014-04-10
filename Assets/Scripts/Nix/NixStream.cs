using System;
using System.IO;
using System.Threading;

public class NixStream : MemoryStream
{
    public bool Enabled { get; set; }
    public bool EchoStream { get; set; }
    private long _ReadPosition = 0;
    public object Lock = new object();

    public override long Position
    {
        get
        {
            return _ReadPosition;
        }
        set
        {
            _ReadPosition = value;
        }
    }
    
	public NixStream()
    {
        Enabled = true;
        EchoStream = true;
    }

    public int Remaining
    {
        get
        {
            return (int)(Length - Position);
        }
    }
    public int Read(byte[] buffer)
    {
        return Read(buffer, 0, buffer.Length);
    }
    public override int ReadByte()
    {
        int result = -1;
        lock (Lock)
        {
            while (Length - Position == 0)
            {
                Monitor.Wait(Lock);
            }
            base.Position = Position;
            result = base.ReadByte();
            if (result != -1)
            {
                Position++;
            }
        }
        return result;
    }
    public override int Read(byte[] buffer, int offset, int count)
    {
        int result = 0;
        lock (Lock)
        {
            while (Length - Position == 0)
            {
                Monitor.Wait(Lock);
            }
            
            base.Position = Position;
            result = base.Read(buffer, offset, count);
            Position += result;
        }
        return result;
    }
    public int ReadString(ref string dest, int maxRead = -1)
    {
        byte[] buffer;
        int result = 0;

        lock (Lock)
        {
            while (Length - Position == 0)
            {
                Monitor.Wait(Lock);
            }
            if (maxRead < 0)
            {
                maxRead = (int)(Length - Position);
            }
            base.Position = Position;
            buffer = new byte[maxRead];
            result = base.Read(buffer, 0, maxRead);
            Position += result;

        }
        dest = System.Text.Encoding.UTF8.GetString(buffer);

        return result;
    }


    public override void Flush()
    {
        Position = 0;
        base.Position = 0;
        SetLength(0);
    }

    public void Write(byte data)
    {
        if (!Enabled)
        {
            return;
        }
        lock (Lock)
        {
            base.Position = Length;
            this.Write(new byte[] { data }, 0, 1);
            Monitor.PulseAll(Lock);
        }
    }
    
    public void WriteString(string input)
    {
        if (!Enabled || input == null || input.Length == 0)
        {
            return;
        }
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);

        lock (Lock)
        {
            base.Position = Length;
            Write(bytes, 0, bytes.Length);
            Monitor.PulseAll(Lock);
        }
    }
	public void WriteStringLine(string input)
	{
		Write (input + "\n");
	}
}
