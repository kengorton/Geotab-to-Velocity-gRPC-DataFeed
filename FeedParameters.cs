﻿/* Copyright 2022 Esri
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

using Geotab.Checkmate.ObjectModel;
using Geotab.Checkmate.ObjectModel.Engine;
using Geotab.Checkmate.ObjectModel.Exceptions;

namespace Geotab.SDK.DataFeed
{
    /// <summary>
    /// Contains latest data tokens and collections to populate during <see cref="FeedProcessor.GetAsync"/> call.
    /// </summary>
    class FeedParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedParameters"/> class.
        /// </summary>
        /// <param name="lastGpsDataToken">The latest <see cref="LogRecord" /> token</param>
        /// <param name="lastStatusDataToken">The latest <see cref="StatusData" /> token</param>
        /// <param name="lastFaultDataToken">The latest <see cref="FaultData" /> token</param>
        /// <param name="lastTripToken">The latest <see cref="Trip" /> token</param>
        /// <param name="lastExceptionToken">The latest <see cref="ExceptionEvent" /> token</param>
        public FeedParameters(long? lastGpsDataToken, long? lastStatusDataToken, long? lastFaultDataToken, long? lastTripToken, long? lastExceptionToken)
        {
            LastGpsDataToken = lastGpsDataToken;
        }

        /// <summary>
        /// Gets or sets the latest <see cref="ExceptionEvent" /> token.
        /// </summary>
        /// <value>
        /// The last exception token.
        /// </value>
        public long? LastExceptionToken { get; set; }

        /// <summary>
        /// Gets or sets the latest <see cref="FaultData" /> token.
        /// </summary>
        /// <value>
        /// The last fault data token.
        /// </value>
        public long? LastFaultDataToken { get; set; }

        /// <summary>
        /// Gets or sets the latest <see cref="LogRecord" /> token.
        /// </summary>
        /// <value>
        /// The last GPS data token.
        /// </value>
        public long? LastGpsDataToken { get; set; }

        /// <summary>
        /// Gets or sets the latest <see cref="StatusData" /> token.
        /// </summary>
        /// <value>
        /// The last status data token.
        /// </value>
        public long? LastStatusDataToken { get; set; }

        /// <summary>
        /// Gets or sets the latest <see cref="Trip" /> token.
        /// </summary>
        /// <value>
        /// The last trip token.
        /// </value>
        public long? LastTripToken { get; set; }
    }
}
