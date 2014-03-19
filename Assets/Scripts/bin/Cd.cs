using UnityEngine;
using System.Collections;
using System.IO;

public class Cd : Program {

	public Cd(int pid) : base(pid) {

	}
	public override string GetCommand() {
		return "cd";
	}
	protected override void Run() {
		if (Argv.Length == 0) {
			StdOut.Write("Need help");
			return;
		}

		NixPath newPath = MainSession.WorkingDirectory.Combine(Argv[0]);
		if (MainSystem.RootDrive.IsDirectory(newPath.ToString())) {
			MainSession.WorkingDirectory = newPath;
		}
		else {
			StdOut.Write(newPath.ToString() + " is not a directory.\n");
		}
		return;
	}
}

