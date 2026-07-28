# Authentication and authorization

[Русская версия](authentication.ru.md) · [Back to README](../README.md)

## Authentication

YouMentor acts as an OAuth 2.0 / OpenID Connect resource server.

Keycloak issues JWT access tokens. The API validates:

- issuer;
- audience;
- token lifetime;
- signing key;
- role claims.

The configured audience is `youmentor-api`, and roles are read from the `roles` claim.

## Authorization model

All session endpoints require an authenticated user.

Two policies are currently configured:

- `MentorOnly` requires the `mentor` role;
- `StudentOnly` requires the `student` role.

| Operation | Requirement |
|---|---|
| Create session | `mentor` role |
| List sessions | Authenticated user |
| Book session | `student` role |
| Cancel session | `mentor` role and ownership check |

## Current user

Application handlers do not parse JWT claims directly. They depend on `ICurrentUser`, which exposes the current user identifier from the request context.

This keeps application code independent from `HttpContext` and allows tests to inject authenticated, unauthenticated, mentor, or student scenarios.

## Resource ownership

Role checks alone are not sufficient for cancellation. The handler also verifies that the current mentor owns the selected session.

A mentor with the correct role therefore cannot cancel another mentor's session.

## Local Keycloak setup

The development environment currently requires manual creation of the realm, client, roles, users, and role claim mapping. See the [local development guide](local-development.md).
