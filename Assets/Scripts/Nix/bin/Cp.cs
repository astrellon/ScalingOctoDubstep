using UnityEngine;
using System.Collections;
using System.IO;

public class Cp : Program {
	
	public Cp(int pid) : base(pid) {
		
	}
	public override string GetCommand() {
		return "cp";
	}
	protected override void Run() {
		if (Argv.Length <= 2) {
			WriteLine(StdOut, "Need help");
			return;
		}
		
		NixPath fromPath = MainSession.WorkingDirectory.Combine(Argv[1]);
		NixPath toPath = MainSession.WorkingDirectory.Combine(Argv[2]);

		WriteLine(StdOut, "Copying from: " + fromPath + "\nCopying to: " + toPath);
		try {
			MainSystem.RootDrive.Copy(fromPath, toPath);
		}
		catch (System.IO.FileNotFoundException exp) {
			WriteLine(StdOut, "No such file or directory: " + Argv[1]);
		}
		catch (System.Exception exp) {
			WriteLine(StdOut, exp.Message);
		}

		//StdOut.Write ("No such file or directory: " + Argv[1]);
		/*if (MainSystem.RootDrive.IsDirectory(newPath.ToString())) {
			MainSession.SetWorkingDirectory(newPath);
		}
		else {
			StdOut.Write(newPath.ToString() + " is not a directory.\n");
		}*/
		return;
	}
}

