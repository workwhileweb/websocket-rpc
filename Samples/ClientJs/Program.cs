using SampleBase;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WebSocketRPC;

// ReSharper disable CommentTypo

namespace ClientJs
{
    /// <summary>
    /// Progress API.
    /// </summary>
    interface IProgressApi
    {
        /// <summary>
        /// Writes progress.
        /// </summary>
        /// <param name="progress">Progress value [0..1].</param>
        void WriteProgress(float progress);
        void WriteHeader(string msg);
    }

    /// <summary>
    /// Task API.
    /// </summary>
    class TaskApi
    {
        /// <summary>
        /// Executes long running addition task.
        /// </summary>
        /// <param name="a">First number.</param>
        /// <param name="b">Second number.</param>
        /// <returns>Result.</returns>
        // ReSharper disable once UnusedMember.Global
        public async Task<int> LongRunningTask(int a, int b)
        {
            await RPC.For<IProgressApi>(this).CallAsync(x => x.WriteHeader($"we calculating {a} + {b} = ???, it's will take times..."));

            for (var p = 0; p <= 100; p += 1)
            {
                await Task.Delay(250);
                await RPC.For<IProgressApi>(this).CallAsync(x => x.WriteProgress((float)p / 100));
            }
            
            return a + b;
        }
    }

    internal class Program
    {
        /// <summary>
        /// if access denied execute: "netsh http delete urlacl url=http://+:8001/" (delete for 'localhost', add for public address)
        /// open Index.html to run the client
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            //generate js code
            File.WriteAllText($"./../../Site/{nameof(TaskApi)}.js", RPCJs.GenerateCallerWithDoc<TaskApi>());
          
            //start server and bind its local and remote API
            var cts = new CancellationTokenSource();
            var t = Server.ListenAsync("http://localhost:8001/", cts.Token, (c, ws) =>
            {
                c.Bind<TaskApi, IProgressApi>(new TaskApi());
                //c.BindTimeout(TimeSpan.FromSeconds(100)); //close connection if there is no incommming message after X seconds
            });
            
            Console.Write("{0} ", nameof(ClientJs));

            //System.Diagnostics.Process.Start(new ProcessStartInfo(Path.GetFullPath("./../../Site/Index.html")) { UseShellExecute= true });

            AppExit.WaitFor(cts, t);
        }
    }
}