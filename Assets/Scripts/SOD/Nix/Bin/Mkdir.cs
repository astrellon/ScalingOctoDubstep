using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace SOD
{
    namespace Nix
    {
        namespace Bin
        {
            public class Mkdir : Program
            {

                public Mkdir(int pid)
                    : base(pid)
                {

                }
                public override string GetCommand()
                {
                    return "mkdir";
                }
                protected override void Run()
                {
                    if (Argv.Count == 1)
                    {
                        // Read from stdin
                        StdOut.WriteLine(GetCommand() + ": missing operand");
                    }
                    else
                    {
                        bool createParents = false;
                        //string[] copy = new string[Argv.Count - 1];
                        List<NixPath> copy = new List<NixPath>();
                        bool error = false;
                        for (int i = 1; i < Argv.Count; i++) {
                            string arg = Argv[i];
                            if (arg[0] == '-') {
                                for (int j = 1; j < arg.Length; j++) {
                                    if (arg[j] == 'p') {
                                        createParents = true;
                                    }
                                    else {
                                        StdOut.WriteLine("Unknown argument: " + arg[j]);
                                        error = true;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                copy.Add(MainSession.PhysicalDirectory.Combine(arg));
                            }
                        }
                        if (error) {
                            return;
                        }
                        /*
                        for (int i = 1; i < Argv.Count; i++)
                        {
                            if (Argv[i] == "-p")
                            {
                                createParents = true;
                            }
                            else
                            {
                                copy[j++] = MainSession.PhysicalDirectory.Combine(Argv[i]).ToString();
                            }
                        }
                        */
                        for (int i = 0; i < copy.Count; i++)
                        {
                            Debug.Log("Copy arg " + copy[i]);
                            if (copy[i].ToString().Length == 0)
                            {
                                break;
                            }
                            try
                            {
                                MainSystem.RootDrive.MakeDirectory(copy[i], createParents);
                            }
                            catch (Exception exp)
                            {
                                StdOut.WriteLine("Error creating directory: " + exp.Message);
                                //Debug.Log("Error creating directory: " + exp.Message);
                            }
                        }
                    }
                }
            }
        }
    }
}
