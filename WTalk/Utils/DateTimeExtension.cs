using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WTalk.Core.Utils
{
    public static class DateTimeExtension
    {   
        static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime FromUnixTime(this long unixTime)
        {   
            return epoch.AddMilliseconds(unixTime/1000);
        }

        public static long ToUnixTime(this DateTime date)
        {            
            return Convert.ToInt64((date - epoch).TotalMilliseconds);
        }
    }
}
