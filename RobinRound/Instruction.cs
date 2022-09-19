namespace RobinRound;

public class Instruction
{
    public InstructionType Type { get; set; }
    public int RunTime { get; set; }

    public Instruction(InstructionType type, int runTime)
    {
        Type = type;
        RunTime = runTime;
    }
}