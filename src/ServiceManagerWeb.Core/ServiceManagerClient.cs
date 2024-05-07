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
using System.Linq;
using log4net;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using ServiceManagerWeb.Core.Builders;

namespace ServiceManagerWeb.Core
{
    public class ServiceManagerClient : IDisposable
    {
        private readonly EnterpriseManagementGroup _managementGroup;
        private readonly ILog _log;

        public ServiceManagerClient(string server, ILog log)
        {
            _log = log;
            _managementGroup = new EnterpriseManagementGroup(server);
        }

        public ChangeRequest GetChangeRequest(string id, string username)
        {
            var changeBuilder = new ChangeRequestBuilder(_managementGroup, _log);
            var wi = GetWorkItem(id);
            var icr = changeBuilder.GetAdditionalChangeInfo(wi, changeBuilder.GetUserRecord(username));
            return changeBuilder.AsChangeRequest(icr);
        }

        public IEnumerable<ChangeRequest> GetChangeRequests(string identityName)
        {
            var crb = new ChangeRequestBuilder(_managementGroup, _log);

            var username = identityName.Split('\\')[1];
            return crb.GetApprovalRequiredWorkItems(username);
        }

        public bool SetApprovalStatusForChangeForMyself(string id, string userName, string approvalStatus)
        {
            var crb = new ChangeRequestBuilder(_managementGroup, _log);

            var cr = GetWorkItem(id);

            return crb.SetApprovalStatusChangeForMyself(cr, userName, approvalStatus);
        }

        public IncidentRecord GetIncidentRecord(string id)
        {
            return new IncidentRecordBuilder(_managementGroup).AsIncidentRecord(GetWorkItem(id));
        }

        public string GetTemplate(string name)
        {
            var criteria = new ManagementPackObjectTemplateCriteria($"DisplayName like '{name}'");
            var template = _managementGroup.Templates.GetObjectTemplates(criteria);
            return template.FirstOrDefault()?.DisplayName;
        }

        public string AddChangeRequest(ChangeRequest input, string userName, string templateName)
        {
            var changeBuilder = new ChangeRequestBuilder(_managementGroup, _log);

            changeBuilder.ValidateChangeRequest(input);

            var impact = input.ImpactQuestionResponses?.GetImpactLevel();
            var riskAssessmentPlan = input.RiskQuestionResponses?.ToString();
            var risk = input.RiskQuestionResponses?.GetRiskLevel();

            var changeRecord = changeBuilder
                .CreateNewRecord(templateName)
                .SetCreatedBy(userName)
                .SetCustomer(input.Customer)
                .AddApprovers(input.Approvers)
                .SetProperty("Title", input.Title)
                .SetProperty("Description", input.Description)
                .SetProperty("Reason", input.Reason)
                .SetProperty("Area", "Change Area", input.Area)
                .SetProperty("Priority", "Change Priority", input.Priority)
                .SetProperty("Impact", "Change Impact", impact)
                .SetProperty("Risk", "Change Risk", risk)
                .SetProperty("ScheduledStartDate", input.ScheduledStartDate)
                .SetProperty("ScheduledEndDate", input.ScheduledEndDate)
                .SetProperty("ImplementationPlan", input.ImplementationPlan)
                .SetProperty("TestPlan", input.TestPlan)
                .SetProperty("BackoutPlan", input.BackoutPlan)
                .SetProperty("Notes", input.Notes)
                .SetProperty("RiskAssessmentPlan", riskAssessmentPlan)
                .SetProperty("SupportGroup", "Incident Tier Queue", input.SupportGroup)
                .WorkItem;

            changeRecord.Commit();

            string crNumber = (string)changeRecord.Object[null, "Id"].Value;

            if (!string.IsNullOrEmpty(input.InitialActivityToComplete))
                changeBuilder.SetActivityToCompleted(crNumber, input.InitialActivityToComplete);

            return crNumber;
        }
        public void SetChangeRecordActivityToCompleted(string id, string activityName, string username)
        {
            var changeBuilder = new ChangeRequestBuilder(_managementGroup, _log);
            var icr = changeBuilder.GetAdditionalChangeInfo(GetWorkItem(id), changeBuilder.GetUserRecord(username));
            ChangeRequest cr = changeBuilder.AsChangeRequest(icr);

            changeBuilder.SetActivityToCompleted(cr.Id, activityName);
        }

