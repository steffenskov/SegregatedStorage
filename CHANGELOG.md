# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.0.0] - 2026-10-03

### Changed

- Split Asp.Net endpoint mapping into a separate project: [SegregatedStorage.AspNetCore](https://www.nuget.org/packages/SegregatedStorage.AspNetCore)~~~~

## [1.9.1] - 2026-08-13

### Fixed

- `DeletionBackgroundService` could bubble up a `FileNotFoundException` causing the entire Web application to crash.

## [1.9.0] - 2026-06-26

### Added

- StoredFile now contains a `FileHash` property which is auto-generated as part of the upload. You configure which algorithm to use via the configuration argument to `AddStorageService()` (Default is `MD5`)