using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Terminal : MonoBehaviour {

	public NixSystem System {get; set;}
	public int Width {get; set;}
	public int Height {get; set;}
	public List<char[]> Buffer {get; private set;}
	public int Cursor {get; set;}
	public int CursorX {get; set;}
	public int CursorY {get; set;}
	public string InputBuffer {get; set;}
    public Session Session {get; set;}
    public Program Shell {get; set;}
    protected List<byte> CommandSequence = null;

	public Terminal(int width = 80, int height = 25) {
	}
    public void Start() {
		Width = 80;
		Height = 25;
		CursorX = 0;
		CursorY = 0;

		UpdateBufferSize();
    }

	public void UpdateBufferSize() {
		if (Buffer == null) {
			Buffer = new List<char[]>();
			Buffer.Add(new char[Width]);
		}
	}

	public void BeginNewInput() {
		Write("$ ");
	}
	public void Write(byte data) {
        if (CommandSequence != null) {
            ParseCommand(data);
            return;
        }
        if (data == 0x1b) {
            CommandSequence = new List<byte>();
            return;
        }
		if (data == '\b') {
			int posX = CursorX - 1;
			int posY = CursorY;
			if (posX < 0) {
				posX = 0;
				posY--;
				if (posY < 0) {
					posY = 0;
				}
			}
			Buffer[posY][posX] = '\0';

			CursorX = posX;
			CursorY = posY;
		}
		else {
			int posX = CursorX;
			int posY = CursorY;
			if (data == '\n' || data == '\r' || posX >= Width - 1) {
				posY++;
				posX = 0;
				while (Buffer.Count <= posY) {
					Buffer.Add(new char[Width]);
				}
			}
			else {
				Buffer[posY][posX] = (char)data;
                posX++;
			}
			CursorX = posX;
			CursorY = posY;
		}
	}
    protected int GetNumber(int index, ref int result) {
        if (CommandSequence.Count <= 1) {
            return 0;
        }
        int r = 0;
        int start = index;
        int exp = 1;
        byte d = CommandSequence[index];
        while (d >= '0' && d <= '9') {
            index++;
            if (index < CommandSequence.Count) {
                d = CommandSequence[index];
            }
            else {
                break;
            }
        }
        for (int i = index - 1; i >= start; i--) {
            r += (CommandSequence[i] - '0') * exp;
            exp *= 10;
        }
        result = r;
        return index - start;
    }
    protected void SetCursor(int column, int line) {
        CursorX = column;
        if (CursorX < 0) {
            CursorX = 0;
        }
        if (CursorX >= Width) {
            CursorX = Width - 1;
        }
        CursorY = line;
        if (CursorY < 0) {
            CursorY = 0;
        }
        
    }
    protected void ParseCommand(byte data) {
        if (CommandSequence.Count == 0) {
            if (data == '[') {
                CommandSequence.Add(data);
            }
            else {
                Debug.Log("Currently unsupported command sequence: " + (char)data);
            }
        }
        else if (CommandSequence.Count >= 1) {
            if (CommandSequence[0] == '[') {
                if (data >= '0' && data <= '9' || data == ';' || data == '?') {
                    CommandSequence.Add(data);
                }
                else {
                    int line = -1;
                    int column = -1;
                    int count = 0;
                    try
                    {
                        if (data == 'A') {
                            count = GetNumber(1, ref line);
                            if (line < 0) {
                                throw new Exception("Invalid 'A' command sequence");
                            }
                            SetCursor(CursorX, CursorY - line);
                        }
                        else if (data == 'B') {
                            count = GetNumber(1, ref line);
                            if (line < 0) {
                                throw new Exception("Invalid 'B' command sequence");
                            }
                            SetCursor(CursorX, CursorY + line);
                        }
                        else if (data == 'C') {
                            count = GetNumber(1, ref column); 
                            if (column < 0) {
                                throw new Exception("Invalid 'C' command sequence");
                            }
                            SetCursor(CursorX + column, CursorY);
                        }
                        else if (data == 'D') {
                            count = GetNumber(1, ref column); 
                            if (column < 0) {
                                throw new Exception("Invalid 'D' command sequence");
                            }
                            SetCursor(CursorX - column, CursorY);
                        }
                        else if (data == 'H' || data == 'f') {
                            byte last = CommandSequence[CommandSequence.Count - 1];
                            if (last == ';' || last == '[') {
                                line = 0;
                                column = 0;
                            }
                            else {
                                count = GetNumber(1, ref line);
                                count = GetNumber(2 + count, ref column);
                            }
                            if (line < 0 || column < 0) {
                                throw new Exception("Invalid 'H' command sequence");
                            }
                            SetCursor(column, line);
                        }
                        else {
                            Debug.Log("Unknown command sequence");
                        }
                    }
                    catch (Exception exp) {
                        Debug.Log(exp.Message);
                    }
                    finally {
                        CommandSequence = null;
                    }
                }
            }
        }
    }
	public void Write(string data) {
		for (int i = 0; i < data.Length; i++) {
			Write((byte)data[i]);
		}
	}
    public void Write(byte []data) {
        for (int i = 0; i < data.Length; i++) {
            Write(data[i]);
        }
    }
    void OnGUI() {
        string text = "";
        int max = Mathf.Min(Buffer.Count, Height);
        //int max = Mathf.Min(0, Height);
        for (int i = 0; i < max; i++) {
            if (i > 0) {
                text += "\n";
            }
            text += new string(Buffer[i]);
        }
        text += InputBuffer;
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), text);
	}

	void AddToBuffer(string input) {
		for (int i = 0; i < input.Length; i++) {
			char data = input[i];
			if (data == '\b') {
				if (InputBuffer.Length > 0) {
					InputBuffer = InputBuffer.Remove(InputBuffer.Length - 1);
				}
				input = input.Remove(i);
			}
		}
		InputBuffer += input;
	}

	// Update is called once per frame
	void Update () {
        if (Shell != null) {
            if (Shell.StdOut.Length() > 0) {
                byte []data = new byte[Shell.StdOut.Length()];
                Shell.StdOut.Read(data);
                Write(data);
            }
        }
		if (Input.inputString.Length > 0) {
			if (Input.inputString[0] == '\r' || Input.inputString[0] == '\n') {
				Write(InputBuffer);
				Write("\n");
                Shell.StdIn.Write(InputBuffer);
				InputBuffer = "";
			}
			else {
				AddToBuffer(Input.inputString);
			}
		}
	}
}
