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
using Lamar;
using log4net;

namespace ServiceManagerWeb.Api.Lamar
{
    public class ApiRegistry : ServiceRegistry
    {
        public ApiRegistry()
        {
            try
            {
                For<IApiAccess>().Use<ApiAccess>();
                For<ILog>().Use(LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType));
            }
            catch (Exception e)
            {
                var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

                log.Error(e);
                throw;
            }
        }
    }
}