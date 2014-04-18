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
                    bool physical = false;
                    bool error = false;
                    for (int i = 1; i < Argv.Count; i++) {
                        string arg = Argv[i];
                        if (arg[0] == '-') {
                            for (int j = 1; j < arg.Length; j++) {
                                if (arg[j] == 'P') {
                                    physical = true;
                                }
                                else {
                                    StdOut.WriteLine("Unknown argument: " + arg[j]);
                                    error = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (error) {
                        return;
                    }
                    string pwd = "No session";
                    if (MainSession != null)
                    {
                        if (!physical && MainSession.WorkingDirectory != null) {
                            pwd = MainSession.WorkingDirectory.ToString();
                        }
                        else if (physical && MainSession.PhysicalDirectory != null) {
                            pwd = MainSession.PhysicalDirectory.ToString();
                        }
                    }
                    StdOut.WriteLine(pwd);
                }

            }
        }
    }
}
