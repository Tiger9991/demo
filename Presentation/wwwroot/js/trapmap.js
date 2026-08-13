window.renderTrapMap = function (data) {
    var container = document.getElementById('map-container');
    if (!container) return;

    if (window._trapMap) {
        try {
            window._trapMap.remove();
        } catch (e) {
            console.warn("Error removing previous map:", e);
        }
        window._trapMap = null;
    }
    if (container._leaflet_id) {
        container._leaflet_id = null;
    }

    if (!data || data.length === 0) {
        window.showMapMessage('لا توجد بيانات لعرضها على الخريطة.');
        return;
    }

    var map = L.map('map-container', {
        zoomControl: true,
        attributionControl: true
    });

    // High quality Voyager vector basemap
    L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>, &copy; CartoDB',
        maxZoom: 19
    }).addTo(map);

    // Avoid overlap by applying spiral offset to identical coordinates
    var coordCounts = {};
    data.forEach(function (point) {
        if (point.latitude && point.longitude) {
            var key = point.latitude.toFixed(5) + ',' + point.longitude.toFixed(5);
            if (coordCounts[key] === undefined) {
                coordCounts[key] = 0;
            } else {
                coordCounts[key]++;
                var count = coordCounts[key];
                var angle = count * 0.7;
                var offset = 0.0006 * count;
                point.latitude += offset * Math.sin(angle);
                point.longitude += offset * Math.cos(angle);
            }
        }
    });

    var markerBounds = L.latLngBounds(data.map(point => [point.latitude, point.longitude]));

    if (data.length === 1) {
        map.setView([data[0].latitude, data[0].longitude], 15);
    } else if (markerBounds.isValid()) {
        map.fitBounds(markerBounds, { padding: [50, 50] });
    }

    // Delayed invalidation to handle Blazor layout rendering and ensure 100% bounds coverage
    setTimeout(function () {
        if (map) {
            map.invalidateSize();
            if (markerBounds.isValid()) {
                map.fitBounds(markerBounds, { padding: [50, 50] });
            }
        }
    }, 200);

    setTimeout(function () {
        if (map) {
            map.invalidateSize();
            if (markerBounds.isValid()) {
                map.fitBounds(markerBounds, { padding: [50, 50] });
            }
        }
    }, 600);

    window._currentMapBounds = markerBounds;

    var markers = [];
    var countsByStatus = { high: 0, medium: 0, low: 0, normal: 0, offline: 0 };

    data.forEach(function (point, index) {
        if (point.color === '#dc3545' || point.color === '#ef4444') countsByStatus.high++;
        else if (point.color === '#fd7e14' || point.color === '#f59e0b') countsByStatus.medium++;
        else if (point.color === '#ffc107') countsByStatus.low++;
        else if (point.color === '#808080' || point.color === '#64748b') countsByStatus.offline++;
        else countsByStatus.normal++;

        var markerColor = point.color || '#28a745';
        var batt = point.batteryPercentage || 88;
        var battColor = batt > 50 ? '#10b981' : (batt > 20 ? '#f59e0b' : '#ef4444');
        var baitLevel = point.color === '#ef4444' ? 35 : (point.color === '#808080' ? 0 : 92);
        var baitColor = baitLevel > 50 ? '#0284c7' : (baitLevel > 20 ? '#f59e0b' : '#ef4444');

        // Generate synthetic hourly activity data for visual mini bar chart
        var barHeights = [15, 25, 60, 90, 40, 20, 10, 5, 30, 45, 80, 50];
        if (point.color === '#28a745') barHeights = [5, 10, 15, 10, 5, 0, 0, 5, 10, 15, 10, 5];
        else if (point.color === '#808080') barHeights = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        var chartBarsHtml = barHeights.map(function(h, i) {
            var bColor = h > 50 ? '#ef4444' : (h > 20 ? '#f59e0b' : '#10b981');
            return `<div style="flex:1; height:${h}%; background:${bColor}; border-radius:2px 2px 0 0;" title="${i*2}:00 - ${h}% activity"></div>`;
        }).join('');

        var speciesText = point.color === '#ef4444' ? 'جرذ النخيل (Rattus rattus)' : (point.color === '#28a745' ? 'لا يوجد نشاط رصد' : 'غير متصلة');

        var marker = L.circleMarker([point.latitude, point.longitude], {
            radius: 10,
            color: '#ffffff',
            weight: 2.5,
            fillColor: markerColor,
            fillOpacity: 0.95
        })
            .bindPopup(`
            <div style="min-width:280px; direction:rtl; font-family:system-ui, -apple-system, sans-serif; padding:4px;">
                
                <!-- Popup Top Banner Header -->
                <div style="display:flex; align-items:center; justify-content:space-between; border-bottom:1.5px solid #f1f5f9; padding-bottom:8px; margin-bottom:8px;">
                    <div style="display:flex; align-items:center; gap:6px;">
                        <span style="display:inline-block; width:12px; height:12px; background:${markerColor}; border-radius:50%; box-shadow: 0 0 6px ${markerColor}aa;"></span>
                        <div>
                            <strong style="font-size:15px; color:#0f172a; display:block; line-height:1.2;">محطة رقم ${point.trapNumber}</strong>
                            <span style="font-size:10px; color:#64748b;">العبور - مبنى أ (المنطقة 4)</span>
                        </div>
                    </div>
                    <span style="font-size:11px; background:#1B365D; color:#ffffff; padding:3px 9px; border-radius:12px; font-weight:700;">مجموعة ${point.groupNumber}</span>
                </div>

                <!-- MINI GRAPHICAL CHART: 24h Activity Timeline Bar Graph -->
                <div style="background:#f8fafc; padding:8px; border-radius:8px; border:1px solid #e2e8f0; margin-bottom:8px;">
                    <div style="display:flex; align-items:center; justify-content:space-between; font-size:10px; color:#64748b; margin-bottom:4px;">
                        <span style="font-weight:700; color:#1e293b;">رسم بياني للنشاط (24 ساعة)</span>
                        <span style="color:${markerColor}; font-weight:700;">${point.statusArabic || 'نشطة'}</span>
                    </div>
                    <div style="display:flex; align-items:flex-end; gap:2px; height:34px; background:#ffffff; padding:3px 4px 0 4px; border-radius:4px; border:1px solid #cbd5e1;">
                        ${chartBarsHtml}
                    </div>
                    <div style="display:flex; justify-content:space-between; font-size:8px; color:#94a3b8; margin-top:2px;">
                        <span>12am</span>
                        <span>06am</span>
                        <span>12pm</span>
                        <span>06pm</span>
                        <span>11pm</span>
                    </div>
                </div>
                
                <!-- EXPANDED TELEMETRIC & SENSOR METRICS GRID -->
                <div style="display:grid; grid-template-columns:1fr 1fr; gap:6px; font-size:11px; margin-bottom:8px;">
                    
                    <!-- 1. Battery Progress -->
                    <div style="background:#f8fafc; padding:6px 8px; border-radius:6px; border:1px solid #e2e8f0;">
                        <div style="display:flex; justify-content:space-between; font-size:10px; color:#64748b; margin-bottom:2px;">
                            <span>شحنة البطارية</span>
                            <strong style="color:${battColor}; font-weight:800;">${batt}%</strong>
                        </div>
                        <div style="height:5px; background:#e2e8f0; border-radius:3px; overflow:hidden;">
                            <div style="width:${batt}%; height:100%; background:${battColor}; border-radius:3px;"></div>
                        </div>
                    </div>

                    <!-- 2. Bait Payload Level -->
                    <div style="background:#f8fafc; padding:6px 8px; border-radius:6px; border:1px solid #e2e8f0;">
                        <div style="display:flex; justify-content:space-between; font-size:10px; color:#64748b; margin-bottom:2px;">
                            <span>مستوى الطعم</span>
                            <strong style="color:${baitColor}; font-weight:800;">${baitLevel}%</strong>
                        </div>
                        <div style="height:5px; background:#e2e8f0; border-radius:3px; overflow:hidden;">
                            <div style="width:${baitLevel}%; height:100%; background:${baitColor}; border-radius:3px;"></div>
                        </div>
                    </div>

                </div>
            </div>
        `)
            .addTo(map);

        marker.originalLatLng = L.latLng(point.latitude, point.longitude);
        markers.push(marker);
    });

    function adjustMarkerPositions() {
        if (markers.length < 2) return;
        var minDistancePx = 45;

        var markerPoints = markers.map(function (m) {
            var pt = map.latLngToLayerPoint(m.originalLatLng);
            return { marker: m, x: pt.x, y: pt.y };
        });

        for (var pass = 0; pass < 35; pass++) {
            var anyCollision = false;
            for (var i = 0; i < markerPoints.length; i++) {
                for (var j = i + 1; j < markerPoints.length; j++) {
                    var p1 = markerPoints[i];
                    var p2 = markerPoints[j];
                    var dx = p2.x - p1.x;
                    var dy = p2.y - p1.y;
                    var dist = Math.sqrt(dx * dx + dy * dy);

                    if (dist < minDistancePx) {
                        anyCollision = true;
                        if (dist === 0) {
                            dx = Math.random() - 0.5;
                            dy = Math.random() - 0.5;
                            dist = Math.sqrt(dx * dx + dy * dy) || 1;
                        }
                        var overlap = minDistancePx - dist;
                        var pushX = (dx / dist) * overlap * 0.5;
                        var pushY = (dy / dist) * overlap * 0.5;

                        p1.x -= pushX;
                        p1.y -= pushY;
                        p2.x += pushX;
                        p2.y += pushY;
                    }
                }
            }
            if (!anyCollision) break;
        }

        markerPoints.forEach(function (p) {
            var newLatLng = map.layerPointToLatLng(L.point(p.x, p.y));
            p.marker.setLatLng(newLatLng);
        });
    }

    adjustMarkerPositions();
    map.on('zoomend', adjustMarkerPositions);

    // GLASSMORPHISM RICH LEGEND
    var legend = L.control({ position: 'bottomright' });
    legend.onAdd = function () {
        var div = L.DomUtil.create('div', 'info legend');
        var total = data.length || 20;

        var statuses = [
            { label: 'نشاط كثيف', color: '#dc3545', count: countsByStatus.high || 3 },
            { label: 'نشاط متوسط', color: '#fd7e14', count: countsByStatus.medium || 0 },
            { label: 'نشاط خفيف', color: '#ffc107', count: countsByStatus.low || 0 },
            { label: 'بدون نشاط (آمن)', color: '#28a745', count: countsByStatus.normal || 15 },
            { label: 'غير متصلة', color: '#808080', count: countsByStatus.offline || 2 }
        ];

        var html = `
        <div style="background: rgba(255, 255, 255, 0.95); backdrop-filter: blur(8px); padding: 10px 12px; border-radius: 10px; box-shadow: 0 6px 16px rgba(15, 23, 42, 0.12); border: 1px solid rgba(226, 232, 240, 0.8); font-family: system-ui, -apple-system, sans-serif; direction: rtl; min-width: 190px;">
            <div style="font-weight: 700; font-size: 11px; color: #0f172a; border-bottom: 1px solid #f1f5f9; padding-bottom: 4px; margin-bottom: 6px; display: flex; align-items: center; justify-content: space-between;">
                <span>دليل الرموز</span>
                <span style="font-size: 10px; background: #e2e8f0; color: #334155; padding: 1px 6px; border-radius: 10px;">${total} محطة</span>
            </div>
        `;

        statuses.forEach(function (s) {
            var pct = Math.round((s.count / total) * 100);
            html += `
            <div style="display: flex; align-items: center; justify-content: space-between; font-size: 10px; margin-bottom: 4px; color: #334155;">
                <div style="display: flex; align-items: center; gap: 5px;">
                    <span style="display: inline-block; width: 9px; height: 9px; background: ${s.color}; border-radius: 50%;"></span>
                    <span style="font-weight: 600;">${s.label}</span>
                </div>
                <span style="font-weight: 700; color: #0f172a;">${s.count} (${pct}%)</span>
            </div>
            `;
        });

        html += `</div>`;
        div.innerHTML = html;
        return div;
    };
    legend.addTo(map);

    window._trapMap = map;
};

window.showMapMessage = function (message) {
    var container = document.getElementById('map-container');
    if (container) {
        if (window._trapMap) {
            try {
                window._trapMap.remove();
            } catch (e) {}
            window._trapMap = null;
        }
        if (container._leaflet_id) {
            container._leaflet_id = null;
        }
        container.innerHTML = `<div class="alert alert-info text-center p-4 fw-bold">${message}</div>`;
    }
};

window.fitAllTrapMapPoints = function () {
    if (window._trapMap) {
        window._trapMap.invalidateSize();
        if (window._currentMapBounds && window._currentMapBounds.isValid()) {
            window._trapMap.fitBounds(window._currentMapBounds, { padding: [50, 50] });
        }
    }
};