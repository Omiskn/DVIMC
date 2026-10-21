# ADR-0001 — Target `net8.0-windows`

**Status:** Accepted · **Date:** 2026-10-21

## Context

The product brief asks for .NET 10 LTS. The development machine has only the **.NET 8.0.100
SDK** installed, and no .NET 9 or .NET 10 SDK. Installing a new SDK would change the user's
machine toolchain, which is outside the scope of ordinary feature work and was not authorised.

## Decision

The solution targets **`net8.0-windows`** (WPF) with `net8.0` for the core library and the test
project. The target framework is declared once per `.csproj` and can be raised in a single edit.

## Consequences

- The application builds and runs on the installed toolchain today, with no machine changes.
- All APIs used are available on .NET 8 and remain available on .NET 10, so upgrading later is a
  version bump, not a rewrite.
- `net10.0-windows` was not used because it cannot be built here; claiming it would have meant
  shipping unverified configuration.
