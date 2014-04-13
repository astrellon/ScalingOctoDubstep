using System;
using System.IO;

public class FileNode
{
	public FileInfo Info {get; set;}
	public FileNode (string path)
	{
		Info = new FileInfo(path);
	}
}

