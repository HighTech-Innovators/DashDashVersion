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

using System.Text.RegularExpressions;

namespace DashDashVersion
{
    /// <summary>
    /// This class contains the patterns used for input validation and value extraction.
    /// </summary>
    internal static class Patterns
    {
        internal static readonly Regex IsCoreVersionTag = new Regex(@"^(?<BaseVersion>(?<Major>\d+)\.(?<Minor>\d+)\.(?<Patch>\d+))$", RegexOptions.Compiled);

        internal static readonly Regex ContainsVersionNumber = new Regex(@"(?<BaseVersion>(?<Major>\d+)\.(?<Minor>\d+)\.(?<Patch>\d+))", RegexOptions.Compiled);

        internal static readonly Regex SemverAllowedPreReleaseLabelCharacters = new Regex(@"^[A-Za-z0-9\-]*$", RegexOptions.Compiled);

        internal static readonly Regex ValidVersionNumber = new Regex(
            @"^(?<Major>\d+)\.(?<Minor>\d+)\.(?<Patch>\d+)(-((?<CoreDebugLabel>\.?debug\.(?<Major2>\d+)\.(?<Minor2>\d+)\.(?<Patch2>\d+))|(?<PreReleaseLabel>(?<PreReleaseLabelBase>[a-zA-Z0-9-]+\.\d+)(\.(?<PreReleaseLabelFeature>[a-zA-Z0-9-]+\.\d+))?)?(?<PreDebugLabel>\.debug)?))?(\+(?<BuildMetadata>[a-zA-Z0-9-]+))?$", RegexOptions.Compiled);

        internal static readonly Regex DetermineBranchType = new Regex(@$"\b(\/)?(?<branchType>({Constants.MasterBranchName}|{Constants.MainBranchName}|{Constants.DevelopBranchName}|{Constants.FeatureBranchName}|{Constants.ReleaseBranchName}|{Constants.SupportBranchName}|{Constants.HotfixBranchName}|{Constants.BugFixBranchName}))\b(\/)?", RegexOptions.Compiled);
    }

}
