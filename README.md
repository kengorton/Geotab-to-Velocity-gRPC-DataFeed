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
 # Data Feed

The Geotab-Velocity gRPC Data Feed is an application that allows a third party to request LogRecord GPS data from your devices and send them to a Velocity gRPC feed at a user-defined rate.  


## Prerequisites

The application requires:

- [.Net core 2.0 SDK](https://dot.net/core) or higher

- An active subscription to ArcGIS Velocity
	- a Velocity gRPC feed created using the sample data in the included Geotab to Velocity gRPC schema.csv file.
	- the gRPC endpoint URL value from the gRPC feed
	- the gRPC endpoint header path value from the gRPC feed

The Geotab-Velocity gRPC Data Feed application connects to the MyGeotab cloud hosting services, please ensure that devices have been registered and added to the database. The following information is required:

	- Server (my.geotab.com)
	- Username
	- Password
	- Database (customer)

- Open the included app.config file in a text editor and update it with valid values for each parameter.
- Follow the instructions in the included Deploy Geotab-to-Velocity-gRPC-DataFeed to Azure App Service using Visual Studio Code.pdf to deploy to Azure.

If deployed to Microsoft Azure as an app service, start the application. If deployed as a Windows console application, launch the app by double-clicking the DataFeed.exe file:


#### GPS data

| **#** | **Field Name** | **Description** | **Example** |
| --- | --- | --- | --- |
| 1 | Vehicle Name | The vehicle name/description as displayed to users in Checkmate | Truck 123 |
| 2 | Vehicle Serial Number | The unique serial number printed on the GO device | GT8010000001 |
| 3 | VIN | The Vehicle Identification Number of the vehicle | 1FUBCYCS111111111 |
| 3 | LicensePlate | The license plate number of the vehicle | 1RS9F |
| 3 | LicenseState | The state issuing the vehicle's license plate | PA |
| 4 | Date | The date and time as an epoch milliseconds value for the GPS position | 1665235030000 |
| 5 | Longitude | The coordinate longitude in decimal degrees | -80.6860275268555 |
| 6 | Latitude | The coordinate latitude in decimal degrees | 37.0907897949219 |
| 7 | Speed | The speed in km/h | 103 |

## Contributing

Esri welcomes contributions from anyone and everyone. Please see our [guidelines for contributing](https://github.com/esri/contributing).

## Licensing
Copyright 2022 Esri

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

A copy of the license is available in the repository's [license.txt]( https://github.com/kengorton/Geotab-to-Velocity-gRPC-DataFeed/blob/main/License.txt) file.
