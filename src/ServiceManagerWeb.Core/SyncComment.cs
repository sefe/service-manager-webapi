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
    public class SyncComment
    {
        public string Title { get; set; }
        public DateTime EnteredDate { get; set; }
        public string AuthorIdentity { get; set; }
        public string Comment { get; set; }
        public CommentDataSource CommentDataSource { get; set; }
    }

    public enum CommentDataSource
    {
        AzureDevOps,
        Scsm
    }
}