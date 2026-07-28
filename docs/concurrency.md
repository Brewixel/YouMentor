# Concurrency handling

[Русская версия](concurrency.ru.md) · [Back to README](../README.md)

## Problem

Two students can read the same free session and try to book it at nearly the same time. Without concurrency control, the later write could overwrite the earlier one or produce an inconsistent result.

## Current approach

`Session.Version` is configured as an EF Core concurrency token. PostgreSQL and EF Core detect when the row has changed since it was loaded and throw `DbUpdateConcurrencyException`.

The booking handler executes inside a named Polly `ResiliencePipeline`:

1. Load the latest session state.
2. Recheck the domain rules through `Session.Book`.
3. Attempt to save the change.
4. On `DbUpdateConcurrencyException`, clear the stale EF Core change tracker and rethrow.
5. Polly retries the complete operation, causing a fresh load and another business-rule check.

The pipeline uses a bounded number of retries, exponential backoff, jitter, and the request cancellation token.

## What is retried

Only `DbUpdateConcurrencyException` is handled by this pipeline.

The following outcomes are returned immediately:

- unauthenticated user;
- session not found;
- validation failure;
- session already booked or cancelled;
- session already started.

This prevents ordinary domain conflicts from being treated as transient infrastructure failures.

## Result of competing bookings

After one booking is committed, another attempt reloads the changed row. The domain model sees that the session is no longer free and returns a conflict instead of overwriting the successful booking.

## Current follow-up work

- complete dedicated tests for retry execution and exhaustion;
- cover a real competing-booking scenario;
- apply the same pattern to cancellation only where a retry is semantically safe.
