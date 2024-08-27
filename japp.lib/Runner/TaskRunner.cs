using japp.lib.Models;
using Serilog;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace japp.lib.Runner
{
    public class TaskRunner
    {
        private readonly ILogger log;
        private readonly ConfigModel myConfig;

        public TaskRunner(ILogger log, ConfigModel myConfig)
        {
            this.log = log;
            this.myConfig = myConfig;
        }

        public int Run(TaskList tasklist, string workingDir, bool useShell = false, bool runParallel = false)
        {
            var tasks = tasklist.Tasks;
            int index = 0;
            (int returncode, string stdout, string stderr) taskResult = new() { returncode = 0, stdout = "", stderr = "" }; 

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            log.Information("Run {count} tasks in {parallel}...", tasks.Count, runParallel ? "parallel" : "sequence");

            if (runParallel)
            {
                // Parallel
                ConcurrentBag<(int returncode, string stdout, string stderr)> resultCollection = new ConcurrentBag<(int, string, string)>();
                ParallelLoopResult result = Parallel.ForEach(tasks, task =>
                {
                    resultCollection.Add(Helper.RunCommand(log, task.Command, useShell, workingDir));
                });

                if (result.IsCompleted)
                {
                    // Find first failed task or default
                    taskResult = resultCollection.Where(t => t.returncode > 0).FirstOrDefault();
                    index = resultCollection.Where(t => t.returncode == 0).Count();
                }
            }
            else
            {
                // Sequence
                foreach (var task in tasks)
                {
                    log.Information("Task ({index}/{count}) {name} - {command}", ++index, tasks.Count, task.Name, task.Command);

                    taskResult = Helper.RunCommand(log, task.Command, useShell, workingDir);

                    if (taskResult.returncode != 0)
                    {
                        // Break sequence if task failed
                        break;
                    }
                }
            }

            stopwatch.Stop();

            if (taskResult.returncode != 0)
            {
                log.Error("Done {index} tasks (Returncode: {returncode}, Duration: {elapsed})", index, taskResult.returncode, stopwatch.Elapsed);
            }
            else
            {
                log.Information("Done {index} tasks (Returncode: {returncode}, Duration: {elapsed})", index, taskResult.returncode, stopwatch.Elapsed);
            }

            return taskResult.returncode;
        }
    }
}
