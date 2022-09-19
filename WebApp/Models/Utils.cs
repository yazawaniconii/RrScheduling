using System.Text.RegularExpressions;
using RobinRound;

namespace WebApp.Models;

public class Utils
{
    public static async Task<LinkedList<Pcb>?> Parser(StreamReader stream)
    {
        var line = await stream.ReadLineAsync();
        var list = new LinkedList<Pcb>();
        string? name = null;
        List<Instruction> itList = new();
        while (line != null)
        {
            if (line.Length == 0)
            {
                line = await stream.ReadLineAsync();
                continue;
            }

            if (line[0] == 'P')
            {
                if (name != null)
                {
                    list.AddLast(new Pcb(name, itList));
                }

                name = line.Substring(1);
                itList = new List<Instruction>();
            }
            else
            {
                if (name == null)
                {
                    // error
                    return null;
                }

                switch (line[0])
                {
                    case 'H':
                        itList.Add(new Instruction(InstructionType.Halt, 0));
                        list.AddLast(new Pcb(name, itList));
                        name = null;
                        break;
                    case 'C':
                    case 'I':
                    case 'O':
                    case 'W':
                    {
                        if (IsNumeric(line.Substring(1)))
                        {
                            var type = GetTypeFromChar(line[0]);
                            var runtime = int.Parse(line.Substring(1));
                            itList.Add(new Instruction(type, runtime));
                        }
                        else
                        {
                            return null;
                        }

                        break;
                    }
                    default:
                        // error
                        return null;
                }
            }

            line = await stream.ReadLineAsync();
        }

        // in case the last process do not have a Halt
        if (name != null)
        {
            list.AddLast(new Pcb(name, itList));
        }

        return list;
    }

    public static bool IsNumeric(string value)
    {
        return Regex.IsMatch(value, @"^[0-9]*$");
    }

    public static InstructionType GetTypeFromChar(char c)
    {
        switch (c)
        {
            case 'C':
                return InstructionType.Calculation;
            case 'I':
                return InstructionType.Input;
            case 'O':
                return InstructionType.Output;
            case 'W':
                return InstructionType.Wait;
            case 'H':
                return InstructionType.Halt;
            default:
                return InstructionType.Unknown;
        }
    }
}