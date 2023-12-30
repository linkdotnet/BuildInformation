# Changelog

All notable changes to **ValueStringBuilder** will be documented in this file. The project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

<!-- The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) -->

## [Unreleased]

## [1.0.0] - 2023-12-30

### Added

-   Option, to use the root namespace for the generated class (see README.md for more details)

### Changed

-   Use auto-generated header so analyzers don't complain
-   Made `BuildInformation` class internal if used across different assemblies

## [0.4.2] - 2023-08-29

### Changed

-   Used meta-package for analyzers to reduce dependency errors

## [0.4.1] - 2023-08-12

### Changed

-   `Configuration` also takes custom configuration into account

## [0.4.0] - 2023-06-15

### Added

-   Added support for nullability analysis level
-   Added support for deterministic builds

## [0.3.0] - 2023-03-24

### Added

-   Added framework moniker to the build information

## [0.2.0] - 2023-03-24

### Added

-   New assembly related objects

[Unreleased]: https://github.com/linkdotnet/BuildInformation/compare/1.0.0...HEAD

[1.0.0]: https://github.com/linkdotnet/BuildInformation/compare/0.4.2...1.0.0

[0.4.2]: https://github.com/linkdotnet/BuildInformation/compare/0.4.1...0.4.2

[0.4.1]: https://github.com/linkdotnet/BuildInformation/compare/0.4.0...0.4.1

[0.4.0]: https://github.com/linkdotnet/BuildInformation/compare/0.3.0...0.4.0

[0.3.0]: https://github.com/linkdotnet/BuildInformation/compare/0.2.0...0.3.0

[0.2.0]: https://github.com/linkdotnet/BuildInformation/compare/9866bfb38171ce0b36aae085d07d15f6e2bc6ff3...0.2.0
