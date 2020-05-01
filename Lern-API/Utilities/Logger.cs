using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using log4net;
using Nancy;

namespace Lern_API.Utilities
{
    public interface ILogger
    {
        void Request(int id, string method, string route, string address, string useragent);
        void Response(int id, HttpStatusCode status);
        void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "",
            [CallerLineNumber] int lineNumber = 0);
        void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "",
            [CallerLineNumber] int lineNumber = 0);
        void Warning(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "",
            [CallerLineNumber] int lineNumber = 0);
        void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "",
            [CallerLineNumber] int lineNumber = 0);
    }

    public class Logger : ILogger
    {
        public static string DefaultFormat { get; } = "{0}|{1}:{2} : {3}";
        public static string Format { get; set; } = DefaultFormat;

        private static string GetFileName(string path) => Path.GetFileNameWithoutExtension(path.Substring(path.LastIndexOfAny(new []{'/', '\\'}) + 1));

        private ILog Log { get; }

        public Logger(ILog logger)
        {
            Log = logger;
        }

        public static ILogger GetLogger(object instance)
        {
            return GetLogger(instance.GetType());
        }

        public static ILogger GetLogger(Type type)
        {
            return new Logger(LogManager.GetLogger(type));
        }

        public void Request(int id, string method, string route, string address, string useragent)
        {
            Log.InfoFormat(CultureInfo.InvariantCulture, "[{0}] {1} {2} - {3} ({4})", id, method, route, address, useragent);
        }

        public void Response(int id, HttpStatusCode status)
        {
            Log.InfoFormat(CultureInfo.InvariantCulture, "[{0}] {1}", id, status);
        }

        public void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log.DebugFormat(CultureInfo.InvariantCulture, Format, GetFileName(file), memberName, lineNumber, message);
        }

        public void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log.InfoFormat(CultureInfo.InvariantCulture, Format, GetFileName(file), memberName, lineNumber, message);
        }
		
        public void Warning(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log.WarnFormat(CultureInfo.InvariantCulture, Format, GetFileName(file), memberName, lineNumber, message);
        }

        public void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log.ErrorFormat(CultureInfo.InvariantCulture, Format, GetFileName(file), memberName, lineNumber, message);
        }
    }
}
