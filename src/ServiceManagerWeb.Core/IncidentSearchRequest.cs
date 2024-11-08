// Copyright 2024 SEFE Securing Energy for Europe GmbH
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;

namespace ServiceManagerWeb.Core
{
    public class IncidentSearchRequest
    {
        /// <summary>
        /// String contained in a title using case-insensitive search
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Date after which incident is created. Value should contain timezone offset (i.e. +00:00 or Z for UTC) otherwise local server timezone is assumed.
        /// Avoid using big periods as it may result in a timeout and big load on the server.
        /// </summary>
        public DateTimeOffset? CreatedAfter { get; set; }

        /// <summary>
        /// Statuses of incidents to retrieve
        /// </summary>
        public List<string> Statuses { get; set; } = new List<string>();
    }
}
