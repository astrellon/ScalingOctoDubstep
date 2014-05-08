using UnityEngine;
using System;
using System.IO;
using System.Collections;
using NLua;

namespace SOD
{
    namespace Nix
    {
        namespace Bin
        {
            public class RunLua : Program
            {

                public RunLua(int pid)
                    : base(pid)
                {

                }
                public override string GetCommand()
                {
                    return "lua";
                }
                protected override void Run()
                {
                    LuaSystem luaSys = new LuaSystem(MainSession, MainSystem, StdOut, StdIn, StdErr);
                    Lua l = luaSys.Lua;

                    try
                    {
                        if (Argv.Count > 1)
                        {
                            NixPath newPath = OpenPath(Argv[1]);
                            string file = MainSystem.RootDrive.GetPathTo(newPath.ToString());
                            Debug.Log("File to load: " + newPath.ToString());
                            if (File.Exists(file))
                            {
                                string argStr = "arg={}\n";
								argStr += "arg[0]=\"" + Argv[1].Replace ("\\", "/") + "\"\n";
                                for (int i = 2; i < Argv.Count; i++)
                                {
                                    argStr += "arg[" + (i - 1) + "]=\"" + Argv[i] + "\"\n";
                                }
                                l.DoString(argStr);
                                l.DoFile(newPath.ToString());
                            }
                            else
                            {
                                StdOut.WriteLine("Unable to find file: " + Argv[1]);
                            }
                        }
                        else
                        {
                            // Do stdin stuff
                        }
                    }
                    catch (Exception exp)
                    {
                        StdOut.WriteLine("Exception executing Lua: " + exp.Message);
                    }
                }
            }
        }
    }
}
