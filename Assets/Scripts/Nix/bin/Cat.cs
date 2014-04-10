using UnityEngine;
using System.Collections;
using System.IO;

public class Cat : Program {
	
	public Cat(int pid) : base(pid) {
		
	}
	public override string GetCommand() {
		return "cat";
	}
	protected override void Run() {
		if (Argv.Count == 1) {
			// Read from stdin
		}
		else {
			for (int i = 1; i < Argv.Count; i++) {
				NixPath path = MainSession.WorkingDirectory.Combine(Argv[i]);
				if (MainSystem.RootDrive.IsFileOrDirectory(path)) {
					FileStream file = File.OpenRead(MainSystem.RootDrive.GetPathTo(path.ToString()));
					StreamReader reader = new StreamReader(file);
					StdOut.WriteLine(reader.ReadToEnd());
				}
				else {
					StdOut.Write(GetCommand() + ": " + Argv[i] + ": No such file or directory\n");
				}
			}
		}
	}
}

