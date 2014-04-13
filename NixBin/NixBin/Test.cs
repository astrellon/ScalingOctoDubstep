using System;

namespace NixBin
{
    public class Test : SOD.Nix.Bin.Program
    {
        public Test(int pid) : base(pid)
        {

        }

        public override string GetCommand()
        {
            return "test";
        }
        protected override void Run()
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes("Test out!");
            StdOut.Write(bytes, 0, bytes.Length);
        }
    }
}
