using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Timers;
using System.Configuration;
using System.IO;
using System.Diagnostics;

namespace Shift.WebJob
{
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            var host = new JobHost();
            StartJob();

            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }

        [NoAutomaticTrigger]
        public static void StartJob()
        {
            //Run Jobs
            var pjob = new ShiftService();
            pjob.Start();
        }

    }

    public class ShiftService
    {
        private static JobServer jobServer; //only one server per WebJob

        public ShiftService()
        {
            var config = new Shift.ServerConfig();
            config.AssemblyFolder = ConfigurationManager.AppSettings["AssemblyFolder"];
            //config.AssemblyListPath = ConfigurationManager.AppSettings["AssemblyListPath"];
            config.MaxRunnableJobs = Convert.ToInt32(ConfigurationManager.AppSettings["MaxRunnableJobs"]);
            config.ProcessID = ConfigurationManager.AppSettings["ShiftPID"];
            config.DBConnectionString = ConfigurationManager.ConnectionStrings["ShiftDBConnection"].ConnectionString;
            config.DBAuthKey = ConfigurationManager.AppSettings["DocumentDBAuthKey"];
            config.Workers = Convert.ToInt32(ConfigurationManager.AppSettings["ShiftWorkers"]);

            config.ServerTimerInterval = Convert.ToInt32(ConfigurationManager.AppSettings["TimerInterval"]); //optional: default every 5 sec for getting jobs ready to run and run them
            config.ServerTimerInterval2 = Convert.ToInt32(ConfigurationManager.AppSettings["CleanUpTimerInterval"]); //optional: default every 10 sec for server CleanUp()

            config.StorageMode = ConfigurationManager.AppSettings["StorageMode"];
            var progressDBInterval = ConfigurationManager.AppSettings["ProgressDBInterval"];
            if (!string.IsNullOrWhiteSpace(progressDBInterval))
                config.ProgressDBInterval = TimeSpan.Parse(progressDBInterval); //Interval when progress is updated in main DB

            var autoDeletePeriod = ConfigurationManager.AppSettings["AutoDeletePeriod"];
            config.AutoDeletePeriod = string.IsNullOrWhiteSpace(autoDeletePeriod) ? null : (int?)Convert.ToInt32(autoDeletePeriod);
            //config.AutoDeleteStatus = new List<JobStatus?> { JobStatus.Completed, JobStatus.Error }; //Auto delete only the jobs that had completed or with error.

            config.ForceStopServer = Convert.ToBoolean(ConfigurationManager.AppSettings["ForceStopServer"]); //Set to true to allow windows service to shut down after a set delay in StopServerDelay
            config.StopServerDelay = Convert.ToInt32(ConfigurationManager.AppSettings["StopServerDelay"]);

            config.PollingOnce = Convert.ToBoolean(ConfigurationManager.AppSettings["PollingOnce"]);

            //config.UseCache = Convert.ToBoolean(ConfigurationManager.AppSettings["UseCache"]);
            //config.CacheConfigurationString = ConfigurationManager.AppSettings["RedisConfiguration"];
            //config.EncryptionKey = ConfigurationManager.AppSettings["ShiftEncryptionParametersKey"]; //optional

            jobServer = new Shift.JobServer(config);

        }

        public async void Start()
        {
            try
            {
                await jobServer.RunServerAsync();
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async void Stop()
        {
            await jobServer.StopServerAsync(); //Attempt to cancel and set stop
        }
    }
}
