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
using System.Collections.Generic;
using Geotab.Checkmate.ObjectModel;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Esri.Realtime.Core.Grpc;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;

namespace Geotab.SDK.DataFeed
{
    /// <summary>
    /// An object that creates a feed to Velocity
    /// </summary>
    class FeedToVelocityGRPC
    {
        const string GpsDataHeader = "Vehicle Serial Number, Date, Longitude, Latitude, Speed";

        static readonly char[] trimChars = { ' ', ',' };
        readonly IDictionary<Id, Device> deviceLookup = new Dictionary<Id, Device>();
        readonly IList<LogRecord> gpsRecords;
        private GrpcFeed.GrpcFeedClient grpcClient;
        private Grpc.Core.Metadata grpcMetadata;
        private AsyncClientStreamingCall<Request,Response> grpcCall;
        private bool authenticationArcGIS;
        private string tokenPortalUrl;
        private string username;
        private string password;
        private int tokenExpiry;
        private static HttpClient httpClient;


        /// <summary>
        /// Initializes a new instance of the <see cref="FeedToVelocityGRPC"/> class.
        /// </summary>
        /// <param name="gpsRecords">The GPS records.</param>
        /// <param name="grpcClient">The GrpcFeed.GrpcFeedClient.</param>
        /// <param name="grpcMetadata">The Grpc.Core.Metadata.</param>
        /// <param name="grpcCall">The Grpc.Core.AsyncClientStreamingCall<Request,Response>.</param>
        /// <param name="authenticationArcGIS">The authenticationArcGIS bool.</param>
        /// <param name="tokenPortalUrl">The url to the token portal.</param>
        /// <param name="username">The username for the owner of the gRPC feed.</param>
        /// <param name="password">The owner's password.</param>
        /// <param name="tokenExpiry">The requested token life in minutes.</param>
        public FeedToVelocityGRPC(IList<LogRecord> gpsRecords, GrpcFeed.GrpcFeedClient grpcClient, Grpc.Core.Metadata grpcMetadata,Grpc.Core.AsyncClientStreamingCall<Request,Response> grpcCall = null,bool authenticationArcGIS = false, string tokenPortalUrl = null, string username = null, string password = null, int tokenExpiry = 21600)
        {
            this.gpsRecords = gpsRecords;  
            
            this.grpcClient = grpcClient;    
            this.grpcMetadata = grpcMetadata;
            this.grpcCall = grpcCall;
            this.authenticationArcGIS = authenticationArcGIS;
            this.tokenPortalUrl = tokenPortalUrl;
            this.username = username;
            this.password = password;
            this.tokenExpiry = tokenExpiry;
            if (authenticationArcGIS){
                httpClient = new HttpClient();
            }
        }

        

        /// <summary>
        /// Runs the feed.
        /// </summary>
        public void Run()
        {
            if (gpsRecords.Count > 0)
            {
                SendData(gpsRecords);
            }
        }       
        

        Request PackFeature(Request request, LogRecord logRecord){
            Feature feature = new Feature();
            
            GoDevice goDevice = logRecord.Device as GoDevice;
            feature.Attributes.Add(Any.Pack(new StringValue() { Value = (goDevice == null ? "" : goDevice.Name ?? "") }));
            feature.Attributes.Add(Any.Pack(new StringValue() { Value = logRecord.Device.SerialNumber }));
            feature.Attributes.Add(Any.Pack(new StringValue() { Value = (goDevice == null ? "" : goDevice.VehicleIdentificationNumber ?? "") }));
            feature.Attributes.Add(Any.Pack(new StringValue() { Value = (goDevice == null ? "" : goDevice.LicensePlate ?? "") }));
            feature.Attributes.Add(Any.Pack(new StringValue() { Value = (goDevice == null ? "" : goDevice.LicenseState ?? "") }));
            feature.Attributes.Add(Any.Pack(new Int64Value() { Value = new DateTimeOffset(((DateTime)logRecord.DateTime)).ToUnixTimeMilliseconds() }));
            feature.Attributes.Add(Any.Pack(new DoubleValue() { Value = logRecord.Longitude }));
            feature.Attributes.Add(Any.Pack(new DoubleValue() { Value = logRecord.Latitude }));
            feature.Attributes.Add(Any.Pack(new Int32Value() { Value = (int)logRecord.Speed }));
            //feature.Attributes.Add(Any.Pack(new DoubleValue() { Value = (goDevice == null ? 0 : goDevice.Odometer ?? 0) }));
            

            //int size = feature.CalculateSize();
            request.Features.Add(feature);
            return request;
        }
        
