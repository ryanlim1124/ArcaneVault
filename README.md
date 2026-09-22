# Arcane Vault — Collection Management Prototype

**Student:** LIM RUI YANG RYAN · **Admin No:** 252206Z · **Tutorial Group:** IT2508
**Module:** IT2814 eBusiness Application Development

A prototype web platform for collectible hobby shops and collectors to manage their
personal collections online, with a staff-only analytics dashboard that measures market
demand from real collection activity.

---

## 1. Quick start

> **Prerequisite:** .NET 10 SDK. Verify with `dotnet --list-sdks`.

The API must be started **first** — it owns the database and seeds it on first run.

### Option A — Visual Studio (recommended)

1. Open `ArcaneVault.sln`.
2. Right-click the **solution** in Solution Explorer → **Configure Startup Projects…**
3. Select **Multiple startup projects** and set both to **Start**:

   | Project | Action |
   |---|---|
   | `ArcaneVault.Api` | Start |
   | `ArcaneVault.Web` | Start |

4. Make sure `ArcaneVault.Api` is listed **above** `ArcaneVault.Web` (use the arrows) so it
   starts first.
5. Press **F5**. Two browser windows open: Swagger UI and the web app.

### Option B — Two terminals

**Terminal 1 (API):**
```bash
cd ArcaneVault.Api
dotnet run
```
Wait for `Now listening on: http://localhost:5001`.

**Terminal 2 (Web):**
```bash
cd ArcaneVault.Web
dotnet run
```

Then open **<http://localhost:5002>**.

### URLs

| Service | HTTP | HTTPS |
|---|---|---|
| Web app | <http://localhost:5002> | <https://localhost:7002> |
| API (Swagger) | <http://localhost:5001/swagger> | <https://localhost:7001/swagger> |

### Demo accounts

The database seeds itself on first run. Two accounts are provided:

| Role | Username | Password | Lands on |
|---|---|---|---|
| **Staff** | `staff` | `Staff@123` | Vault Insights dashboard |
| **Collector** | `collector` | `User@123` | My Collection |

Seven further collector accounts exist (`cardmaster_lee`, `vault_amelia`, `retro_toys_sg`,
`kaiju_ken`, `mintcondition`, `figure_hunter`, `tcg_tanya`) — all with password `User@123`.

> **To reset to a clean demo state:** stop both apps, delete `ArcaneVault.Api/arcanevault.db*`,
> and restart the API. It re-migrates and re-seeds automatically.

---

## 2. Tech stack

| Layer | Technology |
|---|---|
| Web application | ASP.NET Core **Razor Pages** (.NET 10) |
| API | ASP.NET Core **Web API** (.NET 10) |
| Data access | **Entity Framework Core 10** |
| Database | **SQLite** (`arcanevault.db`) |
| UI | Bootstrap 5.3, Bootstrap Icons (CDN) |
| Charts | Chart.js 4.4 (CDN) |
| Auth | Cookie authentication + PBKDF2 password hashing |
| API docs | Swashbuckle / Swagger UI |

Third-party dependencies are kept minimal — EF Core (+ SQLite, Design) and Swashbuckle are
the only NuGet packages; Bootstrap Icons and Chart.js load from CDN.

---

## 3. Architecture

```
┌──────────────────┐         HTTP          ┌──────────────────┐        ┌──────────────┐
│  Browser         │ ───────────────────▶  │ ArcaneVault.Web  │ ─────▶ │ ArcaneVault  │
│                  │                       │  (Razor Pages)   │  HTTP  │ .Api         │
│  Bootstrap +     │ ◀───────────────────  │                  │ ◀───── │              │
│  Chart.js        │      HTML / JSON      │  ApiClient       │  JSON  │  EF Core     │
└──────────────────┘                       └──────────────────┘        └──────┬───────┘
                                                                              │
                                                                       ┌──────▼───────┐
                                                                       │ arcanevault  │
                                                                       │    .db       │
                                                                       └──────────────┘
```

**Two projects, one solution:**

- **`ArcaneVault.Api`** — owns EF Core, the SQLite database, all entities and every
  controller. The only project that touches the database.
- **`ArcaneVault.Web`** — the front end. Has no `DbContext` at all. Every page calls the
  API through one typed `HttpClient` service (`Services/ApiClient.cs`).

**Why the browser never calls the API directly:** all API calls happen server-side from
Razor PageModels. Even the dashboard's charts fetch same-origin page handlers
(`/Insights?handler=Demand`) which relay to the API on the server. This removes the need
for any CORS configuration, keeps the API address off the client, and means the analytics
endpoints inherit the page's authorisation policy.

---

## 4. Data model

