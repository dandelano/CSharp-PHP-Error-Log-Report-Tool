using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace PHP_LogManager
{
    public partial class MainForm : Form
    {
        private string strResults;
        private int intCount;

        public MainForm()
        {
            InitializeComponent();
        }

        #region Button Click functions
        private void btnOpenLog_Click(object sender, EventArgs e)
        {
            fdLog.FileName = "";
            fdLog.Filter = "All Files (*.*)|*.*|Text Files (*.txt)|*.txt";
            fdLog.FilterIndex = 1;
            fdLog.Multiselect = false;
            fdLog.ShowDialog();
        }

        private void btnRunReport_Click(object sender, EventArgs e)
        {
            ReportGenerator report = new ReportGenerator();
            report.FileContents = tbResults.Text;
            bgWorkerRunReport.RunWorkerAsync(report);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            tbResults.Text = String.Empty;
            lblCount.Text = "0";
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

        #region FileDialog Ok Click
        private void fdLog_FileOk(object sender, CancelEventArgs e)
        {
            // Reset UI before loading file
            tbResults.Text = String.Empty;
            lblCount.Text = "0";

            LogFileIO logFile = new LogFileIO();
            logFile.LogFilePath = fdLog.FileName;
            bgWorkerOpenLog.RunWorkerAsync(logFile);
        }
        #endregion

        #region Progress Bar
        private void UpdateProgress()
        {
            btnCancel.Visible = true;
            progressBar1.Visible = true;
            
            // Max amount allowed is 100
            if(progressBar1.Value < 100)
                progressBar1.Value += 1;
        }

        private void ResetBar()
        {
            progressBar1.Value = 0;
            progressBar1.Visible = false;
            btnCancel.Visible = false;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.bgWorkerOpenLog.CancelAsync();
        }
        #endregion

        #region Open Log Worker
        private void bgWorkerOpenLog_DoWork(object sender, DoWorkEventArgs e)
        {
            System.ComponentModel.BackgroundWorker worker;
            worker = (System.ComponentModel.BackgroundWorker)sender;
            LogFileIO log = (LogFileIO)e.Argument;
            log.ReadFile(worker, e);
        }

        private void bgWorkerOpenLog_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            LogFileIO.CurrentState state = (LogFileIO.CurrentState)e.UserState;
            this.strResults = state.Line;
            this.intCount = state.LineCount;
            this.UpdateProgress();
        }

        private void bgWorkerOpenLog_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // This event handler is called when the background thread finishes. 
            // This method runs on the main thread. 
            if (e.Error != null)
                MessageBox.Show("Error: " + e.Error.Message);
            else if (e.Cancelled)
                MessageBox.Show("Open file canceled.");
            else
            {
                this.tbResults.Text = strResults;
                this.lblCount.Text = intCount.ToString();
                MessageBox.Show("Finished Importing Log File.");
            }
            // Reset the progress bar
            ResetBar();
        }
        #endregion

        #region Run Report Worker
        private void bgWorkerRunReport_DoWork(object sender, DoWorkEventArgs e)
        {
            System.ComponentModel.BackgroundWorker worker;
            worker = (System.ComponentModel.BackgroundWorker)sender;
            ReportGenerator report = (ReportGenerator)e.Argument;
            report.RunReport(worker, e);
        }

        private void bgWorkerRunReport_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ReportGenerator.CurrentState state = (ReportGenerator.CurrentState)e.UserState;
            this.strResults = state.Report;
            for (int i = 0; i < state.ProgressCount; i++)
                this.UpdateProgress();
        }

        private void bgWorkerRunReport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // This event handler is called when the background thread finishes. 
            // This method runs on the main thread. 
            if (e.Error != null)
                MessageBox.Show("Error: " + e.Error.Message);
            else if (e.Cancelled)
                MessageBox.Show("Report canceled.");
            else
            {
                this.tbResults.Text = strResults;
                MessageBox.Show("Finished Importing Log File.");
            }

            // Reset the progress bar
            ResetBar();
        }
        #endregion

    }// end of class
}//end namespace
