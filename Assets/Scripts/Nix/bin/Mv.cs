using UnityEngine;
using System.Collections;
using System.IO;

public class Mv : Program {
	
	public Mv(int pid) : base(pid) {
		
	}
	public override string GetCommand() {
		return "mv";
	}
	protected override void Run() {
		if (Argv.Count <= 2) {
			StdOut.WriteLine("Need help");
			return;
		}
		
		NixPath fromPath = MainSession.WorkingDirectory.Combine(Argv[1]);
		NixPath toPath = MainSession.WorkingDirectory.Combine(Argv[2]);
		
		try {
			MainSystem.RootDrive.Move(fromPath, toPath);
		}
		catch (System.IO.FileNotFoundException exp) {
			StdOut.WriteLine("No such file or directory: " + Argv[1]);
		}
		catch (System.Exception exp) {
			StdOut.WriteLine(exp.Message);
		}
		return;
	}
}

