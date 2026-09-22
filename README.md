
# ArcaneVault: Vault Insights Dashboard

**Based on the feature requirements outlined in EAD assignment proposal (1).pdf.**

## Description & Purpose
This project implements the Vault Insights Dashboard for the ArcaneVault platform, focusing on Collection Demand & Market Signal Analytics. 
The primary purpose is to solve the problem of identifying market trends within the collectible platform by transforming passive collection records into actionable insights.
By measuring how collectibles move between users, this tool allows staff to make data-driven decisions, such as promoting dormant items or capitalizing on high-demand trends.

## Key Features
* **Composite Demand Index:** Calculates a composite score for each collectible to rank market interest using three core metrics
  * **Turnover (share of items moved):** $\frac{\sum \text{Start} - \sum \text{Curr}}{\sum \text{Start}} \times 100$
  * **Collector Reach (distribution across distinct users):** $\frac{\text{holders}}{\text{maxHolders}} \times 100
  * **Scarcity (items left in circulation):** $(1-\frac{\sum \text{Curr}}{\text{maxRemaining}}) \times 100$
* **Automated Categorization:** Automatically categorizes items into distinct Demand Tiers, including Hot, Rising, Steady, and Dormant
* **Role-Based Access Control (RBAC):** Enforces security measures so that only Staff accounts can view these market insights
* **Data Visualization:** Uses metric cards, data tables, and dynamic charts to visualize demand distribution and market signals

## Technology Stack & Architecture
* **Solution Structure:** The solution is split into distinct backend and frontend projects: `ArcaneVault.Api` and `ArcaneVault.Web`
* **Backend API:** Built with ASP.NET Core Web API (.NET 10) to create custom internal RESTful endpoints that securely fetch aggregated analytics
* **Frontend Web App:** Built using ASP.NET Core Razor Pages
* **UI/UX:** Utilizes Bootstrap 5.3 for responsive dashboard layouts
* **Data Visualization:** Integrates Chart.js 4.4 for rendering graphs and charts
* **Database:** SQLite is used to store and aggregate the collection records

## Learning Outcomes
* **Full-Stack ASP.NET Core Development:** Gained hands-on experience structuring a decoupled architecture with a dedicated Web API (`ArcaneVault.Api`) and a front-end UI application (`ArcaneVault.Web`)
* **Complex Data Aggregation:** Learned how to query a SQLite database and compute composite mathematical indexes (Turnover, Reach, Scarcity) within a C# backend
* **Security & Authentication:** Implemented role-based access control to restrict sensitive business intelligence features to authorized staff
* **Data Visualization:** Developed skills in transforming raw API data into visual market signals using Chart.js 4.4 and formatting responsive layouts with Bootstrap 5.3
