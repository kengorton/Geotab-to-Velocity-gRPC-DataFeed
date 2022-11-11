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

namespace Geotab.SDK.DataFeed
{
    /// <summary>
    /// Worker for a database
    /// </summary>
    class DatabaseWorker : Worker
    {
        readonly FeedParameters feedParameters;
        readonly FeedProcessor feedService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseWorker" /> class.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <param name="database">The database.</param>
        /// <param name="server">The server.</param>
        /// <param name="gpsToken">The GPS token.</param>
        /// <param name="statusToken">The status token.</param>
        /// <param name="faultToken">The fault token.</param>
        /// <param name="tripToken">The trip token.</param>
        /// <param name="exceptionToken">The exception token.</param>
        /// <param name="path">The path.</param>
        /// <param name="outputToConsole">Whether to output to the console.</param>
        public DatabaseWorker(string user, string password, string database, string server, long? gpsToken, long? statusToken, long? faultToken, long? tripToken, long? exceptionToken, string path, bool outputToConsole)
            : base(path, outputToConsole)
        {
            feedParameters = new FeedParameters(gpsToken, statusToken, faultToken, tripToken, exceptionToken);
            feedService = new FeedProcessor(server, database, user, password);
        }

        /// <summary>
        /// The work action.
        /// </summary>
        /// <inheritdoc />
        public async override Task WorkActionAsync()
        {
            DateTime utcTaskStarted = DateTime.UtcNow;
            await DisplayFeedResultsAsync(await feedService.GetAsync(feedParameters));
            DateTime utcTaskCompleted = DateTime.UtcNow;
            TimeSpan diffResult = utcTaskCompleted.Subtract(utcTaskStarted);
            int delay = sendInterval - diffResult.Milliseconds;
            await Task.Delay(delay > 0 ? delay : 0);
        }
    }
}
