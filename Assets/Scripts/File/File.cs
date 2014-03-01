using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class File {

	static int DIR = 		1;
	static int USER_READ = 	1 << 1;
	static int USER_WRITE =	1 << 2;

	protected Dictionary<string, string> Metadata { get; private set; }
	protected string Owner { get; set; }
	protected int Flags { get; set; }

	public File(bool directory = false) {
		Metadata = new Dictionary<string, string>();
		Directory = directory;
	}

	public bool Directory {
		get {
			return (Flags & DIR) > 1;
		}
		private set {
			if (value) {
				Flags |= DIR;
			}
			else {
				Flags &= ~DIR;
			}
		}
	}
	public bool UserRead {
		get {
			return (Flags & USER_READ) > 1;
		}
		set {
			if (value) {
				Flags |= USER_READ;
			}
			else {
				Flags &= ~USER_READ;
			}
		}
	}
	public bool UserWrite {
		get {
			return (Flags & USER_WRITE) > 1;
		}
		set {
			if (value) {
				Flags |= USER_WRITE;
			}
			else {
				Flags &= ~USER_WRITE;
			}
		}
	}
}
