using UnityEngine;
using System;
using System.Collections.Generic;

namespace SOD
{
    namespace Nix
    {
        public class Session
        {
            public static Session BaseSession = null;
            public static System.IO.Stream BaseStdOut
            {
                get
                {
                    return BaseSession.Shell.StdOut;
                }
            }

            public string User { get; set; }
            public NixPath WorkingDirectory { get; private set; }
            public NixPath PhysicalDirectory {get; private set; }
            public Bin.Program ForegroundProgram { get; set; }
            public Dictionary<string, string> EnvironmentVariables { get; set; }
            public NixSystem MainSystem {get; set;}

            private Bin.Bash _Shell;
            public Bin.Bash Shell
            {
                get
                {
                    return _Shell;
                }
                set
                {
                    _Shell = value;
                    PushForegroundProgram(value);
                }
            }

            public string InputBuffer { get; protected set; }
            public Stack<Bin.Program> ActiveStack { get; private set; }

            public Session()
            {
                BaseSession = this;

                WorkingDirectory = new NixPath();
                WorkingDirectory.Absolute = true;
                PhysicalDirectory = new NixPath();
                PhysicalDirectory.Absolute = true;

                ActiveStack = new Stack<Bin.Program>();
                EnvironmentVariables = new Dictionary<string, string>();
                EnvironmentVariables["HOSTNAME"] = "engineering_comp";
                EnvironmentVariables["USER"] = "astrellon";
                EnvironmentVariables["HOSTTYPE"] = "spaceship";
                //EnvironmentVariables["PWD"] = WorkingDirectory.ToString();
                EnvironmentVariables["PWD"] = "/";
                EnvironmentVariables["PS1"] = @"\[\033[1;32m\]$USER@$HOSTNAME\[\033[0m\]:\[\033[1;34m\]$PWD\[\033[0m\]- ";
                EnvironmentVariables["PATH"] = @"/usr/bin";
                EnvironmentVariables["TMPDIR"] = @"/tmp";

            }

            public void SetWorkingDirectory(NixPath path)
            {
                WorkingDirectory = path;
                EnvironmentVariables["PWD"] = path.ToString();
                PhysicalDirectory = MainSystem.RootDrive.FollowLinks(path); 
            }

            public void PushForegroundProgram(Bin.Program program)
            {
                if (ForegroundProgram != null)
                {
                    ActiveStack.Push(ForegroundProgram);
                }
                ForegroundProgram = program;
            }
            public void PopForegroundProgram()
            {
                if (ActiveStack.Count > 0)
                {
                    ForegroundProgram = ActiveStack.Pop();
                }
            }
            public bool EchoInput()
            {
                if (ForegroundProgram != null)
                {
                    NixStream input = ForegroundProgram.StdIn as NixStream;
                    if (input != null)
                    {
                        return input.EchoStream;
                    }
                }
                return true;
            }
            public string GetEnvValue(string key, string defaultValue = "")
            {
                if (EnvironmentVariables.ContainsKey(key))
                {
                    return EnvironmentVariables[key];
                }
                return defaultValue;
            }

            public void KeyboardEvent(Event e)
            {
                if (e.type == EventType.KeyDown)
                {
                    if (e.character == '\r' || e.character == '\n')
                    {
                        string temp = InputBuffer + e.character;
                        InputBuffer = "";
                        if (ForegroundProgram != null)
                        {
                            if (EchoInput())
                            {
                                Debug.Log("Echoing input: " + temp);
                                ForegroundProgram.StdOut.Write(temp);
                            }
                            ForegroundProgram.StdIn.Write(temp);
                        }
                    }
                    else if (e.character != '\0')
                    {
                        InputBuffer += e.character;
                    }
                    if (ForegroundProgram != null)
                    {
                        ForegroundProgram.PushEvent(new Bin.Program.KeyboardEvent(Bin.Program.KeyboardDown, e.character, (int)e.keyCode));
                    }
                }
                else if (e.type == EventType.KeyUp)
                {
                    if (ForegroundProgram != null)
                    {
                        ForegroundProgram.PushEvent(new Bin.Program.KeyboardEvent(Bin.Program.KeyboardUp, e.character, (int)e.keyCode));
                    }
                }
            }
        }
    }
}
