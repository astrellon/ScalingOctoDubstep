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
                public FileSystem()
                {
                    SymlinkHeader = System.Text.Encoding.ASCII.GetBytes("!<symlink>  ");
                    SymlinkHeader[10] = 0xFF;
                    SymlinkHeader[11] = 0xFE;
                }

                public FileNode[] ListFiles(string path)
                {
                    NixPath nixPath = new NixPath(path);
                    string tpath = GetPathTo(nixPath);
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
                    NixPath path = new NixPath(relative);
                    if (path.Absolute)
                    {
                        return RootFolder + path.ToString(true);
                    }
                    return RootFolder + "\\" + path.ToString(true);
                }
                public string GetPathTo(NixPath path)
                {
                    return GetPathTo(path.ToString(true));
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

                public void MakeDirectory(NixPath path, bool createParents)
                {
                    string pathStr = GetPathTo(path);
                    Debug.Log("Makedir str: " + pathStr);
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
                public NixPath GetLink(string fullPath)
                {
					try
                    {
                        if (!File.Exists(fullPath))
                        {
                            return null;
                        }
                        FileStream stream = File.OpenRead(fullPath);
                        byte []symcheck = new byte[SymlinkHeader.Length];
                        int read = stream.Read(symcheck, 0, symcheck.Length);
                        if (read != symcheck.Length)
                        {
                            return null;
                        }
                        for (int i = 0; i < SymlinkHeader.Length; i++)
                        {
                            if (symcheck[i] != SymlinkHeader[i])
                            {
                                return null;
                            }
                        }
                        long remaining = stream.Length - symcheck.Length;
                        byte []pathBytes = new byte[remaining];
                        stream.Read(pathBytes, 0, (int)remaining); 
                        string pathStr = System.Text.Encoding.UTF8.GetString(pathBytes); 
                        return new NixPath(pathStr);
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
                    if (path.Path.Count == 0) {
                        return path;
                    }
                    NixPath build = new NixPath();
                    if (path.Absolute)
                    {
                        build.Absolute = true;
                    }
                    for (int i = 0; i < path.Path.Count; i++)
                    {
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
            }
        }
    }
}
