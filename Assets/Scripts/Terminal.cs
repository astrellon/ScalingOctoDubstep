using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class Terminal : MonoBehaviour {

	public struct BufferProperty {
		public int Colour;
        public byte ColourAttr;
		public bool Bold;
	}
	public NixSystem System {get; set;}
	public int Width {get; set;}
	public int Height {get; set;}
    public int MaxHeight {get; set;}
	public List<char[]> Buffer {get; private set;}
	public List<BufferProperty[]> BufferProperties { get; private set; }
	public int CursorX {get; set;}
	public int CursorY {get; set;}
	public int SavedCursorX {get; set;}
	public int SavedCursorY {get; set;}
	public string InputBuffer {get; set;}
    public Session CurrentSession {get; set;}
    public Program Shell {get; set;}
    protected List<byte> CommandSequence = null;
    public int ScrollX {get; set;}
    public int ScrollY {get; set;}

	private BufferProperty CurrentProperty;
	public Dictionary<string, int[]> Colours { get; private set; }

    protected GUIStyle style;

	public Terminal(int width = 80, int height = 25) {
	}
    public void Start() {
		Width = 80;
		Height = 16;
        MaxHeight = 20;
		CursorX = 0;
		CursorY = 0;
        ScrollX = 0;
        ScrollY = 0;

		Colours = new Dictionary<string, int[]> ();
		Colours ["xterm"] = new int[]{
			0x000000, 0xcd0000, 0x00cd00, 0xcdcd00, 0x0000ee, 0xcd00cd, 0x00cdcd, 0xe5e5e5,
			0x7f7f7f, 0xff0000, 0x00ff00, 0xffff00, 0x5c5cff, 0xff00ff, 0x00ffff, 0xffffff};

		CurrentProperty.Colour = Colours["xterm"][7];

        Font f = (Font)Resources.LoadAssetAtPath(@"Assets\Fonts\LiberationMono-Regular.ttf", typeof(Font));
        style = new GUIStyle();
        style.fontSize = 11;
        style.font = f;

		UpdateBufferSize();
    }

	public void UpdateBufferSize() {
		if (Buffer == null) {
			Buffer = new List<char[]>();
			Buffer.Add(new char[Width]);
		}
		if (BufferProperties == null) {
			BufferProperties = new List<BufferProperty[]> ();
			BufferProperties.Add (new BufferProperty[Width]);
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
			SetCharacter(posY, posX, '\0');

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
					BufferProperties.Add(new BufferProperty[Width]);

                    if (Buffer.Count > MaxHeight) {
                        try {
                        //Buffer.RemoveAt(0);
                        }
                        catch (Exception ex) {
                            Debug.Log("Unable to remove: " + ex.Message);
                        }
                    }
				}
                ScrollY = Buffer.Count - Height;
                if (ScrollY < 0) {
                    ScrollY = 0;
                }
			}
			else {
				SetCharacter(posY, posX, (char)data);
                posX++;
			}
			CursorX = posX;
			CursorY = posY;
		}
	}
    protected int GetCommandNumber(int index, ref int result) {
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
	protected void SetCharacter(int y, int x, char c) {
		Buffer[y][x] = c;
		BufferProperties[y][x].Colour = CurrentProperty.Colour;
		BufferProperties[y][x].Bold = CurrentProperty.Bold;
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
                        byte last = CommandSequence[CommandSequence.Count - 1];
                        // Move cursor up
                        if (data == 'A') {
                            count = GetCommandNumber(1, ref line);
                            if (line < 0) {
                                throw new Exception("Invalid 'A' command sequence");
                            }
                            SetCursor(CursorX, CursorY - line);
                        }
                        // Move cursor down
                        else if (data == 'B') {
                            count = GetCommandNumber(1, ref line);
                            if (line < 0) {
                                throw new Exception("Invalid 'B' command sequence");
                            }
                            SetCursor(CursorX, CursorY + line);
                        }
                        // Move cursor right
                        else if (data == 'C') {
                            count = GetCommandNumber(1, ref column); 
                            if (column < 0) {
                                throw new Exception("Invalid 'C' command sequence");
                            }
                            SetCursor(CursorX + column, CursorY);
                        }
                        // Move cursor left
                        else if (data == 'D') {
                            count = GetCommandNumber(1, ref column); 
                            if (column < 0) {
                                throw new Exception("Invalid 'D' command sequence");
                            }
                            SetCursor(CursorX - column, CursorY);
                        }
                        // Move cursor to position on the screen
                        else if (data == 'H' || data == 'f') {
                            if (last == ';' || last == '[') {
                                line = 0;
                                column = 0;
                            }
                            else {
                                count = GetCommandNumber(1, ref line);
                                count = GetCommandNumber(2 + count, ref column);
                            }
                            if (line < 0 || column < 0) {
                                throw new Exception("Invalid 'H' command sequence");
                            }
                            SetCursor(column, line);
                        }
                        // Clear screen
						else if (data == 'J') {
                            // Clear down from the cursor
							if (last == '[' || last == '0') {

							}
                            // Clear up from the cursor
							else if (last == '1') {

							}
                            // Clear entire screen
							else if (last == '2') {
								Buffer = null;
								BufferProperties = null;
								UpdateBufferSize();
								CursorX = 0;
								CursorY = 0;
                                ScrollY = 0;
							}
							else {
								throw new Exception("Unknown 'J' command sequence");
							}
						}
                        // Clear line
                        else if (data == 'K') {
                            // Clear to the right of the cursor
                            if (last == '[' || last == '0') {
                                for (int i = CursorX; i < Width; i++) {
									SetCharacter(CursorY, i, '\0');
                                }
                            }
                            // Clear to the left of the cursor
                            else if (last == '1') {
                                for (int i = 0; i <= CursorX; i++) {
									SetCharacter(CursorY, i, '\0');
                                }
                                CursorX = 0;
                            }
                            // Clear entire line
                            else if (last == '2') {
                                Buffer[CursorY] = new char[Width];
								BufferProperties[CursorY] = new BufferProperty[Width];
                                CursorX = 0;
                            }
                            else {
                                throw new Exception("Unknown 'K' command sequence");
                            }

                        }
                        // Save cursor position
                        else if (data == 's') {
                            SavedCursorX = CursorX;
                            SavedCursorY = CursorY;
                        }
                        // Restore cursor position
                        else if (data == 'u') {
                            CursorX = SavedCursorX;
                            CursorY = SavedCursorY;
                        }
						else if (data == 'm') {
							int attr = -1;
							int i = 1;
							while (i < CommandSequence.Count) {
								if ((char)CommandSequence[i] == ';') {
									i++;
								}
								count = GetCommandNumber(i, ref attr);
								i += count;

								if (attr == 0) {
									CurrentProperty.Bold = false;
									CurrentProperty.Colour = Colours["xterm"][7];
                                    CurrentProperty.ColourAttr = 37;
								}
								else if (attr == 1) {
									CurrentProperty.Bold = true;
                                    if (CurrentProperty.ColourAttr >= 30 && CurrentProperty.ColourAttr <= 37) {
                                        int index = CurrentProperty.ColourAttr - 30 + 8;
                                        CurrentProperty.Colour = Colours["xterm"][index];
                                    }
								}
								else if (attr == 2) {
									CurrentProperty.Bold = false;
                                    if (CurrentProperty.ColourAttr >= 30 && CurrentProperty.ColourAttr <= 37) {
                                        int index = CurrentProperty.ColourAttr - 30;
                                        CurrentProperty.Colour = Colours["xterm"][index];
                                    }
                                }
								else if (attr >= 30 && attr <= 37) {
									int bolden = CurrentProperty.Bold ? 8 : 0;
									int index = attr - 30 + bolden;
									CurrentProperty.Colour = Colours["xterm"][index];
                                    CurrentProperty.ColourAttr = (byte)attr;
								}
							}
						}
                        else {
                            Debug.Log("Unknown command sequence");
                        }
                    }
                    catch (Exception exp) {
                        Debug.Log(exp.Message + " | " + exp.StackTrace);
                    }
                    finally {
                        CommandSequence = null;
                    }
                }
            }
        }
    }
    public string StringCommandSequence() {
        string result = "";
        for (int i = 0; i < CommandSequence.Count; i++) {
            result += (char)CommandSequence[i];
        }
        return result;
    }
	public void Write(string data) {
		for (int i = 0; i < data.Length; i++) {
			Write((byte)data[i]);
		}
	}
    public void Write(byte []data) {
        for (int i = 0; i < data.Length; i++) {
            //Debug.Log("Writing to terminal (" + i + "/" + data.Length + "): " + data[i]);
            Write(data[i]);
        }
    }
	public string GetGUIString(int max) {
		int currentColour = 0xffffff;
		bool currentBold = false;
		StringBuilder builder = new StringBuilder (Width * (max - ScrollY));
		builder.Append ("<color=white>");
		for (int i = ScrollY; i < max; i++) {
			if (i > ScrollY) {
				builder.Append('\n');
			}

			char []text = Buffer[i];
			BufferProperty []props = BufferProperties[i];
			for (int j = 0; j < text.Length; j++) {
				bool bold = props[j].Bold;
				int c = props[j].Colour & 0xffffff;
				if (c != currentColour) {
					builder.Append("</color><color=#");
					builder.Append(c.ToString("x6"));
					builder.Append('>');
					currentColour = c;
				}
                /*
				if (bold != currentBold) {
					if (bold) {
						builder.Append("<b>");
					}
					else {
						builder.Append("</b>");
					}
					currentBold = bold;
				}
                */
				builder.Append(text[j]);
			}
		}
		if (CurrentSession.EchoInput()) {
			builder.Append(CurrentSession.InputBuffer);
		}
		builder.Append("</color>");
		return builder.ToString();
	}
    void OnGUI() {
        
        if (Event.current.isKey) {
            CurrentSession.KeyboardEvent(Event.current);
            if (Event.current.type == EventType.KeyDown) {
                if (Event.current.keyCode == KeyCode.PageUp) {
                    ScrollY = Mathf.Max(0, ScrollY - 1);
                }
                else if (Event.current.keyCode == KeyCode.PageDown) {
                    ScrollY = Mathf.Min(Buffer.Count - 1, ScrollY + 1);
                }
            }
        }
        int max = Mathf.Min(Buffer.Count, Height + ScrollY);
		string text = GetGUIString(max);

        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), text, style);
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
        /*
        if (Input.inputString.Length > 0) {
            CurrentSession.(Input.inputString);
        }
        */
	}
}
