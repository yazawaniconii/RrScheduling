namespace RobinRound;

public class Scheduling
{
    /*
     *  using singleton pattern
     *  references:
     *      https://www.cnblogs.com/willick/p/13399194.html
     *      https://www.jianshu.com/p/3ae1bd656c1f
     */
    public static Scheduling Instant { get; } = new Scheduling();

    public LinkedList<Pcb> AllQueue { get; private set; } = new();

    public LinkedList<Pcb> ReadyQueue { get; private set; } = new();
    public LinkedList<Pcb> InputQueue { get; private set; } = new();
    public LinkedList<Pcb> OutputQueue { get; private set; } = new();
    public LinkedList<Pcb> WaitQueue { get; private set; } = new();
    public LinkedListNode<Pcb>? CurrentPcb { get; private set; }

    public int TimeSlice { get; set; } = 500;

    public bool IsStop { get; set; } = false;
    public bool IsPause { get; set; } = false;
    private volatile bool _isInit = false;
    private static readonly object AllQueueLock = new();
    private static readonly object ReadyQueueLock = new();
    private static readonly object InputQueueLock = new();
    private static readonly object OutputQueueLock = new();
    private static readonly object WaitQueueLock = new();

    public void ScheduleCpu()
    {
        while (!IsStop)
        {
            // 队列为空或者设为暂停时，自旋等待
            SpinWait.SpinUntil(() => ReadyQueue.Count != 0 && !IsPause);
            lock (ReadyQueueLock)
            {
// #if DEBUG
//                 Console.WriteLine(ReadyQueue.Count);
//                 Console.WriteLine(InputQueue.Count);
//                 Console.WriteLine(OutputQueue.Count);
//                 Console.WriteLine(WaitQueue.Count);
// #endif
                // acquire the first process node from the ReadyQueue
                if (ReadyQueue.First != null)
                    CurrentPcb = ReadyQueue.First;
                else
                    continue;

                ReadyQueue.Remove(CurrentPcb);

                // check whether the current process is done
                if (CurrentPcb.Value.CurrentInstruction.Type == InstructionType.Halt)
                {
#if DEBUG
                    Console.WriteLine($"finished process {CurrentPcb.Value.Name}");
#endif
                    continue;
                }
            }

            if (CurrentPcb.Value.CurrentInstruction.Type != InstructionType.Calculation)
            {
                AppendSpecificQueue(CurrentPcb);
                continue;
            }

            if (CurrentPcb.Value.RemainTime > 0)
            {
#if DEBUG
                Console.WriteLine(
                    $"running p{CurrentPcb.Value.Name}" +
                    $" {CurrentPcb.Value.CurrentInstruction.RunTime}" +
                    $" {CurrentPcb.Value.RemainTime}");
#endif

                CurrentPcb.Value.RemainTime--;
                Thread.Sleep(TimeSlice);

                lock (ReadyQueueLock)
                {
                    ReadyQueue.AddLast(CurrentPcb);
                }
            }
            else
            {
                CurrentPcb.Value.SetNextInstruction();
                if (CurrentPcb.Value.CurrentInstruction.Type != InstructionType.Calculation)
                {
                    AppendSpecificQueue(CurrentPcb);
                }
                else
                {
                    lock (ReadyQueueLock)
                    {
                        ReadyQueue.AddLast(CurrentPcb);
                    }
                }
            }
        }
    }


    public void ScheduleBlockList(InstructionType type)
    {
        switch (type)
        {
            case InstructionType.Wait:
                _scheduleBlockList(WaitQueue, type, WaitQueueLock);
                break;
            case InstructionType.Input:
                _scheduleBlockList(InputQueue, type, InputQueueLock);
                break;
            case InstructionType.Output:
                _scheduleBlockList(OutputQueue, type, OutputQueueLock);
                break;
            default:
                Console.WriteLine("The block list is undefined");
                break;
        }
    }

