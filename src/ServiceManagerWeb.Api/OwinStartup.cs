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

﻿using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security.Jwt;
using Owin;
using AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode;

[assembly: OwinStartup(typeof(ServiceManagerWeb.Api.OwinStartup))]

namespace ServiceManagerWeb.Api
{
    public class OwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            var openIdMetadataAddress = "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration";
            var openIdConfigManager = new ConfigurationManager<OpenIdConnectConfiguration>(openIdMetadataAddress, new OpenIdConnectConfigurationRetriever());
            var openIdConfig = openIdConfigManager.GetConfigurationAsync().Result;

            var audience = OAuthConfig.ClientAppId;
            var issuer = $"{OAuthConfig.AuthUrl}/{OAuthConfig.TenantId}/v2.0";

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationType = "Bearer",
                AuthenticationMode = AuthenticationMode.Active,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidAudience = audience,
                    ValidIssuer = issuer,
                    RequireSignedTokens = true,
                    IssuerSigningKeys = openIdConfig.SigningKeys
                }
            });
        }
    }
}
