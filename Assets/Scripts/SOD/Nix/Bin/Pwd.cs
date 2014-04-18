using UnityEngine;
using System.Collections;
using System;

namespace SOD
{
    namespace Nix
    {
        namespace Bin
        {
            public class Pwd : Program
            {

                public Pwd(int pid)
                    : base(pid)
                {

                }
                public override string GetCommand()
                {
                    return "pwd";
                }
                protected override void Run()
                {
                    for (int i = 1; i < Argv.Count; i++) {
                    }
                    string pwd = "No session";
                    if (MainSession != null && MainSession.WorkingDirectory != null)
                    {
                        pwd = MainSession.WorkingDirectory.ToString();
                    }
                    StdOut.WriteLine(pwd);
                }

            }
        }
    }
}
