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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using Newtonsoft.Json;

namespace ServiceManagerWeb.Core.Builders
{
    public class ChangeRequestBuilder : WorkItemBuilder
    {
        private const string ChangeAreaEnum = "Change Area";
        private const string ChangeRiskEnum = "Change Risk";
        private const string ChangeImpactEnum = "Change Impact";
        private const string ChangePriorityEnum = "Change Priority";
        private const string IncidentTierQueueEnum = "Incident Tier Queue";
        private const string RiskAssessmentPlanPropName = "RiskAssessmentPlan";
        private const string ChangeLevelPropName = "ChangeLevel";
        private const string CrStatusPropName = "CRStatus";
        private const string SupportGroupPropName = "SupportGroup";
        private const string NotesPropName = "Notes";
        private const string StatusPropName = "Status";
        private const string BackoutPlanPropName = "BackoutPlan";
        private const string TestPlanPropName = "TestPlan";
        private const string ImplementationPlanPropName = "ImplementationPlan";
        private const string ScheduledEndDatePropName = "ScheduledEndDate";
        private const string ScheduledStartDatePropName = "ScheduledStartDate";
        private const string RiskPropName = "Risk";
        private const string ImpactPropName = "Impact";
        private const string PriorityPropName = "Priority";
        private const string AreaPropName = "Area";
        private const string ReasonPropName = "Reason";
        private const string DescriptionPropName = "Description";
        private const string TitlePropName = "Title";
        private const string IdPropName = "Id";

        private static readonly Guid CustomerManagementPackId = new Guid(Constants.CustomerManagementPackIdGuid);

        private static readonly Guid CustomerRelationshipManagementPackId = new Guid(Constants.CustomerRelationshipManagementPackIdGuid);

        private static readonly Guid WorkItemActivityManagementPackId = new Guid(Constants.WorkItemActivityManagementPackIdGuid);

        private readonly ManagementPack _customerManagementPack;

        private readonly ManagementPack _customerRelationshipManagementPack;

        private readonly ManagementPack _workItemActivityManagementPack;
        private readonly ILog _log;
        private const string TechnicalAndBusinessApprovalReviewActivity = "Technical and Business Approval Review Activity";

        public ChangeRequestBuilder(EnterpriseManagementGroup managementGroup, ILog log) : base(managementGroup)
        {
#if DEBUG
            var exportAllManagementPacks = false;
            if (exportAllManagementPacks)
            {
                // Use this when you need to download all the management packs for searching etc...
                DebugManagementPacks.WriteManagementPacks(managementGroup.ManagementPacks.GetManagementPacks().ToArray(), log);
            }
#endif
            _log = log;
            _customerManagementPack = ManagementGroup.ManagementPacks.GetManagementPack(CustomerManagementPackId);
            _customerRelationshipManagementPack = ManagementGroup.ManagementPacks.GetManagementPack(CustomerRelationshipManagementPackId);
            _workItemActivityManagementPack = ManagementGroup.ManagementPacks.GetManagementPack(WorkItemActivityManagementPackId);

            var enumTypes = new[] { ChangeAreaEnum, ChangeRiskEnum, ChangeImpactEnum, ChangePriorityEnum, IncidentTierQueueEnum };
            var allEnums = ManagementGroup.EntityTypes.GetEnumerations();
            EnumTree = BuildTree(allEnums).Where(x => enumTypes.Contains(x.Name)).ToList();
        }

        public ChangeRequestBuilder CreateNewRecord(string templateName)
        {
            var template = GetTemplateByName(templateName);
            WorkItem = new EnterpriseManagementObjectProjection(ManagementGroup, template);
            WorkItem.Object[null, IdPropName].Value = ServiceManagerCore.CRFormatDefinition;
            return this;
        }

        public ChangeRequestBuilder SetCreatedBy(string userName)
        {
            var createdByUser = GetUserRecord(userName) ??
                                throw new ArgumentException(
                                    $"Unable to locate user {userName} to set the CreatedBy field");

            var createdBy = ManagementGroup.EntityTypes.GetRelationshipClass("System.WorkItemCreatedByUser", WorkItemManagementPack);
            WorkItem.Add(createdByUser, createdBy.Target);
            return this;
        }

