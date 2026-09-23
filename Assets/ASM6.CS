public class ASM6 {
    public List<int> registers;
    public List<char> data;
    public Dictionary<char, bool> sreg = new Dictionary<char, bool>{
        {'Z', false},
        {'C', false},
        {'N', false}
    };
    private char getChar(int index) => "\tabcdefg0123456789.,!@#$ABCDEFGHIJKLMNOPQRSTUVWXYZ^&*-+=~?<>[]()"[index];
    private int getInt(char c) => "\tabcdefg0123456789.,!@#$ABCDEFGHIJKLMNOPQRSTUVWXYZ^&*-+=~?<>[]()".IndexOf(c);
    int mod(int a, int b) => b==0?0:((a%b)+b)%b;

    void MOV(int Rd, int Rs){
        registers[Rd] = registers[Rs];
    }
    void ADD(int Rd, int b)
    {
        registers[Rd] += registers[Rs];
        sreg['C'] ^= registers[Rd] > 63;
        registers[Rd] = mod(registers[Rd], 64);
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }
    void SUB(int Rd, int Rs)
    {
        registers[Rd] -= registers[Rs];
        sreg['C'] ^= registers[Rd] < 0;
        registers[Rd] = mod(registers[Rd], 64);
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }

    void CMP(int Rd, int Rs)
    {
        sreg['C'] ^= registers[Rd] < registers[Rs];
        sreg['N'] = mod(registers[Rd] - registers[Rs], 64) > 31;
        sreg['Z'] = registers[Rd] == registers[Rs];
    }
    void AND(int Rd, int Rs)
    {
        registers[Rd] &= registers[Rs];
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }
    void OR(int Rd, int Rs)
    {
        registers[Rd] |= registers[Rs];
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }
    void XOR(int Rd, int Rs)
    {
        registers[Rd] ^= registers[Rs];
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }

    void LDI(int Rd, int imm6){
        registers[Rd] = imm6;
    }
    void ADDI(int Rd, int b)
    {
        registers[Rd] += imm6;
        sreg['C'] ^= registers[Rd] > 63;
        registers[Rd] = mod(registers[Rd], 64);
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }
    void SUBI(int Rd, int imm6)
    {
        registers[Rd] -= imm6;
        sreg['C'] ^= registers[Rd] < 0;
        registers[Rd] = mod(registers[Rd], 64);
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }

    void CMPI(int Rd, int imm6)
    {
        sreg['C'] ^= registers[Rd] < imm6;
        sreg['N'] = mod(registers[Rd] - imm6, 64) > 31;
        sreg['Z'] = registers[Rd] == imm6;
    }
    void ANDI(int Rd, int imm6)
    {
        registers[Rd] &= imm6;
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }
    void ORI(int Rd, int imm6)
    {
        registers[Rd] |= imm6;
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }
    void XORI(int Rd, int imm6)
    {
        registers[Rd] ^= imm6;
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }

    void NOT(int Rd){
        registers[Rd] = 63 - registers[Rd];
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }
    void LD(int Rd){
        registers[Rd] = getInt(data[registers[7]]);
    }
    void ST(int Rd){
        data[registers[7]] = getChar(registers[Rd]);
    }


    void LSL(int Rd)
    {
        sreg['C'] = registers[Rd] > 31;
        registers[Rd] = mod(registers[Rd] << 1, 64);
        sreg['Z'] = registers[Rd] == 0;
    }
    void LSR(int Rd)
    {
        sreg['C'] = registers[Rd] & 1 == 1;
        registers[Rd] >>= 1;
        sreg['Z'] = registers[Rd] == 0;
    }
    void ROL(int Rd)
    {
        registers[Rd] = registers[Rd] << 1 + sreg['C']?1:0;
        sreg['C'] = registers[Rd] > 63;
        registers[Rd] = mod(registers[Rd], 64);
        sreg['Z'] = registers[Rd] == 0;
    }
    void ROR(int Rd)
    {
        registers[Rd] += sreg['C']?64:0;
        sreg['C'] = registers[Rd] & 1 == 1;
        registers[Rd] >>= 1;
        sreg['Z'] = registers[Rd] == 0;
    }
    void JMP(int addr) { counter = addr + counter & 64 - 1; }
    void BREQ(int addr){if (sreg['Z'])  JMP(addr);}
    void BRCS(int addr){if (sreg['C'])  JMP(addr);}
    void BRMI(int addr){if (sreg['N'])  JMP(addr);}
    void BRNE(int addr){if (!sreg['Z']) JMP(addr);}
    void BRCC(int addr){if (!sreg['C']) JMP(addr);}
    void BRPL(int addr){if (!sreg['N']) JMP(addr);}
    
    void DJZ(int Rd, int addr){
        registers[Rd] = mod(registers[Rd] - 1, 64);
        if (registers[Rd] == 0) JMP(addr);
    }
    void JOP(int addr) { counter = counter&64==64?0:64 + addr - 1; }
}