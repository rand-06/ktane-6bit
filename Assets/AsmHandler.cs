using SixBitExtensions;

public class AsmHandler : MonoBehaviour{
    private ASM6 asm6;
    private List<string> program;

    public AsmHandler(){
        asm6 = new ASM6();
        program = Enumerable.Repeat("000000000000",128);
    }

    private bool feed(string bin){
        if (bin[0] == '1'){
            int Rd = fromBinary(bin.Substring(4,2))+4;
            int imm6 = fromBinary(bin.Substring(6,6));
            switch (fromBinary(bin.Substring(1,3))){
                case 0: asm6.LDI(Rd, imm6); break;
                case 1: asm6.ADDI(Rd, imm6); break;
                case 2: asm6.SUBI(Rd, imm6); break;
                case 3: asm6.CMPI(Rd, imm6); break;
                case 4: asm6.ANDI(Rd, imm6); break;
                case 5: asm6.ORI(Rd, imm6); break;
                case 6: asm6.XORI(Rd, imm6); break;
                default: return false;
            }
        }
        else{
            switch (fromBinary(bin.Substring(1,2))){
                case 0:
                    int Rd = fromBinary(bin.Substring(6,3));
                    int Rs = fromBinary(bin.Substring(9,3));
                    switch(fromBinary(bin.Substring(3,3))){
                        case 0: asm6.MOV(Rd, Rs); break;
                        case 1: asm6.ADD(Rd, Rs); break;
                        case 2: asm6.SUB(Rd, Rs); break;
                        case 3: asm6.CMP(Rd, Rs); break;
                        case 4: asm6.AND(Rd, Rs); break;
                        case 5: asm6.OR(Rd, Rs); break;
                        case 6: asm6.XOR(Rd, Rs); break;
                        case 7:
                            switch(Rs){
                                case 0: asm6.NOT(Rd); break;
                                case 1: asm6.LD(Rd); break;
                                case 2: asm6.ST(Rd); break;
                                case 3: asm6.LSL(Rd); break;
                                case 4: asm6.LSR(Rd); break;
                                case 5: asm6.ROL(Rd); break;
                                case 6: asm6.ROR(Rd); break;
                                default: return false;
                            }
                            break;
                    }
                    break;
                case 1:
                    int addr = fromBinary(bin.Substring(6,6));
                    switch(fromBinary(bin.Substring(3,3))){
                        case 0: asm6.JMP(addr); break;
                        case 1: asm6.BREQ(addr); break;
                        case 2: asm6.BRNE(addr); break;
                        case 3: asm6.BRCS(addr); break;
                        case 4: asm6.BRCC(addr); break;
                        case 5: asm6.BRMI(addr); break;
                        case 6: asm6.BRPL(addr); break;
                        case 7: asm6.JOP(addr); break;
                    }
                    break;
                case 2:
                    int Rd = fromBinary(bin.Substring(3,3));
                    int addr = fromBinary(bin.Substring(6,6));
                    asm6.DJZ(Rd, addr);
                    break;
                default: return false;
            }
        }
        if (asm6.counter & 63 == 63) return false;
        asm6.counter++;
    }

    private string commandToString(string bin){
        if (bin == "000000000000") return "NOP";
        if (bin[0] == '1'){
            int Rd = fromBinary(bin.Substring(4,2))+4;
            string imm6 = binToOct(bin.Substring(6,6));
            switch (fromBinary(bin.Substring(1,3))){
                case 0: return $"LDI\t(R{Rd},e{imm6})";
                case 1: return $"ADDI\t(R{Rd},e{imm6})";
                case 2: return $"SUBI\t(R{Rd},e{imm6})";
                case 3: return $"CMPI\t(R{Rd},e{imm6})";
                case 4: return $"ANDI\t(R{Rd},e{imm6})";
                case 5: return $"ORI\t(R{Rd},e{imm6})";
                case 6: return $"XORI\t(R{Rd},e{imm6})";
                default: return $"e{binToOct(bin)}?";
            }
        }
        else{
            switch (fromBinary(bin.Substring(1,2))){
                case 0:
                    int Rd = fromBinary(bin.Substring(6,3));
                    int Rs = fromBinary(bin.Substring(9,3));
                    switch(fromBinary(bin.Substring(3,3))){
                        case 0: return $"MOV\tR{Rd},R{Rs})";
                        case 1: return $"ADD\tR{Rd},R{Rs})";
                        case 2: return $"SUB\tR{Rd},R{Rs})";
                        case 3: return $"CMP\tR{Rd},R{Rs})";
                        case 4: return $"AND\tR{Rd},R{Rs})";
                        case 5: return $"OR\tR{Rd},R{Rs})";
                        case 6: return $"XOR\tR{Rd},R{Rs})";
                        case 7:
                            switch(Rs){
                                case 0: return $"NOT\tR{Rd}";
                                case 1: return $"LD\tR{Rd}";
                                case 2: return $"ST\tR{Rd}";
                                case 3: return $"LSL\tR{Rd}";
                                case 4: return $"LSR\tR{Rd}";
                                case 5: return $"ROL\tR{Rd}";
                                case 6: return $"ROR\tR{Rd}";
                                default: return $"e{binToOct(bin)}?";
                            }
                            break;
                    }
                    break;
                case 1:
                    int addr = binToOct(bin.Substring(6,6));
                    switch(fromBinary(bin.Substring(3,3))){
                        case 0: return $"JMP\te{addr}";
                        case 1: return $"BREQ\te{addr}";
                        case 2: return $"BRNE\te{addr}";
                        case 3: return $"BRCS\te{addr}";
                        case 4: return $"BRCC\te{addr}";
                        case 5: return $"BRMI\te{addr}";
                        case 6: return $"BRPL\te{addr}";
                        case 7: return $"JOP\te{addr}";
                    }
                    break;
                case 2:
                    int Rd = fromBinary(bin.Substring(3,3));
                    int addr = binToOct(bin.Substring(6,6));
                    return $"DJZ\tR{Rd},e{addr}";
                default: return $"e{binToOct(bin)}?";
            }
        }
    }

    public string registersToString() =>$"{asm6.registers.Select(x => "\tabcdefg0123456789.,!@#$ABCDEFGHIJKLMNOPQRSTUVWXYZ^&*-+=~?<>[]()"[x].ToString())
            .Aggregate((a,b)=>a+b)}:{asm6.sreg['Z']?"Z":"-"}{asm6.sreg['C']?"C":"-"}{asm6.sreg['N']?"N":"-"}";
    
    public void init(List<char> data, List<int> registers){
        asm6.counter = 0;
        program = program.Take(128).ToList();
        for (int i=0; i<data.Count; i++) asm6.data[i] = data[i];
        for (int i=0; i<registers.Count; i++) asm6.registers[i] = registers[i];
        asm6.sreg['Z'] = false;
        asm6.sreg['N'] = false;
        asm6.sreg['C'] = false;
    }

    public bool run(){
        return feed(program[asm6.counter]);
    }
}