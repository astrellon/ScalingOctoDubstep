using UnityEngine;
using System.IO;
using System.Collections;

namespace SOD
{
    namespace Nix
    {
        namespace Bin
        {
            public class Ls : Program
            {

                public Ls(int pid)
                    : base(pid)
                {

                }
                public override string GetCommand()
                {
                    return "ls";
                }
                protected override void Run()
                {
                    NixPath newPath = MainSession.PhysicalDirectory;
                    if (Argv.Count > 1)
                    {
                        //newPath = MainSession.WorkingDirectory.Combine(Argv[1]);
                        newPath = OpenPath(Argv[1]);
                    }

                    SOD.Nix.FileSystem.FileNode[] files = MainSystem.RootDrive.ListFiles(newPath.ToString());
                    if (files != null)
                    {
                        for (int i = 0; i < files.Length; i++)
                        {
                            FileInfo info = files[i].Info;
                            if (files[i].Symlink != null) 
                            {
                                StdOut.Write("@");
                            }
                            StdOut.Write(info.Name);
                            if ((info.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                            {
                                StdOut.Write("/");
                            }
                            if (files[i].Symlink != null)
                            {
                                StdOut.Write(" -> " + files[i].Symlink.ToString());
                            }
                            StdOut.Write("\n");
                        }
                    }
                    else
                    {
                        StdOut.Write(newPath.ToString() + " is not a directory.\n");
                    }
                }
            }
        }
    }
}
