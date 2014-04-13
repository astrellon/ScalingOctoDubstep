using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using NLua;

namespace SOD
{
    namespace Nix
    {
        public class Test : MonoBehaviour
        {

            public int ExecuteHandler(string line)
            {
                Debug.Log("Execute line: " + line);
                return 0;
            }

            public string InputBuffer;

            public NixStream StdIn;
            // Use this for initialization
            void Start()
            {
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

                InputBuffer = "";
                Lua.LuaOptions opts = new Lua.LuaOptions();
                opts.ExecuteHandler = ExecuteHandler;
                StdIn = new NixStream();
                opts.StdIn = StdIn;
                //stdin.Write ("Melli\n");

                /*Thread t = new Thread(() =>
                                      {
                    Thread.Sleep(2000);
                    //Console.WriteLine("Waiting for use input: ");
                    //string input = Console.ReadLine();
                    //stdin.Write(input + "\n");
                    stdin.Write("Melli\n");
                });
                t.Start();*/

                Thread t2 = new Thread(() =>
                {
                    Lua l = new Lua(opts);
                    l.DoString(@"os.execute('hello: ')
				s = io.read('*l')
				os.execute('how are you? '..s)");
                });
                t2.Start();
            }

            void OnGUI()
            {
                if (Event.current.isKey)
                {
                    if (Event.current.character != '\0')
                    {
                        InputBuffer += Event.current.character;
                        if (Event.current.character == '\n')
                        {
                            StdIn.Write(InputBuffer);
                        }
                    }
                }
            }

            // Update is called once per frame
            void Update()
            {

            }
        }

    }
}