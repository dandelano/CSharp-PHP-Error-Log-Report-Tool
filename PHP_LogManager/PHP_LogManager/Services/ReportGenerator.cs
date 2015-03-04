using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHP_LogManager
{
    public class ReportGenerator
    {
        public class CurrentState
        {
            public string Report;
            public int ProgressCount;
        }

        public string FileContents;

        public void RunReport(System.ComponentModel.BackgroundWorker worker, System.ComponentModel.DoWorkEventArgs e)
        {
            CurrentState state = new CurrentState();

            // Get the list of parsed entries
            List<LogEntry> logEntries = CreateList();
            // Creating list is about 20% of progress
            state.ProgressCount = 20;
            worker.ReportProgress(0, state);

            // Use builder to create the report
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("---------------------------------------------");
            builder.AppendLine("------- Error Level Counts -------");
            builder.AppendLine("---------------------------------------------");
            // Keep a total count
            int totalCount = 0;
            // Get the names of the error levels and their counts
            foreach (var entry in logEntries.GroupBy(info => info.Level)
                                            .Select(group => new { Metric = group.Key, Count = group.Count()})
                                            .OrderBy(x => x.Metric))
            {
                builder.AppendLine(string.Format("{0}: {1}", entry.Metric, entry.Count));
                totalCount += entry.Count;
            }
            builder.AppendLine("Total: " + totalCount);
            builder.AppendLine("---------------------------------------------");
            builder.AppendLine("------- Entries by Counts -------");
            builder.AppendLine("---------------------------------------------");
            // Set the report string and report back to UI
            state.Report = builder.ToString();
            worker.ReportProgress(0, state);

            // Get the names of the error levels and their counts
            foreach (var entry in logEntries.GroupBy(info => info.Message)
                                            .Select(group => new { Metric = group.Key, Count = group.Count() })
                                            .OrderByDescending(x => x.Count))
            {
                builder.AppendLine(string.Format("{0}: {1}", entry.Count, entry.Metric));
                totalCount += entry.Count;
            }
            // Set the report string and report back to UI
            state.Report = builder.ToString();
            worker.ReportProgress(0, state);

            Console.WriteLine("total: " + totalCount);
        }

        #region Helpers
        private List<LogEntry> CreateList()
        {
            List<LogEntry> allEntries = new List<LogEntry>();

            // Split the TextBox string into a List
            string[] allLines = FileContents.Split(new string[] { Environment.NewLine, "\r\n", "\n" }, StringSplitOptions.None);

            // Iterate over all split lines
            for (int i = 0; i < allLines.Count(); i++)
            {
                // If the line starts with a date, parse it and add to list
                if (allLines[i].StartsWith("["))
                {
                    allEntries.Add(ParseEntry(allLines[i]));
                }
            }

            // Debug check
            Console.WriteLine("Array Size: " + allLines.Count());
            Console.WriteLine("Entries with '[': " + allEntries.Count());

            return allEntries;
        }
        private LogEntry ParseEntry(string line)
        {
            // Create new object with defaults
            LogEntry thisEntry = new LogEntry();

            // Parse date entries '[<--date-->]'
            int indexOfEnd = line.IndexOf(']', 0, 55);
            if(indexOfEnd != -1)
            {
               // If date found, extract it
               thisEntry.Date = line.Substring(1, indexOfEnd - 1);
               line = line.Remove(0, indexOfEnd + 1).TrimStart();
            }

            // Parse error levels 'PHP Notice:'
            indexOfEnd = line.IndexOf(':', 0, 30);
            if(indexOfEnd != -1)
            {
                int indexOfStart = line.IndexOf("PHP ") != -1 ? 4 : 0;

                thisEntry.Level = line.Substring(indexOfStart, indexOfEnd - indexOfStart);
                line = line.Remove(0, indexOfEnd + 1).TrimStart();
            }

            // Parse line number from end 'line 1181'
            indexOfEnd = line.LastIndexOf("on line");
            if(indexOfEnd != -1)
            {
                // set the start to the end of the found string, and the end is the end of the string
                int indexOfStart = indexOfEnd + 8;
                indexOfEnd = line.Length;

                // Extract the line number
                thisEntry.LineNumber = line.Substring(indexOfStart, indexOfEnd - indexOfStart);
                
                // Remove the whole "on line ??" string and the left over string is the message
                line = line.Remove(indexOfStart - 8, indexOfEnd - (indexOfStart -8));
            }

            // What is left should be the message
            thisEntry.Message = line;

            return thisEntry;
        }
        #endregion
    }
}
