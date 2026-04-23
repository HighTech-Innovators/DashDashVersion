// Copyright 2019 Hightech ICT and authors

// This file is part of DashDashVersion.

// DashDashVersion is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// DashDashVersion is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with DashDashVersion. If not, see<https://www.gnu.org/licenses/>.

using System;
using System.Reflection;
using DashDashVersion;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace GitFlowVersion
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            using var app = new CommandLineApplication
            {
                Name = "git-flow-version"
            };
            app.HelpOption("-?|-h|--help");

            var optionBranch = app.Option("-b|--branch", "Manually set the branch to use, for determining the branch 'type' and pre-release label. This can be a full or partial name.", CommandOptionType.SingleValue);
            var optionVersion = app.Option("-v|--version", "Returns the currently installed version of git-flow-version.", CommandOptionType.NoValue);
            var optionDebug = app.Option("-d|--debug", "Adds a debug prerelease label to the version number.", CommandOptionType.NoValue);
            var optionForce = app.Option("-f|--force", "Calculate the version even if the repository is not in a clean state.", CommandOptionType.NoValue);

            app.OnExecute(() =>
            {
                if (optionVersion.HasValue())
                {
                    WriteGitFlowVersion();
                    return 0;
                }
                try
                {
                    OutputJsonToConsole(VersionNumberGenerator.GenerateVersionNumber(
                            Environment.CurrentDirectory,
                            optionBranch.Value() ?? string.Empty,
                            !optionForce.HasValue()),
                        optionDebug.HasValue());
                    return 0;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                    return -1;
                }
            });
            return app.Execute(args);
        }

        private static void OutputJsonToConsole(VersionNumber version, bool debugVersion)
        {
            var newVersion = version;
            using var writer = new JsonTextWriter(Console.Out) { Formatting = Formatting.Indented };
            newVersion.DebugVersion = debugVersion;

            writer.WriteStartObject();

            writer.WritePropertyName(nameof(newVersion.AssemblyVersion));
            writer.WriteValue(newVersion.AssemblyVersion);

            writer.WritePropertyName(nameof(newVersion.FullSemVer));
            writer.WriteValue(newVersion.FullSemVer);

            writer.WritePropertyName(nameof(newVersion.SemVer));
            writer.WriteValue(newVersion.SemVer);

            writer.WriteEndObject();
        }

        private static void WriteGitFlowVersion()
        {
            var version = Attribute
               .GetCustomAttribute(
                   Assembly.GetEntryAssembly(),
                   typeof(AssemblyInformationalVersionAttribute))
               as AssemblyInformationalVersionAttribute;
            Console.WriteLine(version?.InformationalVersion ?? "Unknown Version");
        }
    }
}
