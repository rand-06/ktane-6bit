using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;

public class script : MonoBehaviour {

    public List<TextMesh> symbols = new List<TextMesh>();
    public List<KMSelectable> hovers = new List<KMSelectable>();
    public KMSelectable button0, button1, commandScreen, binaryScreen;
    public TextMesh commandText, binaryText;
    private List<char> data = new List<char>();
    private List<int> colortable = new List<int>();
    private List<Color> colors = new List<Color>();
    private bool ModuleSolved, highlighted;

    private char getChar(int index) => "\tabcdefg0123456789.,!@#$ABCDEFGHIJKLMNOPQRSTUVWXYZ^&*-+=~?<>[]()"[index];
    
    void pressSymbol(int index){}

    void initBoard(){
        colors = Enumerable.Range(0,64).Select(i => new Color(i / 16 / 3f, ((i / 4) % 4) / 3f, (i % 4) / 3f)).ToList();
        for (int i=0; i<64; i++)
        {
            char n = getChar(UnityEngine.Random.Range(1, 64));
            symbols[i].text = n.ToString();
            data.Add(n);
            int c = UnityEngine.Random.Range(1, 64);
            symbols[i].color = colors[c];
            colortable.Add(c);
            int i1=i;
            hovers[i1].OnInteract += delegate{pressSymbol(i1); return false;};
        }
    }

    void Start () {
        GetCompomnent<KMSelectable>().OnFocus+=delegate{highlighted = true;};
        GetCompomnent<KMSelectable>().OnDefocus+=delegate{highlighted = false;};
        initBoard();
        command.text = "";
        binary.text = "";
	}
	
	
	void Update () {
		if (ModuleSolved || !highlighted) return;
        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Numpad0)) button0.OnInteract();
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Numpad1)) button1.OnInteract();
	}
}
