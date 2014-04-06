using UnityEngine;
using System;
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
		/*StdOut.Write("Enter your name: ");
		string name = "";
		StdIn.Read (ref name);
		StdOut.Write ("Welcome " + name);*/

		Lua.LuaOptions opts = new Lua.LuaOptions();
		opts.StdOut = StdOut;
		opts.StdIn = StdIn;
		//string input = "";
		//MainSystem.Shell.StdIn.Read (ref input);
		opts.StdErr = MainSession.Shell.StdErr;
		opts.RootFolder = MainSystem.RootDrive.RootFolder;
		opts.ExecuteHandler = ExecuteHandler;
		Lua l = new Lua(opts);

		try
		{
			l.DoString(@"
				print('hello: ')
				s = io.read('*l')
				print('how are you? '..s..'\nAnd how old are you? ')
				s = io.read('*l')
				print('A whole '..s..' eh\n')
			");
		}
		catch (Exception exp)
		{
			MainSession.Shell.StdOut.Write ("Exception executing Lua: " + exp.Message + "\n");
		}
	}
}
