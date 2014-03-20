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
    public string Parse(string input) {
		string result = "";
		foreach (Match m in Parser.Matches(input)) {
			if (m.Value[0] == '$' && m.Value.Length > 1) {
                result += MainSession.GetEnvValue(m.Value.Substring(1));
			}
			else {
				result += m.Value;
			}
		}
		return result;
    }
    public void BeginInput() {
        //StdOut.Write(MainSession.WorkingDirectory.ToString());
		StdOut.Write(0x1b);
		StdOut.Write ("[0;32m");
		string parsed = Parse ("$USER@$HOSTNAME:$PWD$");
		StdOut.Write (parsed);
		StdOut.Write(0x1b);
		StdOut.Write ("[0m");
        StdOut.Write("> ");
        StdOut.Write(0x1b);
        StdOut.Write("[s");
		StdOut.Write(0x1b);
		StdOut.Write("[37;m");
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
        }

        string strToComplete = input.Substring(startPos, endPos - startPos);
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
            else if (matches.Count == 0) {
                try {
                    matches = CheckFiles(strToComplete, false);
                    if (matches.Count == 1) {
                        strToComplete = matches[0];
                    }
                }
                catch (Exception ex) {
                    Debug.Log("EXCEP: " + ex.Message + "\n" + ex.StackTrace);
                }
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
    protected List<string> CheckFiles(string input, bool onlyOne) {
        List<string> result = new List<string>();
        if (input == null || input.Length == 0) {
            return result;
        }

        // Path is already correct?
        // Extend to deal with situations where the file/folder already exists
        // but there are other entires that will still match it.
        NixPath full = MainSession.WorkingDirectory.Combine(input);
        if (full.IsRoot() || 
            MainSystem.RootDrive.IsFile(full) ||
            MainSystem.RootDrive.IsDirectory(full)) {
            return result;
        }
        int index = input.LastIndexOf('/');
        string filename = input;
        string baseinput = "";
        if (index >= 0) {
            filename = input.Substring(index + 1);
            baseinput = input.Substring(0, index);
        }

        NixPath combined = MainSession.WorkingDirectory.Combine(baseinput);
        if (MainSystem.RootDrive.IsDirectory(combined)) {
            FileNode []files = MainSystem.RootDrive.ListFiles(combined.ToString());
            if (files != null) {
                for (int i = 0; i < files.Length; i++) {
                    FileInfo info = files[i].Info;
                    if (info.Name.IndexOf(filename) == 0) {
                        string entry = info.Name;
                        
                        if (baseinput.Length > 0) {
                            entry = baseinput + "/" + entry;
                        }
                        if (info.Attributes == FileAttributes.Directory) {
                            result.Add(entry + "/");            
                        }
                        else {
                            result.Add(entry);
                        }
                    }
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
                string result = Parse(InputBuffer);
				MainSystem.Execute(MainSession, result);
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
