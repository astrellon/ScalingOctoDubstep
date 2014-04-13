using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using NLua;

namespace SOD
{
    namespace Nix
    {
        public class NixSystem : MonoBehaviour
        {

            public Session BaseSession { get; set; }
            public Bin.Bash Shell { get; private set; }
            public Dictionary<string, Type> BinPrograms { get; set; }
            public FileSystem.FileSystem RootDrive { get; set; }
            public Dictionary<int, Bin.Program> ActivePrograms { get; private set; }
            public int PidCounter { get; protected set; }
            public Lua.LuaOptions BaseLuaOptions { get; protected set; }

            public int NewPid()
            {
                return ++PidCounter;
            }
            public void AddProgram(string cmd, Type type)
            {
                BinPrograms[cmd] = type;
            }

            public void Start()
            {
                RootDrive = new FileSystem.FileSystem();
                RootDrive.RootFolder = Path.Combine(Directory.GetCurrentDirectory(), "root");
                Directory.CreateDirectory(RootDrive.RootFolder);
                ActivePrograms = new Dictionary<int, Bin.Program>();

                BinPrograms = new Dictionary<string, Type>();
                AddProgram("echo", typeof(Bin.Echo));
                AddProgram("pwd", typeof(Bin.Pwd));
                AddProgram("cd", typeof(Bin.Cd));
                AddProgram("cp", typeof(Bin.Cp));
                AddProgram("ls", typeof(Bin.Ls));
                AddProgram("clear", typeof(Bin.Clear));
                AddProgram("lua", typeof(Bin.RunLua));
                AddProgram("cat", typeof(Bin.Cat));
                AddProgram("mkdir", typeof(Bin.Mkdir));
                AddProgram("mv", typeof(Bin.Mv));
                AddProgram("rm", typeof(Bin.Rm));
                BaseSession = new Session();
                Shell = new Bin.Bash(NewPid());
                BaseSession.Shell = Shell;

                Terminal term = GetComponent<Terminal>();
                if (term != null)
                {
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

            public bool BeginBoot()
            {
                Shell.StdOut.WriteLine("Booting...");
                Shell.StdOut.WriteLine("Booting Complete...");

                Shell.ExecuteAsync(this, BaseSession, new string[] { "" });
                return true;
            }

            public void Execute(Session session, string input)
            {
                Execute(session, input, null, null);
            }
            public void Execute(Session session, string input, Stream StdOut, Stream StdIn)
            {
                Debug.Log("Execute: >" + input + "<");
                if (input.Length == 0)
                {
                    return;
                }
                /*if (input == "test")
                {
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFile(@"F:\git\ScalingOctoDubstep\root\usr\bin\Test");

                    string type = "Test";
                    try
                    {
                        Type[] types = assembly.GetTypes();
                        object o = Activator.CreateInstance(types[0], NewPid());
                        Nix.Bin.Program proga = (Nix.Bin.Program)o;
                        proga.StdOut = Shell.StdOut;
                        proga.Execute(this, BaseSession, new string[] { "Test" });
                    }
                    catch (Exception exp)
                    {
                        Debug.Log("QWEASD: " + exp.Message);
                    }
                    return;
                }*/
                Regex regex = new Regex("(\".*\")|([^ \\t\\n\\r]+)");

                MatchCollection matches = regex.Matches(input);
                //string[] args = new string[matches.Count];
                List<string> args = new List<string>(matches.Count + 1);
                int i = 0;
                foreach (Match match in regex.Matches(input))
                {
                    if (match.Value[0] == '"')
                    {
                        args.Add(match.Value.Substring(1, match.Value.Length - 2));
                    }
                    else
                    {
                        args.Add(match.Value);
                    }
                }

                string binName = args[0];

                Bin.Program prog = null;
                if (BinPrograms.ContainsKey(binName))
                {
                    Debug.Log("Attempting to create program: " + binName);
                    prog = (Bin.Program)Activator.CreateInstance(BinPrograms[binName], NewPid());
                    if (prog == null)
                    {
                        Shell.StdOut.WriteLine("Unable to create " + binName + " program");
                    }
                }
                else
                {
                    NixPath binPath = new NixPath("/usr/bin/" + binName);
                    if (RootDrive.IsFile(binPath))
                    {
                        using (StreamReader reader = new StreamReader(RootDrive.GetPathTo(binPath.ToString())))
                        {
                            string firstLine = reader.ReadLine();
                            if (firstLine.Length > 2)
                            {
                                if (firstLine[0] == '#' && firstLine[1] == '!')
                                {
                                    Debug.Log("Has a shebang! " + firstLine);
                                }
                            }
                        }
                        args.Insert(0, "lua");
                        prog = new Bin.RunLua(NewPid(), "/usr/bin/" + binName);
                    }
                }
                if (prog == null)
                {
                    Shell.StdOut.WriteLine("Unable to find command: " + binName);
                }
                else
                {
                    if (StdOut == null)
                    {
                        prog.StdOut = Shell.StdOut;
                    }
                    else
                    {
                        prog.StdOut = StdOut;
                    }

                    if (StdIn != null)
                    {
                        prog.StdIn = StdIn;
                    }
                    session.PushForegroundProgram(prog);
                    Debug.Log("Attempting to run program: " + binName);
                    String argsStr = "";
                    for (int j = 0; j < args.Count; j++)
                    {
                        if (j > 0)
                        {
                            argsStr += ", ";
                        }
                        argsStr += args[j];
                    }
                    Debug.Log("- With args: " + argsStr);
                    prog.Execute(this, session, args);
                    session.PopForegroundProgram();
                    Debug.Log("Fin " + binName);
                }
            }
        }
    }
}
