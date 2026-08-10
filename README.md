# Back-End Development with .NET

This repository contains the final project for the Back-End Development with .NET course.

## User Management API

The project is an ASP.NET Core Web API that provides CRUD operations for managing users.

### Features

- Get all users
- Get a user by ID
- Create a user
- Update a user
- Delete a user
- User data validation
- Duplicate email validation
- Custom request logging middleware
- Swagger/OpenAPI documentation

### API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/users | Get all users |
| GET | /api/users/{id} | Get a user by ID |
| POST | /api/users | Create a user |
| PUT | /api/users/{id} | Update a user |
| DELETE | /api/users/{id} | Delete a user |

## Validation

The API validates user data using Data Annotations.

User fields include:

- Name: required
- Email: required and must be a valid email address
- Age: must be between 18 and 120

The API also prevents duplicate email addresses.

## Middleware

The project includes custom logging middleware that records:

- HTTP method
- Request path
- HTTP response status
- Request processing time

## GitHub Copilot

GitHub Copilot was used during development to assist with writing,
enhancing, debugging, and reviewing the API code.

## Technologies

- C#
- .NET 8
- ASP.NET Core Web API
- Swagger / OpenAPI
# User Management API - Security Project

This project is an ASP.NET Core API developed as part of the
Security and Authentication course.

It demonstrates secure input validation, authentication,
authorization, role-based access control, SQL injection prevention,
XSS mitigation, password hashing, and automated security testing.

## Technologies

- ASP.NET Core
- .NET 8
- ASP.NET Core Identity
- Entity Framework Core
- SQLite
- JWT Bearer Authentication
- xUnit
- Swagger / OpenAPI

## Security Features

### Input Validation

ASP.NET Core Data Annotations are used to validate incoming data.

User names are restricted to expected characters using validation
rules. Invalid input is rejected before being stored.

Example malicious input:

```html
<script>alert('XSS')</script>
