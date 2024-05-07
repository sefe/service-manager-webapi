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
using System.Configuration;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using Lamar;
using log4net;
using ServiceManagerWeb.Api.Lamar;

namespace ServiceManagerWeb.Api
{
    public class WebApiApplication : HttpApplication
    {
        public static Container LamarContainer { get; set; }

        protected void Application_Start()
        {
            LamarContainer = Bootstrapper.ConfigureLamar();

            WebApiConfig.Log = LamarContainer.GetInstance<ILog>();

            GlobalConfiguration.Configuration.DependencyResolver =
                new DependencyResolver(LamarContainer);

            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configuration.Filters.Add(new ApiExceptionFilterAttribute());

            WebApiConfig.Log.Info("Application started.");
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            Regex path = new Regex("/api/", RegexOptions.CultureInvariant);
            var isApi = path.IsMatch(Request.Path);
            if (!isApi)
                return;

            if (Request.UrlReferrer != null)
            {
                var leftPart = Request.UrlReferrer.GetLeftPart(UriPartial.Authority);
                if (ConfigurationManager.AppSettings["AllowedCORSLocations"].Contains(leftPart))
                {
                    HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", leftPart);
                }
            }

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, X-Token");
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Credentials", "true");

            if (Request.HttpMethod != "OPTIONS")
                return;

            HttpContext.Current.Response.StatusCode = 200;
            var httpApplication = sender as HttpApplication;
            httpApplication?.CompleteRequest();
        }
    }
}