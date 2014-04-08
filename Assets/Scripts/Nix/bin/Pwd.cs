using UnityEngine;
using System.Collections;
using System;

public class Pwd : Program {

	public Pwd(int pid) : base(pid) {
		
	}
	public override string GetCommand() {
		return "pwd";
	}
	protected override void Run() {
        string pwd = "No session";
		if (MainSession != null && MainSession.WorkingDirectory != null) {
			pwd = MainSession.WorkingDirectory.ToString();
        }
		StdOut.Write(pwd);
		StdOut.Write("\n");
	}

}
