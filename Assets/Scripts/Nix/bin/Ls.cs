﻿using UnityEngine;
using System.IO;
using System.Collections;

public class Ls : Program {

	public Ls(int pid) : base(pid) {
		
	}
    public override string GetCommand() {
		return "ls";
	}
    protected override void Run() {
		NixPath newPath = MainSession.WorkingDirectory;
		if (Argv.Length > 0) {
			newPath = MainSession.WorkingDirectory.Combine(Argv[0]);
		}

		FileNode []files = MainSystem.RootDrive.ListFiles(newPath.ToString());
		if (files != null) {
			for (int i = 0; i < files.Length; i++) {
				FileInfo info = files[i].Info;
				StdOut.Write(info.Name);
				if ((info.Attributes & FileAttributes.Directory) == FileAttributes.Directory) {
					StdOut.Write("/");
				}
				StdOut.Write("\n");
			}
		}
		else {
			StdOut.Write(newPath.ToString() + " is not a directory.\n");
		}
		return;
	}
}