        async void SendData<T>(IList<T> entities){
            System.Type type = typeof(T);

            Request request = new Request();
            Response response = new Response();

            if (type == typeof(LogRecord))
            {
                IList<LogRecord> logs = (IList<LogRecord>)entities;
                for (int i = 0; i < logs.Count; i++)
                {
                    request = PackFeature(request, logs[i]);
                }

                
                
                try{
                    if (authenticationArcGIS){
                        Metadata.Entry tokenEntry = grpcMetadata.Get("authorization");
                        Metadata.Entry tokenExpiresEntry = grpcMetadata.Get("tokenExpires");                        
                        
                        // if Metadata lacks a token or an expiration DateTime, 
                        //fetch new values and add what's missing or replace what is there
                        if (tokenEntry == null || tokenExpiresEntry == null){
                            string tokenStr = await getToken(tokenPortalUrl,username,password,tokenExpiry);                     
                            if (tokenStr.Contains("Unable to generate token.")){
                                Console.WriteLine(tokenStr);
                                return;
                            }                 
                            dynamic tokenJson = JsonConvert.DeserializeObject(tokenStr); 
                            string token = tokenJson["token"];  
                            string expires = (string)tokenJson["expires"];
                            if (tokenEntry == null)
                                grpcMetadata.Add("authorization", $"Bearer {token}"); 
                            else{
                                grpcMetadata.RemoveAt(grpcMetadata.IndexOf(tokenEntry));
                                grpcMetadata.Add("authorization", $"Bearer {token}");
                            }  
                            if (tokenExpiresEntry == null)
                                grpcMetadata.Add("tokenExpires", expires);    
                            else{
                                grpcMetadata.RemoveAt(grpcMetadata.IndexOf(tokenExpiresEntry));
                                grpcMetadata.Add("tokenExpires", expires);
                            }            
                        }else{ 
                            //if Metadata has a token and an expiration DateTime, but the token has expired,
                            //fetch a new token and update the Metadat.Entries
                            string expires = tokenExpiresEntry.Value;
                            DateTime tokenExpires = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(expires)).DateTime;
                            DateTime now = DateTime.UtcNow;
                            if (tokenExpires <= now){
                                grpcMetadata.RemoveAt(grpcMetadata.IndexOf(tokenEntry));
                                grpcMetadata.RemoveAt(grpcMetadata.IndexOf(tokenExpiresEntry));
                                string tokenStr = await getToken(tokenPortalUrl,username,password,tokenExpiry);                    
                                if (tokenStr.Contains("Unable to generate token.")){
                                    Console.WriteLine(tokenStr);
                                    return;
                                }                 
                                dynamic tokenJson = JsonConvert.DeserializeObject(tokenStr); 
                                string token = tokenJson["token"];  
                                expires = (string)tokenJson["expires"];
                                grpcMetadata.Add("authorization", $"Bearer {token}");   
                                grpcMetadata.Add("tokenExpires", expires);              
                            }    
                        }
                    }

                    grpcCall = grpcCall != null ? grpcClient.Stream(grpcMetadata) : null;

                    string responseString;
                    if (grpcCall == null){
                        response = await grpcClient.SendAsync(request, grpcMetadata);
                        responseString = response.Message;       
                        Console.WriteLine(DateTime.UtcNow.ToString() + ": Sent " + request.Features.Count + " GPS records to Velocity. Response: " + responseString);
                    }
                    else{
                        await grpcCall.RequestStream.WriteAsync(request);
                        Console.WriteLine(DateTime.UtcNow.ToString() + ": Streamed " + request.Features.Count + " GPS records to Velocity.");
                    }
                }
                catch (Exception ex){
                    Console.WriteLine(ex.Message);
                }
                finally{
                    request.Features.Clear();
                } 
            }            
            else
            {
                throw new NotSupportedException(type.ToString());
            }
        }
    
        static async Task<string> getToken(string url, string user, string pass, double expiry)
        {    
            try
            {        
                var values = new Dictionary<string, string>
                {
                    { "username", user },
                    { "password", pass },
                    { "client", "referer" },
                    { "referer", "http://localhost:8888"},
                    { "f", "json"},
                    { "expiration", expiry.ToString()}
                };
                
                var content = new FormUrlEncodedContent(values);
                var response = await httpClient.PostAsync($"{url}/sharing/rest/generateToken", content);   
                if (response.IsSuccessStatusCode)  {       
                    var responseString = await response.Content.ReadAsStringAsync();
                    return responseString;
                }
                else
                {
                    return $"Unable to generate token. {response.ReasonPhrase}";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("getToken Error: " + e.Message);
                return "getToken Error: " + e.Message;
            }
        }
    }
}
