using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Genode
{
    public static class Logger
    {
        public abstract class Listener : IDisposable
        {
            protected internal TraceListener _listener;

            public void Dispose()
            {
                _listener?.Dispose();
            }
        }

        public sealed class LogWriterListener : Listener
        {
            public LogWriterListener(string name, string fileName)
                : base()
            {
                _listener = new TextWriterTraceListener(fileName, name);
            }

            public LogWriterListener(string name, Stream stream)
                : base()
            {
                _listener = new TextWriterTraceListener(stream, name);
            }
        }

        public sealed class ConsoleListener : Listener
        {
            public ConsoleListener()
                : this(false)
            {
            }

            public ConsoleListener(bool useErrorStream)
                : base()
            {
                _listener = new ConsoleTraceListener(useErrorStream);
            }
        }

        public enum Level
        {
            None = 0,
            Information = 1,
            Warning = 2,
            Error = 3
        }

        private static bool isInitialized = false;
        private static bool _useStackTrace = true;
        private static bool _useTimestamp = true;

        public static bool UseStackTrace
        {
            get
            {
                return _useStackTrace;
            }
            set
            {
                _useStackTrace = value;
            }
        }

        public static bool UseTimeStamp
        {
            get
            {
                return _useTimestamp;
            }
            set
            {
                _useTimestamp = value;
            }
        }

        public static int StackFrame
        {
            get;
            set;
        }

        static Logger()
        {
            Initialize();
        }

        public static void AddListener(Listener listener)
        {
#if DEBUG
            Debug.Listeners.Add(listener._listener);
#else
            Trace.Listeners.Add(listener._listener);
#endif
        }

        public static void RemoveListener(Listener listener)
        {
#if DEBUG
            Debug.Listeners.Remove(listener._listener);
#else
            Trace.Listeners.Remove(listener._listener);
#endif
        }

        public static void RemoveListener(string name)
        {
#if DEBUG
            Debug.Listeners.Remove(name);
#else
            Trace.Listeners.Remove(name);
#endif
        }

        private static void Initialize()
        {
            try
            {
#if DEBUG
                Debug.AutoFlush = true;
#else
                Trace.AutoFlush = true;
#endif
            }
            catch (Exception ex)
            {
                Error(ex);
            }
            finally
            {
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

                StackFrame = 1;
                isInitialized = true;
            }
        }

        public static void AddDomainHandler(AppDomain appDomain)
        {
            if (appDomain == AppDomain.CurrentDomain)
            {
                Warning("Domain is already registered.");
                return;
            }

            appDomain.UnhandledException += OnUnhandledException;
        }

        public static void RemoveDomainHandler(AppDomain appDomain)
        {
            if (appDomain == AppDomain.CurrentDomain)
            {
                Warning("Engine Application Domain cannot be unregistered.");
                return;
            }

            appDomain.UnhandledException -= OnUnhandledException;
        }

        public static void Indent()
        {
#if DEBUG
            Debug.Indent();
#else
            Trace.Indent();            
#endif
        }

        public static void Unindent()
        {
#if DEBUG
            Debug.Unindent();
#else
            Trace.Unindent();
#endif
        }

        private static void Log(string message, Level level, int stackFrameIndex)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            string header = "";

            if (UseTimeStamp)
            {
                string timestamp = string.Format("[{0}]", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                header = timestamp;
            }
            
            string levelString = string.Empty;
            switch (level)
            {
                case Level.Information: levelString = "Information"; break;
                case Level.Warning:     levelString = "  Warning  "; break;
                case Level.Error:       levelString = "   Error   "; break;
                default: break;
            }

            if (!string.IsNullOrEmpty(levelString))
            {
                header = string.Format("{0} [{1}]", header, levelString);
            }

            if (UseStackTrace || level == Level.Warning || level == Level.Error)
            {
                string traceInfo = "";
                var stackTrace = new StackTrace(true);
                var stackFrame = stackTrace.GetFrame(stackFrameIndex);
                var method = stackFrame.GetMethod();

                if (method.Name == ".ctor")
                {
                    traceInfo = string.Format("{0}()", method.DeclaringType.Name);
                }
                else
                {
                    string name = method.Name;
                    if (name.StartsWith("get_") || name.StartsWith("set_") || name.StartsWith("add_") || name.StartsWith("remove_"))
                    {
                        if (name.StartsWith("remove_"))
                        {
                            name = name.Remove(0, 7);
                        }
                        else
                        {
                            name = name.Remove(0, 4);
                        }

                        traceInfo = string.Format("{0}::{1}", method.DeclaringType.Name, name);
                    }
                    else
                    {
                        traceInfo = string.Format("{0}::{1}()", method.DeclaringType.Name, name);
                    }
                }

                traceInfo = string.Format("[Ln.{0}] {1}", stackFrame.GetFileLineNumber().ToString("000"), traceInfo);
                header = string.Format("{0} {1} -", header, traceInfo);
            }

            message = string.Format("{0} {1}", header, message);
#if DEBUG
            Debug.WriteLine(message);
#else
            Trace.WriteLine(message);
#endif
        }

        public static void Log(string message, Level level = Level.None)
        {
            Log(message, level, StackFrame + 1);
        }

        public static void Information(string message)
        {
            Log(message, Level.Information, StackFrame + 1);
        }

        public static void Information(string format, params object[] args)
        {
            Log(string.Format(format, args), Level.Information, StackFrame + 1);
        }

        public static void Warning(string message)
        {
            Log(message, Level.Warning, StackFrame + 1);
        }

        public static void Warning(string format, params object[] args)
        {
            Log(string.Format(format, args), Level.Warning, StackFrame + 1);
        }

        public static void Error(string message)
        {
            Log(message, Level.Error, StackFrame + 1);
        }

        public static void Error(string format, params object[] args)
        {
            Log(string.Format(format, args), Level.Error, StackFrame + 1);
        }

        public static void Error(Exception ex)
        {
            Log(ex.Message, Level.Error, StackFrame + 1);
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Log(ex.Message, Level.Error, StackFrame + 1);
        }
    }
}
