using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bourne.BatchLoader.Pipeline;

namespace Bourne.BatchLoader
{
    internal static class AppExtensions
    {
        public static Task[] CreatePump<TIn, TOut>(
            this PipelineQueue<TIn> queue, int threadCount,
            Func<IPipelineTask<TIn, TOut>> pipeFactory,
            Action<TOut> action)
        {
            var tasks = new Task[threadCount];
            for (var i = 0; i < threadCount; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    var p = pipeFactory();
                    await p.Start();
                    await foreach (var item in queue.GetConsumingEnumerable())
                    {
                        var result = await p.Execute(item);
                        action?.Invoke(result);
                    }
                    await p.End();
                });
                Console.WriteLine($"Completed Pump {queue}::{i}");
            }
            return tasks;
        }

        public static void FastWrite(this TextWriter writer, int value)
        {
            if (value == 0)
            {
                writer.Write('0');
                return;
            }

            var buffer = new char[12];
            var p = 12 - 1;
            var a = value < 0 ? -value : value;

            while (a != 0)
            {
                var v = a / 10;
                buffer[p--] = (char)(a - (v * 10) + '0');
                a = v;
            }
            if (value < 0) buffer[p--] = '-';

            writer.Write(buffer, p + 1, (12 - 1) - p);
        }
        public static void FastWrite(this TextWriter writer, short value)
        {
            if (value == 0)
            {
                writer.Write('0');
                return;
            }

            var buffer = new char[8];
            var p = 8 - 1;
            var a = value < 0 ? -value : value;

            while (a != 0)
            {
                var v = a / 10;
                buffer[p--] = (char)(a - (v * 10) + '0');
                a = v;
            }
            if (value < 0) buffer[p--] = '-';

            writer.Write(buffer, p + 1, (8 - 1) - p);
        }

        public static void FastWrite(this TextWriter writer, long value)
        {
            if (value == 0)
            {
                writer.Write('0');
                return;
            }

            var buffer = new char[32];
            var p = 32 - 1;
            var a = value < 0 ? -value : value;

            while (a != 0)
            {
                var v = a / 10;
                buffer[p--] = (char)(a - (v * 10) + '0');
                a = v;
            }
            if (value < 0) buffer[p--] = '-';

            writer.Write(buffer, p + 1, (32 - 1) - p);
        }

        public static void FastWrite(this TextWriter writer, DateTime value)
        {
            var buffer = new char[32];
            X(buffer, 01, 4, value.Year + 10000);
            X(buffer, 06, 2, value.Month + 10000);
            X(buffer, 09, 2, value.Day + 10000);
            X(buffer, 12, 2, value.Hour + 10000);
            X(buffer, 15, 2, value.Minute + 10000);
            X(buffer, 18, 2, value.Second + 10000);
            buffer[0] = '"';
            buffer[5] = '-';
            buffer[8] = '-';
            buffer[11] = ' ';
            buffer[14] = ':';
            buffer[17] = ':';
            buffer[20] = '"';
            writer.Write(buffer, 0, 21);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void X(char[] b, int p, int l, int a)
        {
            while (l-- > 0)
            {
                var v = a / 10;
                b[p + l] = (char)(a - (v * 10) + '0');
                a = v;
            }
        }

        public static void TextWrite(this TextWriter writer, string s)
        {
            if (s.IndexOf('"') == -1)
            {
                writer.Write('"');
                writer.Write(s);
                writer.Write('"');
                return;
            }
            writer.Write('"');
            writer.Write(s.Replace("\"", "\"\""));
            writer.Write('"');
        }
    }
}
