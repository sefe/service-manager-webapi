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

﻿using System.Collections.Generic;
using System.Web.Http;
using log4net;
using ServiceManagerWeb.Core;
using ServiceManagerWeb.Core.Builders;

namespace ServiceManagerWeb.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/requests")]
    public class RequestsController : ApiController
    {
        private readonly IApiAccess _apiAccess;
        private ILog _logger;

        public RequestsController(IApiAccess apiAccess, ILog logger)
        {
            _logger = logger;
            _apiAccess = apiAccess;
        }

        [HttpGet]
        [Route("exists")]
        public bool CheckExists(string id)
        {
            _apiAccess.AssertHasAccess(RequestContext);

            return new ServiceRequestBuilder(Config.Server).CheckServiceRequestExists(id);
        }

        [HttpGet]
        [Route("property")]
        public string PropertyValue(string id, string propertyName)
        {
            _apiAccess.AssertHasAccess(RequestContext);

            return new ServiceRequestBuilder(Config.Server).GetServiceRequestPropertyValue(id, propertyName);
        }

        [HttpPut]
        [Route("close")]
        public void Close(string id)
        {
            _apiAccess.AssertHasAccess(RequestContext);

            new ServiceRequestBuilder(Config.Server).CloseServiceRequest(id);
        }

        [HttpGet]
        [Route("isclosed")]
        public bool IsClosed(string id)
        {
            _apiAccess.AssertHasAccess(RequestContext);

            return new ServiceRequestBuilder(Config.Server).CheckServiceRequestIsClosed(id);
        }

        [HttpGet]
        [Route("comments")]
        public IEnumerable<SyncComment> Comments(string id)
        {
            _apiAccess.AssertHasAccess(RequestContext);

            return new ServiceRequestBuilder(Config.Server).GetServiceRequestComments(id);
        }

        [HttpPut]
        [Route("addcomment")]
        public void AddComment(string id, [FromBody] SyncComment comment)
        {
            _apiAccess.AssertHasAccess(RequestContext);

            new ServiceRequestBuilder(Config.Server).AddComment(id, comment);
        }
    }
}