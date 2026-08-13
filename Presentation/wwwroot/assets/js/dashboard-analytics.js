(function () {
    function initAnalyticsDashboard() {
        var analyticsCardActions = [
            { key: "networkStations", title: "اجمالى محطات الشبكة", modalId: "networkStationsModal" },
            { key: "connectedStations", title: "اجمالى محطات متصلة", modalId: "connectedStationsModal" },
            { key: "offlineStations", title: "محطات غير متصلة", modalId: "offlineStationsModal" },
            { key: "activeTodayStations", title: "محطات نشطة خلال اليوم", modalId: "activeTodayStationsModal" },
            { key: "lowBatteryStations", title: "محطات بطارية ضعيفه", modalId: "lowBatteryStationsModal" },
            { key: "rodentActivity", title: "اجمالى نشاط القوارض", modalId: "rodentActivityModal" },
            { key: "refillStations", title: "محطات اعادة تعبئة", modalId: "refillStationsModal" },
            { key: "latestAlerts", title: "اخر توقيت تنبيهات", modalId: "latestAlertsModal" }
        ];

        var analyticsMenuItems = [
            { key: "details", title: "عرض التفاصيل" },
            { key: "summaryReport", title: "تقرير مختصر" },
            { key: "cardSettings", title: "اعدادات الكارت" }
        ];

        document.querySelectorAll(".anaytics-card").forEach(function (card, index) {
            var action = analyticsCardActions[index];

            if (!action || card.querySelector(".analytics-card-menu")) {
                return;
            }

            var menu = document.createElement("div");
            menu.className = "analytics-card-menu";
            var menuItems = analyticsMenuItems.map(function (item) {
                var modalParams = {
                    cardKey: action.key,
                    cardTitle: action.title,
                    actionKey: item.key,
                    actionTitle: item.title,
                    modalId: action.modalId,
                    source: "analytics-summary-card",
                    range: "current"
                };

                return [
                    "<li>",
                    '    <a class="dropdown-item analytics-card-menu-item" href="#!"',
                    '       data-card-key="' + modalParams.cardKey + '"',
                    '       data-card-title="' + modalParams.cardTitle + '"',
                    '       data-action-key="' + modalParams.actionKey + '"',
                    '       data-action-title="' + modalParams.actionTitle + '"',
                    '       data-modal-id="' + modalParams.modalId + '"',
                    "       data-modal-params='" + JSON.stringify(modalParams) + "'>",
                    item.title,
                    "    </a>",
                    "</li>"
                ].join("");
            }).join("");

            menu.innerHTML = [
                '<button class="btn analytics-card-menu-btn" type="button" aria-expanded="false" aria-label="قائمة ' + action.title + '">',
                '    <i data-eva="more-horizontal-outline" class="size-4"></i>',
                "</button>",
                '<ul class="dropdown-menu dropdown-menu-animated analytics-card-menu-list">',
                menuItems,
                "</ul>"
            ].join("");

            card.prepend(menu);
        });

        if (window.eva) {
            window.eva.replace();
        }

        renderAnalyticsCharts();
    }

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
            var normalized = color.replace(" ", "");
            var pair = color.split(",");

            if (pair.length === 2) {
                return "rgba(" + getComputedStyle(document.documentElement).getPropertyValue(pair[0]) + "," + pair[1] + ")";
            }

            return normalized;
        });
    }

    function renderAnalyticsCharts() {
        if (!window.ApexCharts) {
            return;
        }

        var monthlyElement = document.querySelector("#monthlyEarningsChart");
        var monthlyColors = getChartColors("monthlyEarningsChart");

        if (monthlyElement && monthlyColors && !monthlyElement.dataset.chartReady) {
            monthlyElement.dataset.chartReady = "true";
            new window.ApexCharts(monthlyElement, {
                series: [74],
                chart: {
                    height: 300,
                    type: "radialBar",
                    offsetY: 0
                },
                plotOptions: {
                    radialBar: {
                        startAngle: -90,
                        endAngle: 90,
                        dataLabels: {
                            name: { show: false },
                            value: {
                                offsetY: -20,
                                fontSize: "22px",
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
                grid: {
                    padding: {
                        top: -20,
                        bottom: -10,
                        left: 10,
                        right: 10
                    }
                },
                stroke: { dashArray: 4 },
                labels: ["Median Ratio"]
            }).render();
        }

        var productivityElement = document.querySelector("#teamProductivityChart");
        var productivityColors = getChartColors("teamProductivityChart");

        if (productivityElement && productivityColors && !productivityElement.dataset.chartReady) {
            productivityElement.dataset.chartReady = "true";
            new window.ApexCharts(productivityElement, {
                chart: {
                    height: 400,
                    toolbar: { show: false },
                    zoom: { enabled: false },
                    dropShadow: {
                        enabled: true,
                        top: 10,
                        left: 2,
                        blur: 4,
                        color: "#000",
                        opacity: 0.2
                    }
                },
                series: [
                    {
                        name: "استهلاك الطعام",
                        type: "line",
                        data: [20000, 28500, 18000, 19800, 15500, 22200, 29000, 21200, 18800, 28600, 18000, 28000]
                    },
                    {
                        name: "انماط الزيارة",
                        type: "area",
                        data: [10000, 18500, 9000, 25000, 12000, 14000, 6000, 22000, 12000, 17000, 10000, 18000]
                    }
                ],
                stroke: {
                    curve: "smooth",
                    width: [2, 2]
                },
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
                xaxis: {
                    categories: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"]
                },
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
                grid: {
                    borderColor: "#f1f1f1",
                    row: { opacity: 0 },
                    strokeDashArray: 4,
                    padding: {
                        top: -15,
                        bottom: -3
                    }
                },
                legend: { show: false }
            }).render();
        }
    }

    function closeAnalyticsMenus() {
        document.querySelectorAll(".analytics-card-menu-list.is-open").forEach(function (menuList) {
            menuList.classList.remove("is-open");
        });

        document.querySelectorAll(".analytics-card-menu-btn[aria-expanded='true']").forEach(function (button) {
            button.setAttribute("aria-expanded", "false");
        });

        document.querySelectorAll(".anaytics-card.analytics-menu-open").forEach(function (card) {
            card.classList.remove("analytics-menu-open");
        });
    }

    document.addEventListener("click", function (event) {
        var menuButton = event.target.closest(".analytics-card-menu-btn");
        var menuItem = event.target.closest(".analytics-card-menu-item");

        if (menuButton) {
            event.preventDefault();
            event.stopPropagation();

            var currentMenu = menuButton.closest(".analytics-card-menu");
            var currentCard = menuButton.closest(".anaytics-card");
            var currentMenuList = currentMenu.querySelector(".analytics-card-menu-list");
            var shouldOpen = !currentMenuList.classList.contains("is-open");

            closeAnalyticsMenus();

            if (shouldOpen) {
                currentMenuList.classList.add("is-open");
                menuButton.setAttribute("aria-expanded", "true");
                currentCard.classList.add("analytics-menu-open");
            }

            return;
        }

        if (menuItem) {
            event.preventDefault();

            var modalId = menuItem.getAttribute("data-modal-id");
            var modalElement = document.getElementById(modalId);
            window.currentAnalyticsModalParams = JSON.parse(menuItem.getAttribute("data-modal-params"));

            closeAnalyticsMenus();

            if (modalElement && window.bootstrap) {
                window.bootstrap.Modal.getOrCreateInstance(modalElement).show();
            }

            return;
        }

        if (!event.target.closest(".analytics-card-menu")) {
            closeAnalyticsMenus();
        }
    });

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initAnalyticsDashboard);
    } else {
        initAnalyticsDashboard();
    }

    document.addEventListener("enhancedload", initAnalyticsDashboard);
    window.initAnalyticsDashboard = initAnalyticsDashboard;
})();
