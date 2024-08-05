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
using System.Net.NetworkInformation;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace ServiceManagerWeb.Core.Builders
{
    public class WorkItemBuilder
    {
        public WorkItemBuilder(EnterpriseManagementGroup managementGroup)
        {
            ManagementGroup = managementGroup;

            ManagementPack mpSystem = ManagementGroup.ManagementPacks.GetManagementPack(SystemManagementPack.System);
            WorkItemManagementPack = ManagementGroup.ManagementPacks.GetManagementPack("System.WorkItem.Library", mpSystem.KeyToken, new Version("7.5.1464.0"));

            _windowsManagementPack = ManagementGroup.ManagementPacks.GetManagementPack(SystemManagementPack.Windows);
        }

        protected IList<ServiceManagerEnumItem> EnumTree;
        protected readonly EnterpriseManagementGroup ManagementGroup;
        private readonly ManagementPack _windowsManagementPack;
        protected ManagementPack WorkItemManagementPack;

        public WorkItemBuilder SetProperty(string name, string value)
        {
            if (value != null) WorkItem.Object[null, name].Value = value;

            return this;
        }

        public WorkItemBuilder SetProperty(string name, DateTime? value)
        {
            if (value != null) WorkItem.Object[null, name].Value = value.Value;

            return this;
        }

        public WorkItemBuilder SetProperty(string name, string enumerationType, string value)
        {
            if (value != null)
            {
                var enumValue = GetEnumValue(enumerationType, value);
                WorkItem.Object[null, name].Value = enumValue;
            }

            return this;
        }

        private ManagementPackEnumeration GetEnumValue(string enumerationType, string value)
        {
            var node = EnumTree.First(x => x.Name == enumerationType);

            var path = value.Split('\\');

            for (var i = 0; i < path.Length; i++) node = node.Children.First(x => x.Name == path[i]);

            return node.Value;
        }

        public EnterpriseManagementObjectProjection WorkItem { get; set; }

        protected static IList<ServiceManagerEnumItem> BuildTree(IList<ManagementPackEnumeration> items)
        {
            var lookup = items.ToDictionary(item => item.Id,
                item => new ServiceManagerEnumItem {Id = item.Id, Name = item.DisplayName, Value = item});

            foreach (var item in lookup.Values)
                if (item.Value.Parent != null)
                    if (lookup.TryGetValue(item.Value.Parent.Id, out var proposedParent))
                    {
                        item.Parent = proposedParent;
                        proposedParent.Children.Add(item);
                    }

            return lookup.Values.Where(x => x.Parent == null).ToList();
        }


        protected void Assert(Func<bool> condition, string message)
        {
            if (!condition()) throw new ArgumentException(message);
        }

        protected internal EnterpriseManagementObject GetUserRecord(string userName)
        {
            if (userName == null)
                throw new ArgumentException("Username cannot be null");

            var userClass = ManagementGroup.EntityTypes.GetClass("Microsoft.AD.User", _windowsManagementPack);

            var searchCriteria = GetUserSearchQuery(userName);
            var criteria = new EnterpriseManagementObjectCriteria(searchCriteria, userClass, _windowsManagementPack,
                ManagementGroup);

            var items = (IEnumerable<EnterpriseManagementObject>) ManagementGroup.EntityObjects
                .GetObjectReader<EnterpriseManagementObject>(criteria, ObjectQueryOptions.Default);

            return items.FirstOrDefault();
        }

        private static string GetUserSearchQuery(string userName)
        {
            var domain = IPGlobalProperties.GetIPGlobalProperties().DomainName.Split('.')[0];
            var user = string.Empty;
            if (userName.Contains('\\'))
            {
                domain = userName.Split('\\')[0];
                user = userName.Split('\\')[1];
            }
            else
            {
                user = userName;
            }

            return $@"
                <Criteria xmlns=""http://Microsoft.EnterpriseManagement.Core.Criteria/"">
                   <Expression>
                    <And>
                    <Expression>
                    <SimpleExpression>
                      <ValueExpressionLeft>
                        <Property>$Target/Property[Type='Microsoft.AD.User']/UserName$</Property>
                      </ValueExpressionLeft>
                      <Operator>Equal</Operator>
                      <ValueExpressionRight>
                        <Value>{user}</Value>
                      </ValueExpressionRight>
                    </SimpleExpression>
                    </Expression>
                    <Expression>
                    <SimpleExpression>
                      <ValueExpressionLeft>
                        <Property>$Target/Property[Type='Microsoft.AD.User']/Domain$</Property>
                      </ValueExpressionLeft>
                      <Operator>Equal</Operator>
                      <ValueExpressionRight>
                        <Value>{domain}</Value>
                      </ValueExpressionRight>
                    </SimpleExpression>
                    </Expression>
                    </And>
                  </Expression>
                </Criteria>
                ";
        }

        public bool IsValidEnumerationValue(string enumerationType, string value)
        {
            var node = this.EnumTree.FirstOrDefault(x => x.Name == enumerationType);
            if (node == null)
            {
                return false;
            }

            var path = value.Split('\\');

            for (var i = 0; i < path.Length; i++)
            {
                node = node.Children.FirstOrDefault(x => x.Name == path[i]);
                if (node == null)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsValidUser(string userName)
        {
            return this.GetUserRecord(userName) != null;
        }

        public static T GetValue<T>(IDictionary<string, object> properties, string name)
        {
            if (properties.ContainsKey(name))
            {
                return properties[name] == null ? default : (T)properties[name];
            }

            return default(T);
        }

        public static string GetEnumValue(IDictionary<string, object> properties, string name)
        {
            var item = properties.ContainsKey(name) ? properties[name] as ManagementPackEnumeration : null;
            var value = GetEnumValue(item);
            return value;
        }

        public static string GetEnumValue(IList<EnterpriseManagementSimpleObject> properties, string name)
        {
            var item = properties.First(prop => prop.Type.Name == name).Value as ManagementPackEnumeration;
            var value = GetEnumValue(item);
            return value;
        }

        private static string GetEnumValue(ManagementPackEnumeration item)
        {
            if (item == null)
            {
                return null;
            }

            var value = string.Empty;

            while (item.Parent != null)
            {
                value = value.Length == 0 ? item.DisplayName : $"{item.DisplayName}\\{value}";
                item = item.Parent.GetElement();
            }
            return value;
        }
    }
}