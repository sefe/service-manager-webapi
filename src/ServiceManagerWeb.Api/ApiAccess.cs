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

﻿using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace ServiceManagerWeb.Api
{
    public interface IApiAccess
    {
        void AssertHasAccess(HttpRequestContext context);
        void AssertHasAccess(HttpRequestContext context, IEnumerable<string> allowedRoles);
    }

    public class ApiAccess : IApiAccess
    {
        private readonly ILog _logger = LogManager.GetLogger("ApiAccess");

        public void AssertHasAccess(HttpRequestContext context)
        {
            AssertHasAccess(context, Array.Empty<string>());
        }

        public void AssertHasAccess(HttpRequestContext context, IEnumerable<string> allowedRoles)
        {
            _logger.InfoFormat("Checking {0} has access.", context.Principal.Identity.Name);
            var principal = context.Principal;
            if (!principal.IsInRole(Config.AccessGroup) && !allowedRoles.Any(role => principal.IsInRole(role)))
            {
                _logger.Error("No access.");
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
            _logger.Info("Access Granted.");
        }
    }
}