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
                public int ExecuteHandler(string line)
                {
                    MainSession.Shell.Execute(line);
                    return 0;
                }
                public Stream OpenFileHandler(string filename, FileMode mode, FileAccess access)
                {
                    NixPath path = OpenPath(filename);
                    Debug.Log("Attempting to opening Lua file: " + path.ToString());

                    //return new FileStream(MainSystem.RootDrive.GetPathTo(path), mode, access);
                    return MainSystem.RootDrive.OpenFile(path, access, mode);
                }
                public int RemoveFileHandler(string filename)
                {
                    NixPath path = MainSession.PhysicalDirectory.Combine(new NixPath(filename));
                    try
                    {
                        if (MainSystem.RootDrive.IsDirectory(path))
                        {
                            MainSystem.RootDrive.DeleteDirectory(path, false);
                        }
                        else
                        {
                            MainSystem.RootDrive.DeleteFile(path);
                        }
                    }
                    catch (Exception exp)
                    {
                        Debug.Log("Error removing file from Lua: " + exp.Message);
                        return -1;
                    }
                    return 0;
                }
                public int RenameFileHandler(string fromname, string toname)
                {
                    NixPath frompath = MainSession.PhysicalDirectory.Combine(new NixPath(fromname));
                    NixPath topath = MainSession.PhysicalDirectory.Combine(new NixPath(toname));
                    try
                    {
                        MainSystem.RootDrive.Rename(frompath, topath);
                    }
                    catch (Exception exp)
                    {
                        Debug.Log("Error renaming file from Lua: " + exp.Message);
                        return -1;
                    }
                    return 0;
                }
                public string GetTempFilenameHandler()
                {
                    string tmpFolder = MainSession.GetEnvValue("TMPDIR", "/tmp");
                    return tmpFolder + "/" + System.Guid.NewGuid().ToString();
                }
                protected override void Run()
                {
                    Lua.LuaOptions opts = new Lua.LuaOptions();
                    opts.StdOut = StdOut;
                    opts.StdIn = StdIn;
                    opts.StdErr = StdErr;
                    opts.ExecuteHandler = ExecuteHandler;
					opts.OpenFileHandler = OpenFileHandler;
					opts.RenameFileHandler = RenameFileHandler;
					opts.RemoveFileHandler = RemoveFileHandler;
					opts.GetTempFilenameHandler = GetTempFilenameHandler;
                    Lua l = new Lua(opts);
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
								Debug.Log ("Lua args: " + argStr);
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
