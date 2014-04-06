using UnityEngine;
using System.Collections;
using System.IO;

public class FileSystem {

	public string RootFolder = "";
	public FileSystem() {

	}

	public FileNode []ListFiles(string path) {
		string tpath = GetPathTo(path);
		if (!Directory.Exists(tpath)) {
			return null;
		}
		string []entries = Directory.GetFileSystemEntries(tpath);
		FileNode []result = new FileNode[entries.Length];
		for (int i = 0; i < entries.Length; i++) {
			result[i] = new FileNode(entries[i]);
		}
		return result;
	}
	public string GetPathTo(string relative) {
        if (relative[0] != '/' || relative[0] != '\\') {
            return RootFolder + '/' + relative;
        }
		return RootFolder + relative;
	}
	public bool IsDirectory(string path) {
		string tpath = GetPathTo(path);
		return Directory.Exists(tpath);
	}
    public bool IsDirectory(NixPath path) {
        string tpath = GetPathTo(path.ToString());
        return Directory.Exists(tpath);
    }
    public bool IsFile(string path) {
        string tpath = GetPathTo(path);
        return File.Exists(tpath);
    }
    public bool IsFile(NixPath path) {
        string tpath = GetPathTo(path.ToString());
        return File.Exists(tpath);
    }
    public bool IsFileOrDirectory(string path) {
        string tpath = GetPathTo(path);
        return File.Exists(tpath) || Directory.Exists(tpath);
    }
    public bool IsFileOrDirectory(NixPath path) {
        string tpath = GetPathTo(path.ToString());
        return File.Exists(tpath) || Directory.Exists(tpath);
    }
}
