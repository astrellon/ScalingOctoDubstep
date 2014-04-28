using System;
using System.IO;

namespace SOD
{
    namespace Nix
    {
        namespace FileSystem
        {
            public class FileNode
            {
                public FileInfo Info {get; set;}
                public NixPath Symlink {get; set;}
                public FileNode (string path)
                {
                    Info = new FileInfo(path);
                    Symlink = null;
                }
            }
        }
    }
}
