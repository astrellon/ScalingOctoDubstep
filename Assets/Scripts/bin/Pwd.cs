using UnityEngine;
using System.Collections;
using System;

public class Pwd : Program {

	public Pwd() {
		
	}
	public override string GetCommand() {
		return "pwd";
	}
	protected override void Run() {
        string pwd = "No session";
		if (MainSession != null && MainSession.WorkingDirectory != null) {
			pwd = MainSession.WorkingDirectory.ToString();
        }
        Debug.Log("PWD: " + pwd);
		StdOut.Write(pwd);
		StdOut.Write("\n");
	}

}
