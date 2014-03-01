using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NixPath {

	public List<string> Path {get; private set;}
	public bool Absolute {get; set;}

	public NixPath(string path = "") {
		Absolute = true;
		Path = new List<string>();
		if (path.Length > 0) {
			SetPath(path);
		}
	}
	public void AppendPath(string path) {
		if (path[0] == '/') {
			Path.Clear ();
		}
		string []split = path.Split('/');
		for (int i = 0; i < split.Length; i++) {
			if (split[i].Length == 0) {
				continue;
			}
			if (split[i] == ".") {
				continue;
			}
			else if (split[i] == "..") {
				if (Path.Count > 0) {
					Path.RemoveAt(Path.Count - 1);
				}
			}
			else {
				Path.Add(split[i]);
			}
		}
	}
	public NixPath Combine(string path) {
		NixPath newPath = new NixPath(this.ToString());
		newPath.AppendPath(path);
		return newPath;
	}
	public void SetPath(string path) {
		Path.Clear ();
		AppendPath(path);
	}
	override public string ToString() {
		string result = "";
		if (Absolute) {
			result = "/";
		}
		for (int i = 0; i < Path.Count; i++) {
			if (i > 0) {
				result += '/';
			}
			result += Path[i];
		}
		return result;
	}

}
