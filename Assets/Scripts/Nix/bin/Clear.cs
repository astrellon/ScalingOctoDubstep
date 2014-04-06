using UnityEngine;
using System.Collections;
using System;

public class Clear : Program {

	public Clear(int pid) : base(pid) {
		
	}
	public override string GetCommand() {
		return "clear";
	}
	protected override void Run() {
		StdOut.Write(0x1b);
		StdOut.Write("[2J");
	}

}
