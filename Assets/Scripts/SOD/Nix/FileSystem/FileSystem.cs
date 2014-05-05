using UnityEngine;
using System;
using System.Collections;
using System.IO;

namespace SOD
{
    namespace Nix
    {
        namespace FileSystem
        {
            public class FileSystem
            {

                public string RootFolder = "";
                public byte []SymlinkHeader = null;
                public byte []CharacterDeviceHeader = null;
                public NixSystem MainSystem { get; private set; }
                public FileSystem(NixSystem system)
                {
                    MainSystem = system;
                    SymlinkHeader = System.Text.Encoding.ASCII.GetBytes("!<symlink>  ");
                    SymlinkHeader[10] = 0xFF;
                    SymlinkHeader[11] = 0xFE;

                    CharacterDeviceHeader = System.Text.Encoding.ASCII.GetBytes("!<chardevice>  ");
                    CharacterDeviceHeader[10] = 0xFF;
                    CharacterDeviceHeader[11] = 0xFE;
                }

                public FileNode[] ListFiles(string path)
                {
                    return ListFiles(new NixPath(path));
                }
                public FileNode[] ListFiles(NixPath path)
                {
                    string tpath = GetPathTo(path);
                    if (!Directory.Exists(tpath))
                    {
                        return null;
                    }
                    string[] entries = Directory.GetFileSystemEntries(tpath);
                    FileNode[] result = new FileNode[entries.Length];
                    for (int i = 0; i < entries.Length; i++)
                    {
                        try{
                        result[i] = new FileNode(entries[i]);
                        result[i].Symlink = GetLink(entries[i]);
                        }
                        catch (Exception x)
                        {
                            Debug.Log("Exception: " + x.Message);
                        }
                    }
                    return result;
                }
                public string GetPathTo(string relative)
                {
                    return GetPathTo(new NixPath(relative));
                }
                public string GetPathTo(NixPath path)
                {
                    try
                    {
                        if (path.Absolute)
                        {
                            return RootFolder + path.BuildString(true, true);
                        }
                        return RootFolder + "\\" + path.BuildString(true, true);
                    }
                    catch (Exception exp)
                    {
                        Debug.Log("Error getting path: " + exp.Message);
                        return "err";
                    }
                }
                public bool IsDirectoryEmpty(string path)
                {
                    string tpath = GetPathTo(path);
                    if (Directory.Exists(tpath))
                    {
                        string[] entries = Directory.GetFileSystemEntries(tpath);
                        return entries.Length == 0;
                    }
                    return true;
                }
                public bool IsDirectoryEmpty(NixPath path)
                {
                    return IsDirectoryEmpty(path.ToString());
                }
                public bool IsDirectory(string path)
                {
                    string tpath = GetPathTo(path);
                    return Directory.Exists(tpath);
                }
                public bool IsDirectory(NixPath path)
                {
                    string tpath = GetPathTo(path);
                    return Directory.Exists(tpath);
                }
                public bool IsFile(string path)
                {
                    string tpath = GetPathTo(path);
                    return File.Exists(tpath);
                }
                public bool IsFile(NixPath path)
                {
                    string tpath = GetPathTo(path);
                    return File.Exists(tpath);
                }
                public bool IsFileOrDirectory(string path)
                {
                    string tpath = GetPathTo(path);
                    return File.Exists(tpath) || Directory.Exists(tpath);
                }
                public bool IsFileOrDirectory(NixPath path)
                {
                    string tpath = GetPathTo(path);
                    return File.Exists(tpath) || Directory.Exists(tpath);
                }
                public bool IsSymlink(NixPath path)
                {
                    return GetLink(path) != null;
                }
                public bool IsDeviceFile(NixPath path)
                {
                    return GetDeviceId(path) != -1;
                }

