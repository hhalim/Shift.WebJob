﻿using System;
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
            config.MaxRunnableJobs = Convert.ToInt32(ConfigurationManager.AppSettings["MaxRunableJobs"]);
            config.ProcessID = ConfigurationManager.AppSettings["ShiftPID"];
            config.DBConnectionString = ConfigurationManager.ConnectionStrings["ShiftDBConnection"].ConnectionString;

            config.ServerTimerInterval = Convert.ToInt32(ConfigurationManager.AppSettings["TimerInterval"]); //optional: default every 5 sec for getting jobs ready to run and run them
            config.ServerTimerInterval2 = Convert.ToInt32(ConfigurationManager.AppSettings["CleanUpTimerInterval"]); //optional: default every 10 sec for server CleanUp()

            var autoDeletePeriod = ConfigurationManager.AppSettings["AutoDeletePeriod"];
            config.AutoDeletePeriod = string.IsNullOrWhiteSpace(autoDeletePeriod) ? null : (int?)Convert.ToInt32(autoDeletePeriod);
            //config.AutoDeleteStatus = new List<JobStatus?> { JobStatus.Completed, JobStatus.Error }; //Auto delete only the jobs that had Stopped or with Error

            var threadMode = ConfigurationManager.AppSettings["ThreadMode"];
            config.ThreadMode = string.IsNullOrWhiteSpace(threadMode) ? null : threadMode;

            config.StorageMode = ConfigurationManager.AppSettings["StorageMode"];
            var progressDBInterval = ConfigurationManager.AppSettings["ProgressDBInterval"];
            if (!string.IsNullOrWhiteSpace(progressDBInterval))
                config.ProgressDBInterval = TimeSpan.Parse(progressDBInterval); //Interval when progress is updated in main DB

            //config.UseCache = Convert.ToBoolean(ConfigurationManager.AppSettings["UseCache"]);
            //config.CacheConfigurationString = ConfigurationManager.AppSettings["RedisConfiguration"];
            //config.EncryptionKey = ConfigurationManager.AppSettings["ShiftEncryptionParametersKey"]; //optional

            jobServer = new Shift.JobServer(config);

        }

        public void Start()
        {
            try
            {
                jobServer.RunServer();
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public void Stop()
        {
            jobServer.StopServer();
        }
    }
}
