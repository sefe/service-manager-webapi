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
using System.Text;

namespace ServiceManagerWeb.Core
{
    public class RiskQuestions
    {
        [Description(
            "There is no scheduled clash with other changes using shared infrastructure or resources (consider both "
            + "technology & people)  nor does the affected application/service interface with another application/service ?")]
        public bool Question1 { get; set; }

        [Description(
            "The Change does not introduce new technology or significantly upgrade the version in production nor does "
            + "it introduce new complex functionality or a large scale rewrite of existing core functionality?")]
        public bool Question2 { get; set; }

        [Description(
            "The Change deployment has been successfully tested by the implementation team(s) in an environment which "
            + "is representative of production and for which access is controlled the Production Support teams. ")]
        public bool Question3 { get; set; }

        [Description("The Change is automated or vendor documented and not subject to human error")]
        public bool Question4 { get; set; }

        [Description(
            "The new functionality been tested in an environment representative of production and the testing signed off "
            + "by QA and where applicable UAT has been signed off by nominated business users.")]
        public bool Question5 { get; set; }

        [Description(
            "The Change has a remediation plan - including rollback scripts where applicable - which has been sufficiently  "
            + "tested. Also the change window does include sufficient time to implement this, in the event issues are encountered")]
        public bool Question6 { get; set; }

        public static IDictionary<string, string> GetQuestions()
        {
            return typeof(RiskQuestions).GetProperties().Where(p => Attribute.IsDefined(p, typeof(DescriptionAttribute))).ToDictionary(
                p => p.Name,
                p => ((DescriptionAttribute)Attribute.GetCustomAttribute(p, typeof(DescriptionAttribute))).Description);
        }

        public static RiskQuestions Parse(string input)
        {
            var lines = input.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            var answersText = new Dictionary<string, bool>();

            for (var i = 0; i < lines.Length; i = i + 2)
            {
                answersText.Add(lines[i], lines[i + 1] == "Yes");
            }

            var questions = GetQuestions();

            var answers = new RiskQuestions();

            foreach (var property in questions)
            {
                typeof(RiskQuestions).GetProperty(property.Key).SetValue(answers, answersText[property.Value]);
            }

            return answers;
        }

        public string GetRiskLevel()
        {
            var positiveResponses = (this.Question1 ? 1 : 0) + (this.Question2 ? 1 : 0) + (this.Question3 ? 1 : 0) + (this.Question4 ? 1 : 0) + (this.Question5 ? 1 : 0) + (this.Question6 ? 1 : 0);
            return positiveResponses < 2 ? "High" :
                   positiveResponses < 4 ? "Medium" : "Low";
        }

        public override string ToString()
        {
            var properties = typeof(RiskQuestions).GetProperties().Where(p => Attribute.IsDefined(p, typeof(DescriptionAttribute)));

            var stringBuilder = new StringBuilder();

            foreach (var p in properties)
            {
                var text = ((DescriptionAttribute)Attribute.GetCustomAttribute(p, typeof(DescriptionAttribute))).Description;

                stringBuilder.AppendLine(text);
                stringBuilder.AppendLine(((bool)p.GetValue(this)) ? "Yes" : "No");
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }
    }
}