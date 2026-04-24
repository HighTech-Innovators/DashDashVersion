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
using System.Diagnostics;
using System.IO;
using Xunit;
using FluentAssertions;
using DashDashVersion;
using LibGit2Sharp;

namespace DashDashVersionTests
{
    public class RepositoryCleanlinessTests : IDisposable
    {
        private readonly string _tempDir;

        public RepositoryCleanlinessTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), $"ddv-test-{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
            {
                try
                {
                    // Give git a chance to release file handles
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    System.Threading.Thread.Sleep(100);
                    Directory.Delete(_tempDir, recursive: true);
                }
                catch
                {
                    // If cleanup fails, don't fail the test
                }
            }
        }

        private string CreateTestRepository(string repoName)
        {
            var repoPath = Path.Combine(_tempDir, repoName);
            Directory.CreateDirectory(repoPath);
            
            Repository.Init(repoPath);
            using var repo = new Repository(repoPath);

            // Configure git user for commits
            repo.Config.Set("user.name", "Test User");
            repo.Config.Set("user.email", "test@example.com");

            // Create initial commit
            var testFile = Path.Combine(repoPath, "initial.txt");
            File.WriteAllText(testFile, "initial content");
            Commands.Stage(repo, testFile);
            var signature = new Signature("Test User", "test@example.com", DateTimeOffset.Now);
            repo.Commit("Initial commit", signature, signature);

            // Create develop branch
            repo.CreateBranch("develop");

            // Create master tag
            repo.ApplyTag("1.0.0");

            return repoPath;
        }

        private void CreateUnstagedChanges(string repoPath)
        {
            // Modify existing file but don't stage it
            var testFile = Path.Combine(repoPath, "initial.txt");
            File.WriteAllText(testFile, "modified unstaged content");
        }

        private static string CliDll()
        {
            var baseDir = AppContext.BaseDirectory;
            // Extract the build configuration (e.g. "Debug", "Release") and TFM
            // (e.g. "net8") from the test assembly's own output path so the CLI
            // path stays valid regardless of the active build configuration.
            var parts = baseDir
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
            var binIndex = Array.LastIndexOf(parts, "bin");
            var configuration = binIndex >= 0 && binIndex + 1 < parts.Length
                ? parts[binIndex + 1]
                : "Debug";
            var tfm = binIndex >= 0 && binIndex + 2 < parts.Length
                ? parts[binIndex + 2]
                : "net8";
            return Path.GetFullPath(Path.Combine(
                baseDir, "..", "..", "..", "..", "..",
                "src", "git-flow-version", "bin", configuration, tfm,
                "git-flow-version.dll"));
        }

        private static int RunCli(string workingDir, params string[] args)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"\"{CliDll()}\" {string.Join(" ", args)}",
                    WorkingDirectory = workingDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.Start();
            process.WaitForExit();
            return process.ExitCode;
        }

        private void CreateStagedChanges(string repoPath)
        {
            using var repo = new Repository(repoPath);

            // Create a new file
            var newFile = Path.Combine(repoPath, "staged.txt");
            File.WriteAllText(newFile, "staged content");

            // Stage it
            Commands.Stage(repo, newFile);
        }

        [Fact]
        public void When_repo_has_unstaged_changes_Then_generation_fails_without_force()
        {
            // Arrange
            var repoPath = CreateTestRepository("unstaged-repo");
            CreateUnstagedChanges(repoPath);

            // Act & Assert
            Action act = () => VersionNumberGenerator.GenerateVersionNumber(
                repoPath,
                "master",
                checkIfRepoIsClean: true);

            act.Should()
                .Throw<InvalidDataException>()
                .WithMessage("*not in a clean state*");
        }

        [Fact]
        public void When_repo_has_staged_changes_Then_generation_fails_without_force()
        {
            // Arrange
            var repoPath = CreateTestRepository("staged-repo");
            CreateStagedChanges(repoPath);

            // Act & Assert
            Action act = () => VersionNumberGenerator.GenerateVersionNumber(
                repoPath,
                "master",
                checkIfRepoIsClean: true);

            act.Should()
                .Throw<InvalidDataException>()
                .WithMessage("*not in a clean state*");
        }

        [Fact]
        public void When_repo_is_dirty_Then_generation_succeeds_with_force()
        {
            // Arrange
            var repoPath = CreateTestRepository("force-repo");
            CreateUnstagedChanges(repoPath);
            CreateStagedChanges(repoPath);

            // Act & Assert
            var version = VersionNumberGenerator.GenerateVersionNumber(
                repoPath,
                "master",
                checkIfRepoIsClean: false);
            version.Should().NotBeNull();
        }

        [Fact]
        public void When_cli_invoked_on_dirty_repo_without_force_Then_exits_nonzero()
        {
            // Arrange
            var repoPath = CreateTestRepository("cli-dirty-repo");
            CreateUnstagedChanges(repoPath);

            // Act
            var exitCode = RunCli(repoPath, "-b", "master");

            // Assert
            exitCode.Should().NotBe(0);
        }

        [Fact]
        public void When_cli_invoked_on_dirty_repo_with_force_Then_exits_zero()
        {
            // Arrange
            var repoPath = CreateTestRepository("cli-force-repo");
            CreateUnstagedChanges(repoPath);

            // Act
            var exitCode = RunCli(repoPath, "-b", "master", "--force");

            // Assert
            exitCode.Should().Be(0);
        }
    }
}
