# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project
adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Releases up to and including 3.3.0 are documented on the
[GitHub releases page](https://github.com/wdolek/w4k-aspnetcore-correlator/releases).

## [3.5.0] - 2026-09-13

### Added

- `CorrelationValuePatternValidator`: shipped default validator accepting non-empty values up to 80 characters consisting of characters safe for logging only
- `WithDefaultValidator()`: shorthand for registering the shipped default validator
- Security guidance: correlation header values treated as attacker-controlled input, see [README](README.md#security)

## [3.4.0] - 2025-11-18

### Added

- Explicit .NET 10 support (#69)