```
ArcaneVaultUserRoles                ArcaneVaultUsers
┌────────────────────┐             ┌──────────────────────┐
│ RoleId      PK     │◀────────────│ UserName        PK   │
│ RoleName           │      1    * │ Email      (unique)  │
└────────────────────┘             │ IsDeleted            │
                                   │ RoleId          FK   │
                                   │ PasswordHash  (added)│
                                   └──────────┬───────────┘
                                              │ 1
                                              │
                                              │ *
Categories                       CollectionItems
┌────────────────────┐          ┌──────────────────────────┐
│ CategoryCode  PK   │          │ ItemId            PK     │
│ CategoryName       │          │ ItemName                 │
└─────────┬──────────┘          │ IsDeleted                │
          │ 1                   │ StartingQuantity         │
          │                     │ CurrentQuantity          │
          │ *                   │ UserName          FK     │
   ┌──────▼───────────────────┐ │ CreatedAt      (added)   │
   │ CollectionItemCategories │ │ LastUpdatedAt  (added)   │
   │ ItemId        PK, FK     │ └──────────┬───────────────┘
   │ CategoryCode  PK, FK     │◀───────────┘  *
   │  (composite primary key) │      1
   └──────────────────────────┘
```

### Design decisions

| Decision | Reasoning |
|---|---|
| `UserName` and `CategoryCode` are **string primary keys** | Specified by the assignment schema — natural keys rather than surrogate integers |
| `CollectionItemCategories` is an **explicit entity** with a composite PK | The brief names the table and specifies the composite key. Skip navigations are still exposed on both sides for convenience |
| **Restrict** delete on Category→Item and User→Item | Prevents silently orphaning collection records. The API checks first and returns a clear `400` rather than letting a raw FK constraint error escape |
| **Soft delete** (`IsDeleted`) on collection items | "Removing" hides an item from its owner but preserves the historical data behind the analytics |
| `PasswordHash` column **added** | The brief requires registration and login authentication, which is impossible without storing a credential. Hashed with PBKDF2 — plain text is never persisted |
| `CreatedAt` / `LastUpdatedAt` **added** | Supports the Vault Insights activity trend and engagement metrics (the brief permits adding columns for the custom feature) |

---

## 5. Vault Insights — the custom feature

> Full proposal in **[FEATURE-PROPOSAL.md](FEATURE-PROPOSAL.md)**.

A staff-only dashboard that answers the question the brief's Background section actually
poses: *is there real trading demand on this platform?*

### The insight

`CollectionItems` stores both `StartingQuantity` and `CurrentQuantity`. The gap between
them is the only evidence in the system that a collectible has **left a collection**.
Aggregated across all collectors, that gap becomes a measurable demand signal.

### The Demand Index

```
Turnover      = (ΣStarting − ΣCurrent) / ΣStarting × 100      ← 50% weight
ReachScore    = holders / maxHolders × 100                    ← 30% weight
ScarcityScore = (1 − ΣCurrent / maxRemaining) × 100           ← 20% weight

DemandIndex   = Turnover×0.50 + ReachScore×0.30 + ScarcityScore×0.20
```

Banded into **Hot** (≥60) · **Rising** (40–59) · **Steady** (20–39) · **Dormant** (<20).

### Worked example (from seeded data)

| Collectible | Holders | Moved | Turnover | Index | Tier |
|---|---:|---:|---:|---:|---|
| One Piece Luffy Gear 5 Figure | 8 | 21 / 28 | 75.0% | **80.1** | Hot |
| Charizard Base Set Holo | 7 | 29 / 37 | 78.4% | **77.0** | Hot |
| Dragon Ball Z Shenron Statue | 5 | 15 / 26 | 57.7% | **56.0** | Rising |
| Tamagotchi P1 Original | 1 | 0 / 4 | 0.0% | **11.7** | Dormant |

Note Charizard has the *higher* turnover (78.4% vs 75.0%) but the *lower* index — it is
held by one fewer collector, and reach is 30% of the score. That trade-off is the whole
point of a composite metric.

### Dashboard contents

KPI row · ranked Demand Index leaderboard with component breakdown · category-popularity
doughnut · platform-activity trend line · rule-based plain-English highlights.

---

## 6. Assignment requirements map

### Core requirements (70 marks)

