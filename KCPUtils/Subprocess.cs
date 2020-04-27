using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace KCPUtils.Subprocess
{
    // Need to completely redo this.
    class Subprocess
    {
    }

    public class GenericConsoleApplication
    {
        public string Application { get; set; }
        public Process Proc { get; set; }

        public string LogDbpath { get; set; }

        public string LastOutput;

        private string _allContent;
        public string AllContent
        {
            get { return this._allContent; }
            set
            {
                this.LastOutput = value;
                this._allContent += value;
            }
        }
        public bool IsSetup { get; set; }

        public GenericConsoleApplication(string application, string logDb=null)
        {
            if (!this.VerifyApplication(application))
            {
                throw new System.IO.FileNotFoundException($"Application: {application} does not exist.");
            }
            var startInfo = new ProcessStartInfo
            {
                FileName = application,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            this.Proc = new Process
            {
                StartInfo = startInfo
            };
            this.LogDbpath = logDb ?? string.Empty;
        }

        static void CaptureOutput(object sender, DataReceivedEventArgs e)
        {
            ShowOutput(e.Data);
        }

        static void CaptureError(object sender, DataReceivedEventArgs e)
        {
            ShowOutput(e.Data);
        }

        static void ShowOutput(string data)
        {
            if (data != null)
            {
                Console.WriteLine(data);
            }
        }

        void AddContent(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null) { this._allContent += e.Data + '\n'; }
        }

        public bool VerifyApplication(string application)
        {
            return true;
        }

        public string TakeContent()
        {
            var currentContent = this.AllContent;
            this.AllContent = "";
            return currentContent;
        }
        
        void StoreInDb()
        {
            // using this.LogDbpath
            Console.WriteLine(this.AllContent);
        }

        public GenericConsoleApplication Start(string args)
        {
            var argsAsArray = args.Split(' ');

            // return Start(argsAsArray);
            return this;
        }

        private void Setup(bool printToConsole, bool wait_for_exit = true)
        {
            if (!this.IsSetup)
            {
                this.Proc.OutputDataReceived += AddContent;
                this.Proc.ErrorDataReceived += AddContent;

                if (printToConsole)
                {
                    this.Proc.OutputDataReceived += CaptureOutput;
                    this.Proc.ErrorDataReceived += CaptureError;

                    // I have no idea why this works?
                    // from: http://www.blackwasp.co.uk/CaptureProcessOutput.aspx
                    this.Proc.BeginOutputReadLine();
                    this.Proc.BeginErrorReadLine();

                } else
                {
                    this.Proc.StartInfo.RedirectStandardError = true;
                    this.Proc.StartInfo.RedirectStandardOutput = true;
                    this.Proc.StartInfo.CreateNoWindow = true;
                    this.Proc.StartInfo.UseShellExecute = false;
                    this.Proc.EnableRaisingEvents = true;
                }

                this.Proc.Start();

                // I have no idea why this works?
                // from: http://www.blackwasp.co.uk/CaptureProcessOutput.aspx
                this.Proc.BeginOutputReadLine();
                this.Proc.BeginErrorReadLine();

                if (wait_for_exit) { this.Proc.WaitForExit(); }

                // if (this.LogDbpath != null) { this.StoreInDb(); }

                this.IsSetup = true;
            }


        }

        public GenericConsoleApplication Start(string[] args, bool wait_for_exit = true)
        {
            var unsized = args.ToList();
            var arguments = string.Empty;
            foreach (var element in unsized) { arguments += element + " "; }

            this.Proc.StartInfo.Arguments = arguments;

            this.Proc.OutputDataReceived += CaptureOutput;
            this.Proc.OutputDataReceived += AddContent;
            this.Proc.ErrorDataReceived += CaptureError;
            
            this.Proc.Start();


            // I have no idea why this works?
            // from: http://www.blackwasp.co.uk/CaptureProcessOutput.aspx
            this.Proc.BeginOutputReadLine();
            this.Proc.BeginErrorReadLine();

            if (wait_for_exit) { this.Proc.WaitForExit(); }

            if (this.LogDbpath != null) { this.StoreInDb(); } 

            return this;
        }

        public GenericConsoleApplication StartWithArgs(string args, bool wait_for_exit = true)
        {
            var arguments = args;

            this.Proc.StartInfo.Arguments = arguments;
            this.Setup(false, wait_for_exit);

            return this;
        }

        public GenericConsoleApplication RunAndPrint(string args, bool wait_for_exit = true)
        {
            var arguments = args;

            this.Proc.StartInfo.Arguments = arguments;
            this.Setup(true, wait_for_exit);

            return this;
        }

        public GenericConsoleApplication RunAndWaitForExit(string args, bool wait_for_exit = true)
        {
            var arguments = args;

            this.Proc.StartInfo.Arguments = arguments;
            this.Setup(false, wait_for_exit);

            return this;
        }
    }

    public class Standard : GenericConsoleApplication
    {
        public Standard(string application)
            : base(application)
        {

        }

        public string RunCommand(string arguments, bool waitUntilFinished=false)
        {
            return this.StartWithArgs(arguments, waitUntilFinished).TakeContent();
        }
    }
}
