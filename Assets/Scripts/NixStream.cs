using System;
using System.IO;
using System.Threading;

public class NixStream : MemoryStream
{
	public bool Enabled { get; set; }
	public bool EchoStream { get; set; }
	private int _ReadPosition = 0;
	public object _Lock = new object();
	
	public int ReadPosition
	{
		get
		{
			return _ReadPosition;
		}
	}
	public void SetReadPosition(int value, int oldPos = -1)
	{
		_ReadPosition += value;
		if (_ReadPosition == Position)
		{
			Position = 0;
			_ReadPosition = 0;
			SetLength(0);
		}
		else if (oldPos >= 0)
		{
			Position = oldPos;
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
		lock (_Lock)
		{
			while (ReadPosition - Length == 0)
			{
				Monitor.Wait(_Lock);
			}
			result = base.ReadByte();
			if (result != -1)
			{
				SetReadPosition(_ReadPosition + 1);
			}
		}
		return result;
	}
	public override int Read(byte[] buffer, int offset, int count)
	{
		int result = 0;
		lock (_Lock)
		{
			while (ReadPosition - Length == 0)
			{
				Monitor.Wait(_Lock);
			}
			int oldPos = (int)Position;
			Position = _ReadPosition;
			result = base.Read(buffer, offset, count);
			SetReadPosition(_ReadPosition + result, oldPos);
		}
		return result;
	}
	public int Read(ref string dest, int maxRead = -1)
	{
		byte[] buffer;
		int result = 0;
		
		lock (_Lock)
		{
			while (Length == 0)
			{
				Monitor.Wait(_Lock);
			}
			if (maxRead < 0)
			{
				maxRead = (int)(Length - ReadPosition);
			}
			int oldPos = (int)Position;
			Position = ReadPosition;
			buffer = new byte[maxRead];
			result = base.Read(buffer, 0, maxRead);
			SetReadPosition(_ReadPosition + result, oldPos);
			//ReadPosition += result;
			
		}
		dest = System.Text.Encoding.UTF8.GetString(buffer);
		
		return result;
	}
	
	public void Write(byte data)
	{
		if (!Enabled)
		{
			return;
		}
		lock (_Lock)
		{
			this.Write(new byte[] { data }, 0, 1);
			Monitor.PulseAll(_Lock);
		}
	}
	public void Write(string input)
	{
		if (!Enabled || input == null || input.Length == 0)
		{
			return;
		}
		byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
		
		lock (_Lock)
		{
			Write(bytes, 0, bytes.Length);
			Monitor.PulseAll(_Lock);
		}
	}	
}
