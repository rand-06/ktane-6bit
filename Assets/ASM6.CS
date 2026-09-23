using SixBitExtensions;

public class ASM6 {
    public List<int> registers;
    public List<char> data;
    public Dictionary<char, bool> sreg;

    public ASM6(){
        registers = Enumerable.Repeat(0,8).ToList();
        data = Enumerable.Repeat('\t',128).ToList();
        sreg = new Dictionary<char, bool>{
            `{'Z', false},
            `{'C', false},
            `{'N', false}
        };
    }
    

    public void MOV(int Rd, int Rs){
        registers[Rd] = registers[Rs];
    }
    public void ADD(int Rd, int b)
    {
        registers[Rd] += registers[Rs];
        sreg['C'] ^= registers[Rd] > 63;
        registers[Rd] = mod(registers[Rd], 64);
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }
    public void SUB(int Rd, int Rs)
    {
        registers[Rd] -= registers[Rs];
        sreg['C'] ^= registers[Rd] < 0;
        registers[Rd] = mod(registers[Rd], 64);
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }

    public void CMP(int Rd, int Rs)
    {
        sreg['C'] ^= registers[Rd] < registers[Rs];
        sreg['N'] = mod(registers[Rd] - registers[Rs], 64) > 31;
        sreg['Z'] = registers[Rd] == registers[Rs];
    }
    public void AND(int Rd, int Rs)
    {
        registers[Rd] &= registers[Rs];
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }
    public void OR(int Rd, int Rs)
    {
        registers[Rd] |= registers[Rs];
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }
    public void XOR(int Rd, int Rs)
    {
        registers[Rd] ^= registers[Rs];
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }

    public void LDI(int Rd, int imm6){
        registers[Rd] = imm6;
    }
    public void ADDI(int Rd, int b)
    {
        registers[Rd] += imm6;
        sreg['C'] ^= registers[Rd] > 63;
        registers[Rd] = mod(registers[Rd], 64);
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }
    public void SUBI(int Rd, int imm6)
    {
        registers[Rd] -= imm6;
        sreg['C'] ^= registers[Rd] < 0;
        registers[Rd] = mod(registers[Rd], 64);
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }

    public void CMPI(int Rd, int imm6)
    {
        sreg['C'] ^= registers[Rd] < imm6;
        sreg['N'] = mod(registers[Rd] - imm6, 64) > 31;
        sreg['Z'] = registers[Rd] == imm6;
    }
    public void ANDI(int Rd, int imm6)
    {
        registers[Rd] &= imm6;
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }
    public void ORI(int Rd, int imm6)
    {
        registers[Rd] |= imm6;
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }
    public void XORI(int Rd, int imm6)
    {
        registers[Rd] ^= imm6;
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }

    public void NOT(int Rd){
        registers[Rd] = 63 - registers[Rd];
        sreg['N'] = registers[Rd] > 31;
        sreg['Z'] = registers[Rd] == 0;
    }
    public void LD(int Rd){
        registers[Rd] = getInt(data[registers[7]]);
    }
    public void ST(int Rd){
        data[registers[7]] = getChar(registers[Rd]);
    }


    public void LSL(int Rd)
    {
        sreg['C'] = registers[Rd] > 31;
        registers[Rd] = mod(registers[Rd] << 1, 64);
        sreg['Z'] = registers[Rd] == 0;
    }
    public void LSR(int Rd)
    {
        sreg['C'] = registers[Rd] & 1 == 1;
        registers[Rd] >>= 1;
        sreg['Z'] = registers[Rd] == 0;
    }
    public void ROL(int Rd)
    {
        registers[Rd] = registers[Rd] << 1 + sreg['C']?1:0;
        sreg['C'] = registers[Rd] > 63;
        registers[Rd] = mod(registers[Rd], 64);
        sreg['Z'] = registers[Rd] == 0;
    }
    public void ROR(int Rd)
    {
        registers[Rd] += sreg['C']?64:0;
        sreg['C'] = registers[Rd] & 1 == 1;
        registers[Rd] >>= 1;
        sreg['Z'] = registers[Rd] == 0;
    }
    public void JMP(int addr) { counter = addr + counter & 64 - 1; }
    public void BREQ(int addr){if (sreg['Z'])  JMP(addr);}
    public void BRCS(int addr){if (sreg['C'])  JMP(addr);}
    public void BRMI(int addr){if (sreg['N'])  JMP(addr);}
    public void BRNE(int addr){if (!sreg['Z']) JMP(addr);}
    public void BRCC(int addr){if (!sreg['C']) JMP(addr);}
    public void BRPL(int addr){if (!sreg['N']) JMP(addr);}
    
    public void DJZ(int Rd, int addr){
        registers[Rd] = mod(registers[Rd] - 1, 64);
        if (registers[Rd] == 0) JMP(addr);
    }
    public void JOP(int addr) { counter = counter&64==64?0:64 + addr - 1; }
}