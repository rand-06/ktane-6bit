using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using SixBitExtensionsNamespace;

public class SixBitScript : MonoBehaviour
{

    public TextMesh symbol;
    public KMSelectable hover;
    public KMSelectable button0, button1, commandScreen, binaryScreen;
    public TextMesh commandText, binaryText;
    public GameObject cursorRenderer;
    
    private List<TextMesh> symbols = new List<TextMesh>();
    private List<KMSelectable> hovers = new List<KMSelectable>();
    
    private List<char> data = new List<char>();
    private List<int> colortable = new List<int>();
    private List<Color> colors = new List<Color>();
    private AsmHandler asmHandler = new AsmHandler();
    private bool ModuleSolved, highlighted;

    private string currentInputtedBinary = "";
    private int cursor;

    private Coroutine currentCorout;

    private string currentMode = "DATA";
    private string insertionMode = "REPLACE";

    

    void setCursor(int index){
        if (index == -1)
        {
            cursorRenderer.SetActive(false);
            currentMode = "DATA";
        }
        else{
            cursorRenderer.SetActive(true);
            cursorRenderer.transform.position = symbols[index%64].transform.position;
            cursor = index;
            currentMode = index>63?"PROG1":"PROG0";
        }

    }
    void pressSymbol(int index){
        if (currentMode != "DATA") setCursor(index + (currentMode == "PROG1"?64:0));
    }
    void hoverSymbol(int index){
        if (ModuleSolved) return;
        if (currentCorout != null) return;
        switch (currentMode){
            case "DATA": 
                commandText.text = $"e{SixBitExtensions.decToOct(SixBitExtensions.getInt(data[index]),2)}{SixBitExtensions.decToOct(colortable[index],2)}";
                break;
            case "PROG0":
                commandText.text = asmHandler.commandToString(asmHandler.program[index]);
                break;
            case "PROG1":
                commandText.text = asmHandler.commandToString(asmHandler.program[index+64]);
                break;
            default: return;
        }
    }

    void hoverSymbolEnded()
    {
        if (currentCorout != null) return;
        if (currentMode == "DATA")
        {
            commandText.text = "";
            return;
        }
        if (currentInputtedBinary.Length == 12) commandText.text = asmHandler.commandToString(currentInputtedBinary);
        else
        {
            commandText.text = asmHandler.commandToString(asmHandler.program[cursor]);
        }
    }
    void pressCommand(){
        //if seeing data - run code; if seeing program and input is "" - change insertion mode; if seeing program and input is not "" - clear input
        //if run() - stop
        if (currentCorout != null)
        {
            StopCoroutine(currentCorout);
            currentCorout = null;
            binaryText.text = "";
            
        }
        if (currentMode == "DATA"){
            currentCorout = StartCoroutine(run());
        }
        else if (currentInputtedBinary != ""){
            currentInputtedBinary = "";
            binaryText.text = "";
        }
        else{
            insertionMode = insertionMode == "REPLACE"?"INSERT":"REPLACE";
            commandText.text = insertionMode;
        }
    }
    void pressBinary()
    {
        if (currentCorout != null) return;
        //if seeing data - go to prog0, if seeing prog and input is "" - go to data, if input is "0" - delete @ cursor, input "1" - change page, else nothing
        if (currentMode == "DATA"){
            currentMode = "PROG0";
        }
        else if (currentInputtedBinary == "") currentMode = "DATA";
        else if (currentInputtedBinary == "0") deleteAtCursor();
        else if (currentInputtedBinary == "1") currentMode=(currentMode == "PROG0") ? "PROG1" : "PROG0";
        render();
    }
    void deleteAtCursor(){
        asmHandler.program.RemoveAt(cursor + (currentMode == "PROG1"?64:0));
        if (asmHandler.program.Count < 128) asmHandler.program.Add("000000000000");
    }
    void render()
    {
        setCursor(currentMode == "DATA"?-1:cursor);
        List<char> n = currentMode == "DATA"?asmHandler.asm6.data:
            currentMode=="PROG0"?asmHandler.program.Take(64).Select(s => SixBitExtensions.getChar(SixBitExtensions.fromBinary(s.Substring(0,6)))).ToList():
            asmHandler.program.Take(128).TakeLast(64).Select(s => SixBitExtensions.getChar(SixBitExtensions.fromBinary(s.Substring(0,6)))).ToList();
        List<int> c = currentMode == "DATA"?colortable:
            currentMode=="PROG0"?asmHandler.program.Take(64).Select(s => SixBitExtensions.fromBinary(s.Substring(6,6))).ToList():
            asmHandler.program.Take(128).TakeLast(64).Select(s => SixBitExtensions.fromBinary(s.Substring(6,6))).ToList();
        for (int i=0; i<64; i++){
            symbols[i].text = n[i].ToString();
            symbols[i].color = colors[c[i]];
        }

        hoverSymbolEnded();
    }

