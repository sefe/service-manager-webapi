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

﻿using Microsoft.EnterpriseManagement.Configuration.IO;
using Microsoft.EnterpriseManagement.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using log4net;

namespace ServiceManagerWeb.Core
{
    internal class DebugManagementPacks
    {
        public static void WriteManagementPacks(ManagementPack[] mpList, ILog logger)
        {
            try
            {
                var tempPath = Path.GetTempPath();
                logger.Info($"Outputting the Debug Files to temp location {tempPath}");
                StreamWriter sw = new StreamWriter(tempPath + "ManagementPackGuids.txt");
                ManagementPackXmlWriter managementPackXmlWriter = new ManagementPackXmlWriter(tempPath);
                foreach (ManagementPack mp in mpList)
                {

                    if (mp != null)
                    {
                        sw.WriteLine(mp.Name + " " + mp.KeyToken + " " + mp.Version + " " + mp.Id);
                        managementPackXmlWriter.WriteManagementPack(mp);
                        Console.WriteLine(@"Exported management pack: {0}", mp.Name);
                    }
                }
                sw.Close();
            }
            catch
            {
            }
        }

        public static void ExportAllEnumsToFile(IList<ManagementPackEnumeration> allEnums)
        {
            foreach (var managementPackEnumeration in allEnums)
            {
                File.AppendAllText(Path.GetTempPath() + "allenums.txt",
                    managementPackEnumeration.DisplayName + ", " + managementPackEnumeration.Id +
                    Environment.NewLine);
            }
        }
    }
}
