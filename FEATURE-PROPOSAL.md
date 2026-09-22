# Propose-a-Feature Submission

**Student:** LIM RUI YANG RYAN
**Admin No:** 252206Z
**Tutorial Group:** IT2508
**Module:** IT2814 eBusiness Application Development
**System:** Arcane Vault — Collection Management Prototype

---

## 1. Feature Name

**Vault Insights — Collection Demand & Market Signal Dashboard**

A staff-only business analytics dashboard that measures which collectibles are actually
moving between collections, and ranks them using a composite **Demand Index**.

---

## 2. Purpose

### The problem it solves

The assignment brief states that Arcane Vault's reason for building this prototype is to
gather insight into:

> - Popular collectible items and collections
> - User activity and engagement
> - Market demand and collection trends
> - Overall interest within the collector community

Without this feature, the prototype stores that information but never surfaces it. Staff
can see *that* data exists, but cannot answer the actual business question the company is
asking: **is there enough real trading appetite to justify building the full marketplace?**

### The key insight

The required `CollectionItems` schema stores **two** quantity columns:

| Column | Meaning |
|---|---|
| `StartingQuantity` | How many the collector originally acquired |
| `CurrentQuantity` | How many they still hold today |

There is no reason to store both unless the *difference* matters. That difference is the
only evidence anywhere in the prototype that a collectible has **left a collection** —
traded, sold, or given away.

Aggregated across every collector on the platform, that gap stops being an accounting
detail and becomes a **measurable demand signal**. Vault Insights is built entirely on
that observation.

### Value added

- Converts passive collection records into a decision-making tool for the business.
- Directly answers all four bullet points in the brief's Background section.
- Gives Arcane Vault an evidence base for which collectible segments a future marketplace
  should launch with — the stated purpose of the prototype.

---

## 3. Key Functionalities

### 3.1 The Demand Index (the core)

For each distinct collectible (grouped by name across all collectors), three components
are computed, each normalised to a 0–100 scale so they can be meaningfully combined:

| # | Component | Weight | What it measures | Formula |
|---|---|---|---|---|
| 1 | **Turnover** | 50% | Share of everything acquired that has since moved on | `(ΣStarting − ΣCurrent) / ΣStarting × 100` |
| 2 | **Collector Reach** | 30% | How widely held it is, vs. the most widely held item | `holders / maxHolders × 100` |
| 3 | **Scarcity** | 20% | How little remains in circulation, vs. the largest holding | `(1 − ΣCurrent / maxRemaining) × 100` |

```
DemandIndex = (Turnover × 0.50) + (ReachScore × 0.30) + (ScarcityScore × 0.20)
```

**Why these weights:** turnover carries the most weight because units physically leaving
collections is the most *direct* evidence of demand. Reach and scarcity are supporting
context — a high turnover on an item only one person owns is far weaker evidence than the
same turnover across eight collectors.

**Why normalise:** the three components are measured on incompatible natural scales
("held by 8 people" vs. "72% turnover"). Normalising reach and scarcity against the
strongest item in the dataset makes them comparable so the weighted sum is meaningful.

Scores are then banded for at-a-glance reading:

| Tier | Index | Interpretation |
|---|---|---|
| 🔴 **Hot** | ≥ 60 | Strong, active trading demand |
| 🟠 **Rising** | 40–59 | Building interest |
| 🔵 **Steady** | 20–39 | Stable, low movement |
| ⚪ **Dormant** | < 20 | Sitting unused — promotion candidate |

### 3.2 Supporting functionality

| Function | Description |
|---|---|
| **Platform KPI row** | Total collectors, active collectors (last 30 days), distinct collectibles, units held, units moved, overall platform turnover rate |
| **Demand leaderboard** | Ranked table with per-item component breakdown, progress-bar visualisation, and tier badge. Adjustable Top 10 / 20 / All |
| **Category popularity** | Share of classified items per category, with distinct-collector reach |
| **Activity trend** | Items added and updated per month over a selectable 3 / 6 / 12-month window |
| **Rule-based highlights** | Plain-English observations generated from the figures, colour-coded by severity |

