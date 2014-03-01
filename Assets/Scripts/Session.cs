using System;

public class Session {

    public string User {get; set;}
    public NixPath WorkingDirectory {get; set;}

    public Session() {
        WorkingDirectory = new NixPath();
    }

}
