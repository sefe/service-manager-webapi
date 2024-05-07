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

﻿namespace ServiceManagerWeb.Core
{
    internal class Constants
    {
        public const string CustomerManagementPackIdGuid = "4b035fd3-e72a-c329-3fa9-3e1921598459";

        public const string CustomerRelationshipManagementPackIdGuid = "d203b33c-216d-912a-b92a-389d7e441516";

        public const string WorkItemActivityManagementPackIdGuid = "aa265d90-3e2e-b9a2-d929-be0d36f1a53e";

        public const string ServiceRequestStatusEnumCompletedGuid = "b026fdfd-89bd-490b-e1fd-a599c78d440f";

        public const string ServiceRequestStatusEnumClosedGuid = "c7b65747-f99e-c108-1e17-3c1062138fc4";

        public const string WorkItemHasCommentLogGuid = "79d27435-5917-b0a1-7911-fb2b678f32a6";

        public const string ActionTypeAnalystCommentGuid = "f14b70f4-878c-c0e1-b5c1-06ca22d05d40";

        /*
         *  <ClassTypes>
             <ClassType ID="ClassExtension_c849bcc0_301d_4f93_8d3d_b9de5d1004e0" Accessibility="Public" Abstract="false" Base="Alias_de551087_cb78_4ee3_b22f_91427567e119!System.WorkItem.ChangeRequest" Hosted="false" Singleton="false" Extension="true">
               <Property ID="CRStatus" Type="enum" AutoIncrement="false" Key="false" CaseSensitive="false" MaxLength="256" MinLength="0" Required="true" Scale="0" EnumType="Alias_de551087_cb78_4ee3_b22f_91427567e119!ChangeStatusEnum" DefaultValue="Alias_de551087_cb78_4ee3_b22f_91427567e119!ChangeStatusEnum.New" />
               <Property ID="ChangeLevel" Type="enum" AutoIncrement="false" Key="false" CaseSensitive="false" MaxLength="256" MinLength="0" Required="false" Scale="0" MinValue="-2147483647" EnumType="ChangeLevel_Enum" />
               <Property ID="SupportGroup" Type="enum" AutoIncrement="false" Key="false" CaseSensitive="false" MaxLength="256" MinLength="0" Required="false" Scale="0" EnumType="Alias_61cb27c9_8f61_4d14_ab0a_4d10c47d9545!IncidentTierQueuesEnum" />
             </ClassType>
         */
        public const string WorkItemChangeRequestExtension = "ClassExtension_c849bcc0_301d_4f93_8d3d_b9de5d1004e0";

        public const string CrStatusEnumApproveGuid = "74559e7c-5132-954e-68e7-19f053bda975";

        /*
         * <ClassType ID="ClassExtension_371a1afa_d4b5_45d5_8669_8a64bfe93a92" Accessibility="Public" Abstract="false" Base="Alias_5c7067ec_6fb5_4726_b54a_37648adadee4!System.WorkItem.Incident" Hosted="false" Singleton="false" Extension="true">
             <Property ID="BreachReason" Type="enum" AutoIncrement="false" Key="false" CaseSensitive="false" MaxLength="256" MinLength="0" Required="false" Scale="0" EnumType="Alias_937d4b64_011c_4210_a51f_72878d52a00f!BreachReason" />
             <Property ID="ThirdPartyReferenceNumber" Type="string" AutoIncrement="false" Key="false" CaseSensitive="false" MaxLength="256" MinLength="0" Required="false" Scale="0" />
             <Property ID="VendorResponse" Type="enum" AutoIncrement="false" Key="false" CaseSensitive="false" MaxLength="256" MinLength="0" Required="false" Scale="0" EnumType="VendorResponseList" />
             <Property ID="Customer" Type="string" AutoIncrement="false" Key="false" CaseSensitive="false" MaxLength="256" MinLength="0" Required="false" Scale="0" />
           </ClassType>
         */
        public const string WorkItemIncidentExtension = "ClassExtension_371a1afa_d4b5_45d5_8669_8a64bfe93a92";
    }
}