        public ChangeRequestBuilder SetCustomer(string customerName)
        {
            var customer = GetCustomerRecord(customerName);
            var changeRelatesToCustomer = ManagementGroup.EntityTypes.GetRelationshipClass("ChangeRelatesToCustomer", _customerRelationshipManagementPack);
            WorkItem.Add(customer, changeRelatesToCustomer.Target);
            return this;
        }

        public bool IsValidCustomer(string customerName)
        {
            return GetCustomerRecord(customerName) != null;
        }

        public bool AreValidUsers(IList<string> userNames)
        {
            if (userNames == null || userNames.Count == 0)
            {
                return true;
            }

            return userNames.All(IsValidUser);
        }

        public ChangeRequestBuilder AddApprovers(IList<string> userNames)
        {
            if (userNames == null || userNames.Count == 0)
            {
                return this;
            }

            var allProjections = WorkItem.ToList();
            var approvalActivity = allProjections.First(x =>
                x.Value.Object.Values.First(y => y.Type.Name == TitlePropName).Value.ToString() ==
                TechnicalAndBusinessApprovalReviewActivity).Value;
            var reviewerClass = ManagementGroup.EntityTypes.GetClass("System.Reviewer", _workItemActivityManagementPack);
            var reviewerIsUserRelationship = ManagementGroup.EntityTypes.GetRelationshipClass("System.ReviewerIsUser", _workItemActivityManagementPack);
            var reviewActivityHasReviewerRelationship = ManagementGroup.EntityTypes.GetRelationshipClass("System.ReviewActivityHasReviewer", _workItemActivityManagementPack);

            foreach (var userName in userNames)
            {
                var user = GetUserRecord(userName);

                var reviewer = new EnterpriseManagementObjectProjection(ManagementGroup, reviewerClass);
                reviewer.Object[reviewerClass, "MustVote"].Value = true;
                reviewer.Add(user, reviewerIsUserRelationship.Target);

                approvalActivity.Add(reviewer, reviewActivityHasReviewerRelationship.Target);
            }

            return this;
        }

        public bool SetApprovalStatusChangeForMyself(EnterpriseManagementObject changeRequest, string username,
            string approvalStatus)
        {
            _log.Info($"About to {approvalStatus} Change {changeRequest.DisplayName} for user {username}");
            var changeRequestReviewActivities = ManagementGroup.EntityObjects.GetRelatedObjects<EnterpriseManagementObject>
                (changeRequest.Id, TraversalDepth.OneLevel, ObjectQueryOptions.Default);

            foreach (var reviewActivities in changeRequestReviewActivities)
            {
                var title = reviewActivities.Values.FirstOrDefault(o => o.Type.Name == TitlePropName);

                if (title == null || !title.Value.Equals(TechnicalAndBusinessApprovalReviewActivity))
                {
                    Debug.WriteLine("Throwing away review activity " + title);
                    continue;
                }

                var enterpriseManagementRelationshipObjects = ManagementGroup.EntityObjects.GetRelationshipObjectsWhereSource<EnterpriseManagementObject>(
                    reviewActivities.Id,
                    TraversalDepth.Recursive, ObjectQueryOptions.Default);

                var user = GetUserRecord(username);

                foreach (var relationship in enterpriseManagementRelationshipObjects)
                {
                    if (!relationship.SourceObject.FullName.Contains("System.Reviewer") || relationship.TargetObject.DisplayName != user.DisplayName)
                    {
                        var src = relationship.SourceObject == null
                            ? string.Empty
                            : relationship.SourceObject.FullName;
                        var target = relationship.TargetObject == null
                            ? string.Empty
                            : relationship.TargetObject.FullName;
                        Debug.WriteLine("Throwing away relationship " + src + " : " + target);
                        continue;
                    }

                    var managementPackProperties = relationship.SourceObject.GetProperties();
                    foreach (var managementPackProperty in managementPackProperties)
                    {
                        switch (managementPackProperty.Name)
                        {
                            case "DecisionDate":
                                relationship.SourceObject[managementPackProperty].Value = DateTime.Now;
                                break;
                            case "Decision":
                                {
                                    var identifier = approvalStatus;

                                    var enumType = managementPackProperty.EnumType.GetElement();

                                    var regex1 = new Regex(identifier, RegexOptions.IgnoreCase);
                                    var regex2 = new Regex(".*\\.");
                                    foreach (var childEnumeration in enumType.ManagementGroup.EntityTypes.GetChildEnumerations(enumType.Id, TraversalDepth.Recursive))
                                    {
                                        if (string.Compare(identifier, childEnumeration.Id.ToString(), StringComparison.OrdinalIgnoreCase) == 0 ||
                                            string.Compare(identifier, childEnumeration.Name, StringComparison.OrdinalIgnoreCase) == 0 ||
                                            string.Compare(identifier, regex2.Replace(childEnumeration.Name, ""),
                                                StringComparison.OrdinalIgnoreCase) == 0 ||
                                            string.Compare(identifier, childEnumeration.DisplayName, StringComparison.OrdinalIgnoreCase) == 0 ||
                                            regex1.Match(childEnumeration.Name).Success ||
                                            regex1.Match(childEnumeration.DisplayName).Success)

                                            relationship.SourceObject[managementPackProperty].Value = childEnumeration;
                                    }

                                    break;
                                }
                            case "Comments":
                                relationship.SourceObject[managementPackProperty].Value = approvalStatus + " by API";
                                break;
                        }
                    }
                    _log.Info($"About to commit Change {changeRequest.DisplayName} for user {username}");
                    relationship.SourceObject.Commit();
                    return true;
                }
                return false;
            }
            return false;
        }

