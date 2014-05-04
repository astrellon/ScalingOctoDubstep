using UnityEngine;
using System.Collections;

namespace SOD
{
    namespace Nix
    {
        namespace Bin
        {
            public class Echo : Program
            {

                public Echo(int pid)
                    : base(pid)
                {

                }
                public override string GetCommand()
                {
                    return "echo";
                }
                protected override void Run()
                {
                    // TODO Change to string builder
                    string output = "";
                    for (int i = 1; i < Argv.Count; i++)
                    {
                        if (i > 1)
                        {
                            output += " ";
                        }
                        output += Argv[i];
                    }
                    output += "\n";
                    StdOut.Write(output);
                    return;
                }

            }
        }
    }
}
