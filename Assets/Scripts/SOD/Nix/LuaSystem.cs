using System;
using System.IO;
using NLua;

namespace SOD
{
    namespace Nix
    {
        public class LuaSystem
        {
            public Session MainSession { get; private set; }
            public NixSystem MainSystem { get; private set; }
            public Lua Lua { get; private set; }
            public Stream StdOut { get; private set; }
            public Stream StdIn { get; private set; }
            public Stream StdErr { get; private set; }
            public LuaSystem(Session session, NixSystem system, Stream stdOut, Stream stdIn, Stream stdErr)
            {
                MainSession = session;
                MainSystem = system;
                StdOut = stdOut;
                StdIn = stdIn;
                StdErr = stdErr;

                Lua.LuaOptions opts = new Lua.LuaOptions();
                opts.StdOut = stdOut;
                opts.StdIn = stdIn;
                opts.StdErr = stdErr;
                opts.ExecuteHandler = ExecuteHandler;
                opts.OpenFileHandler = OpenFileHandler;
                opts.RenameFileHandler = RenameFileHandler;
                opts.RemoveFileHandler = RemoveFileHandler;
                opts.GetTempFilenameHandler = GetTempFilenameHandler;
                Lua = new Lua(opts);
            }

            public int ExecuteHandler(string line)
            {
                MainSession.Shell.Execute(line);
                return 0;
            }
            public Stream OpenFileHandler(string filename, FileMode mode, FileAccess access)
            {
                //NixPath path = OpenPath(filename);
                NixPath path = MainSystem.RootDrive.FollowLinks(MainSession.WorkingDirectory.Combine(new NixPath(filename)));
                return MainSystem.RootDrive.OpenFile(path, access, mode);
            }
            public int RemoveFileHandler(string filename)
            {
                NixPath path = MainSession.PhysicalDirectory.Combine(new NixPath(filename));
                try
                {
                    if (MainSystem.RootDrive.IsDirectory(path))
                    {
                        MainSystem.RootDrive.DeleteDirectory(path, false);
                    }
                    else
                    {
                        MainSystem.RootDrive.DeleteFile(path);
                    }
                }
                catch (Exception exp)
                {
                    StdOut.WriteLine("Error removing file from Lua: " + exp.Message);
                    return -1;
                }
                return 0;
            }
            public int RenameFileHandler(string fromname, string toname)
            {
                NixPath frompath = MainSession.PhysicalDirectory.Combine(new NixPath(fromname));
                NixPath topath = MainSession.PhysicalDirectory.Combine(new NixPath(toname));
                try
                {
                    MainSystem.RootDrive.Rename(frompath, topath);
                }
                catch (Exception exp)
                {
                    StdOut.WriteLine("Error renaming file from Lua: " + exp.Message);
                    return -1;
                }
                return 0;
            }
            public string GetTempFilenameHandler()
            {
                string tmpFolder = MainSession.GetEnvValue("TMPDIR", "/tmp");
                return tmpFolder + "/" + System.Guid.NewGuid().ToString();
            }
        }
    }
}

