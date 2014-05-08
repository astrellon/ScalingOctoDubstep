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
            public struct FindCommandResult
            {
                public string Path;
                public bool Builtin;
                public bool Found;
            }
            public struct ProgramRunType
            {
                public string Shebang;
                public bool BinaryCode;
            }

            public Session BaseSession { get; set; }
            public Bin.Bash Shell { get; private set; }
            public Dictionary<string, Type> BinPrograms { get; set; }
            public FileSystem.FileSystem RootDrive { get; set; }
            public Dictionary<int, Bin.Program> ActivePrograms { get; private set; }
            public int PidCounter { get; protected set; }
            public Lua.LuaOptions BaseLuaOptions { get; protected set; }
            public Device.DeviceManager MainDeviceManager { get; protected set; }

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
                BeginBoot();
            }

            public bool BeginBoot()
            {
                BaseSession = new Session();
                Session.BaseSession = BaseSession;

                Shell = new Bin.Bash(NewPid());
                BaseSession.Shell = Shell;
                BaseSession.MainSystem = this;
                Shell.StdOut.WriteLine("Booting...");

                MainDeviceManager = new Device.DeviceManager(this);

                RootDrive = new FileSystem.FileSystem(this);
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
                AddProgram("ln", typeof(Bin.Ln));
                AddProgram("mkdevice", typeof(Bin.MakeDevice));

                RootDrive.MakeDirectory(new NixPath("/dev"), true);
                Device.CharacterDevice device = new Device.NullDevice();
                MainDeviceManager.AddDevice(device);
                RootDrive.MakeCharacterDevice(new NixPath("/dev/null"), device.Id);

                device = new Device.ZeroDevice();
                MainDeviceManager.AddDevice(device);
                RootDrive.MakeCharacterDevice(new NixPath("/dev/zero"), device.Id);

                device = new Device.TestDevice(); 
                MainDeviceManager.AddDevice(device);
                RootDrive.MakeCharacterDevice(new NixPath("/dev/test"), device.Id);

                Terminal term = GetComponent<Terminal>();
                if (term != null)
                {
                    term.Shell = Shell;
                    term.CurrentSession = BaseSession;
                }

                Shell.StdOut.WriteLine("Booting Complete...");

                Shell.ExecuteAsync(this, BaseSession, new string[] { "" });

                return true;
            }

            public FindCommandResult FindCommand(string cmd)
            {
                Debug.Log("Searching for: " + cmd);
                FindCommandResult result = new FindCommandResult();
                string []envPathSplit = BaseSession.GetEnvValue("PATH").Split(new char[]{':'});

                for (int i = 0; i < envPathSplit.Length; i++) {
                    NixPath path = new NixPath(envPathSplit[i]);
                    path.AppendPath(cmd);
                    if (RootDrive.IsFile(path)) {
                        result.Path = path.ToString();
                        result.Builtin = false;
                        result.Found = true;
                        return result;
                    }
                }

                if (BinPrograms.ContainsKey(cmd)) {
                    result.Path = cmd;
                    result.Builtin = true;
                    result.Found = true;
                    return result;
                }

                result.Found = false;
                return result;
            }
            public ProgramRunType GetProgramRunType(string cmd)
            {
                NixPath path = new NixPath(cmd);

                ProgramRunType result = new ProgramRunType();
                string pathTo = RootDrive.GetPathTo(RootDrive.FollowLinks(path));
                Debug.Log("GPRT: " + path.ToString() + " | " + pathTo);
                using (StreamReader reader = new StreamReader(pathTo))
                {
                    string firstLine = reader.ReadLine();
                    if (firstLine.Length > 2)
                    {
                        if (firstLine[0] == '#' && firstLine[1] == '!')
                        {
                            Debug.Log("Has a shebang! " + firstLine);
                            result.BinaryCode = false;
                            result.Shebang = firstLine.Substring(2);
                        }
                        // Definitly not a good way of determining if it is a DLL
                        // Need to look further into the file to determine if it really is.
                        else if (firstLine[0] == 'M' && firstLine[1] == 'Z')
                        {
                            Debug.Log("Possibly a DLL");
                            result.BinaryCode = true;
                        }
                    }
                }
                return result;
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

                FindCommandResult cmdLookup = FindCommand(binName);
                if (!cmdLookup.Found) 
                {
                    Shell.StdOut.WriteLine("Unable to find command: " + binName);
                    return;
                }
                args[0] = cmdLookup.Path;

                Bin.Program prog = null;
                if (cmdLookup.Builtin)
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
                    ProgramRunType programRunType = GetProgramRunType(cmdLookup.Path);
                    Debug.Log("Program type: " + programRunType.Shebang);
                    while (programRunType.Shebang.Length > 0) 
                    {
                        cmdLookup = FindCommand(programRunType.Shebang);
                        if (!cmdLookup.Found)
                        {
                            Shell.StdOut.WriteLine("Unable to execute command: " + programRunType.Shebang + " for " + binName);
                            return;
                        }
                        args.Insert(0, cmdLookup.Path);
                        if (cmdLookup.Builtin)
                        {
                            binName = cmdLookup.Path;
                            prog = (Bin.Program)Activator.CreateInstance(BinPrograms[binName], NewPid());
                            programRunType.Shebang = "";
                        }
                        else
                        {
                            programRunType = GetProgramRunType(cmdLookup.Path);
                        }
                    }
                    if (programRunType.BinaryCode) 
                    {
                        // Do something about loading up a DLL
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
