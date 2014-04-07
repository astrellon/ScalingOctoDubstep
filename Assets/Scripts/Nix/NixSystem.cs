using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using NLua;

public class NixSystem : MonoBehaviour {

    public Session BaseSession {get; set;}
    public Program Shell {get; private set;}
    public Dictionary<string, Type> BinPrograms {get; set;}
	public FileSystem RootDrive {get; set;}
    public Dictionary<int, Program> ActivePrograms {get; private set;}
    public int PidCounter {get; protected set;}
	public Lua.LuaOptions BaseLuaOptions {get; protected set;}

    public int NewPid() {
        return ++PidCounter;
    }
	public void AddProgram(string cmd, Type type) {
		BinPrograms[cmd] = type; 
	}

	public void Start() {
		RootDrive = new FileSystem();
		RootDrive.RootFolder = Path.Combine(Directory.GetCurrentDirectory(), "root");
		Directory.CreateDirectory(RootDrive.RootFolder);
        ActivePrograms = new Dictionary<int, Program>();
	
		BinPrograms = new Dictionary<string, Type>();
		AddProgram("echo", typeof(Echo));
		AddProgram("pwd", typeof(Pwd));
		AddProgram("cd", typeof(Cd));
		AddProgram("ls", typeof(Ls));
		AddProgram("clear", typeof(Clear));
		AddProgram("lua", typeof(RunLua));

        BaseSession = new Session();
        Shell = new Bash(NewPid());
        BaseSession.Shell = Shell;

		Terminal term = GetComponent<Terminal>();
		if (term != null) {
    	    term.Shell = Shell;
            term.CurrentSession = BaseSession;
		}
/*
		BaseLuaOptions = new Lua.LuaOptions();
		BaseLuaOptions.StdOut = Shell.StdOut;
		BaseLuaOptions.StdIn = Shell.StdIn;
		BaseLuaOptions.StdErr = Shell.StdErr;
		BaseLuaOptions.RootFolder = RootDrive.RootFolder;
*/
		/*
		Lua l = new Lua();
		Lua.LuaOptions opts = new Lua.LuaOptions();
		opts.StdOut = Shell.StdOut.InternalStream;
		opts.RootFolder = @"C:\git\ScalingOctoDubstep\root\";
		l.SetOptions(opts);
		l.DoString(@"f = io.open('test.out', 'r')
                print(f:read('*all'))
				print('\n')
                a='hello'");
		string a = l.GetString("a");
		Debug.Log("A: " + a);
		*/

		BeginBoot();
	}

	public bool BeginBoot() {
		Shell.StdOut.Write("Booting...\n");
		Shell.StdOut.Write("Booting Complete...\n");
        
        Shell.ExecuteAsync(this, BaseSession, new string[]{""});
		return true;
	}

	public void Execute(Session session, string input) {
        Debug.Log("Execute: >" + input + "<");
        if (input.Length == 0) {
            return;
        }
		Regex regex = new Regex("(\".*\")|([^ \\t\\n\\r]+)");

		MatchCollection matches = regex.Matches(input);
		string []args = new string[matches.Count - 1];
		string binName = "";
		int i = 0;
		foreach (Match match in regex.Matches(input)) {
			if (binName.Length == 0) {
				binName = match.Value;
			}
			else {
				if (match.Value[0] == '"') {
					args[i++] = match.Value.Substring(1, match.Value.Length - 2);
				}
				else {
					args[i++] = match.Value;
				}
			}
		}

		Program prog = null;
		if (BinPrograms.ContainsKey(binName)) {
            Debug.Log("Attempting to create program: " + binName);
            prog = (Program)Activator.CreateInstance(BinPrograms[binName], NewPid());
			if (prog == null) {
				Shell.StdOut.Write("Unable to create " + binName + " program.\n");
			}
		}
		else {
			prog = new RunLua(NewPid ());
		}
		if (prog == null) {
			Shell.StdOut.Write("Unable to find command: " + binName);
			Shell.StdOut.Write("\n");
		}
		else {
			prog.StdOut = Shell.StdOut;
			session.PushForegroundProgram(prog);
			Debug.Log("Attempting to run program: " + binName);
			prog.Execute(this, session, args);
			session.PopForegroundProgram();
			Debug.Log("Fin " + binName);
		}
	}
}
