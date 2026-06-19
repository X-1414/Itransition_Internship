# User Management App

## Overview

A simple ASP.NET Core MVC user management system with authentication, email verification, and admin controls (block/unblock/delete unverified).

---

## Requirements Met

| # | Requirement | Implementation |
|---|------------|----------------|
| 1 | Unique index in DB | EF Core creates PostgreSQL unique index `ix_users_email_unique` for `Email` field |
| 2 | Clean UI table + toolbar | Bootstrap table with action toolbar above it |
| 3 | Sorted by last login | `OrderByDescending(u => u.LastLoginAt)` with fallback sorting |
| 4 | Multi-select user actions | Checkbox selection + select-all in table header |
| 5 | Server-side session validation | `UserStatusFilter` checks session user exists and is not blocked |
| 6 | Email verification | Brevo API-based verification email system |

---

## Features

### Authentication
- User registration
- Email verification required for activation
- Login with password hashing (BCrypt)
- Logout with session cleanup

### User Management
- View all users in a table
- Filter by status and email
- Block / Unblock users
- Delete selected users
- Delete all unverified users

### Session Handling
- Session stored in server-side session (`UserId`, `UserName`)
- Automatic logout if user is deleted or blocked
- Server-side validation before every request (`UserStatusFilter`)

### Email System
- Uses Brevo Transactional Email API (HTTP via `HttpClient`)
- Sends verification email asynchronously after registration

---

## Tech Stack

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core (Npgsql for PostgreSQL)
- PostgreSQL database
- BCrypt.Net-Next for password hashing
- Brevo Email API (HTTP-based, no SMTP)
- Bootstrap 5.3 + Bootstrap Icons

---

## Database

### Users Table (simplified)

- Id (PK)
- Name
- Email (unique index)
- PasswordHash
- Status (Unverified / Active / Blocked)
- RegisteredAt
- LastLoginAt
- EmailVerificationToken
- WasEverVerified
- CurrentSessionStartUnix

---

## Notes

- Email uniqueness is enforced at database level (not only application level)
- Invalid duplicate registration attempts are safely handled using PostgreSQL error code `23505`
- No frontend charting or sparklines are currently implemented (removed)