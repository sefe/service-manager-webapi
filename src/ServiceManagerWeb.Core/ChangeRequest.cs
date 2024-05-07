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

namespace ServiceManagerWeb.Core
{
    public class ChangeRequest
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Reason { get; set; }

        public string Area { get; set; }

        public string Priority { get; set; }

        public string Impact { get; set; }

        public string Risk { get; set; }

        public string RiskAssessmentPlan { get; set; }

        public string Notes { get; set; }

        public string SupportGroup { get; set; }

        public DateTime? ScheduledStartDate { get; set; }

        public DateTime? ScheduledEndDate { get; set; }

        public string ImplementationPlan { get; set; }

        public string TestPlan { get; set; }

        public string BackoutPlan { get; set; }

        public string Status { get; set; }

        public string CRStatus { get; set; }

        public string ChangeLevel { get; set; }

        public string Customer { get; set; }

        public IList<string> Approvers { get; set; }

        public ImpactQuestions ImpactQuestionResponses { get; set; }

        public RiskQuestions RiskQuestionResponses { get; set; }

        public string TemplateName { get; set; }

        public string InitialActivityToComplete { get; set; }

        public string CreatedBy { get; set; }

        public IEnumerable<ApprovalStatus> ApprovalStatuses { get; set; }
    }
}