| # | Requirement | Where |
|---|---|---|
| 1a | SQLite database file | `ArcaneVault.Api/arcanevault.db` (created by migration) |
| 1b | 5 tables + relationships | `ArcaneVault.Api/Models/`, configured in `Data/AppDbContext.cs` |
| 2a | EF Core connected to SQLite | `Program.cs` → `AddDbContext` + `UseSqlite` |
| 2b | Entities generated | `Models/ArcaneVaultUser.cs`, `ArcaneVaultUserRole.cs`, `Category.cs`, `CollectionItem.cs`, `CollectionItemCategory.cs` |
| 3a–3c | Registration page, model, consuming API | `Pages/Account/Register.cshtml(.cs)`, `Models/AccountDtos.cs`, `Services/ApiClient.cs` |
| 3d | Registration API | `Controllers/AccountsController.cs` → `POST /api/accounts/register` |
| 3e | Validations: required / correct type / duplicate email | DataAnnotations on both DTOs; `[EmailAddress]`; explicit duplicate check + unique index |
| 4a–4b | NavBar shows Login when out, Logout when in | `Pages/Shared/_Layout.cshtml` |
| 4c | Login page | `Pages/Account/Login.cshtml(.cs)` |
| 4d | Performs authentication | API verifies PBKDF2 hash; Web issues auth cookie with role claim |
| 5a–5c | Category CRUD: 5 pages, consuming API, API | `Pages/Categories/*`, `Services/ApiClient.cs`, `Controllers/CategoriesController.cs` |
| 6a | CollectionItem: 5 pages | `Pages/CollectionItems/*` |
| 6b | Search matching **any** field | `Controllers/CollectionItemsController.cs` — matches name, owner, category code/name, and numerically against ItemId and both quantities |
| 6c–6d | Consuming API + API | `Services/ApiClient.cs`, `Controllers/CollectionItemsController.cs` |
| 7a | Category link **only if Staff** | `_Layout.cshtml` `@if (User.IsInRole("Staff"))` + `StaffOnly` policy in `Program.cs` |
| 7b | CollectionItem link | `_Layout.cshtml` |

### Propose-a-Feature (20 marks)

`Controllers/InsightsController.cs` · `Pages/Insights/Index.cshtml(.cs)` ·
`wwwroot/js/insights.js` — see [FEATURE-PROPOSAL.md](FEATURE-PROPOSAL.md).

---

## 7. Security notes

| Concern | Handling |
|---|---|
| Password storage | PBKDF2 via ASP.NET Core `PasswordHasher<T>`, per-user salt. Plain text never stored |
| Login enumeration | Identical error message whether username is unknown, account deactivated, or password wrong |
| Privilege escalation | Self-registration always assigns the `User` role — a visitor cannot grant themselves Staff |
| Role enforcement | Hiding nav links is cosmetic; the real boundary is the `StaffOnly` policy applied to whole folders in `Program.cs`. Direct URL access returns Access Denied |
| Cross-collector access | Every item page verifies the record's owner against the signed-in user before displaying or editing |
| Open redirect | `ReturnUrl` is only honoured when `Url.IsLocalUrl()` passes |
| Logout | POST-only, so it cannot be triggered by a stray link or image request |
| XSS | Razor auto-encodes; the dashboard JS escapes item names before inserting into `innerHTML` |

---

## 8. Project structure

```
EAD Assignment sem1/
├─ ArcaneVault.sln
├─ README.md
├─ FEATURE-PROPOSAL.md
│
├─ ArcaneVault.Api/                  ← Web API + EF Core + SQLite
│  ├─ Controllers/
│  │  ├─ AccountsController.cs        register / login
│  │  ├─ CategoriesController.cs      category CRUD
│  │  ├─ CollectionItemsController.cs item CRUD + any-field search
│  │  └─ InsightsController.cs        ★ Vault Insights analytics
│  ├─ Data/
│  │  ├─ AppDbContext.cs              DbSets, keys, relationships, composite PK
│  │  └─ DbInitializer.cs             relative-date demo data seeder
│  ├─ Dtos/                           read + input DTOs (never expose entities)
│  ├─ Models/                         the five entities
│  ├─ Migrations/
│  └─ Program.cs
│
└─ ArcaneVault.Web/                  ← Razor Pages front end
   ├─ Pages/
   │  ├─ Account/                     Register, Login, Logout, AccessDenied
   │  ├─ Categories/                  Index, Create, Details, Edit, Delete  (Staff)
   │  ├─ CollectionItems/             Index, Create, Details, Edit, Delete
   │  ├─ Insights/                    ★ Vault Insights dashboard            (Staff)
   │  ├─ Shared/_Layout.cshtml        role-aware navbar
   │  └─ Index.cshtml                 landing page
   ├─ Models/                          DTO mirrors for deserialisation
   ├─ Services/
   │  ├─ ApiClient.cs                 typed HttpClient — the only HTTP in the project
   │  └─ ApiClientException.cs
   ├─ wwwroot/js/insights.js          dashboard charts + rendering
   └─ Program.cs                      cookie auth, StaffOnly policy, HttpClient wiring
```

Every `.cs` and `.cshtml` file carries the required student header comment.

---

## 9. Troubleshooting

| Symptom | Cause / fix |
|---|---|
| *"Could not reach the Arcane Vault API"* | `ArcaneVault.Api` is not running. Start it first |
| Port already in use | Another instance is running. Stop it, or change the ports in `Properties/launchSettings.json` (and `ApiBaseUrl` in `ArcaneVault.Web/appsettings.json` to match) |
| Dashboard shows no data | Delete `ArcaneVault.Api/arcanevault.db*` and restart the API to re-seed |
| Browser HTTPS certificate warning | Run `dotnet dev-certs https --trust` once |
| Categories link missing | You are signed in as a collector, not Staff. Sign in as `staff` |
