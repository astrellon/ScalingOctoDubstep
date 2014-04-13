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