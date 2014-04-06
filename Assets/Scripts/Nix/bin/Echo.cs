using UnityEngine;
using System.Collections;

public class Echo : Program {

	public Echo(int pid) : base(pid) {

	}
	public override string GetCommand() {
		return "echo";
	}
	protected override void Run() {
		for (int i = 0; i < Argv.Length; i++) {
			StdOut.Write(Argv[i]);
		}
		StdOut.Write("\n");
		return;
	}

}
