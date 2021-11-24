module WebHost.BackgroundProcessing.CronBackgroundService

open System
open System.Threading
open System.Threading.Tasks
open FSharp.Control
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open NCrontab

type CronBackgroundService(logger: ILogger<CronBackgroundService>, cronExpression: string, job: unit -> Async<unit>) =
    inherit BackgroundService()

    override _.ExecuteAsync(ct: CancellationToken) =
        let executeJobAndSleepUntilNextSchedule (schedule: CrontabSchedule) job =
            async {
                let nextExecutionTime = schedule.GetNextOccurrence(DateTime.Now)
                do! job ()
                return!
                    match nextExecutionTime.Ticks - DateTime.Now.Ticks with
                    | ticks when ticks > 0L -> Task.Delay(TimeSpan(ticks)) |> Async.AwaitTask
                    | _ -> async.Return()
            }

        let schedule =
            CrontabSchedule.Parse(cronExpression, CrontabSchedule.ParseOptions(IncludingSeconds = true))

        AsyncSeq.initInfinite (fun _ -> executeJobAndSleepUntilNextSchedule schedule job)
        |> AsyncSeq.takeWhile (fun _ -> ct.IsCancellationRequested = false)
        |> AsyncSeq.iterAsync id
        |> Async.StartAsTask
        :> Task
