# Changelog

All notable changes to **ValueStringBuilder** will be documented in this file. The project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

<!-- The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) -->

## [Unreleased]

## [2.1.0] - 2025-12-08

### Added

- Added AssemblyCopyright and AssemblyCompany properties to the BuildInformation class.

## [2.0.0] - 2025-12-03

`v2.0` release which drops the Git Information, as the API was deprecated.

### Added

- New properties for .SDK Version and Compiler Version
- Made object `partial` so it can be easily extended.

### Breaking Changes / Removed

- Removed Git Information support and related properties

## [1.3.0] - 2024-10-11

### Added

- Added support for git information. Please see the README.md for more details and how to activate it.

## [1.2.1] - 2024-05-02

### Changed

- Fixed a wrong example tag which shows a language version instead of the language itself

## [1.2.0] - 2024-04-25

### Added

- Language and LanguageVersion properties to the BuildInformation (i.e. C# 12.0)

## [1.1.2] - 2024-04-20

### Added

- Added logo for nuget package

## [1.1.1] - 2024-04-19

### Changed

- Minor version bump because of failed deployment

## [1.1.0] - 2024-04-19

### Added

The following options were added:

- `AnalysisLevel` to represent the `<AnalysisLevel>` property in the project file
- `ProjectDirectory` to represent the current path to the project file

## [1.0.0] - 2023-12-30

### Added

- Option, to use the root namespace for the generated class (see README.md for more details)

### Changed

- Use auto-generated header so analyzers don't complain
- Made `BuildInformation` class internal if used across different assemblies

## [0.4.2] - 2023-08-29

### Changed

- Used meta-package for analyzers to reduce dependency errors

## [0.4.1] - 2023-08-12

### Changed

- `Configuration` also takes custom configuration into account

## [0.4.0] - 2023-06-15

### Added

- Added support for nullability analysis level
- Added support for deterministic builds

## [0.3.0] - 2023-03-24

### Added

- Added framework moniker to the build information

## [0.2.0] - 2023-03-24

### Added

- New assembly related objects

[unreleased]: https://github.com/linkdotnet/BuildInformation/compare/2.1.0...HEAD
[2.1.0]: https://github.com/linkdotnet/BuildInformation/compare/2.0.0...2.1.0
[2.0.0]: https://github.com/linkdotnet/BuildInformation/compare/1.3.0...2.0.0
[1.3.0]: https://github.com/linkdotnet/BuildInformation/compare/1.2.1...1.3.0
[1.2.1]: https://github.com/linkdotnet/BuildInformation/compare/1.2.0...1.2.1
[1.2.0]: https://github.com/linkdotnet/BuildInformation/compare/1.1.2...1.2.0
[1.1.2]: https://github.com/linkdotnet/BuildInformation/compare/1.1.1...1.1.2
[1.1.1]: https://github.com/linkdotnet/BuildInformation/compare/1.1.0...1.1.1
[1.1.0]: https://github.com/linkdotnet/BuildInformation/compare/1.0.0...1.1.0
[1.0.0]: https://github.com/linkdotnet/BuildInformation/compare/0.4.2...1.0.0
[0.4.2]: https://github.com/linkdotnet/BuildInformation/compare/0.4.1...0.4.2
[0.4.1]: https://github.com/linkdotnet/BuildInformation/compare/0.4.0...0.4.1
[0.4.0]: https://github.com/linkdotnet/BuildInformation/compare/0.3.0...0.4.0
[0.3.0]: https://github.com/linkdotnet/BuildInformation/compare/0.2.0...0.3.0
[0.2.0]: https://github.com/linkdotnet/BuildInformation/compare/9866bfb38171ce0b36aae085d07d15f6e2bc6ff3...0.2.0
