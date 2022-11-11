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
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Esri.Realtime.Core.Grpc;
using System.Threading;

namespace Geotab.SDK.DataFeed
{
    /// <summary>
    /// Worker base class
    /// </summary>
    abstract class Worker
    {
        readonly string path;
        readonly bool outputToConsole;        
        
        bool stop;

        protected internal string gRPC_endpoint_URL;
        protected internal string gRPC_endpoint_header_path;
        protected internal bool streamData;
        protected internal AsyncClientStreamingCall<Request,Response> grpcCall;
        protected internal bool authenticationArcGIS;
        protected internal string tokenPortalUrl;
        protected internal string username;
        protected internal string password;
        protected internal int tokenExpiry;
        protected internal int sendInterval;        
        protected internal GrpcChannel grpcChannel;
        protected internal GrpcFeed.GrpcFeedClient grpcClient;
        protected internal Metadata grpcMetadata;
        

        /// <summary>
        /// Initializes a new instance of the <see cref="Worker"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="outputToConsole">Whether to output results to the console.</param>
        internal Worker(string path, bool outputToConsole)//, string logFilePath)
        {
            this.path = path;
            this.outputToConsole = outputToConsole;     
        }

        protected internal void CreateGrpcClient(){
            //string token = "";

            var handler = new System.Net.Http.SocketsHttpHandler
            {
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan
            //    KeepAlivePingDelay = TimeSpan.FromSeconds(20),
            //    KeepAlivePingTimeout = TimeSpan.FromSeconds(20),
            //    EnableMultipleHttp2Connections = true
            };
            

            grpcChannel = GrpcChannel.ForAddress($"https://{gRPC_endpoint_URL}:443");
            grpcClient = new GrpcFeed.GrpcFeedClient(grpcChannel);   

            grpcMetadata = new Grpc.Core.Metadata
            {
                { "grpc-path", gRPC_endpoint_header_path }
            };                 

            grpcCall = streamData ? grpcClient.Stream(grpcMetadata) : null;  
        }

        /// <summary>
        /// Displays the feed results.
        /// </summary>
        /// <param name="results">The results.</param>
        public async Task DisplayFeedResultsAsync(FeedResultData results)
        {
            //Console.WriteLine(DateTime.UtcNow.ToString() + "; Worker: " + results.GpsRecords.Count + " GPS records returned from Geotab API.");
                
            // Output to console
            if (outputToConsole)
                new FeedToConsole(results.GpsRecords, results.StatusData, results.FaultData).Run();
            
            
            // Optionally we can output to csv:
            if (!string.IsNullOrEmpty(path))
                new FeedToCsv(path, results.GpsRecords, results.StatusData, results.FaultData, results.Trips, results.ExceptionEvents).Run();
            
            if (results.GpsRecords.Count > 0){
                Console.WriteLine(DateTime.UtcNow.ToString() + ": Worker: Sending " + results.GpsRecords.Count + " GPS records to " + gRPC_endpoint_URL + ": " + gRPC_endpoint_header_path);
                new FeedToVelocityGRPC(results.GpsRecords, grpcClient, grpcMetadata, grpcCall,authenticationArcGIS, tokenPortalUrl, username, password, tokenExpiry).Run();
            }
            await Task.Delay(0);            
        }

        /// <summary>
        /// Do the work.
        /// </summary>
        /// <param name="obj">The object.</param>
        public async Task DoWorkAsync(bool continuous)
        {
            do
            {
                await WorkActionAsync();
            }
            while (continuous && !stop);
        }

        /// <summary>
        /// Requests to stop.
        /// </summary>
        public async void RequestStop()
        {
            stop = true;
            if (streamData){
                await grpcCall.RequestStream.CompleteAsync();
                Console.WriteLine(DateTime.UtcNow.ToString() + ": the gRPC stream has been closed.");                
            }
        }

        /// <summary>
        /// The work action.
        /// </summary>
        public abstract Task WorkActionAsync();

        
    }
}