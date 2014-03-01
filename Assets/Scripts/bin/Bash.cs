using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

public class Bash : Program {
    
    public List<string> History {get; private set;}
    public Bash() {
        History = new List<string>();
    }

    public override string GetCommand() {
        return "bash";
    }
    protected override void Run() {
        while (Running) {
            string input = "";
            int read = StdIn.Read(ref input);
			History.Add(input);
        }
    }
}
