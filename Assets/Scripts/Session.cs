using UnityEngine;
using System;
using System.Collections.Generic;

public class Session {

    public string User {get; set;}
    public NixPath WorkingDirectory {get; set;}
    public Program ForegroundProgram {get; set;}
    public Dictionary<string, string> EnvironmentVariables {get; set;}

    private Program _Shell;
    public Program Shell {
        get {
            return _Shell;
        }
        set {
            _Shell = value;
            PushForegroundProgram(value);
        }
    }

    public string InputBuffer {get; protected set;} 
    public Stack<Program> ActiveStack {get; private set;}

    public Session() {
        WorkingDirectory = new NixPath();
        WorkingDirectory.Absolute = true;
        ActiveStack = new Stack<Program>();
        EnvironmentVariables = new Dictionary<string, string>();
        EnvironmentVariables["HOSTNAME"] = "unknown_host";
        EnvironmentVariables["USER"] = "alan";
        EnvironmentVariables["HOSTTYPE"] = "magic";
		EnvironmentVariables["PWD"] = WorkingDirectory.ToString();
        EnvironmentVariables["PS1"] = @"\[\033[1;32m\]$USER@$HOSTNAME\[\033[0m\]:\[\033[1;34m\]$PWD\[\033[0m\]- ";
    }

	public void SetWorkingDirectory(NixPath path) {
		WorkingDirectory = path;
		EnvironmentVariables ["PWD"] = path.ToString();
	}

    public void PushForegroundProgram(Program program) {
        if (ForegroundProgram != null) {
            ActiveStack.Push(ForegroundProgram);
        }
        ForegroundProgram = program;
    }
    public void PopForegroundProgram() {
        if (ActiveStack.Count > 0) {
            ForegroundProgram = ActiveStack.Pop();
        }
    }
    public bool EchoInput() {
        if (ForegroundProgram != null) {
            return ForegroundProgram.StdIn.EchoStream;
        }
        return true;
    }
    public string GetEnvValue(string key, string defaultValue = "") {
        if (EnvironmentVariables.ContainsKey(key)) {
            return EnvironmentVariables[key];
        }
        return defaultValue;
    }

    public void KeyboardEvent(Event e) {
        if (e.type == EventType.KeyDown) {
            if (e.character == '\r' || e.character == '\n') {
                if (ForegroundProgram != null) {
                    ForegroundProgram.StdIn.Write((byte)e.character);
                }
                InputBuffer = "";
            }
            else if (e.character != '\0') {
                InputBuffer += e.character;
            }
            if (ForegroundProgram != null) {
                ForegroundProgram.PushEvent(new Program.KeyboardEvent(Program.KeyboardDown, e.character, (int)e.keyCode));
            }
        }
        else if (e.type == EventType.KeyUp) {
            if (ForegroundProgram != null) {
                ForegroundProgram.PushEvent(new Program.KeyboardEvent(Program.KeyboardUp, e.character, (int)e.keyCode));
            }
        }
    }

}
