using System;

namespace SOD
{
    namespace Nix
    {
        namespace Bin
        {
            public class MakeDevice : Program
            {
                public MakeDevice(int pid)
                    : base(pid)
                {

                }

                public override string GetCommand()
                {
                    return "mkdevice";
                }
                protected override void Run()
                {
                    if (Argv.Count < 4)
                    {
                        StdOut.WriteLine("Need help");
                        return;
                    }

                    string type = "lua";

                    for (int i = 1; i < Argv.Count; i++)
                    {
                        switch (Argv[i])
                        {
                            case "-t":
                                type = Argv[i + 1];
                                break;
                        }
                    }

                    NixPath sourceFile = OpenPath(Argv[Argv.Count - 2]);
                    NixPath destFile = OpenPath(Argv[Argv.Count - 1]);

                    if (type != "lua")
                    {
                        StdOut.WriteLine("Unknown device type: " + type);
                        return;
                    }

                    if (!MainSystem.RootDrive.IsFile(sourceFile)) 
                    {
                        StdOut.WriteLine("Cannot find file: " + sourceFile.ToString());
                        return;
                    }

                    if (type == "lua")
                    {
                        Device.LuaDevice device = new Device.LuaDevice(MainSession, MainSystem);
                        device.LoadFile(sourceFile);
                        MainSystem.MainDeviceManager.AddDevice(device);
                        MainSystem.RootDrive.MakeCharacterDevice(destFile, device.Id);
                    }
                }
            }
        }
    }
}
