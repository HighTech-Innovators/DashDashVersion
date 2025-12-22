# DashDashVersion

[![GitHub Actions](https://github.com/HighTech-Innovators/DashDashVersion/workflows/Build%20and%20Test/badge.svg)](https://github.com/HighTech-Innovators/DashDashVersion/actions)
[![nuget](https://img.shields.io/nuget/v/git-flow-version.svg?color=green)](https://www.nuget.org/packages/git-flow-version/)
![licenceTag](https://img.shields.io/github/license/HighTech-Innovators/DashDashVersion.svg?color=purple)
[![docs](https://img.shields.io/badge/-docs-blue.svg)](https://www.dashdashversion.net/)
[![docs](https://img.shields.io/badge/-repository-green.svg)](https://github.com/HighTech-Innovators/DashDashVersion)

DashDashVersion creates predictable and opinionated [SemVer 2.0.0 version][SemVer2] numbers for [git flow][gitFlow] repositories.

It consists of an executable that can be installed as a [`dotnet` global tool][globalTool] and a reusable library [NET Standard 2.0](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) that can be used in other applications or .NET based build scripts.

## Why DashDashVersion?

DashDashVersion is an application to generate a version number for a git repository that follows [git-flow][gitFlow].
Ideal if you are using [Continuous Integration](https://en.wikipedia.org/wiki/Continuous_integration).
The version number can be used for packages or to simply tag your commits.

DashDashVersion provides automated versioning during development, while leaving control over release versions to the user.
It complies with [SemVer 2.0][SemVer2]. DashDashVersion only supports repositories that strictly follow the [git flow][gitFlow] conventions, this may seem overly restrictive but we believe strongly in simplicity and convention over configuration.

## Installation

DashDashVersion can be installed as a [`dotnet` global tool][globalTool] by using the following command:

```bash
dotnet tool install --global git-flow-version
```

or the library part can be added as a package using:

```bash
dotnet add package DashDashVersion
```

## Usage

To use DashDashVersion run the command `git-flow-version` inside a git repository.
It will return a JSON string with the following properties:

* `AssemblyVersion` = an [`AssemblyVersionAttribute`](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.assemblyversionattribute?view=netstandard-2.0) compatible version with the format `<major>.<minor>.<patch>.<revison>`,
* `FullSemVer` = a [SemVer 2.0.0][SemVer2] compliant version number with build metadata,
* `SemVer` = a [SemVer 2.0.0][SemVer2] compliant version number without build metadata.

For example:

```bash
$ git-flow-version
{
    "AssemblyVersion":"0.2.0.128",
    "FullSemVer":"0.2.0-dev.1+8af6c6d",
    "SemVer":"0.2.0-dev.1"
}
```

### Usage in Continuous Integration environments

If your Continuous Integration (CI) environment uses [detached heads](https://git-scm.com/docs/git-checkout#_detached_head) `git-flow-version` cannot determine what the _type_ (`master`, `release/*`, `feature/`, `hotfix/`, `support/*`) of the branch is for which it should generate the version number. In these cases the branch name can be specified via a command-line parameter:

```bash
git-flow-version --branch develop
```

Matching of the branch name is performed using the following strategy:

* if there is a branch with the exact same name, use that branch, otherwise,
* find all branches that end with the same string as the provided parameter:
  * if all of these branches have the same _type_ use that _type_, otherwise,
  * if all of these branches have more than one different _type_ generate an error.

### Command-Line reference

The number of command-line parameters and/or options supported by `git-flow-version` is very limited, however below you find them explained in detail:

#### Help information

To show the help information, use any of the following command-line switches:

```bash
git-flow-version -?
git-flow-version -h
git-flow-version --help
```

#### Setting the branch name

To use the branch in the repository with the name that matches the supplied value to determine the _type_ of the branch, add either the `‑b` or the `‑‑branch` command-line parameter followed by the name of the branch.

```bash
git-flow-version -b develop
git-flow-version --branch feature/issue-1234-improve-error-handling
```

#### Version information

To list the version of `git‑flow‑version` use either the `‑v` or `‑‑version` command-line switches.

```bash
git-flow-version -v
git-flow-version --version
```

## Requirements

DashDashVersion only supports repositories that strictly follow the [git flow][gitFlow] conventions with the branch names:

* `master` or `main`
* `develop`
* `release/<major>.<minor>.<patch>`
* `feature/<feature name>`
* `bugfix/<bugfix name>`
* `hotfix/<major>.<minor>.<patch>`
* `support/<support branch name>`

this may seem overly restrictive but we believe strongly in simplicity and convention over configuration.

### Bootstrap requirements

* A tag on `master` with a [SemVer 2.0.0][SemVer2] compliant name for the latest release, without a pre-release label and without build metadata, if not it will assume 0.0.0 as the current version at the first commit on the repository and count from there.
* On any active `release/` or `hotfix/` branch add a tag using the  `<major>.<minor>.<patch>-rc.<number>` format if having consecutive `rc` versions is required.

### Configuration

There is nothing to configure 🙌, you have already done the necessary configuration by adhering to [git flow][gitFlow] 😜.

### Keep in mind

* Only `release/`, `hotfix/`, `develop` and `feature/` branches are supported, for other branches a human has to decide on a version number.
* Make sure to tag `release/` and `hotfix/` branches after generating a version number, the next version number is solely reliant upon that tag.
* For unsupported branches if a tag is already present in the correct format, that version number will be returned, to facilitate rebuilds on `master` for example.

## What version number to expect

### On `develop`

The number of commits since the last release version `<major>.<minor>.<patch>` is used for the numeric part of the pre-release label. Since the content of `develop` will be in the next version of your project, the `<minor>` version number will be `1` higher than the `<minor>` number of the last release version.
For example if there are 3 commits on `develop` and the last tag on `master` is `0.0.0` the version number will be `0.1.0-dev.3`.
After performing:

```bash
git flow release start 0.1.0
git flow release finish
```

the output of `git-flow-version` will be:

```bash
$ git-flow-version
{
    "AssemblyVersion":"0.2.0.128",
    "FullSemVer":"0.2.0-dev.1+8af6c6d",
    "SemVer":"0.2.0-dev.1"
}
```

### On a `feature/` branch

For a feature branch a version number will be calculated for the branch-off point from `origin/develop` or `develop` if there is no `origin/develop`. The number of commits on the feature branch is counted and these pieces are glued together.
For example with a feature named `featureA` with 5 commits since branching from `origin/develop` with version number `0.1.0-dev.12` the calculated version number would be `0.1.0-dev.12.featureA.5`.
If a second feature is started from the `feature/featureA` branch named `featureB` the number of commits since branching from `origin/develop` will be used, so the version number would be `0.1.0-dev.12-featureB.6`, if only a single commit is made on branch `featureB`.  
![feature branch](https://raw.githubusercontent.com/HighTech-Innovators/DashDashVersion/develop/doc/images/feature.svg?sanitize=true)

### On a `release/` branch

Release candidates use a simple `+1` strategy, if the `release/0.2.0` branch has just been created and has no commits yet, the version number will be `0.2.0-rc.1`.
If there exists a tag on the release branch that has the form `0.2.0-rc.<a=highest rc number>` the calculated version number will be `0.2.0-rc.<a+1>`.
So if the last tag was `0.2.0-rc.4` the next version number would be `0.2.0-rc.5`.  
![release branch](https://raw.githubusercontent.com/HighTech-Innovators/DashDashVersion/develop/doc/images/release.svg?sanitize=true)

### On `master`, `main` or `support`

The version number of the tag on `HEAD` will be returned if it can be found and parsed otherwise an error is generated.

## Contributing

If you want to contribute to DashDashVersion you are more than welcome.  
Read more about contributing [here][contribute].

## License

This work is licensed under the LGPL license, refer to the [COPYING.md][license] and [COPYING.LESSER.md][licenseExtension] files for details.

## Repository

The source code of this project can be found on [github](https://github.com/HighTech-Innovators/DashDashVersion).

[license]: https://raw.githubusercontent.com/HighTech-Innovators/DashDashVersion/develop/COPYING
[licenseExtension]: https://raw.githubusercontent.com/HighTech-Innovators/DashDashVersion/develop/COPYING.LESSER
[SemVer2]: https://semver.org/
[gitFlow]: https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow
[globalTool]: https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools
[contribute]: https://github.com/HighTech-Innovators/DashDashVersion/blob/develop/CONTRIBUTING.md
