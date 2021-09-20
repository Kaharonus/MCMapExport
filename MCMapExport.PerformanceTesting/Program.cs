using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using MCMapExport.Common;
using MCMapExport.NBT;
using MCMapExport.Reader;
using MCMapExport.Reader.Models;

namespace MCMapExport.PerformanceTesting {
    internal static class Program {
        private static string[] args;

        private static int RunParser() {
            EnumHelpers.BuildBlockTypeCache();
            if (args.Length == 1) {
                Console.WriteLine("Missing 2nd arg -> path to world save");
                return 1;
            }

            var reader = WorldReader.Create(args[1], out var error);
            if (reader is null) {
                Console.WriteLine(error);
                return 1;
            }

            var sw = Stopwatch.StartNew();
            
            Parallel.For(0, 8, i => {
                reader.ReadRegion(0, i);
            });
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
            return 0;
        }

        private static async Task<int> RunQueue() {
            JobQueue<Task> queue = new();
            for (var i = 0; i < 32; i++) {
                var tmp = i;
                var j = new Job<Task>(() => {
                    Console.WriteLine($"Task: {tmp} running");
                    Task.Delay(1000).Wait();
                    return Task.CompletedTask;
                });
                j.Callback += (s, ev) => {
                    Console.WriteLine($"Task: {tmp} completed");
                };
                queue.Add(j);
            }

            await queue.WaitUntilEmpty();
            
            return 0;
        }

        private static int NBTTest() {
            if (args.Length == 1) {
                Console.WriteLine("Missing 2nd arg -> path to world save");
                return 1;
            }

            var data = File.ReadAllBytes(args[1]);
            var serializer = new NBTSerializer<RawRegion>(data, CompressionType.Uncompressed);
            var region = serializer.Serialize();

            return 0;
        }

        private static int NBTBenchy() {
            BenchmarkRunner.Run<NBTBenchy>();
            return 0;
        }
        
        private static async Task<int> Main(string[] arg) {
            args = arg;
            if (args.Length == 0) {
                Console.WriteLine("Missing 1st arg -> code to run");
                return 1;
            }

            if (!int.TryParse(args[0], out var runCase)) {
                Console.WriteLine("First arg is NaN");
                return 1;
            }

            return runCase switch {
                0 => RunParser(),
                1 => await RunQueue(),
                2 => NBTTest(),
                3 => NBTBenchy(),
                _ => 1
            };
            
        }
    }
}