    private void _scheduleBlockList(LinkedList<Pcb> list, InstructionType type, in object listLock)
    {
        while (!IsStop)
        {
            SpinWait.SpinUntil(() => list.Count != 0 && !IsPause);
            var currentNode = list.First;
            while (currentNode != null)
            {
                if (currentNode.Value.CurrentInstruction.Type == type)
                {
                    if (currentNode.Value.RemainTime > 0)
                    {
                        currentNode.Value.RemainTime--;
                    }
                    else
                    {
                        currentNode.Value.SetNextInstruction();
                        continue;
                    }
                }
                else
                {
                    // if currentNode is the last one, just move it to another queue and break loop
                    if (currentNode.Next == null)
                    {
                        lock (listLock)
                        {
                            list.Remove(currentNode);
                        }

                        AppendSpecificQueue(currentNode);
                        break;
                    }

                    // to avoid breaking the LinkedList
                    // save the currentNode and set currentNode to the next
                    var originalNode = currentNode;
                    currentNode = currentNode.Next;
                    lock (listLock)
                    {
                        list.Remove(originalNode);
                    }

                    AppendSpecificQueue(originalNode);

                    continue;
                }

                currentNode = currentNode.Next;
            }

#if DEBUG
            for (var node = list.First; node != null; node = node.Next)
            {
                Console.WriteLine(
                    "\t\t\t\t\t" +
                    $" waiting p{node.Value.Name}" +
                    $" {type.ToString()}" +
                    $" {node.Value.CurrentInstruction.RunTime}" +
                    $" {node.Value.RemainTime}");
            }

            Console.WriteLine("\t\t\t\t\t waited a time slice");
#endif

            Thread.Sleep(TimeSlice);
        }
    }

    public void AppendSpecificQueue(LinkedListNode<Pcb> pcbNode)
    {
        switch (pcbNode.Value.CurrentInstruction.Type)
        {
            case InstructionType.Input:
                _appendQueue(InputQueue, pcbNode, in InputQueueLock);
                break;
            case InstructionType.Output:
                _appendQueue(OutputQueue, pcbNode, in OutputQueueLock);
                break;
            case InstructionType.Wait:
                _appendQueue(WaitQueue, pcbNode, in WaitQueueLock);
                break;
            case InstructionType.Calculation:
                _appendQueue(ReadyQueue, pcbNode, in ReadyQueueLock);
                break;
            case InstructionType.Halt:
                break;
            default:
                Console.WriteLine("Instruction type is undefined");
                break;
        }
    }

    private static void _appendQueue(LinkedList<Pcb> list, LinkedListNode<Pcb> node, in object listLock)
    {
        lock (listLock)
        {
            list.AddLast(node);
        }
    }

    public void AddProcess(Pcb pcb)
    {
        if (!_isInit)
        {
            _isInit = true;
        }

        _appendQueue(AllQueue, new LinkedListNode<Pcb>(pcb), in AllQueueLock);
        AppendSpecificQueue(new LinkedListNode<Pcb>(pcb));
    }

    public void AddProcess(LinkedListNode<Pcb> node)
    {
        if (!_isInit)
        {
            _isInit = true;
        }

        _appendQueue(AllQueue, node, in AllQueueLock);
        AppendSpecificQueue(node);
    }
    public void AddProcesses(LinkedList<Pcb> list)
    {
        foreach (var pcb in list)
        {
            AddProcess(pcb);
        }
    }

    public void InitQueue(LinkedList<Pcb> list)
    {
        if (_isInit)
        {
            return;
        }

        AddProcesses(list);

        _isInit = true;
    }

    public void Cleanup()
    {
        IsPause = true;
        AllQueue.Clear();
        ReadyQueue.Clear();
        InputQueue.Clear();
        WaitQueue.Clear();
        OutputQueue.Clear();
        IsPause = false;
    }

    private Scheduling()
    {
    }

    static Scheduling()
    {
    }
}