                public void Copy(NixPath fromPath, NixPath toPath)
                {
                    if (!IsFileOrDirectory(fromPath))
                    {
                        throw new System.IO.FileNotFoundException();
                    }
                    if (IsDirectory(toPath))
                    {
                        // Cannot copy to a directory.
                        throw new Exception("Cannot copy to a directory");
                    }
                    try
                    {
                        File.Copy(GetPathTo(fromPath), GetPathTo(toPath));
                    }
                    catch (Exception exp)
                    {
                        throw new Exception(exp.Message);
                    }
                }
                public void Move(NixPath fromPath, NixPath toPath)
                {
                    if (!IsFileOrDirectory(fromPath))
                    {
                        throw new System.IO.FileNotFoundException();
                    }
                    if (IsDirectory(toPath))
                    {
                        NixPath newPath = new NixPath(toPath.ToString());
                        newPath.AppendPath(fromPath.TopPath());
                        File.Move(GetPathTo(fromPath), GetPathTo(newPath));
                        return;
                    }
                    try
                    {
                        File.Move(GetPathTo(fromPath), GetPathTo(toPath));
                    }
                    catch (Exception exp)
                    {
                        throw new Exception(exp.Message);
                    }
                }
                public int MakeLink(NixPath source, NixPath destination)
                {
                    byte []pathBytes = System.Text.Encoding.UTF8.GetBytes(source.ToString());
                    NixPath outputFile = FollowLinks(destination);
                    if (outputFile != null)
                    {
                        using (FileStream output = File.OpenWrite(GetPathTo(destination)))
                        {
                            output.Write(SymlinkHeader, 0, SymlinkHeader.Length);
                            output.Write(pathBytes, 0, pathBytes.Length);
                        }
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                public int MakeCharacterDevice(NixPath destination, int id)
                {
                    byte []idBytes = System.Text.Encoding.UTF8.GetBytes(id.ToString());
                    NixPath outputFile = FollowLinks(destination);
                    if (outputFile != null)
                    {
                        using (FileStream output = File.OpenWrite(GetPathTo(destination)))
                        {
                            output.Write(CharacterDeviceHeader, 0, CharacterDeviceHeader.Length);
                            output.Write(idBytes, 0, idBytes.Length);
                        }
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }

                public Stream OpenFile(NixPath path, FileAccess access, FileMode mode)
                {
                    string openPath = GetPathTo(path);
                    int deviceId = GetDeviceId(path);
                    if (deviceId != -1)
                    {
                        //return MainSystem.MainDeviceManager.FindDevice(deviceId);
                        Bin.CharacterDevice device = MainSystem.MainDeviceManager.FindDevice(deviceId);
                        if (device != null)
                        {
                            return device.CreateStream();
                        }
                        return null;
                    }
                    else
                    {
                        return File.Open(openPath, mode, access); 
                    }
                    return null;
                }

                public void MakeDirectory(NixPath path, bool createParents)
                {
                    string pathStr = GetPathTo(path);
                    if (createParents)
                    {
                        Directory.CreateDirectory(pathStr);
                    }
                    else
                    {
                        NixPath t = new NixPath(path.ToString());
                        t.PopPath();
                        if (IsDirectory(t))
                        {
                            Directory.CreateDirectory(pathStr);
                        }
                        else
                        {
                            throw new DirectoryNotFoundException();
                        }
                    }
                }
                public void DeleteDirectory(NixPath path, bool recursive)
                {
                    string tpath = path.ToString();
                    Debug.Log("Delete dir: " + tpath);
                    Directory.Delete(GetPathTo(tpath), recursive);
                }
                public void DeleteFile(NixPath path)
                {
                    File.Delete(GetPathTo(path));
                }

                public void Rename(NixPath frompath, NixPath topath)
                {
                    File.Move(GetPathTo(frompath), GetPathTo(topath));
                }

                // Special Symbolic Link and Device files {{{
                protected string ReadSpecial(string fullPath, byte []headerCheck)
                {
                    if (!File.Exists(fullPath))
                    {
                        return null;
                    }
                    using (FileStream stream = File.OpenRead(fullPath)) 
                    {
                        byte []check = new byte[headerCheck.Length];
                        int read = stream.Read(check, 0, check.Length);
                        if (read != check.Length)
                        {
                            return null;
                        }
                        for (int i = 0; i < headerCheck.Length; i++)
                        {
                            if (check[i] != headerCheck[i])
                            {
                                return null;
                            }
                        }
                        long remaining = stream.Length - check.Length;
                        byte []bytes= new byte[remaining];
                        stream.Read(bytes, 0, (int)remaining); 
                        string str = System.Text.Encoding.UTF8.GetString(bytes); 
                        return str;
                    }
                }

                public int GetDeviceId(string fullPath)
                {
                    try
                    {
                        string deviceIdStr = ReadSpecial(fullPath, CharacterDeviceHeader);
                        if (deviceIdStr != null)
                        {
                            int deviceId = -1;
                            bool parsed = Int32.TryParse(deviceIdStr, out deviceId);
                            if (parsed)
                            {
                                return deviceId;
                            }
                        }
                        return -1;
                    }
                    catch (Exception exp)
                    {
                        Debug.Log("Error getting device id: " + exp.Message);
                    }
                    return -1;
                }
                public int GetDeviceId(NixPath path)
                {
                    return GetDeviceId(GetPathTo(path));
                }

                public NixPath GetLink(string fullPath)
                {
					try
                    {
                        string path = ReadSpecial(fullPath, SymlinkHeader);
                        if (path != null)
                        {
                            return new NixPath(path);
                        }
                        return null;
                    }
                    catch (Exception exp) 
                    {
                        Debug.Log("Error following link: " + exp.Message);
                    }
					return null;
                }
                public NixPath GetLink(NixPath path)
                {
                    return GetLink(GetPathTo(path));
                }
                public NixPath FollowLinks(NixPath path)
                {
                    if (path.Path.Count == 0) 
                    {
                        return path;
                    }
                    NixPath build = new NixPath();
                    if (path.Absolute)
                    {
                        build.Absolute = true;
                    }
                    for (int i = 0; i < path.Path.Count; i++)
                    {
                        if (path.Path[i] == "..") 
                        {
                            build.PopPath();
                            continue;
                        }
                        build.AppendPath(path.Path[i]);
                        if (IsDirectory(build))
                        {
                            continue;
                        }

                        NixPath link = GetLink(build);
                        while (link != null)
                        {
                            build = link;
                            link = GetLink(link);
                        }

                        if (build == null)
                        {
                            return path;
                        }
                    }
                    return build;
                }
                // End }}}
            }
        }
    }
}
