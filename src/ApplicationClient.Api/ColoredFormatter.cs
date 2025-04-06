
using Serilog.Events;
using Serilog.Formatting;
using System;
using System.IO;

namespace ApplicationClient.Api
{
    public class ColoredFormatter : ITextFormatter
    {
        public void Format(LogEvent logEvent, TextWriter output)
        {
            var originalColor = Console.ForegroundColor;

            switch (logEvent.Level)
            {
                case LogEventLevel.Information:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogEventLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                default:
                    Console.ForegroundColor = originalColor;
                    break;
            }

            output.Write($"[{logEvent.Timestamp:HH:mm:ss} {logEvent.Level}] {logEvent.MessageTemplate.Render(logEvent.Properties)}{Environment.NewLine}");

            if (logEvent.Exception != null)
            {
                output.Write(logEvent.Exception);
            }
        }
    }
}


