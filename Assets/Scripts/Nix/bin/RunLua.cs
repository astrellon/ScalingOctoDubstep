using UnityEngine;
using System;
using System.IO;
using System.Collections;
using NLua;

public class RunLua : Program {

	private string PathToLua = "";
	public RunLua(int pid) : base(pid) {
		
	}
	public RunLua(int pid, string pathToLua) : base(pid) {
		PathToLua = pathToLua;
	}
	public override string GetCommand() {
		return "lua";
	}
	public int ExecuteHandler(string line) {
		MainSession.Shell.Execute(line);
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
			if (Argv.Count > 1 || PathToLua.Length > 0) {
				//string file = MainSystem.RootDrive.GetPathTo(Argv[0]);
				NixPath newPath;
				if (PathToLua.Length > 0) {
					newPath = new NixPath(PathToLua);
				}
				else {
                	newPath = MainSession.WorkingDirectory.Combine(Argv[1]);
				}
                string file = MainSystem.RootDrive.GetPathTo(newPath.ToString());
				Debug.Log ("File to load: " + file);
				if (File.Exists(file)) {
					string argStr = @"arg={}";
					//argStr += "arg[0]=\"" + Argv[0] + "\"\n";
					for (int i = 0; i < Argv.Count; i++) {
						argStr += "arg[" + i + "]=\"" + Argv[i] + "\"\n";
					}
					l.DoString(argStr);
					l.DoFile(newPath.ToString());
				}
				else {
					StdOut.WriteLine("Unable to find file: " + Argv[0]);
				}
			}
			else {
				// Do stdin stuff
			}
		}
		catch (Exception exp)
		{
			StdOut.WriteLine("Exception executing Lua: " + exp.Message);
		}
	}
}
