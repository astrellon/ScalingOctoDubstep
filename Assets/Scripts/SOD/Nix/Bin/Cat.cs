using UnityEngine;
using System.Collections;
using System.IO;

namespace SOD
{
    namespace Nix
    {
        namespace Bin
        {
            public class Cat : Program
            {

                public Cat(int pid)
                    : base(pid)
                {

                }
                public override string GetCommand()
                {
                    return "cat";
                }
                protected override void Run()
                {
                    if (Argv.Count == 1)
                    {
                        // Read from stdin
                    }
                    else
                    {
                        for (int i = 1; i < Argv.Count; i++)
                        {
                            NixPath path = OpenPath(Argv[i]);
                            if (MainSystem.RootDrive.IsFileOrDirectory(path))
                            {
                                try
                                {
                                    using (Stream file = MainSystem.RootDrive.OpenFile(path, FileAccess.Read, FileMode.Open))
                                    {
                                        StreamReader reader = new StreamReader(file);
                                        StdOut.Write(reader.ReadToEnd());
                                    }
                                }
                                catch (System.Exception exp)
                                {
                                    StdOut.WriteLine("Error reading file for cat: " + exp.Message);
                                }
                            }
                            else
                            {
                                StdOut.Write(GetCommand() + ": " + Argv[i] + ": No such file or directory\n");
                            }
                        }
                    }
                }
            }
        }
    }
}
