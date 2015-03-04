using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHP_LogManager
{
    public class LogEntry
    {
        public LogEntry()
        {
            Date = "No Date Found";
            Level = "Unknown";
            Message = "No Message Found";
            LineNumber = "No Line Number";
        }

        public string Date { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string LineNumber { get; set; }
    }
}
