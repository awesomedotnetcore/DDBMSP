﻿using System;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;
using Orleans.Streams;

namespace DDBMSP.Silo
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var exitCode = StartSilo(args);

            Console.WriteLine("Press Enter to terminate...");
            Console.ReadLine();

            return exitCode;
        }

        private static int StartSilo(string[] args)
        {
            var siloConfig = ClusterConfiguration.LocalhostPrimarySilo(); 
            siloConfig.AddSimpleMessageStreamProvider("Default", true);
            siloConfig.AddMemoryStorageProvider();
            siloConfig.LoadFromFile("OrleansConfiguration.xml");
            var silo = new SiloHost("DDDMSPSilo", siloConfig); 
            silo.InitializeOrleansSilo(); 
            //silo.Config.Globals.RegisterDashboard();
            silo.StartOrleansSilo();
            

            return 0;
        }
    }
}