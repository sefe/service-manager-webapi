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

﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using log4net;
using Newtonsoft.Json;
using ServiceManagerWeb.Core;
using Swashbuckle.Swagger.Annotations;

namespace ServiceManagerWeb.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/changes")]
    public class ChangesController : ApiController
    {
        private readonly ServiceManagerClient _client;

        private readonly IApiAccess _apiAccess;
        private readonly ILog _logger;

        public ChangesController(IApiAccess apiAccess, ILog logger)
        {
            _logger = logger;
            _apiAccess = apiAccess;
            _client = new ServiceManagerClient(Config.Server, _logger);
        }

        [HttpGet]
        [Route("riskquestions")]
        public IDictionary<string, string> RiskQuestions()
        {
            _apiAccess.AssertHasAccess(RequestContext);

            return global::ServiceManagerWeb.Core.RiskQuestions.GetQuestions();
        }

        [HttpGet]
        [Route("impactquestions")]
        public IDictionary<string, string> ImpactQuestions()
        {
            _apiAccess.AssertHasAccess(RequestContext);

            return global::ServiceManagerWeb.Core.ImpactQuestions.GetQuestions();
        }

        [HttpGet]
        [Route("{id}")]
        public ChangeRequest Get(string id)
        {
            _apiAccess.AssertHasAccess(RequestContext);

            return _client.GetChangeRequest(id, User.Identity.Name.Split('\\')[1]);
        }

        [HttpGet]
        [Route("myapprovals")]
        public IEnumerable<ChangeRequest> GetMyApprovals()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _apiAccess.AssertHasAccess(RequestContext);

            var changeRequests = _client.GetChangeRequests(User.Identity.Name);

            stopwatch.Stop();
            Debug.WriteLine($"Get all Change Requests took {stopwatch.ElapsedMilliseconds}ms");
            return changeRequests;
        }

        [HttpGet]
        [Route("me")]
        public string GetMe()
        {
            return ActiveDirectoryAccess.GetDisplayNameFromActiveDirectory(User.Identity.Name.Split('\\')[1]);
        }

        [HttpPost]
        [Route("new")]
        public HttpResponseMessage Post([FromBody] ChangeRequest value)
        {
            _apiAccess.AssertHasAccess(RequestContext);

            var userName = RequestContext.Principal.Identity.Name;
            try
            {
                if (string.IsNullOrEmpty(value.CreatedBy) || string.IsNullOrEmpty(value.TemplateName))
                {
                    var response = Request.CreateResponse(HttpStatusCode.BadRequest);
                    if (string.IsNullOrEmpty(value.CreatedBy))
                    {
                        response.ReasonPhrase = "CreatedBy field must be supplied!";
                    }

                    if (string.IsNullOrEmpty(value.TemplateName))
                    {
                        response.ReasonPhrase = "TemplateName field must be supplied!";
                    }

                    return response;
                }

                _logger.InfoFormat("Attempting to create Change Record... {0}", JsonConvert.SerializeObject(value));
                var changeRequest = _client.AddChangeRequest(value, value.CreatedBy, value.TemplateName);
                _logger.InfoFormat($"Created Change Record: {changeRequest}");
                return Request.CreateResponse(HttpStatusCode.OK, changeRequest);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failure to create Change Record..", ex);
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = ex.Message.Replace("\r\n", string.Empty) };
                throw new HttpResponseException(message);
            }
        }

        [HttpPut]
        [Route("activitysucceeded")]
        public HttpResponseMessage PutActivitySucceeded(string id, string activityName)
        {
            _apiAccess.AssertHasAccess(RequestContext);

            var userName = RequestContext.Principal.Identity.Name;
            try
            {
                _logger.InfoFormat("Attempting to alter Change Record {0}, activity name {1}", id, activityName);
                _client.SetChangeRecordActivityToCompleted(id, activityName, User.Identity.Name.Split('\\')[1]);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (ArgumentException ex)
            {
                _logger.ErrorFormat("Failure to alter Change Record because, {0}", ex.Message.Replace("\r\n", string.Empty));
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = ex.Message.Replace("\r\n", string.Empty) };
                throw new HttpResponseException(message);
            }
        }

        [HttpPut]
        [Route("activityfailed")]
        public HttpResponseMessage PutActivityFailed(string id, string activityName)
        {
            _apiAccess.AssertHasAccess(RequestContext);

            var userName = RequestContext.Principal.Identity.Name;
            try
            {
                _logger.InfoFormat("Attempting to alter Change Record {0}, activity name {1}", id, activityName);
                _client.SetChangeRecordActivityToFailed(id, activityName, User.Identity.Name.Split('\\')[1]);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (ArgumentException ex)
            {
                _logger.ErrorFormat("Failure to alter Change Record because, {0}", ex.Message.Replace("\r\n", string.Empty));
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = ex.Message.Replace("\r\n", string.Empty) };
                throw new HttpResponseException(message);
            }
        }

        [HttpPut]
        [Route("approvechange")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(bool))]
        public HttpResponseMessage PutApproveChange(string id)
        {
            _apiAccess.AssertHasAccess(RequestContext);

            var enterpriseManagementObject = _client.SetApprovalStatusForChangeForMyself(id, User.Identity.Name, "Approved");
            return Request.CreateResponse(HttpStatusCode.OK, enterpriseManagementObject);
        }

        [HttpPut]
        [Route("rejectchange")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(bool))]
        public HttpResponseMessage PutRejectChange(string id)
        {
            _apiAccess.AssertHasAccess(RequestContext);

            var enterpriseManagementObject = _client.SetApprovalStatusForChangeForMyself(id, User.Identity.Name, "Rejected");
            return Request.CreateResponse(HttpStatusCode.OK, enterpriseManagementObject);
        }

        [HttpPut]
        [Route("clearapprovestatus")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(bool))]
        public HttpResponseMessage PutClearApproveStatus(string id)
        {
            _apiAccess.AssertHasAccess(RequestContext);

            var enterpriseManagementObject = _client.SetApprovalStatusForChangeForMyself(id, User.Identity.Name, "Not Yet Voted");
            return Request.CreateResponse(HttpStatusCode.OK, enterpriseManagementObject);
        }

        protected override void Dispose(bool disposing)
        {
            _client?.Dispose();
            base.Dispose(disposing);
        }
    }
}