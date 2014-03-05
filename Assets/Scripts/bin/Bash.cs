using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

public class Bash : Program {
    
    public List<string> History {get; private set;}
    public string InputBuffer {get; private set;}

    public Bash(int pid) : base(pid) {
        History = new List<string>();
        InputBuffer = "";
    }

    public override string GetCommand() {
        return "bash";
    }
    public void Parse(string input) {
		Regex r = new Regex("(\\$[a-zA-Z][a-zA-Z0-9]*)|([^\\$]+)");
		string result = "";
		foreach (Match m in r.Matches(input)) {
			if (m.Value[0] == '$') {
                result += MainSession.GetEnvValue(m.Value.Substring(1));
			}
			else {
				result += m.Value;
			}
		}
        MainSystem.Execute(MainSession, result);
    }
    public void BeginInput() {
        StdOut.Write(MainSession.WorkingDirectory.ToString());
        StdOut.Write("> ");
    }
    protected override void Run() {
        StdIn.EchoStream = false;
        /*string path = MainSystem.RootDrive.GetPathTo("test.out"); 
        Debug.Log("Path: " + path);
        using (FileStream file = System.IO.File.Create(path)) {
            
            NixStream stream = new NixStream(file);
            string input = "";
            stream.Read(ref input);
            Debug.Log("Read from file: " + input);
        }
        */

        BeginInput();
        while (Running) {
            while (HasEvents()) {
                ProgramEvent e = PopEvent();
                if (e.Message == Program.KeyboardDown) {
                    Event unityEvent = (Event)e.Param;
                    char c = unityEvent.character;
                    if (c != '\0') {
                        if (c == '\r' || c == '\n') {
                            StdOut.Write("\n");
                            Parse(InputBuffer);
                            BeginInput();
                            InputBuffer = "";
                        }
                        else {
                            if (c == '\b') {
                                InputBuffer = InputBuffer.Remove(InputBuffer.Length - 1);
                            }
                            else {
                                InputBuffer += c; 
                            }
                            StdOut.Write(0x1b);
                            StdOut.Write("[2K");
                            StdOut.Write(InputBuffer);
                        }
                    }
                }
            }
            Thread.Sleep(100); 
        }
    }
}
