using UnityEngine;
using System.Collections;
using IronPython;
using IronPython.Modules;
using IronPython.Hosting;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		var ScriptEngine = IronPython.Hosting.Python.CreateEngine();  
		// and the scope (ie, the python namespace)  
		var ScriptScope = ScriptEngine.CreateScope();  
		// execute a string in the interpreter and grab the variable  
		string example = "f = open('testfile', 'w')\nf.write('Testoutput')\nf.close()";  

		var ScriptSource = ScriptEngine.CreateScriptSourceFromString(example);  
		ScriptSource.Execute(ScriptScope);  
		//string came_from_script = ScriptScope.GetVariable<string>("output");  
		// Should be what we put into 'output' in the script.  
		//Debug.Log(came_from_script);  
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
