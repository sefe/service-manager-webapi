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

namespace ServiceManagerWeb.Core
{
    public class ApprovalStatus : IEquatable<ApprovalStatus>
    {
        public string Approver { get; set; }

        public string Status { get; set; }

        public bool Equals(ApprovalStatus other)
        {
            if (ReferenceEquals(null, other)) return false;
            return Approver == other.Approver && Status == other.Status;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApprovalStatus)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Approver != null ? Approver.GetHashCode() : 0) * 397) ^ (Status != null ? Status.GetHashCode() : 0);
            }
        }
    }
}