using System;

public interface INixStream {

    bool Enabled {get; set;}
    int Length(); 
    int Read(byte []dest, int offset, int maxRead); 
    int Read(ref string dest, int maxRead); 

    void Write(byte []buffer, int offset, int count); 
    void Write(byte data);
    void Write(string buffer); 
}

