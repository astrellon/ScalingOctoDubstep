using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class Mkdir : Program {
	
	public Mkdir(int pid) : base(pid) {
		
	}
	public override string GetCommand() {
		return "mkdir";
	}
	protected override void Run() {
		if (Argv.Length == 1) {
			// Read from stdin
			WriteLine(StdOut, GetCommand() + ": missing operand");
		}
		else {
			bool createParents = false;
			string []copy = new string[Argv.Length - 1];
			int j = 0;
			for (int i = 1; i < Argv.Length; i++) {
				if (Argv[i] == "-p") {
					createParents = true;
				}
				else {
					copy[j++] = Argv[i];
				}
			}
			for (int i = 0; i < copy.Length; i++) {
                Debug.Log("Copy arg " + copy[i]);
				if (copy[i].Length == 0) {
					break;
				}
                try {
				    MainSystem.RootDrive.MakeDirectory(new NixPath(copy[i]), createParents);
                }
                catch (Exception exp) {
                    WriteLine(StdOut, "Error creating directory: " + exp.Message);
                    //Debug.Log("Error creating directory: " + exp.Message);
                }
			}
		}
	}
}

