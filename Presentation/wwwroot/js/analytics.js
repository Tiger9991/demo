window.openAnalyticsModal = function (modalId, cardTitle) {
    console.log("openAnalyticsModal called for:", modalId);
    var modalElement = document.getElementById(modalId);
    if (!modalElement) {
        console.error("Modal element not found:", modalId);
        return;
    }
    
    // Set title if title element exists
    var titleEl = modalElement.querySelector(".modal-title");
    if (titleEl && cardTitle) {
        titleEl.textContent = cardTitle;
    }

    // Try Bootstrap 5 native API
    if (window.bootstrap && window.bootstrap.Modal && typeof window.bootstrap.Modal.getOrCreateInstance === "function") {
        try {
            console.log("Showing modal via bootstrap.Modal API");
            var modalInstance = window.bootstrap.Modal.getOrCreateInstance(modalElement);
            modalInstance.show();
            return;
        } catch (e) {
            console.error("Error showing modal via bootstrap:", e);
        }
    }

    // Try jQuery Bootstrap 3/4 API
    if (window.jQuery && window.jQuery.fn && typeof window.jQuery.fn.modal === "function") {
        try {
            console.log("Showing modal via jQuery API");
            window.jQuery(modalElement).modal('show');
            return;
        } catch (e) {
            console.error("Error showing modal via jQuery:", e);
        }
    }

    // Fallback: Direct DOM manipulation (if everything else fails)
    try {
        console.log("Showing modal via direct class manipulation");
        modalElement.classList.add("show");
        modalElement.style.display = "block";
        document.body.classList.add("modal-open");
        
        // Add backdrop
        var backdrop = document.querySelector(".modal-backdrop");
        if (!backdrop) {
            backdrop = document.createElement("div");
            backdrop.className = "modal-backdrop fade show";
            document.body.appendChild(backdrop);
        }
        
        // Setup close buttons for manual fallback
        var closeButtons = modalElement.querySelectorAll('[data-bs-dismiss="modal"]');
        closeButtons.forEach(function (btn) {
            if (!btn.dataset.fallbackHandlerAdded) {
                btn.dataset.fallbackHandlerAdded = "true";
                btn.addEventListener("click", function () {
                    modalElement.classList.remove("show");
                    modalElement.style.display = "none";
                    document.body.classList.remove("modal-open");
                    var bd = document.querySelector(".modal-backdrop");
                    if (bd) bd.remove();
                });
            }
        });
        return;
    } catch (e) {
        console.error("Fallback failed:", e);
    }
};

// ===== Download File Helper (for Excel/PDF Export) =====
window.downloadFile = function (base64, fileName, mimeType) {
    try {
        const byteChars = atob(base64);
        const byteNums = new Array(byteChars.length);
        for (let i = 0; i < byteChars.length; i++) {
            byteNums[i] = byteChars.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNums);
        const blob = new Blob([byteArray], { type: mimeType });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    } catch (e) {
        console.error('downloadFile error:', e);
    }
};