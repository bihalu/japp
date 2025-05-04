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

                    foreach (var item in resultCollection)
                    {
                        if (item.returncode == 0 && !string.IsNullOrWhiteSpace(item.stdout))
                        {
                            log.Information("\n{stdout}", taskResult.stdout);
                        }
                    }
                }
            }
            else
            {
                // Sequence
                foreach (var task in tasks)
                {
                    var description = task.Description ?? task.Command;

                    log.Information("Task ({index}/{count}) {name} - {description}", ++index, tasks.Count, task.Name, description);

                    // Check if alias exists for command
                    var myCommand = task.Command.Split(' ').FirstOrDefault();
                    var alias = myConfig.Aliases.FirstOrDefault(x => x.Alias == myCommand);

                    if (alias != null)
                    {
                        // Substitute command alias
                        var args = task.Command.Substring(myCommand!.Length);
                        task.Command = alias.Command + args;

                        log.Debug("Command alias {command}", task.Command);
                    }

                    taskResult = Helper.RunCommand(log, task.Command, useShell, workingDir);

                    if (taskResult.returncode != 0)
                    {
                        // Break sequence if task failed
                        break;
                    }

                    if (!string.IsNullOrWhiteSpace(taskResult.stdout))
                    {
                        log.Information("\n{stdout}", taskResult.stdout);
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
