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
                        destination = MainSession.WorkingDirectory.Combine(Argv[2]);
                    }

                    if (MainSystem.RootDrive.MakeLink(source, MainSession.WorkingDirectory.Combine(destination)) != 1)
                    {
                        StdOut.WriteLine("Error attempting to write link.");
                    }
                }
            }
        }
    }
}
