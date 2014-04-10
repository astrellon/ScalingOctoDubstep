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
		Write(StdOut, 0x1b);
		Write(StdOut, "[2J");
	}

}
