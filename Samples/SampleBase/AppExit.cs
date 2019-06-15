using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SampleBase
{
    internal static class AppExit
    {
        public static void WaitFor(CancellationTokenSource cts, params Task[] tasks)
        {
            if (cts == null)
                throw new ArgumentNullException(nameof(cts));

            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            Task.Run(() =>
            {
                Console.WriteLine("------Press [Enter] to stop------");
                Console.ReadLine();

                CancelTasks(cts);
            });

            WaitTasks(tasks);
        }

        static void CancelTasks(CancellationTokenSource cts)
        {
            Console.WriteLine("\nWaiting for the tasks to complete...");
            cts.Cancel();
        }

        private static void WaitTasks(IEnumerable<Task> tasks)
        {
            try
            {
                foreach (var t in tasks) t.Wait();
            }
            catch (Exception ex)
            {
                WriteError(ex);
            }
        }

        private static void WriteError(Exception ex)
        {
            switch (ex)
            {
                case null:
                    return;
                case AggregateException _:
                    ex = ex.InnerException;
                    break;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: " + ex.Message);
            Console.ResetColor();
        }
    }
}