        public IEnumerable<ChangeRequest> GetApprovalRequiredWorkItems(string username)
        {
            var output = new List<IntermediateChangeRequest>();
            var pack2 = ManagementGroup.ManagementPacks.GetManagementPack(new Guid(Constants.CustomerRelationshipManagementPackIdGuid));
            var cls = ManagementGroup.EntityTypes.GetClass(Constants.WorkItemChangeRequestExtension, pack2);

            var search = $@"
                    <Criteria xmlns=""http://Microsoft.EnterpriseManagement.Core.Criteria/"">
                      <Expression>
                        <SimpleExpression>
                          <ValueExpressionLeft>
                            <Property>$Context/Property[Type='{Constants.WorkItemChangeRequestExtension}']/CRStatus$</Property>
                          </ValueExpressionLeft>
                          <Operator>Equal</Operator>
                          <ValueExpressionRight>
                            <Value>{{{Constants.CrStatusEnumApproveGuid}}}</Value>
                          </ValueExpressionRight>
                        </SimpleExpression>
                      </Expression>
                    </Criteria>
                ";

            var criteria = new EnterpriseManagementObjectCriteria(search, cls, pack2, ManagementGroup);

            var changeRequests = (IEnumerable<EnterpriseManagementObject>)ManagementGroup.EntityObjects.GetObjectReader<EnterpriseManagementObject>(criteria, ObjectQueryOptions.Default);
            var user = GetUserRecord(username);

            foreach (var changeRequest in changeRequests)
            {
                var icr = GetAdditionalChangeInfo(changeRequest, user);
                if (icr.ApprovalStatuses.Any(s => s.Approver == user.DisplayName))
                    output.Add(icr);
            }

            return this.AsChangeRequests(output, Array.Empty<string>());
        }

        internal IntermediateChangeRequest GetAdditionalChangeInfo(EnterpriseManagementObject changeRequest, EnterpriseManagementObject user)
        {
            var changeRequestReviewActivities = ManagementGroup.EntityObjects.GetRelatedObjects<EnterpriseManagementObject>
                (changeRequest.Id, TraversalDepth.OneLevel, ObjectQueryOptions.Default);

            var createdBy =
                ManagementGroup.EntityTypes.GetRelationshipClass("System.WorkItemCreatedByUser", WorkItemManagementPack);
            var createdByObjects = ManagementGroup.EntityObjects.GetRelatedObjects<EnterpriseManagementObject>(changeRequest.Id,
                createdBy, TraversalDepth.OneLevel, ObjectQueryOptions.Default);

            var reviewers = new List<ApprovalStatus>();
            foreach (var reviewActivities in changeRequestReviewActivities)
            {
                var title = reviewActivities.Values.FirstOrDefault(o => o.Type.Name == TitlePropName);

                if (title == null || !title.Value.Equals(TechnicalAndBusinessApprovalReviewActivity))
                {
                    Debug.WriteLine("Throwing away review activity " + title);
                    continue;
                }

                var enterpriseManagementRelationshipObjects =
                    ManagementGroup.EntityObjects.GetRelationshipObjectsWhereSource<EnterpriseManagementObject>(
                        reviewActivities.Id,
                        TraversalDepth.Recursive, ObjectQueryOptions.Default);

                reviewers.AddRange(
                    GetReviewersFilterByActiveUser(enterpriseManagementRelationshipObjects));
            }

            return new IntermediateChangeRequest
            {
                EnterpriseManagementObject = changeRequest,
                CreatedBy = createdByObjects.FirstOrDefault()?.DisplayName,
                ApprovalStatuses = reviewers
            };
        }

