using UnityEngine;
using System.Collections;
using NLua;

public class RunLua : Program {

	public RunLua(int pid) : base(pid) {
		
	}
	public override string GetCommand() {
		return "lua";
	}
	protected override void Run() {
		/*StdOut.Write("Enter your name: ");
		string name = "";
		StdIn.Read (ref name);
		StdOut.Write ("Welcome " + name);*/
		Lua l = new Lua();
		Lua.LuaOptions opts = new Lua.LuaOptions();
		opts.StdOut = MainSession.Shell.StdOut;
		opts.StdIn = MainSession.Shell.StdIn;
		opts.StdErr = MainSession.Shell.StdErr;
		opts.RootFolder = MainSystem.RootDrive.RootFolder;
		l.SetOptions(opts);

		l.DoString(@"print('hello\n')
			s = io.read('*l')
			print('how are you? '..s)
		");
	}
}
