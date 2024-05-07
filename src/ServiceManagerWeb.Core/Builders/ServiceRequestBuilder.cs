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
using System.Text.RegularExpressions;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace ServiceManagerWeb.Core.Builders
{
    public class ServiceRequestBuilder
    {
        private readonly string _enterpriseManagementGroup;

        public ServiceRequestBuilder(string enterpriseManagementGroup)
        {
            _enterpriseManagementGroup = enterpriseManagementGroup;
        }

        private EnterpriseManagementObject GetServiceRequest(string workItemId)
        {
            var mg = new EnterpriseManagementGroup(_enterpriseManagementGroup);
            var genericCr = new EnterpriseManagementObjectGenericCriteria($"Name = '{workItemId}'");
            var reader =
                mg.EntityObjects.GetObjectReader<EnterpriseManagementObject>(genericCr, ObjectQueryOptions.Default);
            return reader.FirstOrDefault();
        }

        public bool CheckServiceRequestExists(string workItemId)
        {
            return GetServiceRequest(workItemId) != null;
        }

        public string GetServiceRequestPropertyValue(string workItemId, string propertyName)
        {
            try
            {
                return GetServiceRequest(workItemId).Values.ToList().Find(v => v.Type.Name.Equals(propertyName)).Value
                    .ToString();
            }
            catch (NullReferenceException)
            {
                return string.Empty;
            }
        }

        public void CloseServiceRequest(string workItemId)
        {
            var wi = GetServiceRequest(workItemId);
            var emg = new EnterpriseManagementGroup(_enterpriseManagementGroup);
            var serviceRequestStatusEnum = new Guid();

            if (workItemId.StartsWith(ServiceManagerCore.SRPrefix))
            {
                serviceRequestStatusEnum = new Guid(Constants.ServiceRequestStatusEnumCompletedGuid);
            }

            foreach (var enterpriseManagementSimpleObject in wi.Values)
            {
                if (!enterpriseManagementSimpleObject.Type.Name.Equals("Status")) continue;
                enterpriseManagementSimpleObject.Value = emg.EntityTypes.GetEnumeration(serviceRequestStatusEnum);
                break;
            }

            wi.Commit();
        }

        public bool CheckServiceRequestIsClosed(string workItemId)
        {
            var wi = GetServiceRequest(workItemId);
            var emg = new EnterpriseManagementGroup(_enterpriseManagementGroup);
            var serviceRequestStatusEnum = new Guid();
            var enumClosedGuid = new Guid();

            if (workItemId.StartsWith(ServiceManagerCore.SRPrefix))
            {
                serviceRequestStatusEnum = new Guid(Constants.ServiceRequestStatusEnumCompletedGuid);
                enumClosedGuid = new Guid(Constants.ServiceRequestStatusEnumClosedGuid);
            }

            return (from enterpriseManagementSimpleObject in wi.Values
                    where enterpriseManagementSimpleObject.Type.Name.Equals("Status")
                    select enterpriseManagementSimpleObject.Value.Equals(emg.EntityTypes.GetEnumeration(serviceRequestStatusEnum)) ||
                           enterpriseManagementSimpleObject.Value.Equals(emg.EntityTypes.GetEnumeration(enumClosedGuid))
                )
                .FirstOrDefault();
        }

        public IEnumerable<SyncComment> GetServiceRequestComments(string workItemId)
        {
            var wi = GetServiceRequest(workItemId);
            Guid gRelAnalystLog;
            Guid gRelUserLog;

            if (wi.FullName.Contains("ServiceRequest"))
            {
                gRelAnalystLog = new Guid(Constants.WorkItemHasCommentLogGuid);
                gRelUserLog = new Guid(Constants.WorkItemHasCommentLogGuid);
            }
            else
            {
                throw new Exception("Work Item is not a Service request");
            }

            var mg = new EnterpriseManagementGroup(_enterpriseManagementGroup);

            var mpcAnalystLog =
                mg.EntityTypes.GetClass(new Guid(Constants.ActionTypeAnalystCommentGuid));

            var mpcUserLog = mg.EntityTypes.GetClass(new Guid("a3d4e16f-5e8a-18ba-9198-d9815194c986"));
            //Use a list so we can sort the 3 log classes into chronological order
            IList<EnterpriseManagementObject> list =
                (from objLog in mg.EntityObjects.GetRelationshipObjectsWhereSource<EnterpriseManagementObject>(wi.Id,
                        TraversalDepth.OneLevel, ObjectQueryOptions.Default)
                 where objLog.RelationshipId == gRelAnalystLog || objLog.RelationshipId == gRelUserLog
                 select objLog.TargetObject).ToList();

            var resultedList = new List<SyncComment>();
            if (list.Count == 0) return resultedList;
            foreach (var emoLog in list.OrderByDescending(x => x.TimeAdded))
            {
                var sEnteredDate = new DateTime();
                var sTitle = "";
                var sEnteredBy = "";
                var sDescription = "";

                ManagementPackClass mpcLog = null;

                if (emoLog.IsInstanceOf(mpcAnalystLog))
                {
                    sTitle = "Analyst Comment";
                    if (emoLog[mpcAnalystLog, ServiceManagerCore.ServiceRequestComment].Value != null)
                    {
                        sDescription = emoLog[mpcAnalystLog, ServiceManagerCore.ServiceRequestComment].Value.ToString();
                    }

                    mpcLog = mpcAnalystLog;
                }
                else if (emoLog.IsInstanceOf(mpcUserLog))
                {
                    sTitle = "End-User Comment";
                    if (emoLog[mpcUserLog, ServiceManagerCore.ServiceRequestComment].Value != null)
                    {
                        sDescription = emoLog[mpcUserLog, ServiceManagerCore.ServiceRequestComment].Value.ToString();
                    }

                    mpcLog = mpcUserLog;
                }

                if (emoLog[mpcLog, ServiceManagerCore.ServiceRequestComments_EnteredBy].Value != null)
                {
                    sEnteredBy = emoLog[mpcLog, ServiceManagerCore.ServiceRequestComments_EnteredBy].Value.ToString();
                }

                if (emoLog[mpcLog, "EnteredDate"].Value != null)
                {
                    var time = (DateTime)emoLog[mpcLog, "EnteredDate"].Value;
                    sEnteredDate = time.ToLocalTime();
                }

                resultedList.Add(new SyncComment
                {
                    Title = sTitle,
                    AuthorIdentity = sEnteredBy,
                    EnteredDate = sEnteredDate.AddTicks(-(sEnteredDate.Ticks % TimeSpan.TicksPerSecond)),
                    Comment = Regex.Replace(sDescription, @"<\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*>", ""),
                    CommentDataSource = sEnteredBy.Equals("AzureDevOps Sync") ? CommentDataSource.AzureDevOps : CommentDataSource.Scsm
                });
            }

            return resultedList;
        }

        public void AddComment(string workItemId, SyncComment comment)
        {
            var emg = new EnterpriseManagementGroup(_enterpriseManagementGroup);
            var workItemManagementMp =
                emg.ManagementPacks.GetManagementPack("System.WorkItem.Library", "31bf3856ad364e35", null);
            var wi = emg.EntityTypes.GetClass("System.WorkItem.TroubleTicket.AnalystCommentLog",
                workItemManagementMp);
            ManagementPackRelationship wiToWi;

            if (workItemId.StartsWith(ServiceManagerCore.SRPrefix))
            {
                wiToWi =
                    workItemManagementMp.GetRelationship("System.WorkItemHasCommentLog");
            }
            else
            {
                throw new Exception("Work Item is not a Service request");
            }

            foreach (var incident in
                emg.EntityObjects.GetObjectReader<EnterpriseManagementObject>(
                    new EnterpriseManagementObjectGenericCriteria($"Name='{workItemId}'"), ObjectQueryOptions.Default))
            {
                var emoAnalyst = new EnterpriseManagementObjectProjection(emg, wi);
                emoAnalyst.Object[wi, "Id"].Value = Guid.NewGuid().ToString();
                emoAnalyst.Object[wi, ServiceManagerCore.ServiceRequestComment].Value = comment.Comment;
                emoAnalyst.Object[wi, ServiceManagerCore.ServiceRequestComments_EnteredBy].Value = "AzureDevOps Sync";
                emoAnalyst.Object[wi, "EnteredDate"].Value = comment.EnteredDate;
                emoAnalyst.Object[wi, "DisplayName"].Value =
                    string.IsNullOrEmpty(comment.Title) ? "Analyst Comment" : comment.Title;
                emoAnalyst.Object[wi, "IsPrivate"].Value = "True";
                var emoIncident =
                    new EnterpriseManagementObjectProjection(incident) { { emoAnalyst, wiToWi.Target } };
                emoIncident.Commit();
            }
        }
    }
}
