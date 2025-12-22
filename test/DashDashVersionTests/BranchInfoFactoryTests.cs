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

using FluentAssertions;
using System;
using DashDashVersion;
using Xunit;

namespace DashDashVersionTests
{
    public class BranchInfoFactoryTests
    {
        [Theory]
        [InlineData("master")]
        [InlineData("main")]
        [InlineData("support/1.0")]
        [InlineData("develop")]
        [InlineData("hotfix/1.0.0")]
        [InlineData("release/0.1.0")]
        [InlineData("feature/a")]
        [InlineData("feature/b")]
        [InlineData("bugfix/a")]
        public void BranchInfoCreation(string name)
        {
            var branch = BranchInfoFactory.CreateBranchInfo(name);
            branch.Name.Should().Be(name);
            Patterns.DetermineBranchType.IsMatch(name).Should().BeTrue();
        }

        [Theory]
        [InlineData("laLaLa")]
        [InlineData("hotfix")]
        [InlineData("release")]
        [InlineData("feature/b/daf")]
        [InlineData("feature/b/")]
        [InlineData("b")]
        [InlineData("feature/")]
        public void BranchInfoInvalidCreation(string name)
        {
            Action action = () =>
            {
                // ReSharper disable once AssignmentIsFullyDiscarded
                _ = VersionNumber.Parse(name);
            };
            action.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("hotfix/1.0.0", 1, 0, 0)]
        [InlineData("release/0.1.0", 0, 1, 0)]
        public void BranchInfoVersionFromName(string name, uint major, uint minor, uint patch)
        {
            var branch = (ReleaseCandidateBranchInfo)BranchInfoFactory.CreateBranchInfo(name);
            var nameVersion = branch.VersionFromName;
            nameVersion.Major.Should().Be(major);
            nameVersion.Minor.Should().Be(minor);
            nameVersion.Patch.Should().Be(patch);

        }

        [Theory]
        [InlineData("hotfix/1.0.0", 1, "rc")]
        [InlineData("release/1.0.0", 1, "rc")]
        public void BranchInfoPreReleaseMod(string name, uint count, string label)
        {
            var branch = (ReleaseCandidateBranchInfo)BranchInfoFactory.CreateBranchInfo(name);
            var preReleaseLabel = branch.DeterminePreReleaseLabel(count);
            preReleaseLabel.BranchLabel.Label.Should().Be(label);
            preReleaseLabel.BranchLabel.Ordinal.Should().Be(count);
        }

        [Theory]
        [InlineData("feature/a", 1, 1, "a")]
        [InlineData("feature/DivDev12", 1, 2, "DivDev12")]
        [InlineData("bugfix/a", 1, 1, "a")]
        public void BranchInfoPreReleaseFeatureMod(string name, uint devCount, uint featureCount, string label)
        {
            var branch = (FeatureBranchInfo)BranchInfoFactory.CreateBranchInfo(name);
            var preReleaseLabel = branch.DeterminePreReleaseLabel(devCount, featureCount);
            preReleaseLabel.BranchLabel.Label.Should().Be("dev");
            preReleaseLabel.BranchLabel.Ordinal.Should().Be(devCount);
            preReleaseLabel.FeatureBranchLabel.Label.Should().Be(label);
            preReleaseLabel.FeatureBranchLabel.Ordinal.Should().Be(featureCount);
        }
    }
}
