# CV Management System

## Overview

CV Management System is an ASP.NET Core MVC recruitment platform that allows candidates to maintain reusable professional profiles and automatically generate tailored CVs for recruiter/admin-created positions. The application supports three user roles (Candidate, Recruiter, Administrator) and includes integrations with Salesforce, Odoo, and Microsoft Power Automate.

---

# Technology Stack

* ASP.NET Core MVC (.NET 10)
* Entity Framework Core
* PostgreSQL
* ASP.NET Identity
* Bootstrap
* Docker
* Odoo 17
* Salesforce REST API
* Microsoft Power Automate
* Dropbox
* Google OAuth
* GitHub OAuth

---

# Installation

## Clone the repository

```bash
git clone git@github.com:X-1414/Itransition_Internship.git
cd final_project/
```

## Restore NuGet packages

```bash
dotnet restore
```

## Apply database migrations

```bash
dotnet ef database update --project src/CV_mng_sys.Core --startup-project src/CV_mng_sys.Web
```

## Run the application

```bash
cd src/CV_mng_sys.Web
dotnet build
dotnet run
```

The application will be available at

```
https://localhost:5249
https://cv-management-system-bh24.onrender.com

```

---

# Docker

Build:

```bash
docker build -t cv-management-system .
```

Run:

```bash
docker run -p 8080:8080 cv-management-system
```

---

# Required Environment Variables

```
ConnectionStrings__DefaultConnection

Authentication__Google__ClientId
Authentication__Google__ClientSecret

Authentication__GitHub__ClientId
Authentication__GitHub__ClientSecret

SeedAdmin__Email
```

For integrations, configure the required Salesforce, Dropbox and Power Automate credentials.

---

# Odoo Setup

Pull required images:

```bash
docker pull postgres:15
docker pull odoo:17
```

Create Docker network:

```bash
docker network create odoo-net
```

Run PostgreSQL:

```bash
docker run -d --name odoo-db --network odoo-net -e POSTGRES_USER=odoo -e POSTGRES_PASSWORD=odoo_pw -e POSTGRES_DB=postgres postgres:15
```

Run Odoo:

```bash
docker run -d --name odoo-app --network odoo-net -p 8069:8069 -e HOST=odoo-db -e USER=odoo -e PASSWORD=odoo_pw -v odoo-custom-addons:/mnt/extra-addons odoo:17
```

Open

```
http://localhost:8069
```

Install the custom **Inventory Import** module.

---

# Demo Accounts

| Role          | Email               | Password    |
| ------------- | -----------         | ----------- |
| Administrator | khv.uzb14@gmail.com | Xadicha1403 |
| Recruiter     | recruiter@gmail.com | recruiter   |
| Candidate     | candidate@gmail.com | candidate   |

---

# Feature Walkthrough

## Candidate

* Register or login
* Complete personal profile
* Add reusable attributes
* Manage projects
* Browse available positions
* Generate CV
* Edit CV values
* Publish / Unpublish CV

---

## Recruiter

* Create positions
* Edit positions
* Duplicate positions
* Configure access rules
* Browse candidate CVs
* Like CVs
* Participate in discussions

---

## Administrator

* Manage users
* Assign/remove roles
* Block / Unblock users
* Edit any candidate profile
* Edit any CV
* Manage all positions

---

# Integration Walkthrough

## Salesforce

1. Open Profile page
2. Click **Export to Salesforce**
3. Fill additional information
4. Submit
5. Verify Account created
6. Verify Contact created

---

## Microsoft Power Automate

1. Click **Create Support Ticket**
2. Enter summary and priority
3. Submit ticket
4. Verify JSON uploaded to Dropbox
5. Verify Power Automate flow executed
6. Verify email notification
7. Verify mobile notification

---

## Odoo

1. Open Position
2. Generate API Token
3. Copy token
4. Open Odoo
5. Create Import record
6. Paste token
7. Click **Import**
8. Verify imported aggregated statistics

---

# Implemented Requirements

## Authentication & Authorization

* ASP.NET Identity
* Candidate role
* Recruiter role
* Administrator role
* Google authentication
* GitHub authentication

## Candidate Profile

* Mandatory profile fields
* Reusable attribute library
* Project management
* Markdown project descriptions
* Optimistic locking

## Attribute Library

* Categories
* Multiple attribute types
* Prefix search
* Recently used attributes
* Reusable attributes

## Position Management

* Create/Edit/Delete positions
* Duplicate positions
* Position templates
* Access rules
* Project tag filtering

## CV Generation

* Automatic CV generation
* Auto-filled profile values
* In-place editing
* Missing value highlighting
* Draft / Published states

## Recruiter Features

* Browse published CVs
* Likes
* Discussions (Polling)

## Administrator Features

* User management
* Role management
* Full application access

## Homepage

* Latest positions
* Most popular positions
* Statistics
* Technology tag cloud

## Additional Features

* English / Russian localization
* Dark / Light theme
* Responsive design
* Optimistic concurrency
* Full-text search

## External Integrations

* Salesforce
* Microsoft Power Automate
* Odoo
* Docker deployment

---