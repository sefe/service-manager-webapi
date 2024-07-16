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
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace ServiceManagerWeb.Core.Builders
{
    public class IncidentRecordBuilder : WorkItemBuilder
    {
        public const string UrgencyEnum = "Urgency";
        public const string ImpactEnum = "Impact";
        public const string IncidentClassificationEnum = "Incident Classification";
        public const string IncidentTierQueueEnum = "Incident Tier Queue";
        public const string IncidentStatusEnum = "Incident Status";

        private static Version Version = new Version("7.5.1464.0");

        private const string ClassificationPropName = "Classification";
        private const string ImpactPropName = "Impact";
        private const string UrgencyPropName = "Urgency";
        private const string TierQueuePropName = "TierQueue";
        private const string TitlePropName = "Title";
        private const string DescriptionPropName = "Description";
        private const string IdPropName = "Id";
        private const string PriorityPropName = "Priority";
        private const string StatusPropName = "Status";

        private List<string> Statuses;

        public IncidentRecordBuilder(EnterpriseManagementGroup managementGroup) : base(managementGroup)
        {
            var enumTypes = new[]
            {
                ImpactEnum,
                UrgencyEnum,
                IncidentClassificationEnum,
                IncidentTierQueueEnum,
                IncidentStatusEnum
            };
            var allEnums = ManagementGroup.EntityTypes.GetEnumerations();

#if DEBUG
            var exportAllEnums = false;
            if (exportAllEnums)
            {
                DebugManagementPacks.ExportAllEnumsToFile(allEnums);
            }
#endif

            this.EnumTree = BuildTree(allEnums).Where(x => enumTypes.Contains(x.Name)).ToList();
            FillStatusEnumValues();

        }

        public IncidentRecordBuilder CreateNewRecord()
        {
            ManagementPack mpSystem = ManagementGroup.ManagementPacks.GetManagementPack(SystemManagementPack.System);
            ManagementPack mpIncidentLibrary = ManagementGroup.ManagementPacks.GetManagementPack("System.WorkItem.Incident.Library", mpSystem.KeyToken, Version);
            ManagementPackClass classIncident = ManagementGroup.EntityTypes.GetClass("System.WorkItem.Incident", mpIncidentLibrary);
            WorkItem = new EnterpriseManagementObjectProjection(ManagementGroup, classIncident);
            WorkItem.Object[classIncident, IdPropName].Value = ServiceManagerCore.IRFormatDefinition;

            return this;
        }

        public IncidentRecordBuilder SetAffectedUser(string userName)
        {
            var affectedUser = this.GetUserRecord(userName);
            var affected = ManagementGroup.EntityTypes.GetRelationshipClass("System.WorkItemAffectedUser", WorkItemManagementPack);
            WorkItem.Add(affectedUser, affected.Target);

            return this;
        }

        public void ValidateIncidentRecord(IncidentRecord input)
        {
            Assert(() => input.Id == null, "Incident Record Id specified. It is not possible to update an existing incident record.");
            Assert(() => !string.IsNullOrEmpty(input.Title), "Title must be specified.");
            Assert(() => !string.IsNullOrEmpty(input.Description), "Description must be specified.");
            Assert(() => !string.IsNullOrEmpty(input.ClassificationCategory), "Classification Category must be specified.");
            Assert(() => IsValidEnumerationValue(IncidentClassificationEnum, input.ClassificationCategory), "The Classification Category specified is not valid.");
            Assert(() => IsValidEnumerationValue(IncidentTierQueueEnum, input.SupportGroup), "The Support Group specified is not valid.");
            Assert(() => input.Priority == null, "Priority must not be specified.");
            Assert(() => !string.IsNullOrEmpty(input.AffectedUser), "Affected User must be specified.");
            Assert(() => IsValidUser(input.AffectedUser), "The Affected User specified is not valid.");
            Assert(() => !string.IsNullOrEmpty(input.Status), "Status must be specified.");
            Assert(() => IsValidEnumerationValue(IncidentStatusEnum, input.Status), "The Status specified is not valid.");
        }

        public IncidentRecord AsIncidentRecord(EnterpriseManagementObject input)
        {
            var properties = input.Values.ToDictionary(p => p.Type.Name, p => p.Value);

            var output = new IncidentRecord
            {
                Id = GetValue<string>(properties, IdPropName),
                Title = GetValue<string>(properties, TitlePropName),
                Description = GetValue<string>(properties, DescriptionPropName),
                Priority = GetValue<int>(properties, PriorityPropName),
                Urgency = GetEnumValue(properties, UrgencyPropName),
                Impact = GetEnumValue(properties, ImpactPropName),
                SupportGroup = GetEnumValue(properties, TierQueuePropName),
                ClassificationCategory = GetEnumValue(properties, ClassificationPropName),
                Status = GetEnumValue(properties, StatusPropName)
            };


            return output;
        }

        public string ValidateAndCreateIncidentRecord(IncidentRecord value)
        {
            ValidateIncidentRecord(value);

            var incidentRecord = CreateNewRecord()
                .SetAffectedUser(value.AffectedUser)
                .SetProperty(ClassificationPropName, IncidentClassificationEnum, value.ClassificationCategory)
                .SetProperty(ImpactPropName, ImpactEnum, value.Impact)
                .SetProperty(UrgencyPropName, UrgencyEnum, value.Urgency)
                .SetProperty(TierQueuePropName, IncidentTierQueueEnum, value.SupportGroup)
                .SetProperty(TitlePropName, value.Title)
                .SetProperty(DescriptionPropName, value.Description)
                .SetProperty(StatusPropName, IncidentStatusEnum, value.Status).WorkItem;

            incidentRecord.Commit();

            string irNum = (string)incidentRecord.Object[null, IdPropName].Value;
            return irNum;
        }

        public IEnumerable<IncidentRecord> SearchIncidents(IncidentSearchRequest searchRequest)
        {
            ValidateIncidentSearchRequest(searchRequest);

            ManagementPack mpSystem = ManagementGroup.ManagementPacks.GetManagementPack(SystemManagementPack.System);
            ManagementPack workItemMp = ManagementGroup.GetManagementPack("System.WorkItem.Incident.Library", mpSystem.KeyToken, Version);
            ManagementPackClass classIncident = ManagementGroup.EntityTypes.GetClass("System.WorkItem.Incident", workItemMp);

            var cr = new EnterpriseManagementObjectCriteria(BuildIncidentSearchCondition(searchRequest), classIncident);
            var reader = ManagementGroup.EntityObjects.GetObjectReader<EnterpriseManagementObject>(cr, ObjectQueryOptions.Default);
            if (reader != null && reader.Count > 0)
            {
                return reader.Where(_ => !searchRequest.Statuses.Any() || searchRequest.Statuses.Contains(GetEnumValue(_.Values, StatusPropName)))
                    .Select(AsIncidentRecord);
            }
            return null;
        }

        private void FillStatusEnumValues()
        {
            ManagementPack mpSystem = ManagementGroup.ManagementPacks.GetManagementPack(SystemManagementPack.System);
            ManagementPack mpIncidentLibrary = ManagementGroup.ManagementPacks.GetManagementPack("System.WorkItem.Incident.Library", mpSystem.KeyToken, Version);
            var statusEnum = mpIncidentLibrary.GetEnumeration("IncidentStatusEnum");
            Statuses = mpIncidentLibrary.GetEnumerations().Where(_ => _.Name.StartsWith(statusEnum.Name) && _.Parent?.Id == statusEnum.Id).Select(_ => _.DisplayName).ToList();
        }

        private void ValidateIncidentSearchRequest(IncidentSearchRequest searchParams)
        {
            Assert(() => searchParams != null, "Search parameters must be specified");
            Assert(() => !string.IsNullOrWhiteSpace(searchParams.Title),
                $"{nameof(IncidentSearchRequest.Title)} filter is missing.");
            Assert(() => searchParams.CreatedAfter.HasValue,
                $"{nameof(IncidentSearchRequest.CreatedAfter)} filter is missing.");
            Assert(() => searchParams.CreatedAfter.Value < DateTime.UtcNow,
                $"{nameof(IncidentSearchRequest.CreatedAfter)} cannot be in the future");
            Assert(() => searchParams.Statuses.All(_ => Statuses.Contains(_)), $"Invalid {nameof(IncidentSearchRequest.Statuses)} specified. Valid values are: {string.Join(",", Statuses)}");
        }

        private static string BuildIncidentSearchCondition(IncidentSearchRequest searchParams)
        {
            var conditions = new List<string>();
            if (!string.IsNullOrWhiteSpace(searchParams.Title))
                conditions.Add($"Title Like '%{searchParams.Title}%'");

            if (searchParams.CreatedAfter.HasValue)
                conditions.Add($"CreatedDate > '{searchParams.CreatedAfter.Value:M/d/yyyy HH:mm:ss}'");

            return string.Join(" AND ", conditions);
        }
    }
}