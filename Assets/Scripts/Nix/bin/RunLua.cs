using UnityEngine;
using System;
using System.IO;
using System.Collections;
using NLua;

public class RunLua : Program {

	public RunLua(int pid) : base(pid) {
		
	}
	public override string GetCommand() {
		return "lua";
	}
	public int ExecuteHandler(string line) {
		MainSystem.Execute(MainSession, line);
		return 0;
	}
	protected override void Run() {
		Lua.LuaOptions opts = new Lua.LuaOptions();
		opts.StdOut = StdOut;
		opts.StdIn = StdIn;
		opts.StdErr = StdErr;
		opts.RootFolder = MainSystem.RootDrive.RootFolder + "\\";
		opts.ExecuteHandler = ExecuteHandler;
		Lua l = new Lua(opts);
		l.SetWorkingDirectory(MainSession.WorkingDirectory.ToString());
		try
		{
			if (Argv.Length > 0) {
				//string file = MainSystem.RootDrive.GetPathTo(Argv[0]);
                NixPath newPath = MainSession.WorkingDirectory.Combine(Argv[0]);
                string file = MainSystem.RootDrive.GetPathTo(newPath.ToString());
				Debug.Log ("File to load: " + file);
				if (File.Exists(file)) {
					string argStr = @"arg={}";
					//argStr += "arg[0]=\"" + Argv[0] + "\"\n";
					for (int i = 0; i < Argv.Length; i++) {
						argStr += "arg[" + i + "]=\"" + Argv[i] + "\"\n";
					}
					l.DoString(argStr);
					//NixPath newPath = MainSession.WorkingDirectory.Combine(Argv[0]);
					l.DoFile(newPath.ToString());
				}
				else {
					MainSession.Shell.StdOut.Write ("Unable to find file: " + Argv[0] + "\n");
				}
			}
			else {
				// Do stdin stuff
			}
		}
		catch (Exception exp)
		{
			MainSession.Shell.StdOut.Write ("Exception executing Lua: " + exp.Message + "\n");
		}
	}
}
