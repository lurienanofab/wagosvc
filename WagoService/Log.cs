using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WagoService
{
    public static class Log
    {
        private class Message
        {
            public string Text { get; set; }
            public string FilePath { get; set; }

            public void Write()
            {
                using (StreamWriter writer = File.AppendText(FilePath))
                {
                    Program.ConsoleWriteLine(Text);
                    string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {Text}";
                    writer.WriteLine(line);
                    writer.Close();
                }
            }
        }

        private static Task _task;
        private static CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private static CancellationToken _token;
        private static readonly BlockingCollection<Message> _queue = new BlockingCollection<Message>();

        public static bool IsStarted { get; private set; }

        public static string GetBaseDirectory()
        {
            string result = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(result))
                Directory.CreateDirectory(result);
            return result;
        }

        public static string GetServiceLogPath()
        {
            return Path.Combine(GetBaseDirectory(), "service.log");
        }

        public static string GetBlockLogPath(int blockId)
        {
            return Path.Combine(GetBaseDirectory(), string.Format("block{0}.log", blockId));
        }

        public static bool UseConsole = false;

        public static void Start()
        {
            // Delete old log files
            foreach (string p in Directory.GetFiles(GetBaseDirectory(), "*.log"))
                File.Delete(p);

            _token = _tokenSource.Token;
            _task = Task.Factory.StartNew(HandleNextMessage, _token);

            IsStarted = true;

            // Indicates when log file has started recording
            Write("Log file started.");
        }

        public static void Stop()
        {
            if (IsStarted)
            {
                _tokenSource.Cancel();
                IsStarted = false;
            }
        }

        /// <summary>
        /// Append a message to a block log (each block has its own log file).
        /// </summary>
        public static void Write(int blockId, string message)
        {
            EnqueueMessage(new Message() { Text = message, FilePath = GetBlockLogPath(blockId) });
        }

        /// <summary>
        /// Append a message to the service log.
        /// </summary>
        public static void Write(string message)
        {
            EnqueueMessage(new Message() { Text = message, FilePath = GetServiceLogPath() });
        }

        private static void EnqueueMessage(Message message)
        {
            if (IsStarted)
                _queue.Add(message);
            else
                throw new InvalidOperationException("Log.Start() has not been called.");
        }

        private static void HandleNextMessage()
        {
            bool canceled = false;

            while (!canceled)
            {
                Message message = null;

                try
                {
                    message = _queue.Take(_token);
                }
                catch (OperationCanceledException)
                {
                    canceled = true;
                    message = new Message() { Text = "Log file stopped.", FilePath = GetServiceLogPath() };
                }

                message.Write();

                if (UseConsole)
                    Console.WriteLine(message.Text);
            }
        }
    }
}
