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
using System.Web.Http.Dependencies;
using System.Web.Http.Dispatcher;
using Lamar;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceManagerWeb.Api.Lamar
{
    public class DependencyResolver : IDependencyResolver
    {
        private readonly IContainer _container;

        public DependencyResolver(IContainer container)
        {
            _container = container;
            // Register the HttpControllerActivatorProxy so we can 
            // inject request scoped objects into the nested container
            // before the controller object graph is built.
            _container.Configure(x => x.AddScoped<IHttpControllerActivator, HttpControllerActivatorProxy>());
        }

        public object GetService(Type serviceType)
        {
            // We only want resolve registered types from 
            // the container (So TryGetInstance, which returns 
            // null if not registered).
            return _container.TryGetInstance(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _container.GetAllInstances(serviceType).Cast<object>();
        }

        public IDependencyScope BeginScope()
        {
            return new DependencyScope(_container.GetNestedContainer() as IContainer);
        }

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}
