using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SOD
{
    namespace Nix
    {

        public class Rm : Program
        {

            public Rm(int pid)
                : base(pid)
            {

            }
            public override string GetCommand()
            {
                return "rm";
            }
            protected override void Run()
            {
                if (Argv.Count <= 1)
                {
                    StdOut.WriteLine("Need help");
                    return;
                }

                bool recursive = false;
                bool force = false;
                bool error = false;

                List<string> files = new List<string>();
                for (int i = 1; !error && i < Argv.Count; i++)
                {
                    string arg = Argv[i];
                    if (arg[0] == '-')
                    {
                        for (int j = 1; j < arg.Length; j++)
                        {
                            if (arg[j] == 'r')
                            {
                                recursive = true;
                            }
                            else if (arg[j] == 'f')
                            {
                                force = true;
                            }
                            else
                            {
                                StdOut.WriteLine(GetCommand() + ": unknown command -- " + arg[j]);
                                error = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        files.Add(arg);
                    }
                }

                try
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        NixPath file = MainSession.WorkingDirectory.Combine(new NixPath(files[i]));
                        if (MainSystem.RootDrive.IsDirectory(file))
                        {
                            if (!recursive)
                            {
                                StdOut.WriteLine(GetCommand() + ": cannot remove '" + files[i] + "': Is a directory");
                                return;
                            }
                            else if (!MainSystem.RootDrive.IsDirectoryEmpty(file))
                            {
                                if (force)
                                {
                                    MainSystem.RootDrive.DeleteDirectory(file, true);
                                }
                                else
                                {
                                    StdOut.WriteLine(GetCommand() + ": cannot remove '" + files[i] + "': Is not empty");
                                }
                            }
                            else
                            {
                                MainSystem.RootDrive.DeleteDirectory(file, false);
                            }
                        }
                        else
                        {
                            MainSystem.RootDrive.DeleteFile(file);
                        }
                    }
                }
                catch (System.Exception exp)
                {
                    Debug.Log("Error removing: " + exp.Message);
                }

                /*NixPath fromPath = MainSession.WorkingDirectory.Combine(Argv[1]);
                NixPath toPath = MainSession.WorkingDirectory.Combine(Argv[2]);
		
                try {
                    MainSystem.RootDrive.Move(fromPath, toPath);
                }
                catch (System.IO.FileNotFoundException exp) {
                    StdOut.WriteLine("No such file or directory: " + Argv[1]);
                }
                catch (System.Exception exp) {
                    StdOut.WriteLine(exp.Message);
                }
                return;
                */
            }
        }

    }
}