    IEnumerator run(){
        asmHandler.init(data, Enumerable.Repeat(0,8).ToList());
        int nonPointerCounter = 0, pointerCounter = 0;
        while (true){
            List<char> dataRun = asmHandler.asm6.data.ToList();
            List<int> registersRun = asmHandler.asm6.registers.ToList();
            int pointerRun = asmHandler.asm6.counter;
            setCursor(pointerRun);
            commandText.text = asmHandler.commandToString(asmHandler.program[pointerRun]);
            binaryText.text = asmHandler.registersToString();
            render();
            bool run = asmHandler.run();
            if (!run){
                GetComponent<KMBombModule>().HandleStrike();
                yield break;
            }
            if (
                asmHandler.asm6.data.SequenceEqual(dataRun) &&
                asmHandler.asm6.registers.SequenceEqual(registersRun)
            ){
                if (asmHandler.asm6.counter == pointerRun) pointerCounter++;
                else pointerCounter = 0;
                nonPointerCounter++;
            }
            else{
                nonPointerCounter = 0;
                pointerCounter = 0;
            }

            if(pointerCounter >= 8 || nonPointerCounter >= 128){
                yield break;
            }
            
            yield return new WaitForSeconds(.2f);
        }
    }

    void initBoard(){
        colors = Enumerable.Range(0,64).Select(i => new Color(i / 16 / 3f, ((i / 4) % 4) / 3f, (i % 4) / 3f)).ToList();
        for (int i=0; i<64; i++)
        {
            char n = SixBitExtensions.getChar(UnityEngine.Random.Range(1, 64));
            data.Add(n);
            int c = UnityEngine.Random.Range(1, 64);
            colortable.Add(c);
            int i1=i;
            hovers[i1].OnInteract += delegate{pressSymbol(i1); return false;};
            hovers[i1].OnHighlight += delegate{hoverSymbol(i1);};
            hovers[i1].OnHighlightEnded += hoverSymbolEnded;
        }
        
    }

    void pressNumber(int num)
    {
        if (currentCorout != null) return;
        if (currentMode == "DATA") return;
        if (currentInputtedBinary.Length < 12)
        {
            currentInputtedBinary += num.ToString();
            binaryText.text = currentInputtedBinary.ToCharArray().Select((x,i)=>x.ToString() + (i%3==2?" ":"")).Aggregate((a, b) => a + b);
            if (currentInputtedBinary.Length == 12) commandText.text = asmHandler.commandToString(currentInputtedBinary);
        }
        else
        {
            if (num == 1)
            {
                if (insertionMode == "INSERT")
                    asmHandler.program.Insert(cursor + (currentMode == "PROG1" ? 64 : 0), currentInputtedBinary);
                else asmHandler.program[cursor + (currentMode == "PROG1" ? 64 : 0)] = currentInputtedBinary;
                if (cursor%64 != 63) setCursor(cursor+1);
            }
            commandText.text = "";
            binaryText.text = "";
            currentInputtedBinary = "";
            render();
        }
    }

    void Start () {
        symbols.Add(symbol);
        hovers.Add(hover);
        for (int i = 1; i < 64; i++)
        {
            TextMesh symbolCopy = Instantiate(symbol, transform);
            symbolCopy.transform.localPosition += new Vector3(i % 8 * .017f,0, i / 8 *-.017f);
            symbols.Add(symbolCopy);
            //KMSelectable hoverCopy = Instantiate(hover, symbolCopy.transform);
            hovers.Add(symbolCopy.GetComponentInChildren<KMSelectable>());
            //hoverCopy.transform.localPosition += new Vector3(i % 8 * .017f, 0, i / 8 *-.017f);
        }
        GetComponent<KMSelectable>().Children = Enumerable.Range(-4, 68).Select(i =>
        {
            switch (i)
            {
                case -4: return button0;
                case -3: return button1;
                case -2: return commandScreen;
                case -1: return binaryScreen;
                default: return hovers[i];
            }
        }).ToArray();
        GetComponent<KMSelectable>().UpdateChildrenProperly();
        //float offset = 0.017f;
        
        GetComponent<KMSelectable>().OnFocus+=delegate{highlighted = true;};
        GetComponent<KMSelectable>().OnDefocus+=delegate{highlighted = false;};
        commandScreen.OnInteract += delegate{pressCommand(); return false;};
        binaryScreen.OnInteract += delegate{pressBinary(); return false;};
        button0.OnInteract += delegate { pressNumber(0); return false;};
        button1.OnInteract += delegate { pressNumber(1); return false;};
        initBoard();
        asmHandler.asm6.data = data.ToList();
        asmHandler.program = Enumerable.Range(0,128).Select(_ => SixBitExtensions.randomBinary(12)).ToList();
        render();
        commandText.text = "";
        binaryText.text = "";
	}
	
	
	void Update () {
		if (ModuleSolved || !highlighted) return;
        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0)) button0.OnInteract();
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) button1.OnInteract();}
}
