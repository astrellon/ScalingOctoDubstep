using UnityEngine;
using System.Collections;
using NLua;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		/*
		var ScriptEngine = IronPython.Hosting.Python.CreateEngine();
		// and the scope (ie, the python namespace)  
		var ScriptScope = ScriptEngine.CreateScope(); 

		// execute a string in the interpreter and grab the variable  
		string example = "f = open('testfile', 'w')\nf.write('Testoutput')\nf.close()";  

		var ScriptSource = ScriptEngine.CreateScriptSourceFromString(example);  
		ScriptSource.Execute(ScriptScope);  
		string came_from_script = ScriptScope.GetVariable<string>("output");  
		// Should be what we put into 'output' in the script.  
		Debug.Log(came_from_script); 

		 var ScriptEngine = IronPython.Hosting.Python.CreateEngine();  
		// and the scope (ie, the python namespace)  
		var ScriptScope = ScriptEngine.CreateScope();  
		// execute a string in the interpreter and grab the variable  
		string example = "from System.Collections import BitArray\nba = BitArray(5)\nba.Set(0, False)\noutput = ba[0]";  
		
		var ScriptSource = ScriptEngine.CreateScriptSourceFromString(example);  
		ScriptSource.Execute(ScriptScope);  
		bool came_from_script = ScriptScope.GetVariable<bool>("output");  
		// Should be what we put into 'output' in the script.  
		Debug.Log(came_from_script);
		  */

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
