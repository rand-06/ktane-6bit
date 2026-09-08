using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;

public class script : MonoBehaviour {

    public TextMesh[] symbols = new TextMesh[64];
    public TextMesh command;
    public TextMesh binary;
    public KMBombInfo info;
    private int[] reg = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
    private int[] fin = new int[4] { 0, 0, 0, 0 };
    private int counter = 0;
    private int sreg = 0;

    private readonly int[] invgray =
        { 0, 1, 3, 2, 7, 6, 4, 5, 15, 14, 12, 13,
            8, 9, 11, 10, 31, 30, 28, 29, 24, 25,
            27, 26, 16, 17, 19, 18, 23, 22, 20, 21,
            63, 62, 60, 61, 56, 57, 59, 58, 48, 49,
            51, 50, 55, 54, 52, 53, 32, 33, 35, 34,
            39, 38, 36, 37, 47, 46, 44, 45, 40, 41, 43, 42 };


    private const string charTable = "\tabcdefg0123456789.,!@#$ABCDEFGHIJKLMNOPQRSTUVWXYZ^&*-+=~?<>[]()";
    private const string base36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private string sn;
    private int snc = 0;
    private string code = "";
    private int[] colortable = new int[64];
    private Color[] colors = new Color[64];

    int mod(int a, int b)
    {
        if (a < 0)
        {
            while (a < 0) a += b;
            return a;
        }
        else if (a < b) return a;
        else
        {
            while (a >= b) a -= b;
            return a;
        }
    }
    string getChar(int num)
    {
        return "" + charTable[num];
    }
    int getNum(char c)
    {
        return charTable.IndexOf(c);
    }

