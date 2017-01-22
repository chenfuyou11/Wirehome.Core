using System;

namespace HA4IoT.Contracts.Logging
{
    public enum LogSeverityLevel
    {
        Info = 10,
        Verbose = 20,
        Warning = 30,
        Error = 40
    }

    public static class Log
    {
        public static event EventHandler<MessageWithExceptionLoggedEventArgs> ErrorLogged;

        public static event EventHandler<MessageWithExceptionLoggedEventArgs> WarningLogged;

        public static event EventHandler<MessageLoggedEventArgs> InfoLogged; 

        public static ILogger Instance { get; set; }

        public static LogSeverityLevel SeverityLevel { get; set; } = LogSeverityLevel.Info;

        public static void Info(string message)
        {
            if (SeverityLevel <= LogSeverityLevel.Info)
            {
                Instance?.Info(message);
                InfoLogged?.Invoke(null, new MessageLoggedEventArgs(message));
            }
        }

        public static void Verbose(string message)
        {
            if (SeverityLevel <= LogSeverityLevel.Verbose)
            {
                Instance?.Verbose(message);
            }
        }

        public static void Warning(string message)
        {
            if (SeverityLevel <= LogSeverityLevel.Warning)
            {
                Instance?.Warning(message);
            }
        }

        public static void Warning(Exception exception, string message)
        {
            if (SeverityLevel <= LogSeverityLevel.Warning)
            {
                Instance?.Warning(exception, message);
                WarningLogged?.Invoke(null, new MessageWithExceptionLoggedEventArgs(message, exception));
            }
        }

        public static void Error(Exception exception, string message)
        {
            Instance?.Error(exception, message);
            ErrorLogged?.Invoke(null, new MessageWithExceptionLoggedEventArgs(message, exception));
        }
    }
}
