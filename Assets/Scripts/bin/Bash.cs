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
    public Regex Parser {get; private set;}

    public Bash(int pid) : base(pid) {
        History = new List<string>();
        InputBuffer = "";
        HistoryPosition = -1;

        Parser = new Regex("(\\$[a-zA-Z][a-zA-Z0-9]*)|([^\\$]+)"); 
    }

    public override string GetCommand() {
        return "bash";
    }
    public void Parse(string input) {
		string result = "";
		foreach (Match m in Parser.Matches(input)) {
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
    protected string AutocompleteInput(string input, int cursor) {
        string result = "";
        string substr = "";
        int startPos = 0;
        int endPos = input.Length;
        for (int i = cursor; i >= 0; i--) {
            char c = input[i];
            if (c == ' ' || c == '\t') {
                startPos = i + 1;
                break;
            }
        }
        for (int i = cursor; i < input.Length; i++) {
            char c = input[i];
            if (c == ' ' || c == '\t') {
                endPos = i;
                break;
            }
        }

        if (startPos >= endPos) {
            return input;
        }

        if (startPos > 0) {
            result = input.Substring(0, startPos);
            Debug.Log("Starting result at: >" + result + "<");
        }

        Debug.Log("Start/End: " + startPos + ", " + endPos);
        string strToComplete = input.Substring(startPos, endPos - startPos);
        Debug.Log("Str to complete: >" + strToComplete + "<");
        if (strToComplete[0] == '$') {
            List<string> matches = CheckEnvVariable(strToComplete, false);
            if (matches.Count == 1) {
                strToComplete = matches[0];
            }
        }
        else {
            List<string> matches = CheckBinProgram(strToComplete, false);
            if (matches.Count == 1) {
                strToComplete = matches[0];
            }
        }

        result += strToComplete;
        if (endPos < input.Length) {
            result += input.Substring(endPos);
        }

        return result;
    }
    protected List<string> CheckEnvVariable(string input, bool onlyOne) {
        List<string> result = new List<string>();
        if (input[0] == '$') {
            string varName = input.Substring(1, input.Length - 2);
            foreach (KeyValuePair<string, string> pair in MainSession.EnvironmentVariables) {
                if (pair.Key.IndexOf(varName) == 0) {
                    result.Add("$" + pair.Key);
                    if (onlyOne) {
                        break;
                    }
                }
            }
        }
        return result;
    }
    protected List<string> CheckBinProgram(string input, bool onlyOne) {
        List<string> result = new List<string>();
        foreach (KeyValuePair<string, Type> pair in MainSystem.BinPrograms) {
            if (pair.Key.IndexOf(input) == 0) {
                result.Add(pair.Key);
                if (onlyOne) {
                    break;
                }
            }
        }
        return result;
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
            else if (c == '\t') {
                InputBuffer = AutocompleteInput(InputBuffer, InputBuffer.Length - 1);
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
        try {
        Debug.Log("Auto complete: " + AutocompleteInput("ec $HO", 1));
        }
        catch (Exception exp) {
            Debug.Log("Exception: " + exp.Message + "\n" + exp.StackTrace);
        }
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
