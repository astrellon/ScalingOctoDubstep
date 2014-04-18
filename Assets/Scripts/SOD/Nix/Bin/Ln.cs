using UnityEngine;
using System.IO;
using System.Collections;

namespace SOD
{
    namespace Nix
    {
        namespace Bin
        {
            public class Ln : Program
            {

                public Ln(int pid)
                    : base(pid)
                {

                }
                public override string GetCommand()
                {
                    return "ln";
                }
                protected override void Run()
                {
                    if (Argv.Count == 1) {
                        StdOut.WriteLine("Need help");
                        return;
                    }
                    bool symbolic = true;
                    NixPath source = MainSession.WorkingDirectory.Combine(Argv[1]); 
                    NixPath destination;
                    if (Argv.Count == 2) 
                    {
                        destination = new NixPath(source.TopPath());
                    }
                    else 
                    {
                        destination = new NixPath(Argv[2]);
                    }

                    byte []path = System.Text.Encoding.UTF8.GetBytes(source.ToString());
                    NixPath outputFile = OpenPath(destination);
                    if (outputFile != null)
                    {
                        FileStream output = File.OpenWrite(MainSystem.RootDrive.GetPathTo(destination));
                        output.Write(MainSystem.RootDrive.SymlinkHeader, 0, MainSystem.RootDrive.SymlinkHeader.Length);
                        output.Write(path, 0, path.Length);
                        output.Close();
                    }
                    else
                    {
                        StdOut.WriteLine("Error attempting to write link.");
                    }
                    /*
                     *
                    NixPath newPath = MainSession.WorkingDirectory;
                    if (Argv.Count > 1)
                    {
                        newPath = MainSession.WorkingDirectory.Combine(Argv[1]);
                    }

                    FileNode[] files = MainSystem.RootDrive.ListFiles(newPath.ToString());
                    if (files != null)
                    {
                        for (int i = 0; i < files.Length; i++)
                        {
                            FileInfo info = files[i].Info;
                            StdOut.Write(info.Name);
                            if ((info.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                            {
                                StdOut.Write("/");
                            }
                            StdOut.Write("\n");
                        }
                    }
                    else
                    {
                        StdOut.Write(newPath.ToString() + " is not a directory.\n");
                    }
                    return;
                    */
                }
            }
        }
    }
}
