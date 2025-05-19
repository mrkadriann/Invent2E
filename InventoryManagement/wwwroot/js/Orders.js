document.addEventListener('DOMContentLoaded', function () {
    // Initialize variables for DOM elements
    const filterOptions = document.querySelectorAll('.filter-option');
    const resetFiltersButton = document.getElementById('resetFilters');
    const searchInput = document.querySelector('.search-input');
    const addNewOrderButton = document.getElementById('addNewOrder');
    const orderList = document.getElementById('orderList');
    const orderCards = document.querySelectorAll('.order-card');
    const dropdowns = document.querySelectorAll('.dropdown');
    const dateInputs = document.querySelectorAll('input[type="date"]');
    const rangeInputs = document.querySelectorAll('.range-inputs input');
    const modal = document.getElementById('orderDetailsModal');

    // Ensure modal is hidden by default
    if (modal) {
        modal.classList.remove('show');
    }

    // Filter options click event
    if (filterOptions.length > 0) {
        filterOptions.forEach(option => {
            option.addEventListener('click', function () {
                const parentSection = this.closest('.filter-section');
                parentSection.querySelectorAll('.filter-option').forEach(opt => {
                    opt.classList.remove('active');
                });
                this.classList.add('active');
                filterOrders();
            });
        });
    }

    // Dropdown functionality
    if (dropdowns.length > 0) {
        dropdowns.forEach(dropdown => {
            dropdown.addEventListener('click', function(e) {
                e.stopPropagation();
                const content = this.nextElementSibling;
                if (content && content.classList.contains('dropdown-content')) {
                    // Close all other dropdowns
                    document.querySelectorAll('.dropdown-content.show').forEach(d => {
                        if (d !== content) d.classList.remove('show');
                    });
                    content.classList.toggle('show');
                }
            });

            // Handle dropdown item selection
            const content = dropdown.nextElementSibling;
            if (content && content.classList.contains('dropdown-content')) {
                content.querySelectorAll('a').forEach(item => {
                    item.addEventListener('click', function(e) {
                        e.preventDefault();
                        const selectedValue = this.textContent.trim();
                        const selectedData = this.dataset[dropdown.dataset.type] || selectedValue;
                        dropdown.textContent = selectedValue;
                        dropdown.dataset.selected = selectedData;
                        content.classList.remove('show');
                        filterOrders();
                    });
                });
            }
        });
    }

    // Date range functionality
    if (dateInputs.length > 0) {
        dateInputs.forEach(input => {
            input.addEventListener('change', function() {
                filterOrders();
            });
        });
    }

    // Range inputs functionality
    if (rangeInputs.length > 0) {
        rangeInputs.forEach(input => {
            input.addEventListener('input', debounce(function() {
                filterOrders();
            }, 300));
        });
    }

    // Function to filter orders
    function filterOrders() {
        if (!searchInput || !orderCards.length) return;

        const searchTerm = searchInput.value.toLowerCase();
        const activeStatus = document.querySelector('.filter-option.active')?.dataset.status;
        const selectedSource = document.querySelector('.dropdown[data-type="source"]')?.textContent.trim();
        const selectedLocation = document.querySelector('.dropdown[data-type="location"]')?.textContent.trim();
        const orderDateFrom = document.getElementById('orderDateFrom')?.value;
        const orderDateTo = document.getElementById('orderDateTo')?.value;

        orderCards.forEach(card => {
            const orderId = card.querySelector('.order-id')?.textContent.toLowerCase();
            const orderDesc = card.querySelector('.order-description')?.textContent.toLowerCase();
            const orderStatus = card.querySelector('.order-status')?.textContent;
            const orderSource = card.dataset.source;
            const orderLocation = card.dataset.location;
            const orderDate = new Date(card.dataset.orderDate);

            let show = true;

            // Search filter
            if (searchTerm && orderId && orderDesc && !orderId.includes(searchTerm) && !orderDesc.includes(searchTerm)) {
                show = false;
            }

            // Status filter
            if (activeStatus && activeStatus !== 'all' && orderStatus !== activeStatus) {
                show = false;
            }

            // Source filter
            if (selectedSource && selectedSource !== 'Select Order Source' && orderSource !== selectedSource) {
                show = false;
            }

            // Location filter
            if (selectedLocation && selectedLocation !== 'Select Location' && orderLocation !== selectedLocation) {
                show = false;
            }

            // Date range filter
            if (orderDateFrom) {
                const fromDate = new Date(orderDateFrom);
                fromDate.setHours(0, 0, 0, 0);
                if (orderDate < fromDate) {
                    show = false;
                }
            }
            if (orderDateTo) {
                const toDate = new Date(orderDateTo);
                toDate.setHours(23, 59, 59, 999);
                if (orderDate > toDate) {
                    show = false;
                }
            }

            card.style.display = show ? 'block' : 'none';
        });

        updateOrderCounts();
    }

    // Function to update order counts in filter badges
    function updateOrderCounts() {
        if (!orderCards.length) return;

        const statuses = ['all', 'quote', 'sample', 'shipment', 'fulfilled'];
        statuses.forEach(status => {
            const count = Array.from(orderCards).filter(card => {
                if (status === 'all') return card.style.display !== 'none';
                const orderStatus = card.querySelector('.order-status')?.textContent.toLowerCase() || '';
                return card.style.display !== 'none' && orderStatus.includes(status);
            }).length;
            const badge = document.querySelector(`.filter-option[data-status="${status}"] .badge`);
            if (badge) badge.textContent = count;
        });
    }

    // Reset filters button click event
    if (resetFiltersButton) {
        resetFiltersButton.addEventListener('click', function () {
            // Reset filter options
            filterOptions.forEach(option => {
                option.classList.remove('active');
            });
            // Set "All" as active
            const allOption = document.querySelector('.filter-option[data-status="all"]');
            if (allOption) {
                allOption.classList.add('active');
            }

            // Reset dropdowns
            dropdowns.forEach(dropdown => {
                dropdown.textContent = dropdown.getAttribute('data-placeholder') || 'Select...';
                dropdown.removeAttribute('data-selected');
            });

            // Reset date inputs
            const dateFrom = document.getElementById('orderDateFrom');
            const dateTo = document.getElementById('orderDateTo');
            if (dateFrom) dateFrom.value = '';
            if (dateTo) dateTo.value = '';

            // Reset search input
            if (searchInput) {
                searchInput.value = '';
            }

            // Close any open dropdowns
            document.querySelectorAll('.dropdown-content.show').forEach(dropdown => {
                dropdown.classList.remove('show');
            });

            // Show all order cards
            orderCards.forEach(card => {
                card.style.display = 'block';
            });

            // Update order counts
            updateOrderCounts();
        });
    }

    // Close dropdowns when clicking outside
    document.addEventListener('click', function(e) {
        if (!e.target.matches('.dropdown')) {
            document.querySelectorAll('.dropdown-content.show').forEach(dropdown => {
                dropdown.classList.remove('show');
            });
        }
    });

    // Search functionality
    if (searchInput) {
        searchInput.addEventListener('input', debounce(function () {
            filterOrders();
        }, 300));
    }

    // Handle Add New Order button
    if (addNewOrderButton) {
        addNewOrderButton.addEventListener('click', function(e) {
            e.preventDefault();
            window.location.href = '/Orders/RecordPayment';
        });
    }

    // Order buttons click events
    const orderButtons = document.querySelectorAll('.order-button');
    if (orderButtons.length > 0) {
        orderButtons.forEach(button => {
            button.addEventListener('click', function () {
                const action = this.textContent.toLowerCase();
                const orderId = this.closest('.order-card').dataset.orderId;
                
                if (action === 'paid' || action === 'unpaid') {
                    handlePaymentStatus(orderId, action);
                }
            });
        });
    }

    // Actions button functionality
    const actionsButtons = document.querySelectorAll('.actions-button');
    actionsButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.stopPropagation();
            const orderId = this.closest('.order-card').dataset.orderId;
            const actionsMenu = document.createElement('div');
            actionsMenu.className = 'actions-menu';
            actionsMenu.innerHTML = `
                <div class="action-item" data-action="edit-details">
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
                        <path d="M12.146.146a.5.5 0 0 1 .708 0l3 3a.5.5 0 0 1 0 .708l-10 10a.5.5 0 0 1-.168.11l-5 2a.5.5 0 0 1-.65-.65l2-5a.5.5 0 0 1 .11-.168l10-10zM11.207 2.5 13.5 4.793 14.793 3.5 12.5 1.207 11.207 2.5zm1.586 3L10.5 3.207 4 9.707V10h.5a.5.5 0 0 1 .5.5v.5h.5a.5.5 0 0 1 .5.5v.5h.293l6.5-6.5zm-9.761 5.175-.106.106-1.528 3.821 3.821-1.528.106-.106A.5.5 0 0 1 5 12.5V12h-.5a.5.5 0 0 1-.5-.5V11h-.5a.5.5 0 0 1-.468-.325z"/>
                    </svg>
                    Update Order
                </div>
                <div class="action-item" data-action="archive">
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
                        <path d="M0 2a1 1 0 0 1 1-1h14a1 1 0 0 1 1 1v2a1 1 0 0 1-1 1v7.5a2.5 2.5 0 0 1-2.5 2.5h-9A2.5 2.5 0 0 1 1 12.5V5a1 1 0 0 1-1-1V2zm2 3v7.5A1.5 1.5 0 0 0 3.5 14h9a1.5 1.5 0 0 0 1.5-1.5V5H2zm13-3H1v2h14V2zM5 7.5a.5.5 0 0 1 .5-.5h5a.5.5 0 0 1 0 1h-5a.5.5 0 0 1-.5-.5z"/>
                    </svg>
                    Archive Order
                </div>
            `;

            const existingMenus = document.querySelectorAll('.actions-menu');
            existingMenus.forEach(menu => menu.remove());

            this.parentNode.appendChild(actionsMenu);
            const buttonRect = this.getBoundingClientRect();
            actionsMenu.style.top = `${buttonRect.bottom + 5}px`;

            // Handle action items
            actionsMenu.querySelectorAll('.action-item').forEach(item => {
                item.addEventListener('click', function() {
                    const action = this.dataset.action;
                    if (action === 'edit-details') {
                        showOrderDetails(orderId);
                    } else if (action === 'archive') {
                        archiveOrder(orderId);
                    }
                    actionsMenu.remove();
                });
            });

            // Close menu when clicking outside
            document.addEventListener('click', function closeMenu(e) {
                if (!actionsMenu.contains(e.target) && e.target !== button) {
                    actionsMenu.remove();
                    document.removeEventListener('click', closeMenu);
                }
            });
        });
    });

    // Function to handle order archiving
    function archiveOrder(orderId) {
        if (confirm('Are you sure you want to archive this order?')) {
            fetch(`/Orders/ArchiveOrder/${orderId}`, { 
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    const orderCard = document.querySelector(`[data-order-id="${orderId}"]`);
                    if (orderCard) {
                        orderCard.remove();
                    }
                    // Show success message
                    const successMessage = document.createElement('div');
                    successMessage.className = 'alert alert-success';
                    successMessage.innerHTML = `
                        <div>
                            <i class="fas fa-check-circle"></i>
                            Order has been archived successfully
                        </div>
                        <button type="button" class="btn-close" onclick="this.parentElement.style.display='none'">×</button>
                    `;
                    document.querySelector('.orders-container').insertBefore(successMessage, document.querySelector('.orders-container').firstChild);
                    
                    // Auto-hide success message after 3 seconds
                    setTimeout(() => {
                        successMessage.style.opacity = '0';
                        setTimeout(() => {
                            successMessage.remove();
                        }, 300);
                    }, 3000);
                } else {
                    alert('Error archiving order');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Error archiving order');
            });
        }
    }

    // Debounce function
    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // Handle success message auto-hide
    const successMessage = document.querySelector('.alert-success');
    if (successMessage) {
        setTimeout(() => {
            successMessage.style.opacity = '0';
            setTimeout(() => {
                successMessage.style.display = 'none';
            }, 300);
        }, 3000);
    }

    // Initialize
    filterOrders();
});

function showOrderDetails(orderId) {
    fetch(`/Orders/GetOrderDetails/${orderId}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                const order = data.order;
                document.getElementById('orderId').value = order.orderID;
                document.getElementById('orderStatus').value = order.status;
                document.getElementById('trackingProvider').value = order.trackingProvider || '';
                document.getElementById('trackingNumber').value = order.trackingNumber || '';
                document.getElementById('progressPercentage').value = order.progressPercentage;
                
                const modal = document.getElementById('orderDetailsModal');
                modal.classList.add('show');
            } else {
                alert('Error loading order details');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Error loading order details');
        });
}

function closeOrderDetails() {
    const modal = document.getElementById('orderDetailsModal');
    modal.classList.remove('show');
}

// Add click outside to close
document.addEventListener('click', function(e) {
    const modal = document.getElementById('orderDetailsModal');
    if (e.target === modal) {
        closeOrderDetails();
    }
});
