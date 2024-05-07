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
using System.DirectoryServices;
using System.Text.RegularExpressions;
using System.Text;

namespace ServiceManagerWeb.Api
{
    public class ActiveDirectoryAccess
    {
        public static string GetDisplayNameFromActiveDirectory(string username)
        {
            // restrict the username and password to letters only
            if (!Regex.IsMatch(username, "^[a-zA-Z-_.' ()]+$"))
            {
                throw new ArgumentException("Invalid search criteria must be \"^[a-zA-Z-_.' ()]+$\"!");
            }

            var dirEntry = new DirectoryEntry();
            var dirSearcher = new DirectorySearcher(dirEntry)
            {
                SearchScope = SearchScope.Subtree,
                Filter = string.Format("(&(objectClass=user)(|(cn={0})(sn={0}*)(givenName={0})(DisplayName={0}*)(sAMAccountName={0}*)))",
                    username)
            };
            var searchResults = dirSearcher.FindAll();

            foreach (SearchResult sr in searchResults)
            {
                var Sid = string.Empty;
                var de = sr.GetDirectoryEntry();
                var obVal = de.Properties["objectSid"].Value;
                if (null != obVal)
                {
                    Sid = ConvertByteToStringSid((byte[])obVal);
                }

                if (!IsActive(de))
                { continue; }

                return de.Properties["DisplayName"][0].ToString();
            }
            throw new ArgumentException("Failed to locate a valid user account for requested user!");
        }

        private static bool IsActive(DirectoryEntry de)
        {
            if (de.NativeGuid == null) return false;

            var flags = (int)de.Properties["userAccountControl"].Value;

            return !Convert.ToBoolean(flags & 0x0002);
        }

        private static string ConvertByteToStringSid(byte[] sidBytes)
        {
            var strSid = new StringBuilder();
            strSid.Append("S-");
            try
            {
                strSid.Append(sidBytes[0].ToString());
                if (sidBytes[6] != 0 || sidBytes[5] != 0)
                {
                    var strAuth =
                        $"0x{(short)sidBytes[1]:2x}{(short)sidBytes[2]:2x}{(short)sidBytes[3]:2x}{(short)sidBytes[4]:2x}{(short)sidBytes[5]:2x}{(short)sidBytes[6]:2x}";
                    strSid.Append("-");
                    strSid.Append(strAuth);
                }
                else
                {
                    long iVal = sidBytes[1] +
                                (sidBytes[2] << 8) +
                                (sidBytes[3] << 16) +
                                (sidBytes[4] << 24);
                    strSid.Append("-");
                    strSid.Append(iVal.ToString());
                }

                var iSubCount = Convert.ToInt32(sidBytes[7]);
                for (var i = 0; i < iSubCount; i++)
                {
                    var idxAuth = 8 + i * 4;
                    var iSubAuth = BitConverter.ToUInt32(sidBytes, idxAuth);
                    strSid.Append("-");
                    strSid.Append(iSubAuth.ToString());
                }
            }
            catch (Exception)
            {
                return "";
            }
            return strSid.ToString();
        }
    }
}