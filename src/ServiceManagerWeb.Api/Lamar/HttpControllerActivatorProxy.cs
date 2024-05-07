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
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using Lamar;
using log4net;

namespace ServiceManagerWeb.Api.Lamar
{
    public class ActivatorRegistry : ServiceRegistry
    {
        public ActivatorRegistry(HttpRequestMessage request, 
            HttpControllerDescriptor controllerDescriptor)
        {
            try
            {
                For<HttpRequestMessage>().Use(request);
                For<HttpControllerDescriptor>().Use(controllerDescriptor);
                For<HttpRequestContext>().Use(request.GetRequestContext());
                For<IHttpRouteData>().Use(request.GetRouteData());
            }
            catch (Exception e)
            {
                var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

                log.Error(e);
                throw;
            }
        }
    }
    
    // This allows us to inject request scoped objects into the 
    // nested container before the controller object graph is built.
    public class HttpControllerActivatorProxy : IHttpControllerActivator
    {
        public IHttpController Create(
            HttpRequestMessage request, 
            HttpControllerDescriptor controllerDescriptor, 
            Type controllerType)
        {
            Container container = (Container) request.GetDependencyScope().GetService(typeof(IContainer));
            container.Configure(new ActivatorRegistry(request, controllerDescriptor));

            return new DefaultHttpControllerActivator()
                .Create(request, controllerDescriptor, controllerType);
        }
    }
}
