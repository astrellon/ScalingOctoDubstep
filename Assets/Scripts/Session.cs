using System;

public class Session {

    public string User {get; set;}
    public NixPath WorkingDirectory {get; set;}
    public Program ForegroundProgram {get; set;}
    public Program Shell {get; set;}
    public string InputBuffer {get; protected set;} 

    public Session(Program shell) {
        WorkingDirectory = new NixPath();
        Shell = shell;
    }

    public void KeyboardInput(string input) {
        if (input.Length == 0) {
            return;
        }
        if (input[0] == '\r' || input[0] == '\n') {
            if (ForegroundProgram != null) {
                ForegroundProgram.StdIn.Write(InputBuffer);
                InputBuffer = "";
            }
        else {
            InputBuffer += input; 
        }
    }

}
