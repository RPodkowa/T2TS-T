using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic.Helpers
{
    public class ProcessResult
    {
        public readonly bool Result;
        public string Message { get; private set; }        

        public ProcessResult(bool result, string message)
        {
            Result = result;
            Message = message;
        }

        public void AddMessage(string message)
        {
            Message += message;
        }
    }

    public class ProcessHelper
    {
        private Stopwatch ProcessStopWatch;
        private int ProcessSteps;
        private int CurrentStep;
        private string ProcessName;
        private bool ProcessInProgress = false;

        public event Action<string, double> StatusChanged;

        /// <summary>
        /// Zmiana progress bara i tekstu statusu
        /// </summary>
        /// <param name="status">tesktowy status</param>
        /// <param name="progress">precent postepu</param>
        private void ChangeStatus(string status, double progress)
        {
            StatusChanged?.Invoke(status, progress);
        }

        protected void StartProcess(string name, int steps)
        {
            ProcessStopWatch = new Stopwatch();
            ProcessStopWatch.Start();
            ProcessInProgress = true;
            ProcessName = name;
            ProcessSteps = steps;
            CurrentStep = 0;
            UpdateStatus(name);
        }

        protected void StartNextProcessStep()
        {
            CurrentStep++;
            UpdateStatus();
        }

        protected void StopProcess()
        {
            ProcessStopWatch.Stop();
            var processTimeSpan = ProcessStopWatch.Elapsed;

            string elapsedTime = string.Format(" [{0:00}:{1:00}:{2:00}.{3:00}]", processTimeSpan.Hours, processTimeSpan.Minutes, processTimeSpan.Seconds, processTimeSpan.Milliseconds / 10);
            var status = $"{ProcessName} done in {elapsedTime}";

            ProcessSteps = 1;
            CurrentStep = 1;
            UpdateStatus(status);
            ProcessInProgress = false;
        }

        private void UpdateStatus(string status = null)
        {
            if (!ProcessInProgress)
                return;

            double progress = 0;
            if (ProcessSteps != 0) progress = (double)CurrentStep / ProcessSteps;
            ChangeStatus(status, progress);
        }
    }
}
