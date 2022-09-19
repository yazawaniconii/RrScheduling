using System.Text.Json;
using RobinRound;

namespace WebApp.Models;

public class GetInfo
{
    public static Task<string> GetAllInfo()
    {
        var sc = Scheduling.Instant;
        var aq = new AllQueue(
            sc.CurrentPcb?.Value,
            sc.ReadyQueue.ToList(),
            sc.WaitQueue.ToList(),
            sc.InputQueue.ToList(),
            sc.OutputQueue.ToList(),
            sc.AllQueue.ToList(),
            sc.IsStop,
            sc.IsPause
        );
        var js = JsonSerializer.Serialize(aq);
        return Task.FromResult(js);
    }

    private class AllQueue
    {
        internal AllQueue(Pcb? currentProcess,List<Pcb> readyQueue, List<Pcb> waitQueue, List<Pcb> inputQueue,
            List<Pcb> outputQueue, List<Pcb> queue, bool isStop, bool isPause)
        {
            ReadyQueue = readyQueue;
            WaitQueue = waitQueue;
            InputQueue = inputQueue;
            OutputQueue = outputQueue;
            Queue = queue;
            IsStop = isStop;
            IsPause = isPause;
            CurrentProcess = currentProcess;
        }

        public bool IsStop { get; set; }
        public bool IsPause { get; set; }

        public Pcb? CurrentProcess { get; set; }
        public List<Pcb> Queue { get; set; }
        public List<Pcb> ReadyQueue { get; set; }
        public List<Pcb> WaitQueue { get; set; }
        public List<Pcb> InputQueue { get; set; }
        public List<Pcb> OutputQueue { get; set; }
    }
}