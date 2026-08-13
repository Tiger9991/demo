(function () {
    window.trapsDashboardChromeReady = true;

    function getChartColors(elementId) {
        var element = document.getElementById(elementId);

        if (!element) {
            return null;
        }

        var colors = element.getAttribute("data-colors");

        if (!colors) {
            return null;
        }

        return JSON.parse(colors).map(function (color) {
            var pair = color.split(",");

            if (pair.length === 2) {
                return "rgba(" + getComputedStyle(document.documentElement).getPropertyValue(pair[0]) + "," + pair[1] + ")";
            }

            return color.replace(" ", "");
        });
    }

    function chartHasContent(element) {
        return !!(element && element.querySelector(".apexcharts-canvas svg"));
    }

    function chartNeedsRender(element) {
        return !!element && (element.dataset.chartReady !== "true" || !chartHasContent(element));
    }

    function renderApexChart(element, options) {
        if (!element || element.dataset.chartRendering === "true" || !chartNeedsRender(element)) {
            return;
        }

        if (element._trapsChart) {
            try {
                element._trapsChart.destroy();
            } catch (_) {
            }
        }

        element.innerHTML = "";
        element.dataset.chartRendering = "true";
        delete element.dataset.chartReady;

        var chart = new window.ApexCharts(element, options);
        element._trapsChart = chart;

        Promise.resolve(chart.render())
            .then(function () {
                element.dataset.chartReady = "true";
            })
            .catch(function () {
                delete element.dataset.chartReady;
            })
            .finally(function () {
                delete element.dataset.chartRendering;
            });
    }

    function renderDashboardCharts() {
        if (!window.ApexCharts) {
            return;
        }

        var monthlyElement = document.querySelector("#monthlyEarningsChart");
        var monthlyColors = getChartColors("monthlyEarningsChart");

        if (monthlyElement && monthlyColors && chartNeedsRender(monthlyElement)) {
            renderApexChart(monthlyElement, {
                series: [74],
                chart: { height: 300, type: "radialBar", offsetY: 0 },
                plotOptions: {
                    radialBar: {
                        startAngle: -135,
                        endAngle: 135,
                        dataLabels: {
                            name: { show: false },
                            value: {
                                show: true,
                                offsetY: 10,
                                fontSize: "28px",
                                fontWeight: "bold",
                                color: "#333",
                                formatter: function (value) {
                                    return value + "%";
                                }
                            }
                        }
                    }
                },
                colors: monthlyColors,
                fill: {
                    type: "gradient",
                    gradient: {
                        shade: "dark",
                        shadeIntensity: 0.1,
                        inverseColors: false,
                        opacityFrom: 1,
                        opacityTo: 1,
                        gradientToColors: monthlyColors,
                        stops: [0, 50, 65, 91]
                    }
                },
                grid: { padding: { top: -20, bottom: -10, left: 10, right: 10 } },
                stroke: { dashArray: 4 },
                labels: ["Median Ratio"]
            });
        }

        var productivityElement = document.querySelector("#teamProductivityChart");
        var productivityColors = getChartColors("teamProductivityChart");

        if (productivityElement && productivityColors && chartNeedsRender(productivityElement)) {
            renderApexChart(productivityElement, {
                chart: {
                    height: 310,
                    toolbar: { show: false },
                    zoom: { enabled: false },
                    dropShadow: { enabled: true, top: 10, left: 2, blur: 4, color: "#000", opacity: 0.2 }
                },
                series: [
                    { name: "Food consumption", type: "line", data: [20000, 28500, 18000, 19800, 15500, 22200, 29000, 21200, 18800, 28600, 18000, 28000] },
                    { name: "Visit patterns", type: "area", data: [10000, 18500, 9000, 25000, 12000, 14000, 6000, 22000, 12000, 17000, 10000, 18000] }
                ],
                stroke: { curve: "smooth", width: [2, 2] },
                fill: {
                    type: ["solid", "gradient"],
                    gradient: {
                        shade: "light",
                        type: "vertical",
                        shadeIntensity: 0.5,
                        gradientToColors: [productivityColors[1]],
                        inverseColors: true,
                        opacityFrom: 0.2,
                        opacityTo: 0,
                        stops: [0, 100]
                    }
                },
                colors: [productivityColors[0], productivityColors[1]],
                xaxis: { categories: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"] },
                yaxis: {
                    min: 0,
                    labels: {
                        formatter: function (value) {
                            return value / 1000 + "k";
                        }
                    }
                },
                markers: { size: 0 },
                tooltip: {
                    shared: true,
                    intersect: false,
                    y: {
                        formatter: function (value) {
                            return value / 1000 + "k";
                        }
                    }
                },
                grid: { borderColor: "#f1f1f1", row: { opacity: 0 }, strokeDashArray: 4, padding: { top: -15, bottom: -3 } },
                legend: { show: false }
            });
        }

        var scatterElement = document.querySelector("#heatmapScatterChart");
        if (scatterElement && chartNeedsRender(scatterElement)) {
            renderApexChart(scatterElement, {
                series: [
                    {
                        name: "Messenger",
                        data: [
                            [16.4, 5.4], [21.7, 4], [25.4, 3], [19, 2], [10.9, 1],
                            [13.6, 3.2], [10.9, 7], [10.9, 8.2], [16.4, 4], [13.6, 4.3],
                            [13.6, 12], [29.9, 3], [10.9, 5.2], [16.4, 6.5], [10.9, 8],
                            [24.5, 7.1], [10.9, 7], [8.1, 4.7], [19, 10], [27.1, 10],
                            [24.5, 8], [27.1, 3], [29.9, 11.5], [27.1, 0.8], [22.1, 2]
                        ]
                    },
                    {
                        name: "Instagram",
                        data: [
                            [6.4, 5.4], [11.7, 4], [15.4, 3], [9, 2], [10.9, 11],
                            [20.9, 7], [12.9, 8.2], [6.4, 14], [11.6, 12]
                        ]
                    }
                ],
                chart: {
                    height: 400,
                    type: "scatter",
                    animations: { enabled: true, easing: "easeinout", speed: 800 },
                    zoom: { enabled: false },
                    toolbar: { show: false }
                },
                colors: ["#056BF6", "#D2376A"],
                xaxis: { tickAmount: 10, min: 0, max: 40 },
                yaxis: { tickAmount: 7 },
                markers: { size: 12, strokeWidth: 2, strokeColors: "#fff", hover: { size: 16 } },
                tooltip: { shared: false, intersect: true },
                grid: { borderColor: "#f1f1f1", strokeDashArray: 4 },
                legend: { show: true, position: "top", horizontalAlign: "right", labels: { useSeriesColors: true } }
            });
        }
    }

    function renderRodentActivityCharts() {
        if (!window.ApexCharts || !document.querySelector(".rodent-activity-page")) {
            return;
        }

        var orderElement = document.querySelector("#orderStatistics");
        var orderColors = getChartColors("orderStatistics");

        if (orderElement && orderColors && chartNeedsRender(orderElement)) {
            renderApexChart(orderElement, {
                chart: { type: "area", height: 215, toolbar: { show: false }, sparkline: { enabled: true } },
                series: [{ name: "Visits", data: [10000, 16000, 8000, 21000, 4000, 15000] }],
                stroke: { curve: "smooth", width: 1 },
                fill: {
                    type: "gradient",
                    gradient: {
                        shadeIntensity: 1,
                        type: "vertical",
                        gradientToColors: orderColors,
                        opacityFrom: 0.4,
                        opacityTo: 0,
                        stops: [0, 100]
                    }
                },
                colors: orderColors,
                tooltip: {
                    y: {
                        formatter: function (value) {
                            return value;
                        }
                    }
                },
                xaxis: { categories: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul"] },
                grid: {
                    show: true,
                    strokeDashArray: 5,
                    xaxis: { lines: { show: true } },
                    yaxis: { lines: { show: true } }
                }
            });
        }

        var salesElement = document.querySelector("#salesRevenue");
        var salesColors = getChartColors("salesRevenue");

        if (salesElement && salesColors && chartNeedsRender(salesElement)) {
            renderApexChart(salesElement, {
                series: [65, 25],
                chart: { type: "donut", height: 220 },
                plotOptions: { pie: { donut: { size: "60%" } } },
                dataLabels: { enabled: false },
                legend: { show: false },
                colors: salesColors,
                responsive: [
                    {
                        breakpoint: 480,
                        options: {
                            chart: { width: 200 },
                            legend: { position: "bottom" }
                        }
                    }
                ]
            });
        }

        var attendanceElement = document.querySelector("#attendanceRecapChart");
        var attendanceColors = getChartColors("attendanceRecapChart");

        if (attendanceElement && attendanceColors && chartNeedsRender(attendanceElement)) {
            renderApexChart(attendanceElement, {
                chart: { type: "bar", height: 270, toolbar: { show: false }, offsetY: 4 },
                plotOptions: {
                    bar: {
                        columnWidth: "32%",
                        borderRadius: 4,
                        distributed: true,
                        dataLabels: { position: "top" }
                    }
                },
                dataLabels: {
                    formatter: function (value) {
                        return value;
                    },
                    style: { fontSize: "13px" },
                    offsetY: -22
                },
                series: [{ name: "Recap", data: [32, 51, 90, 36] }],
                xaxis: {
                    categories: ["Absent", "Late", "Present", "On Leave"],
                    labels: { style: { fontSize: "12.5px" } },
                    axisBorder: { show: false }
                },
                yaxis: { labels: { offsetX: -12, style: { fontSize: "12.5px" } } },
                grid: { padding: { left: 0, right: -16, top: -15, bottom: -3 } },
                legend: { show: false },
                colors: attendanceColors,
                fill: {
                    type: "gradient",
                    gradient: {
                        shade: "light",
                        type: "horizontal",
                        shadeIntensity: 0.01,
                        opacityFrom: 1,
                        opacityTo: 0.75,
                        stops: [0, 35, 55, 100]
                    }
                }
            });
        }

        var revenueElement = document.querySelector("#revenueChart");
        var revenueColors = getChartColors("revenueChart");

        if (revenueElement && revenueColors && chartNeedsRender(revenueElement)) {
            renderApexChart(revenueElement, {
                chart: { type: "area", height: 215, toolbar: { show: false }, sparkline: { enabled: true } },
                series: [{ name: "Activity", data: [12000, 15000, 14000, 17000, 16000, 19000, 15000] }],
                stroke: { curve: "smooth", width: 2 },
                fill: {
                    type: "gradient",
                    gradient: {
                        shadeIntensity: 0.4,
                        opacityFrom: 0.4,
                        opacityTo: 0,
                        gradientToColors: revenueColors,
                        stops: [0, 100]
                    }
                },
                colors: revenueColors,
                tooltip: {
                    y: {
                        formatter: function (value) {
                            return value;
                        }
                    }
                },
                xaxis: { categories: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul"] },
                grid: {
                    show: true,
                    strokeDashArray: 5,
                    xaxis: { lines: { show: false } },
                    yaxis: { lines: { show: true } }
                }
            });
        }
    }

    function initDashboardChrome() {
        document.querySelectorAll("[data-counter]").forEach(function (element) {
            var counterValue = element.getAttribute("data-counter") || "";

            if (element.textContent.trim() !== counterValue) {
                element.textContent = counterValue;
            }
        });

        if (window.eva) {
            window.eva.replace();
        }
    }

    var dashboardChromeQueued = false;

    function scheduleDashboardChrome() {
        if (dashboardChromeQueued) {
            return;
        }

        dashboardChromeQueued = true;

        window.requestAnimationFrame(function () {
            dashboardChromeQueued = false;
            initDashboardChrome();
            window.setTimeout(initDashboardChrome, 120);
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", scheduleDashboardChrome);
    } else {
        scheduleDashboardChrome();
    }

    window.addEventListener("pageshow", scheduleDashboardChrome);
    window.addEventListener("popstate", scheduleDashboardChrome);
    document.addEventListener("enhancedload", scheduleDashboardChrome);

    if (window.MutationObserver) {
        new MutationObserver(function () {
            if (document.querySelector(".dashboard-analytics-page, .rodent-activity-page")) {
                scheduleDashboardChrome();
            }
        }).observe(document.body, { childList: true, subtree: true });
    }
})();


window.updateTeamProductivityChart = function (categories, baitData, visitData, timeframe, monthName) {
    var element = document.querySelector("#teamProductivityChart");
    if (!element) return;

    function getChartColors(elementId) {
        var el = document.getElementById(elementId);
        if (!el) return ["#22c55e", "#3b82f6"];
        var colorsAttr = el.getAttribute("data-colors");
        if (!colorsAttr) return ["#22c55e", "#3b82f6"];
        return JSON.parse(colorsAttr).map(function (color) {
            var pair = color.split(",");
            if (pair.length === 2) {
                return "rgba(" + getComputedStyle(document.documentElement).getPropertyValue(pair[0]).trim() + "," + pair[1] + ")";
            }
            var val = getComputedStyle(document.documentElement).getPropertyValue(color.replace(" ", "")).trim();
            return val || color;
        });
    }

    var colors = getChartColors("teamProductivityChart");

    var options = {
        chart: {
            height: 400,
            type: "line",
            toolbar: { show: false },
            zoom: { enabled: false },
            dropShadow: { enabled: true, top: 10, left: 2, blur: 4, color: "#000", opacity: 0.2 }
        },
        series: [
            { name: "استهلاك الطعام", type: "line", data: baitData },
            { name: "انماط الزيارة", type: "area", data: visitData }
        ],
        stroke: { curve: "smooth", width: [2, 2] },
        fill: {
            type: ["solid", "gradient"],
            gradient: {
                shade: "light",
                type: "vertical",
                shadeIntensity: 0.5,
                gradientToColors: [colors[1]],
                inverseColors: true,
                opacityFrom: 0.2,
                opacityTo: 0,
                stops: [0, 100]
            }
        },
        colors: [colors[0], colors[1]],
        xaxis: { 
            categories: categories,
            title: {
                text: timeframe === "daily" ? "ساعات النشاط (24 ساعة)" : (timeframe === "monthly" ? (monthName ? "الأيام (شهر " + monthName + ")" : "الأيام (31 يوم)") : "المحطات (المجموعة والمصيدة)"),
                style: {
                    fontSize: '12px',
                    fontWeight: 600,
                    fontFamily: 'Inter, Outfit, sans-serif'
                }
            }
        },
        yaxis: {
            min: 0,
            title: {
                text: "الكمية (جرام) / عدد الزيارات",
                style: {
                    fontSize: '12px',
                    fontWeight: 600,
                    fontFamily: 'Inter, Outfit, sans-serif'
                }
            },
            labels: {
                formatter: function (value) {
                    return Math.round(value);
                }
            }
        },
        markers: { size: 0 },
        tooltip: {
            shared: true,
            intersect: false,
            y: {
                formatter: function (value) {
                    return Math.round(value);
                }
            }
        },
        grid: { borderColor: "#f1f1f1", row: { opacity: 0 }, strokeDashArray: 4, padding: { top: -15, bottom: -3 } },
        legend: { show: true, position: 'top', horizontalAlign: 'right' }
    };

    if (element._trapsChart) {
        try {
            element._trapsChart.destroy();
        } catch (_) {}
    }
    
    element.innerHTML = "";
    if (window.ApexCharts) {
        var chart = new window.ApexCharts(element, options);
        element._trapsChart = chart;
        chart.render();
    }
};

window.updateIntensityChart = function (percentage) {
    var canvas = document.getElementById("intensityGaugeCanvas");
    if (!canvas) return;
    var ctx = canvas.getContext("2d");
    
    // Clear canvas
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    
    var centerX = canvas.width / 2;
    var centerY = 120; // Adjusted up to fit nicely inside canvas height
    var radius = 78;
    var thickness = 14;
    
    var startAngle = 0.8 * Math.PI;
    var endAngle = 2.2 * Math.PI;
    var totalSpan = endAngle - startAngle; // 1.4 * Math.PI
    
    // Define segments: [minVal, maxVal, color, label]
    var segments = [
        { min: 0, max: 30, color: "#14b8a6", label: "منخفض" },
        { min: 30, max: 50, color: "#06b6d4", label: "متوسط" },
        { min: 50, max: 80, color: "#3b82f6", label: "مرتفع" },
        { min: 80, max: 90, color: "#f43f5e", label: "مرتفع جداً" },
        { min: 90, max: 100, color: "#e11d48", label: "شديد" }
    ];
    
    // 1. Draw colored segments
    segments.forEach(function(seg) {
        var segStart = startAngle + (seg.min / 100) * totalSpan;
        var segEnd = startAngle + (seg.max / 100) * totalSpan;
        
        ctx.beginPath();
        ctx.arc(centerX, centerY, radius, segStart, segEnd);
        ctx.lineWidth = thickness;
        ctx.strokeStyle = seg.color;
        ctx.lineCap = "butt";
        ctx.stroke();
    });
    
    // 2. Draw white lines between segments to divide them
    ctx.strokeStyle = "#ffffff";
    ctx.lineWidth = 2.5;
    segments.forEach(function(seg, idx) {
        if (idx === 0) return;
        var segStart = startAngle + (seg.min / 100) * totalSpan;
        ctx.beginPath();
        ctx.moveTo(centerX + (radius - thickness/2 - 2) * Math.cos(segStart), centerY + (radius - thickness/2 - 2) * Math.sin(segStart));
        ctx.lineTo(centerX + (radius + thickness/2 + 2) * Math.cos(segStart), centerY + (radius + thickness/2 + 2) * Math.sin(segStart));
        ctx.stroke();
    });
    
    // 3. Draw tick labels: 0, 10, 20, 30, 30, 50, 60, 70, 80, 90, 100
    ctx.fillStyle = "#64748b";
    ctx.font = "bold 9px sans-serif";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    
    var ticks = [0, 10, 20, 30, 50, 60, 70, 80, 90, 100];
    ticks.forEach(function(t) {
        var angle = startAngle + (t / 100) * totalSpan;
        // Draw tick mark
        ctx.strokeStyle = "#cbd5e1";
        ctx.lineWidth = 1.2;
        ctx.beginPath();
        ctx.moveTo(centerX + (radius - thickness/2 - 2) * Math.cos(angle), centerY + (radius - thickness/2 - 2) * Math.sin(angle));
        ctx.lineTo(centerX + (radius - thickness/2 - 5) * Math.cos(angle), centerY + (radius - thickness/2 - 5) * Math.sin(angle));
        ctx.stroke();
        
        // Draw tick text
        var textRadius = radius - thickness - 8;
        var tx = centerX + textRadius * Math.cos(angle);
        var ty = centerY + textRadius * Math.sin(angle);
        ctx.fillText(t, tx, ty);
    });
    
    // 4. Draw Needle pointing to current value
    var currentAngle = startAngle + (percentage / 100) * totalSpan;
    
    ctx.shadowBlur = 4;
    ctx.shadowColor = "rgba(0,0,0,0.15)";
    ctx.shadowOffsetY = 2;
    
    // Needle shaft
    ctx.beginPath();
    ctx.lineWidth = 3.5;
    ctx.strokeStyle = "#1e293b";
    ctx.lineCap = "round";
    ctx.moveTo(centerX, centerY);
    var needleLength = radius - 12;
    ctx.lineTo(centerX + needleLength * Math.cos(currentAngle), centerY + needleLength * Math.sin(currentAngle));
    ctx.stroke();
    
    // Reset shadow
    ctx.shadowBlur = 0;
    ctx.shadowOffsetY = 0;
    
    // Needle center hub (dark circle)
    ctx.beginPath();
    ctx.arc(centerX, centerY, 8, 0, 2 * Math.PI);
    ctx.fillStyle = "#1e293b";
    ctx.fill();
    
    // 5. Update HTML Overlay values (prevents text reversal and vertical clipping)
    var activeSeg = segments[0];
    for (var i = 0; i < segments.length; i++) {
        if (percentage >= segments[i].min && percentage <= segments[i].max) {
            activeSeg = segments[i];
            break;
        }
    }
    if (percentage > 90) {
        activeSeg = segments[segments.length - 1];
    }
    
    var valEl = document.getElementById("intensityValueText");
    var lblEl = document.getElementById("intensityLabelText");
    if (valEl && lblEl) {
        valEl.textContent = Math.round(percentage);
        valEl.style.color = activeSeg.color;
        lblEl.textContent = activeSeg.label;
        lblEl.style.color = activeSeg.color;
    }
};

// --- Rat Activity Screen Modals & Detailed Views ---

window.openMonthlyVisitsModal = function (group, fromDate, toDate) {
    var modalElement = document.getElementById('monthlyVisitsModal');
    if (modalElement) {
        modalElement.dataset.group = group || '';
        modalElement.dataset.fromDate = fromDate || '';
        modalElement.dataset.toDate = toDate || '';
        var modal = new bootstrap.Modal(modalElement);
        modal.show();
    }
};

window.loadMonthlyVisitsData = async function (group, fromDate, toDate) {
    var body = document.getElementById('monthlyVisitsModalBody');
    if (!body) return;
    body.innerHTML = '<div class="spinner-border text-primary"></div>';

    try {
        var url = '/api/stats/daily-visits';
        var params = [];
        if (group) params.push('groupNumber=' + encodeURIComponent(group));
        if (fromDate) params.push('fromDate=' + encodeURIComponent(fromDate));
        if (toDate) params.push('toDate=' + encodeURIComponent(toDate));
        if (params.length > 0) url += '?' + params.join('&');

        var response = await fetch(url);
        if (!response.ok) throw new Error('Network error');
        var data = await response.json();

        if (data.length === 0) {
            body.innerHTML = '<div class="alert alert-info text-right">لا توجد زيارات مسجلة.</div>';
            return;
        }

        var html = `<table class="table table-striped table-hover text-right">
                            <thead><tr>
                                <th class="text-right">التاريخ</th>
                                <th class="text-right">عدد الزيارات</th>
                                <th class="text-right">النسبة</th>
                            </tr></thead><tbody>`;
        var max = Math.max(...data.map(d => d.count));
        data.forEach(item => {
            var date = new Date(item.date).toLocaleDateString('ar-EG');
            var percent = max > 0 ? (item.count / max * 100) : 0;
            var color = percent > 70 ? 'bg-danger' : percent > 40 ? 'bg-warning' : 'bg-primary';
            html += `<tr>
                            <td>${date}</td>
                            <td><span class="badge ${color}">${item.count}</span></td>
                            <td>
                                <div class="progress" style="height:6px;">
                                    <div class="progress-bar ${color}" style="width:${percent}%;"></div>
                                </div>
                            </td>
                        </tr>`;
        });
        html += '</tbody></table>';
        body.innerHTML = html;
    } catch (e) {
        body.innerHTML = '<div class="alert alert-danger text-right">⚠️ حدث خطأ في تحميل البيانات.</div>';
        console.error(e);
    }
};

window.openDailyVisitsModal = function (group) {
    var modalElement = document.getElementById('dailyVisitsModal');
    if (modalElement) {
        modalElement.dataset.group = group || '';
        var modal = new bootstrap.Modal(modalElement);
        modal.show();
    }
};

window.loadDailyVisitsData = async function (group, days) {
    var body = document.getElementById('dailyVisitsModalBody');
    if (!body) return;
    body.innerHTML = '<div class="spinner-border text-success"></div>';

    try {
        var url = '/api/stats/daily-visits?days=' + (days || 30);
        if (group) url += '&groupNumber=' + encodeURIComponent(group);
        var response = await fetch(url);
        if (!response.ok) throw new Error('Network error');
        var data = await response.json();

        if (data.length === 0) {
            body.innerHTML = '<div class="alert alert-info text-right">لا توجد زيارات مسجلة.</div>';
            return;
        }

        var html = `<table class="table table-striped table-hover text-right">
                            <thead><tr>
                                <th class="text-right">التاريخ</th>
                                <th class="text-right">عدد الزيارات</th>
                                <th class="text-right">النسبة</th>
                            </tr></thead><tbody>`;
        var max = Math.max(...data.map(d => d.count));
        data.forEach(item => {
            var date = new Date(item.date).toLocaleDateString('ar-EG');
            var percent = max > 0 ? (item.count / max * 100) : 0;
            var color = percent > 70 ? 'bg-danger' : percent > 40 ? 'bg-warning' : 'bg-success';
            html += `<tr>
                            <td>${date}</td>
                            <td><span class="badge ${color}">${item.count}</span></td>
                            <td>
                                <div class="progress" style="height:6px;">
                                    <div class="progress-bar ${color}" style="width:${percent}%;"></div>
                                </div>
                            </td>
                        </tr>`;
        });
        html += '</tbody></table>';
        body.innerHTML = html;
    } catch (e) {
        body.innerHTML = '<div class="alert alert-danger text-right">⚠️ حدث خطأ في تحميل البيانات.</div>';
        console.error(e);
    }
};

window.openActivityByHourModal = function (group) {
    var modalElement = document.getElementById('activityByHourModal');
    if (modalElement) {
        modalElement.dataset.group = group || '';
        var modal = new bootstrap.Modal(modalElement);
        modal.show();
    }
};

window.loadHourlyActivityData = async function (group) {
    var body = document.getElementById('activityByHourModalBody');
    if (!body) return;
    body.innerHTML = '<div class="spinner-border text-warning"></div>';

    try {
        var url = '/api/stats/hourly-activity';
        if (group) url += '?groupNumber=' + encodeURIComponent(group);
        var response = await fetch(url);
        if (!response.ok) throw new Error('Network error');
        var data = await response.json();

        var detailsUrl = '/api/stats/activity-by-hour-details';
        if (group) detailsUrl += '?groupNumber=' + encodeURIComponent(group);
        var detailsResponse = await fetch(detailsUrl);
        if (!detailsResponse.ok) throw new Error('Network error');
        var details = await detailsResponse.json();

        if (data.length === 0) {
            body.innerHTML = '<div class="alert alert-info text-right">لا توجد بيانات نشاط.</div>';
            return;
        }

        var html = `
                <div id="hourlyChartContainer" style="height:250px;"></div>
                <div class="mt-3">
                    <h6 class="text-right fw-bold">تفاصيل المصائد</h6>
                    <div class="table-responsive" style="max-height:300px; overflow-y:auto;">
                        <table class="table table-sm table-striped table-hover text-right">
                            <thead><tr>
                                <th class="text-right">رقم المصيدة</th>
                                <th class="text-right">المجموعة</th>
                                <th class="text-right">أوقات النشاط</th>
                            </tr></thead>
                            <tbody>
                `;

        details.forEach(trap => {
            var hours = Object.keys(trap.hourlyCounts)
                .map(h => parseInt(h))
                .sort((a, b) => a - b)
                .map(h => `${h}:00-${h + 1}:00 (${trap.hourlyCounts[h]})`)
                .join(', ');
            html += `<tr>
                            <td class="text-right">${trap.trapNumber}</td>
                            <td class="text-right">المجموعة ${trap.groupNumber}</td>
                            <td class="text-right">${hours || 'لا يوجد نشاط'}</td>
                        </tr>`;
        });

        html += `</tbody></table></div></div>`;
        body.innerHTML = html;

        var chartData = data.map(d => ({ hour: d.hour, count: d.count }));
        window.renderHourlyChart(chartData);

    } catch (e) {
        body.innerHTML = '<div class="alert alert-danger text-right">⚠️ حدث خطأ في تحميل البيانات.</div>';
        console.error(e);
    }
};

window.renderHourlyChart = function (data) {
    var container = document.getElementById('hourlyChartContainer');
    if (!container) return;

    var categories = data.map(d => d.hour + ':00');
    var values = data.map(d => d.count);

    var options = {
        series: [{
            name: 'الزيارات',
            data: values
        }],
        chart: {
            type: 'bar',
            height: 250,
            toolbar: { show: false }
        },
        plotOptions: {
            bar: {
                borderRadius: 4,
                horizontal: false,
                columnWidth: '70%'
            }
        },
        dataLabels: {
            enabled: true,
            formatter: function (val) { return val > 0 ? val : ''; }
        },
        xaxis: {
            categories: categories,
            title: { text: 'الساعة' },
            labels: { rotate: -45 }
        },
        yaxis: {
            title: { text: 'عدد الزيارات' },
            min: 0
        },
        colors: ['#ffc107'],
        tooltip: {
            y: {
                formatter: function (val) { return val + ' زيارة'; }
            }
        }
    };

    if (window.ApexCharts) {
        var chart = new ApexCharts(container, options);
        chart.render();
        window._hourlyChart = chart;
    }
};

window.openPeakHourModal = function (group) {
    var modalElement = document.getElementById('peakHourModal');
    if (modalElement) {
        modalElement.dataset.group = group || '';
        var modal = new bootstrap.Modal(modalElement);
        modal.show();
    }
};

window.loadPeakHourDetails = async function (group) {
    var body = document.getElementById('peakHourModalBody');
    if (!body) return;
    body.innerHTML = '<div class="spinner-border text-danger"></div>';

    try {
        var url = '/api/stats/hourly-activity';
        if (group) url += '?groupNumber=' + encodeURIComponent(group);
        var response = await fetch(url);
        if (!response.ok) throw new Error('Network error');
        var hourlyData = await response.json();

        var detailsUrl = '/api/stats/peak-hour-details';
        if (group) detailsUrl += '?groupNumber=' + encodeURIComponent(group);
        var detailsResponse = await fetch(detailsUrl);
        if (!detailsResponse.ok) throw new Error('Network error');
        var details = await detailsResponse.json();

        if (hourlyData.length === 0) {
            body.innerHTML = '<div class="alert alert-info text-right">لا توجد بيانات نشاط.</div>';
            return;
        }

        var html = `
                <div id="peakHourChartContainer" style="height:250px;"></div>
                <div class="mt-3">
                    <h6 class="text-right fw-bold">المصائد النشطة في ساعة الذروة</h6>
                    <div class="table-responsive" style="max-height:300px; overflow-y:auto;">
                        <table class="table table-sm table-striped table-hover text-right">
                            <thead><tr>
                                <th class="text-right">رقم المصيدة</th>
                                <th class="text-right">المجموعة</th>
                                <th class="text-right">عدد الزيارات</th>
                            </tr></thead>
                            <tbody>
                `;

        details.forEach(trap => {
            html += `<tr>
                            <td class="text-right">${trap.trapNumber}</td>
                            <td class="text-right">المجموعة ${trap.groupNumber}</td>
                            <td class="text-right"><span class="badge bg-danger">${trap.count}</span></td>
                        </tr>`;
        });

        if (details.length === 0) {
            html += `<tr><td colspan="3" class="text-center">لا توجد مصائد نشطة في ساعة الذروة.</td></tr>`;
        }

        html += `</tbody></table></div></div>`;
        body.innerHTML = html;

        var chartData = hourlyData.map(d => ({ hour: d.hour, count: d.count }));
        window.renderPeakHourChart(chartData);

    } catch (e) {
        body.innerHTML = '<div class="alert alert-danger text-right">⚠️ حدث خطأ في تحميل البيانات.</div>';
        console.error(e);
    }
};

window.renderPeakHourChart = function (data) {
    var container = document.getElementById('peakHourChartContainer');
    if (!container) return;

    var categories = data.map(d => d.hour + ':00');
    var values = data.map(d => d.count);
    var peakIndex = values.indexOf(Math.max(...values));

    if (window.ApexCharts) {
        var options = {
            series: [{
                name: 'الزيارات',
                data: values
            }],
            chart: {
                type: 'bar',
                height: 250,
                toolbar: { show: false }
            },
            plotOptions: {
                bar: {
                    borderRadius: 4,
                    horizontal: false,
                    columnWidth: '70%'
                }
            },
            dataLabels: {
                enabled: true,
                formatter: function (val) { return val > 0 ? val : ''; }
            },
            xaxis: {
                categories: categories,
                title: { text: 'الساعة' },
                labels: { rotate: -45 }
            },
            yaxis: {
                title: { text: 'عدد الزيارات' },
                min: 0
            },
            colors: ['#6c757d'],
            tooltip: {
                y: {
                    formatter: function (val) { return val + ' زيارة'; }
                }
            }
        };

        var chart = new ApexCharts(container, options);
        chart.render().then(function () {
            if (peakIndex !== -1) {
                chart.updateOptions({
                    plotOptions: {
                        bar: {
                            colors: {
                                ranges: [{
                                    from: peakIndex,
                                    to: peakIndex,
                                    color: '#dc3545'
                                }]
                            }
                        }
                    }
                });
            }
        });
        window._peakHourChart = chart;
    }
};

document.addEventListener('shown.bs.modal', function (event) {
    if (event.target.id === 'monthlyVisitsModal') {
        var group = event.target.dataset.group || '';
        var fromDate = event.target.dataset.fromDate || '';
        var toDate = event.target.dataset.toDate || '';
        window.loadMonthlyVisitsData(group, fromDate, toDate);
    }
    else if (event.target.id === 'dailyVisitsModal') {
        var group = event.target.dataset.group || '';
        window.loadDailyVisitsData(group, 30);
    }
    else if (event.target.id === 'activityByHourModal') {
        var group = event.target.dataset.group || '';
        window.loadHourlyActivityData(group);
    }
    else if (event.target.id === 'peakHourModal') {
        var group = event.target.dataset.group || '';
        window.loadPeakHourDetails(group);
    }
});

document.addEventListener('hidden.bs.modal', function (event) {
    if (event.target.id === 'activityByHourModal' && window._hourlyChart) {
        window._hourlyChart.destroy();
        window._hourlyChart = null;
    }
    else if (event.target.id === 'peakHourModal' && window._peakHourChart) {
        window._peakHourChart.destroy();
        window._peakHourChart = null;
    }
});

// New Chart Renderers for Analysis Screens
window.renderSizeWeightScatterChart = function (elementId, seriesJson) {
    var element = document.getElementById(elementId);
    if (!element) return;
    var seriesData = JSON.parse(seriesJson);
    var options = {
        series: seriesData,
        chart: { height: 350, type: 'scatter', zoom: { enabled: true, type: 'xy' }, toolbar: { show: false } },
        xaxis: { tickAmount: 10, title: { text: "الطول (سم)" }, labels: { formatter: function(val) { return parseFloat(val).toFixed(1) + " سم"; } } },
        yaxis: { tickAmount: 6, title: { text: "الوزن (جرام)" }, labels: { formatter: function(val) { return Math.round(val) + " جم"; } } },
        colors: ["#3b82f6", "#10b981", "#f97316", "#6b7280"],
        grid: { borderColor: "#f1f1f1", strokeDashArray: 4 },
        legend: { show: true, position: 'top', horizontalAlign: 'right' }
    };
    if (element._trapsChart) { try { element._trapsChart.destroy(); } catch (_) {} }
    element.innerHTML = "";
    if (window.ApexCharts) {
        var chart = new window.ApexCharts(element, options);
        element._trapsChart = chart;
        chart.render();
    }
};

window.renderWeightHistogramChart = function (elementId, categoriesJson, valuesJson) {
    var element = document.getElementById(elementId);
    if (!element) return;
    var categories = JSON.parse(categoriesJson);
    var values = JSON.parse(valuesJson);
    var options = {
        series: [{ name: 'عدد القوارض', data: values }],
        chart: { height: 350, type: 'bar', toolbar: { show: false } },
        plotOptions: { bar: { borderRadius: 4, columnWidth: '50%' } },
        xaxis: { categories: categories, title: { text: "فئات الوزن" } },
        yaxis: { title: { text: "العدد" } },
        colors: ["#515f74"],
        grid: { borderColor: "#f1f1f1", strokeDashArray: 4 }
    };
    if (element._trapsChart) { try { element._trapsChart.destroy(); } catch (_) {} }
    element.innerHTML = "";
    if (window.ApexCharts) {
        var chart = new window.ApexCharts(element, options);
        element._trapsChart = chart;
        chart.render();
    }
};

window.renderColonyCompositionDonutChart = function (elementId, labelsJson, valuesJson) {
    var element = document.getElementById(elementId);
    if (!element) return;
    var labels = JSON.parse(labelsJson);
    var values = JSON.parse(valuesJson);
    var options = {
        series: values,
        chart: { height: 320, type: 'donut' },
        labels: labels,
        colors: ["#515f74", "#ff9939", "#bfc9c3", "#2b6954"],
        legend: { position: 'bottom' },
        plotOptions: { pie: { donut: { size: '65%' } } }
    };
    if (element._trapsChart) { try { element._trapsChart.destroy(); } catch (_) {} }
    element.innerHTML = "";
    if (window.ApexCharts) {
        var chart = new window.ApexCharts(element, options);
        element._trapsChart = chart;
        chart.render();
    }
};

window.renderColonyCompositionStackedBarChart = function (elementId, categoriesJson, youngJson, mediumJson, adultJson) {
    var element = document.getElementById(elementId);
    if (!element) return;
    var categories = JSON.parse(categoriesJson);
    var young = JSON.parse(youngJson);
    var medium = JSON.parse(mediumJson);
    var adult = JSON.parse(adultJson);
    var options = {
        series: [
            { name: 'صغير', data: young },
            { name: 'متوسط', data: medium },
            { name: 'بالغ', data: adult }
        ],
        chart: { height: 350, type: 'bar', stacked: true, toolbar: { show: false } },
        plotOptions: { bar: { columnWidth: '45%' } },
        xaxis: { categories: categories },
        colors: ["#064e3b", "#95d3ba", "#bfc9c3"],
        grid: { borderColor: "#f1f1f1", strokeDashArray: 4 },
        legend: { position: 'top' }
    };
    if (element._trapsChart) { try { element._trapsChart.destroy(); } catch (_) {} }
    element.innerHTML = "";
    if (window.ApexCharts) {
        var chart = new window.ApexCharts(element, options);
        element._trapsChart = chart;
        chart.render();
    }
};

window.renderBehaviorHeatmapChart = function (elementId, seriesJson) {
    var element = document.getElementById(elementId);
    if (!element) return;
    var seriesData = JSON.parse(seriesJson);
    var options = {
        series: seriesData,
        chart: { height: 350, type: 'heatmap', toolbar: { show: false } },
        plotOptions: {
            heatmap: {
                shadeIntensity: 0.5,
                colorScale: {
                    ranges: [
                        { from: 0, to: 0, color: '#f8fafc', name: 'لا يوجد' },
                        { from: 1, to: 3, color: '#d1fae5', name: 'منخفض' },
                        { from: 4, to: 8, color: '#6ee7b7', name: 'متوسط' },
                        { from: 9, to: 15, color: '#10b981', name: 'مرتفع' },
                        { from: 16, to: 100, color: '#064e3b', name: 'ذروة' }
                    ]
                }
            }
        },
        dataLabels: { enabled: false },
        colors: ["#10b981"]
    };
    if (element._trapsChart) { try { element._trapsChart.destroy(); } catch (_) {} }
    element.innerHTML = "";
    if (window.ApexCharts) {
        var chart = new window.ApexCharts(element, options);
        element._trapsChart = chart;
        chart.render();
    }
};

window.renderBehaviorLineChart = function (elementId, categoriesJson, valuesJson) {
    var element = document.getElementById(elementId);
    if (!element) return;
    var categories = JSON.parse(categoriesJson);
    var values = JSON.parse(valuesJson);
    var options = {
        series: [{ name: 'معدل النشاط', data: values }],
        chart: { height: 350, type: 'line', toolbar: { show: false } },
        stroke: { curve: 'smooth', width: 3 },
        xaxis: { categories: categories },
        colors: ["#064e3b"],
        grid: { borderColor: "#f1f1f1", strokeDashArray: 4 }
    };
    if (element._trapsChart) { try { element._trapsChart.destroy(); } catch (_) {} }
    element.innerHTML = "";
    if (window.ApexCharts) {
        var chart = new window.ApexCharts(element, options);
        element._trapsChart = chart;
        chart.render();
    }
};

window.renderBaitConsumptionDualChart = function (elementId, categoriesJson, consumedJson, visitsJson) {
    var element = document.getElementById(elementId);
    if (!element) return;
    var categories = JSON.parse(categoriesJson);
    var consumed = JSON.parse(consumedJson);
    var visits = JSON.parse(visitsJson);
    var options = {
        series: [
            { name: 'الطعام المستهلك (جم)', type: 'column', data: consumed },
            { name: 'عدد الزيارات', type: 'line', data: visits }
        ],
        chart: { height: 350, type: 'line', toolbar: { show: false } },
        stroke: { width: [0, 4] },
        xaxis: { categories: categories },
        yaxis: [
            { title: { text: 'الاستهلاك (جرام)' } },
            { opposite: true, title: { text: 'الزيارات' } }
        ],
        colors: ["#10b981", "#3b82f6"]
    };
    if (element._trapsChart) { try { element._trapsChart.destroy(); } catch (_) {} }
    element.innerHTML = "";
    if (window.ApexCharts) {
        var chart = new window.ApexCharts(element, options);
        element._trapsChart = chart;
        chart.render();
    }
};

window.renderInfestationRadarChart = function (elementId, labelsJson, currentJson, historicalJson) {
    var element = document.getElementById(elementId);
    if (!element) return;
    var labels = JSON.parse(labelsJson);
    var current = JSON.parse(currentJson);
    var historical = JSON.parse(historicalJson);
    var options = {
        series: [
            { name: 'مستوى الإصابة الحالي', data: current },
            { name: 'المتوسط التاريخي', data: historical }
        ],
        chart: { height: 320, type: 'radar', toolbar: { show: false } },
        xaxis: { categories: labels },
        colors: ["#ba1a1a", "#707974"],
        stroke: { width: 2 },
        fill: { opacity: 0.2 },
        markers: { size: 4 }
    };
    if (element._trapsChart) { try { element._trapsChart.destroy(); } catch (_) {} }
    element.innerHTML = "";
    if (window.ApexCharts) {
        var chart = new window.ApexCharts(element, options);
        element._trapsChart = chart;
        chart.render();
    }
};

window.renderInfestationBarChart = function (elementId, categoriesJson, valuesJson) {
    var element = document.getElementById(elementId);
    if (!element) return;
    var categories = JSON.parse(categoriesJson);
    var values = JSON.parse(valuesJson);
    var options = {
        series: [{ name: 'التغير في المؤشر', data: values }],
        chart: { height: 320, type: 'bar', toolbar: { show: false } },
        plotOptions: {
            bar: {
                colors: {
                    ranges: [
                        { from: -100, to: -1, color: '#95d3ba' },
                        { from: 0, to: 100, color: '#ba1a1a' }
                    ]
                },
                columnWidth: '50%'
            }
        },
        xaxis: { categories: categories },
        grid: { borderColor: "#f1f1f1", strokeDashArray: 4 }
    };
    if (element._trapsChart) { try { element._trapsChart.destroy(); } catch (_) {} }
    if (window.ApexCharts) {
        var chart = new window.ApexCharts(element, options);
        element._trapsChart = chart;
        chart.render();
    }
};

window.renderTreatmentEfficacyCompareChart = function (elementId, categoriesJson, beforeJson, afterJson) {
    var element = document.getElementById(elementId);
    if (!element) return;
    var categories = JSON.parse(categoriesJson);
    var beforeData = JSON.parse(beforeJson);
    var afterData = JSON.parse(afterJson);
    var options = {
        series: [
            { name: 'قبل المعالجة', data: beforeData },
            { name: 'بعد المعالجة', data: afterData }
        ],
        chart: { height: 320, type: 'area', toolbar: { show: false } },
        stroke: { curve: 'smooth', width: 2 },
        fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.4, opacityTo: 0.1 } },
        xaxis: { categories: categories },
        colors: ["#ba1a1a", "#10b981"]
    };
    if (element._trapsChart) { try { element._trapsChart.destroy(); } catch (_) {} }
    element.innerHTML = "";
    if (window.ApexCharts) {
        var chart = new window.ApexCharts(element, options);
        element._trapsChart = chart;
        chart.render();
    }
};

window.renderTreatmentDeclineChart = function (elementId, categoriesJson, valuesJson) {
    var element = document.getElementById(elementId);
    if (!element) return;
    var categories = JSON.parse(categoriesJson);
    var values = JSON.parse(valuesJson);
    var options = {
        series: [{ name: 'مستوى الإصابة', data: values }],
        chart: { height: 320, type: 'line', toolbar: { show: false } },
        stroke: { curve: 'straight', width: 3 },
        xaxis: { categories: categories },
        colors: ["#dc3545"],
        grid: { borderColor: "#f1f1f1", strokeDashArray: 4 }
    };
    if (element._trapsChart) { try { element._trapsChart.destroy(); } catch (_) {} }
    element.innerHTML = "";
    if (window.ApexCharts) {
        var chart = new window.ApexCharts(element, options);
        element._trapsChart = chart;
        chart.render();
    }
};

window.renderNetworkRadialChart = function (elementId, valuesJson) {
    var element = document.getElementById(elementId);
    if (!element) return;
    var values = JSON.parse(valuesJson);
    var options = {
        series: values,
        chart: { height: 320, type: 'radialBar' },
        plotOptions: {
            radialBar: {
                dataLabels: {
                    name: { fontSize: '22px' },
                    value: { fontSize: '16px' },
                    total: {
                        show: true,
                        label: 'صحة الشبكة',
                        formatter: function (w) {
                            return "94%";
                        }
                    }
                }
            }
        },
        labels: ['البطارية', 'الاتصال', 'الإشارة'],
        colors: ["#003527", "#2b6954", "#95d3ba"]
    };
    if (element._trapsChart) { try { element._trapsChart.destroy(); } catch (_) {} }
    element.innerHTML = "";
    if (window.ApexCharts) {
        var chart = new window.ApexCharts(element, options);
        element._trapsChart = chart;
        chart.render();
    }
};

window.renderNetworkDowntimeChart = function (elementId, categoriesJson, valuesJson) {
    var element = document.getElementById(elementId);
    if (!element) return;
    var categories = JSON.parse(categoriesJson);
    var values = JSON.parse(valuesJson);
    var options = {
        series: [{ name: 'انقطاعات الخدمة', data: values }],
        chart: { height: 320, type: 'bar', toolbar: { show: false } },
        xaxis: { categories: categories },
        colors: ["#ba1a1a"],
        plotOptions: { bar: { borderRadius: 3, columnWidth: '60%' } },
        grid: { borderColor: "#f1f1f1", strokeDashArray: 4 }
    };
    if (element._trapsChart) { try { element._trapsChart.destroy(); } catch (_) {} }
    element.innerHTML = "";
    if (window.ApexCharts) {
        var chart = new window.ApexCharts(element, options);
        element._trapsChart = chart;
        chart.render();
    }
};

window.updateTeamProductivityChart = function (categories, baitData, visitData, activeTimeframe, monthName) {
    var element = document.getElementById('teamProductivityChart');
    if (!element) return;

    if (element._trapsChart) {
        try { element._trapsChart.destroy(); } catch (e) {}
        element._trapsChart = null;
    }
    element.innerHTML = "";

    var formattedCategories = (categories && categories.length) ? categories.map(function(cat) {
        if (activeTimeframe === "daily") {
            var hour = parseInt(cat.split(':')[0], 10);
            if (!isNaN(hour)) {
                var suffix = hour >= 12 ? 'م' : 'ص';
                var displayHour = hour % 12;
                if (displayHour === 0) displayHour = 12;
                return (displayHour < 10 ? '0' + displayHour : displayHour) + ':00 ' + suffix;
            }
        }
        return cat;
    }) : [
        "12:00 ص", "01:00 ص", "02:00 ص", "03:00 ص", "04:00 ص", "05:00 ص",
        "06:00 ص", "07:00 ص", "08:00 ص", "09:00 ص", "10:00 ص", "11:00 ص",
        "12:00 م", "01:00 م", "02:00 م", "03:00 م", "04:00 م", "05:00 م",
        "06:00 م", "07:00 م", "08:00 م", "09:00 م", "10:00 م", "11:00 م"
    ];

    var seriesData = [
        {
            name: "استهلاك الطعم (جرام)",
            type: "line",
            data: baitData && baitData.length ? baitData : [23, 0, 45, 0, 0, 0, 0, 27, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20]
        },
        {
            name: "أنماط الزيارات (تكرار)",
            type: "area",
            data: visitData && visitData.length ? visitData : [1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1]
        }
    ];

    var options = {
        series: seriesData,
        chart: {
            height: 320,
            type: "line",
            toolbar: { show: false },
            zoom: { enabled: false },
            dropShadow: { enabled: true, top: 4, left: 0, blur: 6, color: "#6366F1", opacity: 0.15 }
        },
        stroke: {
            curve: "smooth",
            width: [3, 2.5]
        },
        colors: ["#06B6D4", "#6366F1"],
        fill: {
            type: ["solid", "gradient"],
            gradient: {
                shade: "light",
                type: "vertical",
                shadeIntensity: 0.5,
                gradientToColors: ["#818CF8"],
                inverseColors: false,
                opacityFrom: 0.35,
                opacityTo: 0.05,
                stops: [0, 100]
            }
        },
        labels: formattedCategories,
        xaxis: {
            tickAmount: activeTimeframe === "daily" ? 12 : undefined,
            labels: {
                rotate: -25,
                rotateAlways: false,
                hideOverlappingLabels: true,
                style: { colors: "#475569", fontSize: "11px", fontWeight: 700 }
            },
            axisBorder: { show: true, color: "#e2e8f0" },
            axisTicks: { show: true, color: "#cbd5e1" }
        },
        yaxis: [
            {
                title: { text: "استهلاك الطعم (جرام)", style: { color: "#06B6D4", fontSize: "11px", fontWeight: 600 } },
                labels: { style: { colors: "#64748b", fontSize: "11px" } }
            },
            {
                opposite: true,
                title: { text: "عدد الزيارات", style: { color: "#6366F1", fontSize: "11px", fontWeight: 600 } },
                labels: { style: { colors: "#64748b", fontSize: "11px" } }
            }
        ],
        tooltip: {
            shared: true,
            intersect: false,
            theme: "light",
            y: {
                formatter: function (val, opts) {
                    if (opts.seriesIndex === 0) return val + " جرام";
                    return val + " زيارة";
                }
            }
        },
        grid: {
            borderColor: "#f1f5f9",
            strokeDashArray: 4,
            padding: { top: 0, right: 10, bottom: 0, left: 10 }
        },
        legend: {
            show: true,
            position: "top",
            horizontalAlign: "left",
            fontFamily: "system-ui, sans-serif",
            fontSize: "12px",
            fontWeight: 600,
            markers: { radius: 12 }
        }
    };

    if (window.ApexCharts) {
        var chart = new window.ApexCharts(element, options);
        element._trapsChart = chart;
        chart.render();
    }
};