# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.9.0] - 2026-06-26

### Added

- StoredFile now contains a `FileHash` property which is auto-generated as part of the upload. You configure which algorithm to use via the configuration argument to `AddStorageService()` (Default is `MD5`)