### 3.3 Rule-based insight generation

Rather than only presenting numbers, the feature applies business rules to produce
actionable statements, for example:

- *"One Piece Luffy Gear 5 Figure has the strongest demand signal at 80.1/100 — held by 8 collectors, with 75.0% of acquired units already moved on."*
- *"4 of 24 collectibles are in the Hot tier, suggesting genuine trading appetite worth targeting in the marketplace build."*
- *"4 collectibles show almost no movement, led by SG 50th Anniversary Coin Set. These are candidates for promotion or bundling."*

---

## 4. Expected UI / API Components

### UI (ArcaneVault.Web — Razor Pages + Bootstrap 5)

| Component | Implementation |
|---|---|
| KPI cards | Bootstrap grid, colour-coded by threshold |
| Demand leaderboard | Responsive Bootstrap table with progress bars and tier badges |
| Category popularity | **Chart.js** doughnut chart with custom tooltips |
| Activity trend | **Chart.js** filled line chart, dual series |
| Highlights panel | Bootstrap alerts keyed by severity |
| Controls | Dropdowns (Top N, month window) and a refresh button, re-fetching without a page reload |
| States | Friendly empty states and per-panel error notices — one failing panel never breaks the page |

### API (ArcaneVault.Api — ASP.NET Core Web API)

Five dedicated endpoints on `InsightsController`, all aggregating server-side with LINQ
`GroupBy` and returning purpose-built DTOs:

| Endpoint | Returns |
|---|---|
| `GET /api/insights/overview` | `VaultOverviewDto` — platform KPIs |
| `GET /api/insights/demand?top=` | `DemandSignalDto[]` — ranked Demand Index |
| `GET /api/insights/categories` | `CategoryPopularityDto[]` |
| `GET /api/insights/activity?months=` | `ActivityPointDto[]` |
| `GET /api/insights/highlights` | `InsightHighlightDto[]` |

### Architecture note

The browser never calls the API directly. The dashboard's JavaScript fetches **same-origin
Razor Page handlers** (`/Insights?handler=Demand`), which call the API server-side through
a typed `HttpClient`. This means:

- No CORS configuration is required.
- The analytics endpoints inherit the page's `StaffOnly` authorisation policy.
- The API's address is never exposed to the browser.

### Schema changes

Two columns added to `CollectionItems` (permitted by the brief), used for the engagement
and trend functionality:

| Column | Purpose |
|---|---|
| `CreatedAt` | Plots collection growth over time |
| `LastUpdatedAt` | Identifies recently active collectors |

No changes were made to any of the five required tables' specified columns, keys, or
relationships.

---

## 5. Justification of Complexity

Against the module's stated criteria for a **Complex Feature**:

| Criterion | How Vault Insights meets it |
|---|---|
| *"significantly enhanced UI/UX and thoughtful design improvements"* | Charts, progress-bar visualisations, tier badges, per-panel error isolation, empty states, live filter controls |
| *"meaningful new functionalities beyond standard Visual Studio templates"* | Nothing here is scaffolded CRUD — it is an original derived metric with weighting and normalisation |
| *"application of business analytics to generate actionable insights"* | The Demand Index is a purpose-built composite metric, and the rule engine turns it into written recommendations |

---

## 6. Questions for Tutor Feedback

1. Is the three-component weighting (50/30/20) an appropriate level of analytical depth,
   or would you prefer a simpler or more sophisticated model?
2. Is grouping by `ItemName` an acceptable way to identify "the same collectible" across
   collectors, given the prototype schema has no shared catalogue table?
3. Would adding a second moderate feature strengthen the submission, or is one complex
   feature implemented thoroughly the better use of the remaining time?
