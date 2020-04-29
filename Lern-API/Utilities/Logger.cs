using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using log4net;

namespace Lern_API.Utilities
{
    public class Logger
    {
        public static string DefaultFormat { get; } = "{0}|{1}:{2} : {3}";
        public static string Format { get; set; } = DefaultFormat;

        private ILog Log { get; }

        public Logger(ILog logger)
        {
            Log = logger;
        }

        public static Logger GetLogger(object instance)
        {
            return GetLogger(instance.GetType());
        }

        public static Logger GetLogger(Type type)
        {
            return new Logger(LogManager.GetLogger(type));
        }

        public void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log.InfoFormat(CultureInfo.InvariantCulture, Format, file, memberName, lineNumber, message);
        }
		
        public void Warning(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log.WarnFormat(CultureInfo.InvariantCulture, Format, file, memberName, lineNumber, message);
        }

        public void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log.ErrorFormat(CultureInfo.InvariantCulture, Format, file, memberName, lineNumber, message);
        }
    }
}