        private IEnumerable<ApprovalStatus> GetReviewersFilterByActiveUser(
            IEnumerable<EnterpriseManagementRelationshipObject<EnterpriseManagementObject>> relationships)
        {
            var output = new List<ApprovalStatus>();

            foreach (var relationship in relationships)
            {
                if (!relationship.SourceObject.FullName.Contains("System.Reviewer"))
                {
                    var src = relationship.SourceObject == null
                        ? string.Empty
                        : relationship.SourceObject.FullName;
                    var target = relationship.TargetObject == null
                        ? string.Empty
                        : relationship.TargetObject.FullName;
                    Debug.WriteLine("Throwing away relationship " + src + " : " + target);
                    continue;
                }

                var approvalStatus = new ApprovalStatus
                {
                    Approver = relationship.TargetObject.DisplayName,
                    Status = relationship.SourceObject.Values.First(o => o.Type.Name == "Decision").Value.ToString().Replace("DecisionEnum.", string.Empty),
                };

                if (!output.Contains(approvalStatus))
                    output.Add(approvalStatus);
            }
            return output;
        }

        private void SetActivity(string requestId, string activityTitle, string activityStatusToSet)
        {
            activityStatusToSet = activityStatusToSet.TrimStart().TrimEnd();

            var strWiCriteria = string.Format(@"
                <Criteria xmlns=""http://Microsoft.EnterpriseManagement.Core.Criteria/"">
                  <Reference Id=""System.WorkItem.Library"" PublicKeyToken=""{0}"" Version=""{1}"" Alias=""WILib"" />
                  <Expression>
                    <SimpleExpression>
                      <ValueExpressionLeft>
                        <Property>$Target/Property[Type='WILib!System.WorkItem']/Id$</Property>
                      </ValueExpressionLeft>
                      <Operator>Equal</Operator>
                      <ValueExpressionRight>
                        <Value>" + requestId + @"</Value>
                      </ValueExpressionRight>
                    </SimpleExpression>
                  </Expression>
                </Criteria>
                ", WorkItemManagementPack.KeyToken, WorkItemManagementPack.Version);

            var mpSystem = ManagementGroup.ManagementPacks.GetManagementPack(SystemManagementPack.System);
            var mpCrLib = ManagementGroup.ManagementPacks.GetManagementPack("ServiceManager.ChangeManagement.Library", mpSystem.KeyToken, new Version("7.5.1464.0"));

            var crTypeProjection = mpCrLib.GetTypeProjection("System.WorkItem.ChangeRequestWithDependentActivities.Projection");

            var opcWi = new ObjectProjectionCriteria(strWiCriteria, crTypeProjection, ManagementGroup);
            var oprWIs = ManagementGroup.EntityObjects.GetObjectProjectionReader<EnterpriseManagementObject>(opcWi, ObjectQueryOptions.Default);

            foreach (var emopWi in oprWIs)
            {
                var relatedObjects = ManagementGroup.EntityObjects.GetRelatedObjects<EnterpriseManagementObject>
                    (emopWi.Object.Id, TraversalDepth.Recursive, ObjectQueryOptions.Default);

                EnterpriseManagementObject submitForReview = null;

                foreach (var emp in relatedObjects)
                {
                    foreach (var value in emp.Values)
                    {
                        if (value.Type.Name.Equals(TitlePropName) && value.ToString().TrimStart().TrimEnd().Equals(activityTitle))
                        {
                            submitForReview = emp;
                        }
                    }
                }
                if (submitForReview == null)
                    return;

                var activity =
                    ManagementGroup.EntityObjects.GetObject<EnterpriseManagementObject>(submitForReview.Id, ObjectQueryOptions.Default);

                var activityClass = ManagementGroup.EntityTypes.GetClass("System.WorkItem.Activity", _workItemActivityManagementPack);

                ManagementPackProperty status = activityClass.PropertyCollection[StatusPropName];
                var completed =
                    ManagementGroup.EntityTypes.GetEnumeration(activityStatusToSet, _workItemActivityManagementPack);

                activity[status].Value = completed;
                activity.Commit();
            }
        }

        public void SetActivityToCompleted(string requestId, string activityTitle)
        {
            SetActivity(requestId, activityTitle, "ActivityStatusEnum.Completed");
        }

        public void SetActivityToFailed(string requestId, string activityTitle)
        {
            SetActivity(requestId, activityTitle, "ActivityStatusEnum.Failed");
        }

        private static string GetCustomerSearchQuery(string customerName)
        {
            return $@"
                <Criteria xmlns=""http://Microsoft.EnterpriseManagement.Core.Criteria/"">
                   <Expression>
                    <SimpleExpression>
                      <ValueExpressionLeft>
                        <Property>$Target/Property[Type='Customer']/CustomerName$</Property>
                      </ValueExpressionLeft>
                      <Operator>Equal</Operator>
                      <ValueExpressionRight>
                        <Value>{customerName.Replace("&", "&amp;")}</Value>
                      </ValueExpressionRight>
                    </SimpleExpression>
                    </Expression>
                </Criteria>
                ";
        }

        private ManagementPackObjectTemplate GetTemplateByName(string name)
        {
            var criteria = new ManagementPackObjectTemplateCriteria($"DisplayName like '{name}'");
            var managementPackObjectTemplatesAll = ManagementGroup.Templates.GetObjectTemplates();
            var managementPackObjectTemplates = ManagementGroup.Templates.GetObjectTemplates(criteria);
            var managementPackObjectTemplate = managementPackObjectTemplates.FirstOrDefault();

            var pathName = Path.Combine(Path.GetTempPath(), name + ".json");
            _log.Info("About to write management pack to " + pathName);
            File.WriteAllText(pathName, JsonConvert.SerializeObject(managementPackObjectTemplate));

            return managementPackObjectTemplate;
        }

        private EnterpriseManagementObject GetCustomerRecord(string customerName)
        {
            var customerClass = ManagementGroup.EntityTypes.GetClass("Customer", _customerManagementPack);

            var searchCriteria = GetCustomerSearchQuery(customerName);
            var criteria = new EnterpriseManagementObjectCriteria(searchCriteria, customerClass, _customerManagementPack, ManagementGroup);

            var items = (IEnumerable<EnterpriseManagementObject>)ManagementGroup.EntityObjects.GetObjectReader<EnterpriseManagementObject>(criteria, ObjectQueryOptions.Default);

            return items.FirstOrDefault();
        }

        public void ValidateChangeRequest(ChangeRequest input)
        {
            Assert(() => input.Id == null, "Change Request Id specified. It is not possible to update an existing change record.");
            Assert(() => !string.IsNullOrEmpty(input.Title), "Title must be specified.");
            Assert(() => !string.IsNullOrEmpty(input.Description), "Description must be specified.");
            Assert(() => !string.IsNullOrEmpty(input.Reason), "Reason must be specified.");
            Assert(() => !string.IsNullOrEmpty(input.Area), "Area must be specified.");
            Assert(() => IsValidEnumerationValue(ChangeAreaEnum, input.Area), "The Area specified is not valid.");
            Assert(() => !string.IsNullOrEmpty(input.Priority), "Priority must be specified.");
            Assert(() => IsValidEnumerationValue(ChangePriorityEnum, input.Priority), "The Priority specified is not valid.");
            Assert(() => input.ImpactQuestionResponses != null, "ImpactQuestionResponses must be completed.");
            Assert(() => !string.IsNullOrEmpty(input.ImpactQuestionResponses.Criticality), "Criticality must be specified in ImpactQuestionResponses.");
            Assert(() => new[] { "Critical", "Major", "Minor" }.Contains(input.ImpactQuestionResponses.Criticality), "The Criticality specified is not valid.");
            Assert(() => input.RiskQuestionResponses != null, "RiskQuestionResponses must be completed.");
            Assert(() => !string.IsNullOrEmpty(input.SupportGroup), "SupportGroup must be specified.");
            Assert(() => IsValidEnumerationValue(IncidentTierQueueEnum, input.SupportGroup), "The SupportGroup specified is not valid.");
            Assert(() => !string.IsNullOrEmpty(input.Customer), "Customer must be specified.");
            Assert(() => IsValidCustomer(input.Customer), "The Customer specified is not valid.");
            Assert(() => AreValidUsers(input.Approvers), "One or more of the approvers is not valid.");
        }

        public ChangeRequest AsChangeRequest(IntermediateChangeRequest input)
        {
            var properties = input.EnterpriseManagementObject.Values.ToDictionary(p => p.Type.Name, p => p.Value);

            var output = new ChangeRequest
            {
                Id = GetValue<string>(properties, IdPropName),
                Title = GetValue<string>(properties, TitlePropName),
                Description = GetValue<string>(properties, DescriptionPropName),
                Reason = GetValue<string>(properties, ReasonPropName),
                Area = GetEnumValue(properties, AreaPropName),
                Priority = GetEnumValue(properties, PriorityPropName),
                Impact = GetEnumValue(properties, ImpactPropName),
                Risk = GetEnumValue(properties, RiskPropName),
                ScheduledStartDate = GetValue<DateTime?>(properties, ScheduledStartDatePropName),
                ScheduledEndDate = GetValue<DateTime?>(properties, ScheduledEndDatePropName),
                ImplementationPlan = GetValue<string>(properties, ImplementationPlanPropName),
                TestPlan = GetValue<string>(properties, TestPlanPropName),
                BackoutPlan = GetValue<string>(properties, BackoutPlanPropName),
                Status = GetEnumValue(properties, StatusPropName),
                Notes = GetValue<string>(properties, NotesPropName),
                SupportGroup = GetEnumValue(properties, SupportGroupPropName),
                CRStatus = GetEnumValue(properties, CrStatusPropName),
                ChangeLevel = GetEnumValue(properties, ChangeLevelPropName),
                RiskAssessmentPlan = GetValue<string>(properties, RiskAssessmentPlanPropName),
                CreatedBy = input.CreatedBy,
                ApprovalStatuses = input.ApprovalStatuses
            };

            return output;
        }

        public IEnumerable<ChangeRequest> AsChangeRequests(IEnumerable<IntermediateChangeRequest> input, string[] filterOutCrStatus)
        {
            var output = new List<ChangeRequest>();
            foreach (var intermediateChangeRequest in input)
            {
                var properties = intermediateChangeRequest.EnterpriseManagementObject.Values.ToDictionary(p => p.Type.Name, p => p.Value);

                var cr = new ChangeRequest
                {
                    Id = GetValue<string>(properties, IdPropName),
                    Title = GetValue<string>(properties, TitlePropName),
                    Description = GetValue<string>(properties, DescriptionPropName),
                    Reason = GetValue<string>(properties, ReasonPropName),
                    Area = GetEnumValue(properties, AreaPropName),
                    Priority = GetEnumValue(properties, PriorityPropName),
                    Impact = GetEnumValue(properties, ImpactPropName),
                    Risk = GetEnumValue(properties, RiskPropName),
                    ScheduledStartDate = GetValue<DateTime?>(properties, ScheduledStartDatePropName),
                    ScheduledEndDate = GetValue<DateTime?>(properties, ScheduledEndDatePropName),
                    ImplementationPlan = GetValue<string>(properties, ImplementationPlanPropName),
                    TestPlan = GetValue<string>(properties, TestPlanPropName),
                    BackoutPlan = GetValue<string>(properties, BackoutPlanPropName),
                    Status = GetEnumValue(properties, StatusPropName),
                    Notes = GetValue<string>(properties, NotesPropName),
                    SupportGroup = GetEnumValue(properties, SupportGroupPropName),
                    CRStatus = GetEnumValue(properties, CrStatusPropName),
                    ChangeLevel = GetEnumValue(properties, ChangeLevelPropName),
                    RiskAssessmentPlan = GetValue<string>(properties, RiskAssessmentPlanPropName),
                    CreatedBy = intermediateChangeRequest.CreatedBy,
                    ApprovalStatuses = intermediateChangeRequest.ApprovalStatuses
                };
                if (filterOutCrStatus.Contains(cr.CRStatus))
                    continue;

                output.Add(cr);
            }

            return output;
        }
    }
}