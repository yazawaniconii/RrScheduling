using RobinRound;

namespace WebApp.Services;

public class Scheduling : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task.Run(() => RobinRound.Scheduling.Instant.ScheduleCpu(), stoppingToken);
        Task.Run(() => RobinRound.Scheduling.Instant.ScheduleBlockList(InstructionType.Input), stoppingToken);
        Task.Run(() => RobinRound.Scheduling.Instant.ScheduleBlockList(InstructionType.Output), stoppingToken);
        Task.Run(() => RobinRound.Scheduling.Instant.ScheduleBlockList(InstructionType.Wait), stoppingToken);
        return Task.CompletedTask;
    }
}