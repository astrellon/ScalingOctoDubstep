using UnityEngine;
using System.Collections;
using System.IO;

namespace SOD
{
    namespace Nix
    {
        namespace Bin
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
                    try{

                    NixPath newPath = MainSession.WorkingDirectory.Combine(Argv[1]).ResolvePath();
                    NixPath followedPath = MainSystem.RootDrive.FollowLinks(MainSession.PhysicalDirectory.Combine(Argv[1]));

                    if (MainSystem.RootDrive.IsDirectory(followedPath))
                    {
                        MainSession.SetWorkingDirectory(newPath);
                    }
                    else
                    {
                        StdOut.Write(newPath.ToString() + " is not a directory.\n");
                    }
                    }
                    catch (System.Exception exp) {
                        Debug.Log("CD EXP: " + exp.Message);
                    }
                }
            }
        }
    }
}
