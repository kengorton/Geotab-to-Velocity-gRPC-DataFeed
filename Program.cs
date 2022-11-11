/* Copyright 2022 Esri
 *
 * Licensed under the Apache License Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Geotab.SDK.DataFeed
{
    /// <summary>
    /// Main program
    /// </summary>
    static class Program
    {
        /// <summary>
        /// This is a console app for obtaining the LogRecord data feed from the Geotab and forwarding it to a gRPC feed in ArcGIS Velocity.
        /// </summary>
        
        private static string gRPC_endpoint_URL = ConfigurationManager.AppSettings["gRPC_endpoint_URL"];
        private static string gRPC_endpoint_header_path = ConfigurationManager.AppSettings["gRPC_endpoint_header_path"];
        private static bool streamData = Boolean.Parse(ConfigurationManager.AppSettings["streamData"]);
        private static bool authenticationArcGIS = Boolean.Parse(ConfigurationManager.AppSettings["authenticationArcGIS"]);
        private static string tokenPortalUrl = ConfigurationManager.AppSettings["tokenPortalUrl"];
        private static string username = ConfigurationManager.AppSettings["username"];
        private static string password = ConfigurationManager.AppSettings["password"];
        private static int tokenExpiry = Int32.Parse(ConfigurationManager.AppSettings["tokenExpiry"]);
        private static string geotabServer = ConfigurationManager.AppSettings["geotabServer"];
        private static string geotabDatabase = ConfigurationManager.AppSettings["geotabDatabase"];
        private static string geotabUsername = ConfigurationManager.AppSettings["geotabUsername"];
        private static string geotabPassword = ConfigurationManager.AppSettings["geotabPassword"];
        private static int sendInterval = Int32.Parse(ConfigurationManager.AppSettings["sendInterval"]);
        private static string outputFilepath = ConfigurationManager.AppSettings["outputFilepath"];
        private static bool outputToConsole = Boolean.Parse(ConfigurationManager.AppSettings["outputToConsole"]);
        

       
        
        static void Main()
        {     
            bool continuous = true;
            
            Worker worker = new DatabaseWorker(geotabUsername, geotabPassword, geotabDatabase, geotabServer, null, null, null, null, null, string.IsNullOrWhiteSpace(outputFilepath)?null:outputFilepath,outputToConsole);
            worker.gRPC_endpoint_URL = gRPC_endpoint_URL;
            worker.gRPC_endpoint_header_path = gRPC_endpoint_header_path;
            worker.streamData = streamData;
            worker.authenticationArcGIS = authenticationArcGIS;
            worker.tokenPortalUrl = tokenPortalUrl;
            worker.username = username;
            worker.password = password;
            worker.tokenExpiry = tokenExpiry;
            worker.sendInterval = sendInterval;
            worker.CreateGrpcClient();
            var cancellationToken = new CancellationTokenSource();
            Task task = Task.Run(async () => await worker.DoWorkAsync(continuous), cancellationToken.Token);
            Console.WriteLine("Press ENTER to quit.");
            if (continuous && Console.ReadLine() != null)
            {
                worker.RequestStop();
                cancellationToken.Cancel();
            }

            return;
        }
    }
}
