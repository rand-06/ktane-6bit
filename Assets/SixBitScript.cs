using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;
using SixBitExtensions;

public class SixBitScript : MonoBehaviour {

    public List<TextMesh> symbols = new List<TextMesh>();
    public List<KMSelectable> hovers = new List<KMSelectable>();
    public KMSelectable button0, button1, commandScreen, binaryScreen;
    public TextMesh commandText, binaryText;
    private List<char> data = new List<char>();
    private List<int> colortable = new List<int>();
    private List<Color> colors = new List<Color>();
    private AsmHandler asmHandler = new AsmHandler();
    private bool ModuleSolved, highlighted;

    private string currentInputtedBinary = "";
    private int cursor = 0;

    private Coroutine currentCorout;

    private string currentMode = "DATA";
    private string insertionMode = "REPLACE";

    public GameObject cursorRenderer;
    

    void setCursor(int index){
        if (index == -1) cursorRenderer.SetEnabled(false);
        else{
            cursorRenderer.SetEnabled(true);
            cursorRenderer.transform.position = symbols[index%64].transform.position;
            cursor = index;
        }

    }
    void pressSymbol(int index){
        if (currentMode == "DATA") return;
        else setCursor(index + (currentMode == "PROG1"?64:0));
    }
    void hoverSymbol(int index){
        if (ModuleSolved) return;
        switch (currentMode){
            case "DATA": 
                commandScreen.text = $"e{decToOct(getInt(data[index]))}{decToOct(colorTable[index])}";
                break;
            case "PROG0":
                commandScreen.text = asmHandler.commandToString(asmHandler.program[index]);
                break;
            case "PROG1":
                commandScreen.text = asmHandler.commandToString(asmHandler.program[index+64]);
        }
    }
    void hoverSymbolEnded(int index){}
    void pressCommand(){
        //if seeing data - run code; if seeing program and input is "" - change insertion mode; if seeing program and input is not "" - clear input
        if (currentMode == "DATA"){
            currentCorout = StartCoroutine(run());
        }
        else if (currentInputtedBinary != ""){
            currentInputtedBinary = "";
            binaryScreen.text = "";
        }
        else{
            insertionMode = insertionMode == "REPLACE"?"INSERT":"REPLACE";
            commandScreen.text = insertionMode;
        }
    }
    void pressBinary(){
        //if seeing data - go to prog0, if seeing prog and input is "" - go to data, if input is "0" - delete @ cursor, input "1" - change page, else nothing
        if (currentMode == "DATA"){
            currentMode = "PROG0";
        }
        else if (currentInputtedBinary == "") currentMode = "DATA";
        else if (currentInputtedBinary == "0") deleteAtCursor();
        else if (currentInputtedBinary == "1") currentMode == "PROG0"?"PROG1":"PROG0";
        render();
    }
    void deleteAtCursor(){
        asmHandler.program.RemoveAt(cursor + (currentMode == "PROG1"?64:0));
        if (asmHandler.program.Count < 128) asmHandler.program.Add("000000000000");
    }
    void render(){
        List<char> n = currentMode == "DATA"?asmHandler.asm6.data:
            currentMode=="PROG0"?asmHandler.program.Take(64).Select(s => getChar(fromBinary(s.Substring(0,6)))).ToList():
            asmHandler.program.Take(128).TakeLast(64).Select(s => getChar(fromBinary(s.Substring(0,6)))).ToList();
        for (int i=0; i<64; i++){
            symbols[i].text = n[i].ToString();
            symbols[i].color = colors[c[i]];
        }
    }

    IEnumerator run(){
        asmHandler.init();
        List<char> dataRun = asmHandler.asm6.data.ToList();
        List<int> registersRun = asmHandler.asm6.registers.ToList();
        int pointerRun = asmHandler.asm6.pointer;
        int nonPointerCounter = 0, pointerCounter = 0;
        
        while (true){
            bool run = asmHandler.run;
            if (!run){
                GetComponent<KMBombModule>().HandleStrike();
                yield break;
            }
            if (
                asmHandler.asm6.data.All((x,i)=>x==dataRun[i]) &&
                asmHandler.asm6.registers.All((x,i)=>x==registersRun[i])
            ){
                if (asmHandler.asm6.pointer == pointerRun) pointerCounter++;
                else pointerCounter = 0;
                nonPointerCounter++;
            }
            else{
                nonPointerCounter = 0;
                pointerCounter = 0;
            }

            if(pointerCounter >= 8 || nonPointerCounter >= 128){
                
            }
            
            yield return new WaitForSeconds(.2f);
        }
    }

    void initBoard(){
        colors = Enumerable.Range(0,64).Select(i => new Color(i / 16 / 3f, ((i / 4) % 4) / 3f, (i % 4) / 3f)).ToList();
        for (int i=0; i<64; i++)
        {
            char n = getChar(UnityEngine.Random.Range(1, 64));
            data.Add(n);
            int c = UnityEngine.Random.Range(1, 64);
            colortable.Add(c);
            int i1=i;
            hovers[i1].OnInteract += delegate{pressSymbol(i1); return false;};
            hovers[i1].OnHighlight += delegate{hoverSymbol(i1); return false;};
            hovers[i1].OnHighlightEnded += delegate{hoverSymbolEnded(i1); return false;};
        }
        
    }

    void Start () {
        GetCompomnent<KMSelectable>().OnFocus+=delegate{highlighted = true;};
        GetCompomnent<KMSelectable>().OnDefocus+=delegate{highlighted = false;};
        commandScreen.OnInteract += delegate{pressCommand(); return false;};
        binaryScreen.OnInteract += delegate{pressBinary(); return false;};
        initBoard();
        render();
        command.text = "";
        binary.text = "";
	}
	
	
	void Update () {
		if (ModuleSolved || !highlighted) return;
        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Numpad0)) button0.OnInteract();
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Numpad1)) button1.OnInteract();}
}
