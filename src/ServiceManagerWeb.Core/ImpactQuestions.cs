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
using System.ComponentModel;
using System.Linq;

namespace ServiceManagerWeb.Core
{
    public class ImpactQuestions
    {
        [Description("Will the service be impacted by the change due to planned outage or expected functionality restrictions?")]
        public bool OutageOrRestrictedFunctionality { get; set; }

        [Description("Will the service be impacted if the change fails?")]
        public bool ServiceImpactedOnFailure { get; set; }

        [Description("What is the criticality of the service?")]
        public string Criticality { get; set; }

        public static IDictionary<string, string> GetQuestions()
        {
            return typeof(ImpactQuestions).GetProperties().Where(p => Attribute.IsDefined(p, typeof(DescriptionAttribute))).ToDictionary(
                p => p.Name,
                p => ((DescriptionAttribute)Attribute.GetCustomAttribute(p, typeof(DescriptionAttribute))).Description);
        }

        public string GetImpactLevel()
        {
            if (this.OutageOrRestrictedFunctionality)
            {
                return this.Criticality == "Critical" ? "Severe" :
                       this.Criticality == "Major" ? "Significant" : "Moderate";
            }

            if (this.ServiceImpactedOnFailure)
            {
                return this.Criticality == "Minor" ? "Minor" : "Moderate";
            }

            return "Negligible";
        }
    }
}