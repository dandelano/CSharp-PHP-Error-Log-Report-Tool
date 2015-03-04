using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHP_LogManager
{
    public class LogFileIO
    {
        public class CurrentState
        {
            public string Line;
            public int LineCount;
        }

        public string LogFilePath;

        
        public void ReadFile(System.ComponentModel.BackgroundWorker worker,System.ComponentModel.DoWorkEventArgs e)
        {
            CurrentState state = new CurrentState();
            string buffer = String.Empty;
            string nlReplace = "";

            if(LogFilePath == null || LogFilePath == String.Empty)
            {
                throw new Exception("No File Path Given");
            }

            // Lets get the length so that when we are reading we know
            // when we have hit a "milestone" and to update the progress bar.
            FileInfo fileSize = new FileInfo(LogFilePath);
            long size = fileSize.Length;

            // Next we need to know where we are at and what defines a milestone in the
            // progress. So take the size of the file and divide it into 100 milestones
            // (which will match our 100 marks on the progress bar.
            long currentSize = 0;
            long incrementSize = (size / 100);

            // Open the big text file with open filemode access.
            using (StreamReader stream = new StreamReader(new FileStream(LogFilePath, FileMode.Open)))
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();

                // Read through the file until end of file
                while (!stream.EndOfStream)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }
                    else
                    {
                        // Add to the current position in the file
                        buffer = stream.ReadLine();
                        currentSize += buffer.Length;
                        //Console.WriteLine(buffer.FirstOrDefault());
                        if (buffer.FirstOrDefault() == '[')
                        {
                            // If it is the first line, do not add newline char
                            if(builder.Length == 0)
                                builder.Append(buffer);
                            else
                                builder.Append(System.Environment.NewLine + buffer);

                            // We add to linecount when a valid start to entry is found
                            state.LineCount++;
                        }
                        else
                        {
                            //remove any newlines
                            buffer.Replace("\r\n", nlReplace).Replace("\n", nlReplace).Replace("\r", nlReplace);
                            builder.Append(buffer);
                        }
                       
                        // Once we hit a milestone, subtract the milestone value and
                        // call our delegate we defined above.
                        // We must do this through invoke since progressbar was defined in the other
                        // thread.
                        if (currentSize >= incrementSize)
                        {
                            // add all lines to current state
                            //state.Line = builder.ToString();

                            // reset 
                            currentSize -= incrementSize;
                            //builder.Clear();
                            worker.ReportProgress(0, state);
                        }
                    }
                }
                // add all lines to current state
                state.Line = builder.ToString();
                // Close the stream and show we are done.
                // At the end of this ends the run of our thread.
                stream.Close();
            }
            // Report complete 
            worker.ReportProgress(0, state);
        }
    }
}