        public void SetChangeRecordActivityToFailed(string id, string activityName, string username)
        {
            var changeBuilder = new ChangeRequestBuilder(_managementGroup, _log);
            var icr = changeBuilder.GetAdditionalChangeInfo(GetWorkItem(id), changeBuilder.GetUserRecord(username));
            ChangeRequest cr = changeBuilder.AsChangeRequest(icr);

            changeBuilder.SetActivityToFailed(cr.Id, activityName);
        }

        public void Dispose()
        {
            _managementGroup?.Dispose();
        }

        private static string GetWorkItemSearchQuery(string id)
        {
            return $@"
                <Criteria xmlns=""http://Microsoft.EnterpriseManagement.Core.Criteria/"">
                   <Expression>
                    <SimpleExpression>
                      <ValueExpressionLeft>
                        <Property>$Target/Property[Type='System.WorkItem']/Id$</Property>
                      </ValueExpressionLeft>
                      <Operator>Equal</Operator>
                      <ValueExpressionRight>
                        <Value>{id}</Value>
                      </ValueExpressionRight>
                    </SimpleExpression>
                    </Expression>
                </Criteria>
                ";
        }

        private EnterpriseManagementObject GetWorkItem(string id)
        {
            var pack = _managementGroup.ManagementPacks.GetManagementPack(new Guid("405d5590-b45f-1c97-024f-24338290453e"));
            var cls = _managementGroup.EntityTypes.GetClass("System.WorkItem", pack);

            var searchCriteria = GetWorkItemSearchQuery(id);
            var criteria = new EnterpriseManagementObjectCriteria(searchCriteria, cls, pack, _managementGroup);

            var items = (IEnumerable<EnterpriseManagementObject>)_managementGroup.EntityObjects.GetObjectReader<EnterpriseManagementObject>(criteria, ObjectQueryOptions.Default);

            var cr = items.FirstOrDefault();
            if (cr != null)
                return cr;
            else
                throw new ApplicationException($"Unable to locate {id}");
        }

        private IEnumerable<EnterpriseManagementObject> GetWorkItems()
        {
            var pack = _managementGroup.ManagementPacks.GetManagementPack(new Guid("405d5590-b45f-1c97-024f-24338290453e"));
            var cls = _managementGroup.EntityTypes.GetClass("System.WorkItem", pack);

            var search = $@"
                <Criteria xmlns=""http://Microsoft.EnterpriseManagement.Core.Criteria/"">
                   <Expression>
                    <SimpleExpression>
                      <ValueExpressionLeft>
                        <Property>$Target/Property[Type='System.WorkItem']/Id$</Property>
                      </ValueExpressionLeft>
                      <Operator>Like</Operator>
                      <ValueExpressionRight>
                        <Value>CR%</Value>
                      </ValueExpressionRight>
                    </SimpleExpression>
                    </Expression>
                </Criteria>
                ";
            var criteria = new EnterpriseManagementObjectCriteria(search, cls, pack, _managementGroup);

            var items = (IEnumerable<EnterpriseManagementObject>)_managementGroup.EntityObjects.GetObjectReader<EnterpriseManagementObject>(criteria, ObjectQueryOptions.Default);

            return items;
        }

        public string AddIncidentRecord(IncidentRecord value)
        {
            var incidentRecordBuilder = new IncidentRecordBuilder(_managementGroup);

            return incidentRecordBuilder.ValidateAndCreateIncidentRecord(value);
        }

        public IEnumerable<IncidentRecord> SearchIncidents(IncidentSearchRequest value)
        {
            var incidentRecordBuilder = new IncidentRecordBuilder(_managementGroup);

            return incidentRecordBuilder.SearchIncidents(value);
        }
    }
}