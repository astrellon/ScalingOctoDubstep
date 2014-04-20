using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace SOD
{
    namespace Nix
    {
        namespace Bin
        {
            public abstract class Program
            {

                public static int Unknown = 0;
                public static int KeyboardDown = 1;
                public static int KeyboardUp = 2;

                public class ProgramEvent
                {
                    public int Message { get; private set; }

                    public ProgramEvent(int message)
                    {
                        Message = message;
                    }
                }
                public class KeyboardEvent : ProgramEvent
                {
                    public char Character { get; private set; }
                    public int KeyCode { get; private set; }

                    public KeyboardEvent(int type, char character, int keyCode)
                        : base(type)
                    {
                        Character = character;
                        KeyCode = keyCode;
                    }
                }

                public Stream StdOut { get; set; }
                public Stream StdIn { get; set; }
                public Stream StdErr { get; set; }
                public Thread MainThread { get; set; }
                public int Pid { get; private set; }
                public bool Running { get; protected set; }
                public NixSystem MainSystem { get; private set; }
                public Session MainSession { get; private set; }
                public IList<string> Argv { get; private set; }
                public int Result { get; protected set; }
                public Queue<ProgramEvent> Events { get; private set; }

                public Program(int pid, Stream stdout = null, Stream stdin = null, Stream stderr = null)
                {
                    Running = false;
                    Pid = pid;
                    Events = new Queue<ProgramEvent>();

                    if (stdout == null)
                    {
                        stdout = new NixStream();
                    }
                    StdOut = stdout;
                    if (stdin == null)
                    {
                        NixStream s = new NixStream();
                        s.EchoStream = true;
                        stdin = s;
                    }
                    StdIn = stdin;
                    if (stderr == null)
                    {
                        stderr = new NixStream();
                    }
                    StdErr = stderr;
                    MainThread = new System.Threading.Thread(Run);
                }

                public NixPath OpenPath(NixPath path)
                {
                    return MainSystem.RootDrive.FollowLinks(MainSession.WorkingDirectory.Combine(path));
                }
                public NixPath OpenPath(string path)
                {
                    return MainSystem.RootDrive.FollowLinks(MainSession.WorkingDirectory.Combine(new NixPath(path)));
                }

                public void PushEvent(ProgramEvent e)
                {
                    lock (Events)
                    {
                        Events.Enqueue(e);
                    }
                }
                public ProgramEvent PopEvent()
                {
                    lock (Events)
                    {
                        if (Events.Count > 0)
                        {
                            return Events.Dequeue();
                        }
                        return null;
                    }
                }
                public bool HasEvents()
                {
                    lock (Events)
                    {
                        return Events.Count > 0;
                    }
                }

                public abstract string GetCommand();
                public int Execute(NixSystem system, Session session, IList<string> argv)
                {
                    ExecuteAsync(system, session, argv);
                    MainThread.Join();
                    return Result;
                }

                public void ExecuteAsync(NixSystem system, Session session, IList<string> argv)
                {
                    MainSystem = system;
                    MainSession = session;
                    Argv = argv;
                    Running = true;
                    MainThread.Start();
                }

                protected abstract void Run();

            }
        }
    }
}