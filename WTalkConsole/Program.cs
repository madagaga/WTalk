using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTalk;

namespace WTalkConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client();            
            client.Connect();
            Console.Read();

        }
    }
}
