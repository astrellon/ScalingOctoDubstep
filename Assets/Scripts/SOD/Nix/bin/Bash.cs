using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SOD
{
    namespace Nix
    {
        public class Bash : Program
        {

            public List<string> History { get; private set; }
            public int HistoryPosition { get; private set; }
            public string InputBuffer { get; private set; }
            public Regex Parser { get; private set; }

            public Bash(int pid)
                : base(pid)
            {
                History = new List<string>();
                InputBuffer = "";
                HistoryPosition = -1;

                Parser = new Regex("(\\$[a-zA-Z][a-zA-Z0-9]*)|([\\<\\>\\|])|([^\\$\\<\\>\\|]+)");
            }

            public override string GetCommand()
            {
                return "bash";
            }
            public List<string> Parse(string input)
            {
                List<string> result = new List<string>();
                string curr = "";
                foreach (Match m in Parser.Matches(input))
                {
                    if (m.Value.Length == 0)
                    {
                        continue;
                    }
                    char first = m.Value[0];
                    if (first == '<' || first == '>' || first == '|')
                    {
                        curr = curr.Trim();
                        if (curr.Length > 0)
                        {
                            result.Add(curr);
                        }
                        result.Add(first.ToString());
                        curr = "";
                    }
                    else if (first == '$' && m.Value.Length > 1)
                    {
                        curr += MainSession.GetEnvValue(m.Value.Substring(1));
                    }
                    else
                    {
                        curr += m.Value;
                    }
                }
                curr = curr.Trim();
                if (curr.Length > 0)
                {
                    result.Add(curr);
                }
                return result;
            }
            public string ParsePS1(string input)
            {
                List<string> parsed = Parse(input);

                try
                {
                    if (parsed.Count == 0)
                    {
                        return "";
                    }
                    string result = "";
                    Regex r = new Regex(@"(\\\[\\033([^\\\]]+)\\\])|([^\\\[]+)");
                    foreach (Match m in r.Matches(parsed[0]))
                    {
                        if (m.Value.IndexOf(@"\[\033") == 0)
                        {
                            result += "\x1b";
                            result += m.Groups[2];
                        }
                        else
                        {
                            result += m.Value;
                        }
                    }

                    return result;
                }
                catch (Exception exp)
                {
                    Debug.Log("Exception: " + exp.Message);
                }
                return "";
            }
            public void BeginInput()
            {
                string result = ParsePS1(MainSession.GetEnvValue("PS1"));
                StdOut.Write(result);
                // Important!
                StdOut.Write("\x1b[s");
            }
            protected string AutocompleteInput(string input, int cursor)
            {
                string result = "";
                string substr = "";
                int startPos = 0;
                int endPos = input.Length;
                for (int i = cursor; i >= 0; i--)
                {
                    char c = input[i];
                    if (c == ' ' || c == '\t')
                    {
                        startPos = i + 1;
                        break;
                    }
                }
                for (int i = cursor; i < input.Length; i++)
                {
                    char c = input[i];
                    if (c == ' ' || c == '\t')
                    {
                        endPos = i;
                        break;
                    }
                }

                if (startPos >= endPos)
                {
                    return input;
                }

                if (startPos > 0)
                {
                    result = input.Substring(0, startPos);
                }

                string strToComplete = input.Substring(startPos, endPos - startPos);
                if (strToComplete[0] == '$')
                {
                    List<string> matches = CheckEnvVariable(strToComplete, false);
                    if (matches.Count == 1)
                    {
                        strToComplete = matches[0];
                    }
                }
                else
                {
                    List<string> matches = CheckBinProgram(strToComplete, false);
                    if (matches.Count == 1)
                    {
                        strToComplete = matches[0];
                    }
                    else if (matches.Count == 0)
                    {
                        try
                        {
                            matches = CheckFiles(strToComplete, false);
                            if (matches.Count == 1)
                            {
                                strToComplete = matches[0];
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Log("EXCEP: " + ex.Message + "\n" + ex.StackTrace);
                        }
                    }
                }

                result += strToComplete;
                if (endPos < input.Length)
                {
                    result += input.Substring(endPos);
                }

                return result;
            }
            protected List<string> CheckEnvVariable(string input, bool onlyOne)
            {
                List<string> result = new List<string>();
                if (input[0] == '$')
                {
                    string varName = input.Substring(1, input.Length - 2);
                    foreach (KeyValuePair<string, string> pair in MainSession.EnvironmentVariables)
                    {
                        if (pair.Key.IndexOf(varName) == 0)
                        {
                            result.Add("$" + pair.Key);
                            if (onlyOne)
                            {
                                break;
                            }
                        }
                    }
                }
                return result;
            }
            protected List<string> CheckBinProgram(string input, bool onlyOne)
            {
                List<string> result = new List<string>();
                foreach (KeyValuePair<string, Type> pair in MainSystem.BinPrograms)
                {
                    if (pair.Key.IndexOf(input) == 0)
                    {
                        result.Add(pair.Key);
                        if (onlyOne)
                        {
                            break;
                        }
                    }
                }
                return result;
            }
            protected List<string> CheckFiles(string input, bool onlyOne)
            {
                List<string> result = new List<string>();
                if (input == null || input.Length == 0)
                {
                    return result;
                }

                // Path is already correct?
                // Extend to deal with situations where the file/folder already exists
                // but there are other entires that will still match it.
                NixPath full = MainSession.WorkingDirectory.Combine(input);
                if (full.IsRoot() ||
                        MainSystem.RootDrive.IsFile(full) ||
                        MainSystem.RootDrive.IsDirectory(full))
                {
                    return result;
                }
                int index = input.LastIndexOf('/');
                string filename = input;
                string baseinput = "";
                if (index >= 0)
                {
                    filename = input.Substring(index + 1);
                    baseinput = input.Substring(0, index);
                }

                NixPath combined = MainSession.WorkingDirectory.Combine(baseinput);
                if (MainSystem.RootDrive.IsDirectory(combined))
                {
                    FileNode[] files = MainSystem.RootDrive.ListFiles(combined.ToString());
                    if (files != null)
                    {
                        for (int i = 0; i < files.Length; i++)
                        {
                            FileInfo info = files[i].Info;
                            if (info.Name.IndexOf(filename) == 0)
                            {
                                string entry = info.Name;

                                if (baseinput.Length > 0)
                                {
                                    entry = baseinput + "/" + entry;
                                }
                                if (info.Attributes == FileAttributes.Directory)
                                {
                                    result.Add(entry + "/");
                                }
                                else
                                {
                                    result.Add(entry);
                                }
                            }
                        }
                    }
                }
                return result;
            }
            public void Execute(string input)
            {
                List<string> result = Parse(input);
                if (result.Count == 1)
                {
                    MainSystem.Execute(MainSession, result[0]);
                }
                else if (result.Count > 1)
                {
                    string args = "";
                    string operand = null;
                    string args2 = "";
                    args = result[0];
                    if (result.Count >= 3)
                    {
                        operand = result[1];
                        args2 = result[2];
                    }

                    if (args2.Length == 0)
                    {
                        MainSystem.Execute(MainSession, args);
                    }
                    else
                    {
                        if (operand == ">" || operand == "<")
                        {
                            NixPath filePath = new NixPath(args2);
                            string path = MainSystem.RootDrive.GetPathTo(MainSession.WorkingDirectory.Combine(filePath).ToString());
                            using (FileStream fstream = File.Open(path, FileMode.Create))
                            {
                                if (operand == ">")
                                {
                                    MainSystem.Execute(MainSession, args, fstream);
                                }
                                else if (operand == "<")
                                {
                                    MainSystem.Execute(MainSession, args, null, fstream);
                                }
                            }
                        }
                        else if (operand == "|")
                        {
                            NixStream tmp = new NixStream();
                            MainSystem.Execute(MainSession, args, tmp);
                            MainSystem.Execute(MainSession, args2, null, tmp);
                        }
                    }
                }
            }
            protected void ProcessKeyboardEvent(Program.KeyboardEvent keyEvent)
            {
                char c = keyEvent.Character;
                if (c != '\0')
                {
                    if (c == '\r' || c == '\n')
                    {
                        StdOut.Write("\n");
                        if (InputBuffer.Length > 1 &&
                            (History.Count == 0 || (History.Count > 0 && History[History.Count - 1] != InputBuffer)))
                        {
                            History.Add(InputBuffer);
                        }
                        HistoryPosition = History.Count;
                        Execute(InputBuffer);
                        BeginInput();
                        InputBuffer = "";
                    }
                    else if (c == '\t')
                    {
                        InputBuffer = AutocompleteInput(InputBuffer, InputBuffer.Length - 1);
                    }
                    else
                    {
                        InputBuffer += c;
                    }
                    WriteInputBuffer();
                }
                // Check for backspace
                else if (keyEvent.KeyCode == 8)
                {
                    if (InputBuffer.Length > 0)
                    {
                        InputBuffer = InputBuffer.Remove(InputBuffer.Length - 1);
                        WriteInputBuffer();
                    }
                }
                else if (keyEvent.KeyCode == 274)
                {
                    Debug.Log("History: " + HistoryPosition + ", " + History.Count);
                    if (HistoryPosition >= History.Count - 1)
                    {
                        InputBuffer = "";
                        HistoryPosition = History.Count - 1;
                        Debug.Log("Writing empty buffer");
                        WriteInputBuffer();
                        return;
                    }

                    if (HistoryPosition < 0)
                    {
                        return;
                    }
                    InputBuffer = History[++HistoryPosition];
                    WriteInputBuffer();
                }
                else if (keyEvent.KeyCode == 273)
                {
                    if (HistoryPosition >= History.Count)
                    {
                        HistoryPosition = History.Count - 1;
                    }
                    if (HistoryPosition < 0)
                    {
                        return;
                    }
                    InputBuffer = History[HistoryPosition--];
                    if (HistoryPosition < 0)
                    {
                        HistoryPosition = 0;
                    }
                    WriteInputBuffer();
                }
            }
            protected override void Run()
            {
                StdIn.SetEchoStream(false);
                BeginInput();
                while (Running)
                {
                    while (HasEvents())
                    {
                        Program.ProgramEvent progEvent = PopEvent();
                        if (progEvent.Message == Program.KeyboardDown)
                        {
                            Program.KeyboardEvent keyEvent = (Program.KeyboardEvent)progEvent;
                            if (keyEvent != null)
                            {
                                ProcessKeyboardEvent(keyEvent);
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
            }
            protected void WriteInputBuffer()
            {
                StdOut.Write(0x1b);
                StdOut.Write("[u");
                StdOut.Write(InputBuffer);
                StdOut.Write(0x1b);
                StdOut.Write("[K");
            }
        }

    }
}