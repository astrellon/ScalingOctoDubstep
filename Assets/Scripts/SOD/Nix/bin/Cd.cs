using UnityEngine;
using System.Collections;
using System.IO;

namespace SOD
{
    namespace Nix
    {
        public class Cd : Program
        {

            public Cd(int pid)
                : base(pid)
            {

            }
            public override string GetCommand()
            {
                return "cd";
            }
            protected override void Run()
            {
                if (Argv.Count <= 1)
                {
                    StdOut.WriteLine("Need help");
                    return;
                }

                NixPath newPath = MainSession.WorkingDirectory.Combine(Argv[1]);
                if (MainSystem.RootDrive.IsDirectory(newPath.ToString()))
                {
                    MainSession.SetWorkingDirectory(newPath);
                }
                else
                {
                    StdOut.Write(newPath.ToString() + " is not a directory.\n");
                }
                return;
            }
        }

    }
}