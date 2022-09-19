namespace RobinRound;

public class Pcb
{
    public string Name { get; set; }
    public List<Instruction> Instructions { get; set; } 
    public Instruction CurrentInstruction { get; set; } 

    private int _index = 0;
    public int RemainTime { get; set; }

    // public Pcb()
    // {
    //     Instructions = new List<Instruction>();
    //     Name = "";
    //     CurrentInstruction = Instructions[_index];
    //     RemainTime = 0;
    // }

    public Pcb(string name, List<Instruction> instructions)
    {
        Name = name;
        Instructions = instructions;
        if (instructions.Count != 0)
        {
            CurrentInstruction = instructions[0];
            RemainTime = CurrentInstruction.RunTime;
        }
        else
        {
            CurrentInstruction = new Instruction(InstructionType.Halt, 0);
            RemainTime = 0;
        }
    }

    public void SetNextInstruction()
    {
        if (_index + 1 >= Instructions.Count)
        {
            CurrentInstruction = new Instruction(InstructionType.Halt, 0);
            return;
        }

        _index++;
        CurrentInstruction = Instructions[_index];
        RemainTime = CurrentInstruction.RunTime;
    }
}