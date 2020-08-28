using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace backgroundworkerVsTaskRun
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Background Worker
        /// </summary>
        private BackgroundWorker _bgw;
        private void button1_Click(object sender, EventArgs e)
        {
            var fail = checkBox1.Checked;
            _bgw = new BackgroundWorker();
            var bgw = _bgw;
            bgw.WorkerSupportsCancellation = true;
            bgw.WorkerReportsProgress = true;
            bgw.DoWork += (_, args) =>
            {
                for (int i = 0; i != 100; ++i)
                {
                    //bgw.ReportProgress(0, i + "%");
                    bgw.ReportProgress(i, i + "%");
                    if (bgw.CancellationPending)
                    {
                        args.Cancel = true;
                        return;
                    }
                    Thread.Sleep(100);
                }
                if (fail)
                    throw new InvalidOperationException("Requested to fail.");
                args.Result = 13;
            };
            //bgw.ProgressChanged += (_, args) =>
            bgw.ProgressChanged += (_, args) =>
            {
                label1.Text = (string)args.UserState;
                richTextBox1.Text = richTextBox1.Text + " " + (string)args.UserState;
                progressBar1.Value = args.ProgressPercentage;
            };
            bgw.RunWorkerCompleted += (_, args) =>
            {
                if (args.Cancelled)
                {
                    label1.Text = "Cancelled.";
                }
                else if (args.Error == null)
                {
                    var result = (int)args.Result;
                    label1.Text = "Completed: " + result;
                }
                else
                {
                    label1.Text = args.Error.GetType().Name + ": " + args.Error.Message;
                }
            };
            bgw.RunWorkerAsync();
        }
        private void cancelButton1_Click(object sender, EventArgs e)    {   if (_bgw != null) _bgw.CancelAsync();   }
        /// <summary>
        /// Task.Run
        /// </summary>
        private CancellationTokenSource _cts;
        private async void button2_Click(object sender, EventArgs e)
        {
            var fail = checkBox1.Checked;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            var progressHandler = new Progress<string>(value =>
            {
                label2.Text = value;
                richTextBox2.Text = richTextBox2.Text + " " + value;
            });
            var progress = progressHandler as IProgress<string>;
            try
            {
                var result = await Task.Run(() =>
                {
                    for (int i = 0; i != 100; ++i)
                    {
                        if (progress != null)
                            progress.Report(i + "%");
                        token.ThrowIfCancellationRequested();
                        Thread.Sleep(100);
                    }
                    if (fail)
                        throw new InvalidOperationException("Requested to fail.");
                    return 13;
                });
                label2.Text = "Completed: " + result;
            }
            catch (OperationCanceledException)
            {                
                label2.Text = "Cancelled.";            
            }
            catch (Exception ex)
            {                
                label2.Text = ex.GetType().Name + ": " + ex.Message;            
            }
        }
        private void cancelButton2_Click(object sender, EventArgs e)    {   if (_cts != null) _cts.Cancel();    }

        private void bothBtn_Click(object sender, EventArgs e)
        {
            button1_Click(sender, e);
            button2_Click(sender, e);
        }

        

    }
}
