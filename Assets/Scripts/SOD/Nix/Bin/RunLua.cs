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

                private string PathToLua = "";
                public RunLua(int pid)
                    : base(pid)
                {

                }
                public RunLua(int pid, string pathToLua)
                    : base(pid)
                {
                    PathToLua = pathToLua;
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
                    return new FileStream(MainSystem.RootDrive.GetPathTo(path), mode, access);
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
                    Lua l = new Lua(opts);
                    try
                    {
                        if (Argv.Count > 1 || PathToLua.Length > 0)
                        {
                            NixPath newPath;
                            if (PathToLua.Length > 0)
                            {
                                newPath = new NixPath(PathToLua);
                            }
                            else
                            {
                                newPath = MainSession.WorkingDirectory.Combine(Argv[1]);
                            }
                            string file = MainSystem.RootDrive.GetPathTo(newPath.ToString());
                            Debug.Log("File to load: " + file);
                            if (File.Exists(file))
                            {
                                string argStr = "arg={}\n";
                                for (int i = 1; i < Argv.Count; i++)
                                {
                                    argStr += "arg[" + (i - 1) + "]=\"" + Argv[i] + "\"\n";
                                }
								Debug.Log ("Lua args: " + argStr);
                                l.DoString(argStr);
                                l.DoFile(newPath.ToString());
                            }
                            else
                            {
                                StdOut.WriteLine("Unable to find file: " + Argv[0]);
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
