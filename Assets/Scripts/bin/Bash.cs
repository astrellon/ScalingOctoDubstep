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
    public int HistoryPosition {get; private set;}
    public string InputBuffer {get; private set;}

    public Bash(int pid) : base(pid) {
        History = new List<string>();
        InputBuffer = "";
        HistoryPosition = -1;
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
        StdOut.Write(0x1b);
        StdOut.Write("[s");
    }
    protected void AutocompleteInput() {
        
    }
    protected void ProcessKeyboardEvent(Program.KeyboardEvent keyEvent) {
        char c = keyEvent.Character;
        if (c != '\0') {
            if (c == '\r' || c == '\n') {
                StdOut.Write("\n");
                if (InputBuffer.Length > 1 && 
                    (History.Count == 0 || (History.Count > 0 && History[History.Count - 1] != InputBuffer))) {
                    History.Add(InputBuffer);
                }
                HistoryPosition = History.Count;
                Parse(InputBuffer);
                BeginInput();
                InputBuffer = "";
            }
            else {
                InputBuffer += c; 
            }
            WriteInputBuffer();
        }
        // Check for backspace
        else if (keyEvent.KeyCode == 8) {
            if (InputBuffer.Length > 0) {
                InputBuffer = InputBuffer.Remove(InputBuffer.Length - 1);
                WriteInputBuffer();
            }
        }
        else if (keyEvent.KeyCode == 274) {
            Debug.Log("History: " + HistoryPosition + ", " + History.Count);
            if (HistoryPosition >= History.Count - 1) {
                InputBuffer = "";
                HistoryPosition = History.Count - 1;
                Debug.Log("Writing empty buffer");
                WriteInputBuffer();
                return;
            }

            if (HistoryPosition < 0) {
                return;
            }
            InputBuffer = History[++HistoryPosition];
            WriteInputBuffer();
        }
        else if (keyEvent.KeyCode == 273) {
            if (HistoryPosition >= History.Count) {
                HistoryPosition = History.Count - 1;
            }
            if (HistoryPosition < 0) {
                return;
            }
            InputBuffer = History[HistoryPosition--];
            if (HistoryPosition < 0) {
                HistoryPosition = 0;
            }
            WriteInputBuffer();
        }
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
                Program.ProgramEvent progEvent = PopEvent();
                if (progEvent.Message == Program.KeyboardDown) {
                    Program.KeyboardEvent keyEvent = (Program.KeyboardEvent)progEvent;
                    if (keyEvent != null) {
                        ProcessKeyboardEvent(keyEvent);
                    }
                }
            }
            Thread.Sleep(100); 
        }
    }
    protected void WriteInputBuffer() {
        StdOut.Write(0x1b);
        StdOut.Write("[u");
        StdOut.Write(InputBuffer);
        StdOut.Write(0x1b);
        StdOut.Write("[K");
    }
}
