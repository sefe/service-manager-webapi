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
using Newtonsoft.Json;
using ServiceManagerWeb.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceManagerWeb.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/incidents")]
    public class IncidentsController : ApiController
    {
        private static readonly string[] AllowedRoles = { AppRoles.CloudIncidentsOperator };

        private readonly ServiceManagerClient _client;
        private readonly IApiAccess _apiAccess;
        private readonly ILog _logger;

        public IncidentsController(IApiAccess apiAccess, ILog logger)
        {
            _logger = logger;
            _apiAccess = apiAccess;
            _client = new ServiceManagerClient(Config.Server, _logger);
        }

        [HttpGet]
        [Route("{id}")]
        public IncidentRecord Get(string id)
        {
            _apiAccess.AssertHasAccess(RequestContext, AllowedRoles);

            return _client.GetIncidentRecord(id);
        }

        [HttpPost]
        [Route("new")]
        public string Post([FromBody] IncidentRecord value)
        {
            _apiAccess.AssertHasAccess(RequestContext, AllowedRoles);

            var userName = RequestContext.Principal.Identity.Name;
            try
            {
                _logger.InfoFormat("Attempting to create Incident Record... {0}", JsonConvert.SerializeObject(value));
                return _client.AddIncidentRecord(value);
            }
            catch (ArgumentException ex)
            {
                _logger.ErrorFormat("Failure to create Incident Record because, {0}", ex.Message.Replace("\r\n", string.Empty));
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = ex.Message.Replace("\r\n", string.Empty) };
                throw new HttpResponseException(message);
            }
        }

        [HttpPost]
        [Route("search")]
        public IEnumerable<IncidentRecord> Post([FromBody] IncidentSearchRequest value)
        {
            _apiAccess.AssertHasAccess(RequestContext, AllowedRoles);

            try
            {
                return _client.SearchIncidents(value);
            }
            catch (ArgumentException ex)
            {
                _logger.ErrorFormat("Failure to search Incident Records because, {0}", ex.Message.Replace("\r\n", string.Empty));
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(ex.Message.Replace("\r\n", string.Empty)) };
                throw new HttpResponseException(message);
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Failure to search Incident Records because, {0}", ex.Message.Replace("\r\n", string.Empty));
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent(ex.Message.Replace("\r\n", string.Empty)) };
                throw new HttpResponseException(message);
            }
        }
    }
}