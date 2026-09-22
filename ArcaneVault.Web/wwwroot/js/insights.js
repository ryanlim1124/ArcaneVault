// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
//
// Drives the Vault Insights dashboard. Every request goes to a same-origin Razor Page
// handler on /Insights (see Index.cshtml.cs), which calls ArcaneVault.Api server-side.
// The browser never contacts the API directly, so no CORS configuration is needed.


//The second i open vault insights page, this file fires off the requests to fetch the latest 
//math and thne physically draws the charts, progress bars, and tier badges on the screen using the fresh data


(function () {
    'use strict';

    let categoryChart = null;
    let activityChart = null;

    const TIER_COLORS = {
        Hot: '#dc3545',
        Rising: '#fd7e14',
        Steady: '#0d6efd',
        Dormant: '#6c757d',
    };

    const TIER_BADGES = {
        Hot: 'text-bg-danger',
        Rising: 'text-bg-warning',
        Steady: 'text-bg-primary',
        Dormant: 'text-bg-secondary',
    };

    const SEVERITY_ALERTS = {
        danger: 'alert-danger',
        warning: 'alert-warning',
        success: 'alert-success',
        info: 'alert-info',
    };

    /** Fetches JSON from a page handler, treating both HTTP errors and {error:...} bodies as failures. */
    async function fetchJson(url) {
        const response = await fetch(url, { headers: { Accept: 'application/json' } });
        const body = await response.json().catch(() => null);

        if (!response.ok || (body && body.error)) {
            throw new Error((body && body.error) || `Request failed (${response.status})`);
        }
        return body;
    }

    /** Replaces a panel's contents with an inline error notice. */
    function showPanelError(elementId, message) {
        const el = document.getElementById(elementId);
        if (el) {
            el.classList.remove('d-none');
            el.textContent = message;
        }
    }

    function setText(id, value) {
        const el = document.getElementById(id);
        if (el) {
            el.textContent = value;
        }
    }

    // -----------------------------------------------------------
    // KPI row
    // -----------------------------------------------------------
    async function renderOverview() {
        try {
            const data = await fetchJson('?handler=Overview');

            setText('kpiCollectors', data.totalCollectors.toLocaleString());
            setText('kpiActive', `${data.activeCollectors} active in last 30 days`);
            setText('kpiCollectibles', data.distinctCollectibles.toLocaleString());
            setText('kpiRecords', `across ${data.totalItemRecords} collection records`);
            setText('kpiUnitsHeld', data.totalUnitsHeld.toLocaleString());
            setText('kpiUnitsMoved', `${data.totalUnitsMoved.toLocaleString()} units have moved on`);
            setText('kpiTurnover', `${data.overallTurnoverRate.toFixed(1)}%`);

            // Colour the headline turnover figure by how active the platform looks.
            const turnoverEl = document.getElementById('kpiTurnover');
            if (turnoverEl) {
                turnoverEl.className = 'fs-3 fw-semibold ' +
                    (data.overallTurnoverRate >= 50 ? 'text-danger'
                        : data.overallTurnoverRate >= 25 ? 'text-warning'
                            : 'text-success');
            }
        } catch (err) {
            console.warn('Vault Insights: overview failed', err);
            ['kpiCollectors', 'kpiCollectibles', 'kpiUnitsHeld', 'kpiTurnover']
                .forEach((id) => setText(id, '—'));
        }
    }

    // -----------------------------------------------------------
    // Demand Index leaderboard
    // -----------------------------------------------------------
    async function renderDemand() {
        const tbody = document.getElementById('demandTableBody');
        const emptyState = document.getElementById('demandEmptyState');
        const table = document.getElementById('demandTable');
        const top = document.getElementById('demandTopSelect').value;

        try {
            const items = await fetchJson(`?handler=Demand&top=${encodeURIComponent(top)}`);

            tbody.innerHTML = '';
            const hasData = items.length > 0;
            emptyState.classList.toggle('d-none', hasData);
            table.classList.toggle('d-none', !hasData);

            items.forEach((item, index) => {
                const row = document.createElement('tr');
                const badgeClass = TIER_BADGES[item.tier] || 'text-bg-secondary';
                const barColor = TIER_COLORS[item.tier] || '#6c757d';
                const categories = item.categoryNames.length
                    ? item.categoryNames.join(', ')
                    : 'Uncategorised';

                row.innerHTML = `
                    <td class="text-muted small">${index + 1}</td>
                    <td>
                        <div class="fw-semibold">${escapeHtml(item.itemName)}</div>
                        <div class="text-muted small">${escapeHtml(categories)}</div>
                    </td>
                    <td class="text-end">${item.collectorReach}</td>
                    <td class="text-end">${item.unitsMoved} / ${item.totalStartingQuantity}</td>
                    <td class="text-end">${item.turnoverRate.toFixed(1)}%</td>
                    <td style="min-width:150px;">
                        <div class="d-flex align-items-center gap-2">
                            <div class="progress flex-grow-1" style="height:8px;"
                                 role="progressbar" aria-valuenow="${item.demandIndex}"
                                 aria-valuemin="0" aria-valuemax="100">
                                <div class="progress-bar" style="width:${item.demandIndex}%;background-color:${barColor}"></div>
                            </div>
                            <span class="fw-semibold small" style="min-width:38px;">${item.demandIndex.toFixed(1)}</span>
                        </div>
                    </td>
                    <td class="text-end"><span class="badge ${badgeClass}">${escapeHtml(item.tier)}</span></td>
                `;
                tbody.appendChild(row);
            });
        } catch (err) {
            console.warn('Vault Insights: demand failed', err);
            table.classList.add('d-none');
            showPanelError('demandEmptyState', 'Unable to load the demand ranking.');
        }
    }

    // -----------------------------------------------------------
    // Category popularity (doughnut)
    // -----------------------------------------------------------
    async function renderCategories() {
        const canvas = document.getElementById('categoryChart');
        const emptyState = document.getElementById('categoryEmptyState');

        try {
            const items = await fetchJson('?handler=Categories');
            const hasData = items.length > 0;

            emptyState.classList.toggle('d-none', hasData);
            canvas.classList.toggle('d-none', !hasData);

            if (categoryChart) {
                categoryChart.destroy();
                categoryChart = null;
            }

            if (!hasData) {
                return;
            }

            categoryChart = new Chart(canvas, {
                type: 'doughnut',
                data: {
                    labels: items.map((i) => i.categoryName),
                    datasets: [{
                        data: items.map((i) => i.itemCount),
                        backgroundColor: [
                            '#0d6efd', '#6f42c1', '#d63384', '#fd7e14', '#198754',
                            '#20c997', '#ffc107', '#dc3545', '#0dcaf0', '#6c757d',
                        ],
                    }],
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { position: 'bottom', labels: { boxWidth: 12 } },
                        tooltip: {
                            callbacks: {
                                // Show the share alongside the raw count.
                                label: (ctx) => {
                                    const item = items[ctx.dataIndex];
                                    return `${item.categoryName}: ${item.itemCount} items (${item.shareOfItems.toFixed(1)}%)`;
                                },
                            },
                        },
                    },
                },
            });
        } catch (err) {
            console.warn('Vault Insights: categories failed', err);
            canvas.classList.add('d-none');
            showPanelError('categoryEmptyState', 'Unable to load category popularity.');
        }
    }

    // -----------------------------------------------------------
    // Platform activity trend (line)
    // -----------------------------------------------------------
    async function renderActivity() {
        const canvas = document.getElementById('activityChart');
        const emptyState = document.getElementById('activityEmptyState');
        const months = document.getElementById('activityMonthsSelect').value;

        try {
            const points = await fetchJson(`?handler=Activity&months=${encodeURIComponent(months)}`);
            const hasData = points.some((p) => p.itemsAdded > 0 || p.itemsUpdated > 0);

            emptyState.classList.toggle('d-none', hasData);
            canvas.classList.toggle('d-none', !hasData);

            if (activityChart) {
                activityChart.destroy();
                activityChart = null;
            }

            if (!hasData) {
                return;
            }

            activityChart = new Chart(canvas, {
                type: 'line',
                data: {
                    labels: points.map((p) => p.label),
                    datasets: [
                        {
                            label: 'Items added',
                            data: points.map((p) => p.itemsAdded),
                            borderColor: '#0d6efd',
                            backgroundColor: 'rgba(13,110,253,0.15)',
                            fill: true,
                            tension: 0.3,
                        },
                        {
                            label: 'Items updated',
                            data: points.map((p) => p.itemsUpdated),
                            borderColor: '#fd7e14',
                            backgroundColor: 'rgba(253,126,20,0.15)',
                            fill: true,
                            tension: 0.3,
                        },
                    ],
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: { legend: { position: 'bottom' } },
                    scales: { y: { beginAtZero: true, ticks: { precision: 0 } } },
                },
            });
        } catch (err) {
            console.warn('Vault Insights: activity failed', err);
            canvas.classList.add('d-none');
            showPanelError('activityEmptyState', 'Unable to load the activity trend.');
        }
    }

    // -----------------------------------------------------------
    // Rule-based highlights
    // -----------------------------------------------------------
    async function renderHighlights() {
        const list = document.getElementById('highlightsList');
        const emptyState = document.getElementById('highlightsEmptyState');

        try {
            const highlights = await fetchJson('?handler=Highlights');

            list.innerHTML = '';
            emptyState.classList.toggle('d-none', highlights.length > 0);

            highlights.forEach((h) => {
                const alert = document.createElement('div');
                alert.className = `alert ${SEVERITY_ALERTS[h.severity] || 'alert-info'} mb-2`;
                alert.textContent = h.message;
                list.appendChild(alert);
            });
        } catch (err) {
            console.warn('Vault Insights: highlights failed', err);
            list.innerHTML = '';
            showPanelError('highlightsEmptyState', 'Unable to load insights.');
        }
    }

    /** Escapes user-supplied text before it goes into innerHTML. */
    function escapeHtml(value) {
        const div = document.createElement('div');
        div.textContent = value ?? '';
        return div.innerHTML;
    }

    // -----------------------------------------------------------
    // Wire-up
    // -----------------------------------------------------------
    document.addEventListener('DOMContentLoaded', () => {
        document.getElementById('demandTopSelect').addEventListener('change', renderDemand);
        document.getElementById('activityMonthsSelect').addEventListener('change', renderActivity);
        document.getElementById('refreshButton').addEventListener('click', loadAll);

        loadAll();
    });

    /** Loads every panel in parallel so one slow call does not hold up the rest. */
    function loadAll() {
        return Promise.all([
            renderOverview(),
            renderDemand(),
            renderCategories(),
            renderActivity(),
            renderHighlights(),
        ]);
    }
})();
