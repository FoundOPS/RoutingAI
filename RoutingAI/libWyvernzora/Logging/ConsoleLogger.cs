using System;

namespace libWyvernzora.Logging
{
    /// <summary>
    /// Logger implementation that writes log messages to the console.
    /// </summary>
    public class ConsoleLogger : Logger
    {
        // Maxumum number of characters on the same line
        private readonly Int32 splitPoint = Console.BufferWidth - 35;

        /// <summary>
        /// Constructor
        /// </summary>
        public ConsoleLogger()
        {
            // Sincenwriting stuff to console doesn't really take any resources
                // set submission batch to 1
            submissionBatch = 1;
        }

        /// <summary>
        /// Submits currently queued messages for processing
        /// </summary>
        /// <param name="messages">Array of currently queued messages</param>
        protected override void SubmitMessageQueue(LoggerEventArgs[] messages)
        {
            // Just in case if there are many mesages...
            foreach (LoggerEventArgs e in messages)
            {
                String message = e.Message;

                // Change color if necessary
                ConsoleColor foreColor = Console.ForegroundColor;
                ConsoleColor backColor = Console.BackgroundColor;

                if (e.Flags.HasFlag(MessageFlags.Fatal))
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Yellow;
                }
                else if (e.Flags.HasFlag(MessageFlags.Critical))
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.White;
                }
                else if (e.Flags.HasFlag(MessageFlags.Error))
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (e.Flags.HasFlag(MessageFlags.Warning))
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else if (e.Flags.HasFlag(MessageFlags.Routine))
                    Console.ForegroundColor = ConsoleColor.White;
                else if (e.Flags.HasFlag(MessageFlags.Trivial))
                    Console.ForegroundColor = ConsoleColor.Gray;
                else if (e.Flags.HasFlag(MessageFlags.Verbose))
                    Console.ForegroundColor = ConsoleColor.DarkGray;


                // If message fits in one line then print it as it is
                if (message.Length < splitPoint)
                    WriteMessageLine(e.TimeStamp, e.Tag, e.Message);
                else
                {
                    // Message too long, split it into multiple lines
                    Boolean firstLine = true;
                    while (message.Length != 0)
                    {
                        Int32 split = message.Substring(0, message.Length > splitPoint ? splitPoint : message.Length).LastIndexOf(' ');
                        if (split < 0) split = message.Length > splitPoint ? splitPoint : message.Length;
                        String msgLine = message.Substring(0, split);
                        message = message.Substring(message.Length > split + 1 ? split + 1 : message.Length);
                        if (firstLine)
                        {
                            WriteMessageLine(e.TimeStamp, e.Tag, msgLine);
                            firstLine = false;
                        }
                        else
                            WriteMessageLine(null, null, msgLine);
                    }
                }

                // Restore colors
                Console.BackgroundColor = backColor;
                Console.ForegroundColor = foreColor;
            }
        }

        /// <summary>
        /// Releases all resources taken by this Logger
        /// </summary>
        protected override void ReleaseResources()
        {
        }

        /// <summary>
        /// Disposes the Logger and releases all reasources
        /// associated with it
        /// </summary>
        public override void Dispose()
        {
        }

        private void WriteMessageLine(Int64? timeStamp, String tag, String message)
        {
            String time = null;
            if (timeStamp.HasValue)
            {
                DateTime dt = new DateTime(timeStamp.Value * 10000);
                time = String.Format("{0}:{1}:{2}.{3}", dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
            }

            Console.WriteLine("{0,-15}{1,-20}{2}", time != null ? time : "", tag != null ? tag : "", message);
        }
    }
}
