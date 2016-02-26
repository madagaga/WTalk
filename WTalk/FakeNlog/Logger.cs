using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WTalk.NLog
{

    public static class LogManager
    {
        static Dictionary<string, Logger> _internalLogger = new Dictionary<string, Logger>();

        public static Logger GetLogger(string name)
        {
            if (!_internalLogger.ContainsKey(name))
                _internalLogger.Add(name, new Logger(name));
            return _internalLogger[name];
        }
    }
    public class Logger
    {
        string _name;
        public Logger(string name)
        {
            _name = name;
        }

        internal void Info(string format, params object[] args)
        {
            write("Info : " + format, args);
        }

        internal void Error(string format, params object[] args)
        {
            write("Error : " + format, args);
        }

        internal void Debug(string format, params object[] args)         
        {
            write("Debug : " + format, args);
        }     

        void write(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(_name + " " + string.Format(format, args));
            //Console.Write(_name);
            //Console.WriteLine(format, args);
        }
    }
}
