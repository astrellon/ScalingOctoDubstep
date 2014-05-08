using System;
using System.IO;
using NLua;
using UnityEngine;

namespace SOD
{
    namespace Nix
    {
        namespace Device
        {
            public class LuaDevice : CharacterDevice
            {
                public Session MainSession { get; private set; }
                public NixSystem MainSystem { get; private set; }
                public LuaSystem LuaSystem { get; private set; }

                public LuaDevice(Session session, NixSystem system)
                {
                    MainSession = session;
                    MainSystem = system;
                    Bin.Program shell = session.Shell;

                    LuaSystem = new LuaSystem(session, system, shell.StdOut, shell.StdIn, shell.StdErr);
                }
                public void LoadFile(NixPath path)
                {
                    LuaSystem.Lua.DoFile(path.ToString());
                }

                public override int Read(int socketId, byte []buffer, int offset, int count)
                {
                    object func = LuaSystem.Lua["Read"];
                    if (func == null || !(func is LuaFunction))
                    {
                        return 0;
                    }
                    object []result = (func as LuaFunction).Call(socketId, buffer, offset, count);
                    if (result.Length > 0)
                    {
                        return Int32.Parse(result[0].ToString());
                        //return (int)result[0];
                    }

                    return 0;
                }
                public override void Write(int socketId, byte []buffer, int offset, int count)
                {
                    object func = LuaSystem.Lua["Write"];
                    if (func == null || !(func is LuaFunction))
                    {
                        return;
                    }
                    object []result = (func as LuaFunction).Call(socketId, buffer, offset, count);
                    return;
                }
                public override void Close(int socketId)
                {

                }
                public override int DeinitDevice()
                {
                    return 1;
                }
                public override int InitDevice()
                {
                    return 1;
                }
                public override string DeviceType()
                {
                    return "character";
                }
                public override string DeviceName()
                {
                    return "null";
                }
            }
        }
    }
}
