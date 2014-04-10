using UnityEngine;
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
		if (Argv.Length > 1) {
			newPath = MainSession.WorkingDirectory.Combine(Argv[1]);
		}

		FileNode []files = MainSystem.RootDrive.ListFiles(newPath.ToString());
		if (files != null) {
			for (int i = 0; i < files.Length; i++) {
				FileInfo info = files[i].Info;
				Write(StdOut, info.Name);
				if ((info.Attributes & FileAttributes.Directory) == FileAttributes.Directory) {
					Write(StdOut, "/");
				}
				Write(StdOut, "\n");
			}
		}
		else {
			Write(StdOut, newPath.ToString() + " is not a directory.\n");
		}
		return;
	}
}