    void NOP() { }
    void ADD(int a, int b)
    {
        sreg = 
        reg[a] += reg[b];
        reg[a] = mod(reg[a], 64);

    }       // REFRESHES SREG
    void SUB(int a, int b)
    {
        reg[a] -= reg[b];
        reg[a] = mod(reg[a], 64);
    }       // REFRESHES SREG
    void MUL(int a, int b)
    {
        int ans = reg[a] * reg[b];
        reg[0] = ans / 64;
        reg[1] = ans % 64;
    }
    void LDS(int a, int B)
    {
        reg[a] = getNum(charTable[B]);
    }
    void LDI(int a, int B)
    {
        reg[a] = B;
    }
    void STS(int a, int B)
    {
        code = code.Remove(B, 1).Insert(B, getChar(reg[a]));
    }
    void JMP(int A)
    {
        counter = A;
    }
    void AND(int a, int b)
    {
        reg[a] &= reg[b];
    }       // REFRESHES SREG
    void OR(int a, int b)
    {
        reg[a] |= reg[b];
    }       // REFRESHES SREG
    void XOR(int a, int b)
    {
        reg[a] ^= reg[b];
    }       // REFRESHES SREG
    void OUT(bool n, int fa, int b)
    {
        if (n) fin[fa] = 63 - reg[b];
        else fin[fa] = reg[b];
    }
    void LSH(int n, int a)
    {
        reg[a] = mod(reg[a] << n, 64);
    }               // REFRESHES SREG
    void RSH(int n, int a)
    {
        reg[a] >>= n;
    }       // REFRESHES SREG
    void ROL(int n, int a)
    {
        int ans = (reg[a] + (sreg&2 << 5)) << n;
        if ((ans & (1 << 6)) != 0) sreg |= 2;
        else sreg &= 2;
        for (int i=0; i<n; i++)
            if ((ans & (1 << 7 + i)) != 0) ans |= (1 << i);
            else ans &= (1 << i);
        reg[a] = mod(ans, 64);
    }       // REFRESHES SREG
    void ROR(int n, int a)
    {
        int ans = reg[a] + (sreg & 2 << 5);
        int ans2 = (ans >> n) + ((ans - (ans << n))<<(7-n));
        if ((ans & (1 << 6)) != 0) sreg |= 2;
        else sreg &= 2;
        reg[a] = mod(ans2, 64);
    }       // REFRESHES SREG
    void BREQ(int A){if ((sreg & (1 << 0)) != 0) JMP(A);}
    void BRCS(int A){if ((sreg & (1 << 1)) != 0) JMP(A);}
    void BRMI(int A){if ((sreg & (1 << 2)) != 0) JMP(A);}
    void BRVS(int A){if ((sreg & (1 << 3)) != 0) JMP(A);}
    void BRHS(int A){if ((sreg & (1 << 4)) != 0) JMP(A);}
    void BRIS(int A){if ((sreg & (1 << 5)) != 0) JMP(A);}
    void BRNE(int A){if ((sreg & (1 << 0)) == 0) JMP(A);}
    void BRCC(int A){if ((sreg & (1 << 1)) == 0) JMP(A);}
    void BRPL(int A){if ((sreg & (1 << 2)) == 0) JMP(A);}
    void BRVC(int A){if ((sreg & (1 << 3)) == 0) JMP(A);}
    void BRHC(int A){if ((sreg & (1 << 4)) == 0) JMP(A);}
    void BRIC(int A){if ((sreg & (1 << 5)) == 0) JMP(A);}
    void SEF(int A) { sreg |= A; }
    void CLF(int A) { sreg &= A; }
    void STC(int a, int B)
    {
        colortable[B] = reg[a];
    }
    void SWP(int a, int b)
    {
        int temp = reg[b];
        reg[b] = reg[a];
        reg[a] = temp;
    }
    void ANDC(int a) { colortable[counter] &= 9 * a; }
    void  ORC(int a) { colortable[counter] |= 9 * a; }
    void XORC(int a) { colortable[counter] ^= 9 * a; }
    void SETC(int a) { colortable[counter]  = 9 * a; }
    void ANDN(int a) { colortable[counter]  = 63 - (colortable[counter] & (9 * a)); }
    void ORCN(int a) { colortable[counter]  = 63 - (colortable[counter] | (9 * a)); }
    void XORN(int a) { colortable[counter]  = 63 - (colortable[counter] ^ (9 * a)); }
    void SETN(int a) { colortable[counter]  = 63 - (9 * a); }
    void CREG(int A) { sreg = colortable[A];}
    void CNRG(int A) { sreg = 63-colortable[A]; }
    void ACO(int n, int a)
    {
        int ans = reg[a];
        for (int i = 0; i < n; i++) ans ^= (ans >> 1);
        reg[a] = ans;
    }
    void IACO(int n, int a)
    {
        int ans = reg[a];
        for (int i = 0; i < n; i++) ans = invgray[ans];
        ans = reg[a];
    }
    void WRI(int A)
    {
        int ans = (base36.IndexOf(sn[snc%6]) * 36 + base36.IndexOf(sn[(snc + 1)%6]))%64;
        code = code.Remove(A, 1).Insert(A, getChar(ans));
        snc++;
    }
    void WRIN(int A)
    {
        int ans = (base36.IndexOf(sn[snc % 6]) * 36 + base36.IndexOf(sn[(snc + 1) % 6])) % 64;
        code = code.Remove(A, 1).Insert(A, getChar(63-ans));
        snc++;
    }

    // Use this for initialization
    void Start () {
        for (int i = 0; i < 64; i++)
        {
            colors[i] = new Color(i / 16 / 3f, ((i / 4) % 4) / 3f, (i % 4) / 3f);
        }
        for (int i=0; i<64; i++)
        {
            string n = getChar(Random.Range(1, 64));
            symbols[i].text = n;
            code += n;
            int c = Random.Range(1, 64);
            symbols[i].color = colors[c];
            colortable[i] = c;
        }
        command.text = "";
        binary.text = "";
        sn = info.GetSerialNumber().ToUpper();
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
