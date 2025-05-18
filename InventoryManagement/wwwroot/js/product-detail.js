// Enhanced JavaScript for Product Detail Modal
$(document).ready(function () {
    // Initialize modal enhancements
    initProductModalEnhancements();
});

function initProductModalEnhancements() {
    // Add navigation buttons to header if needed
    $('#productDetailModal').on('shown.bs.modal', function () {
        // Only add navigation controls if they don't already exist
        if ($('.product-navigation').length === 0) {
            $('.product-detail-actions').prepend(`
            `);

            // Initialize navigation controls
            //$('.prev-product').on('click', function (e) {
            //    e.stopPropagation();
            //    navigateProduct('prev');
            //});

            //$('.next-product').on('click', function (e) {
            //    e.stopPropagation();
            //    navigateProduct('next');
            //});
        }
    });

    // Reset modal content when closed
    $('#productDetailModal').on('hidden.bs.modal', function () {
        setTimeout(function () {
            $('#productDetailContent').html(`
                <div class="loader-container">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                </div>
            `);
        }, 300);
    });
}

// Enhanced function to open the product detail popup
function openProductDetailPopup(productId) {
    // Show the modal with loading state
    $('#productDetailModal').modal('show');

    // Reset the content to show the loader with proper styling
    $('#productDetailContent').html(`
        <div class="loader-container">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>
    `);

    // Fetch the product details
    $.ajax({
        url: '/Products/GetProductDetail/' + productId,
        type: 'GET',
        success: function (response) {
            // Update the modal content with the fetched details
            $('#productDetailContent').html(response);

            // Store the current product ID for navigation
            $('#productDetailContent').attr('data-product-id', productId);

            // Initialize thumbnail functionality
            initializeThumbnails();
        },
        error: function () {
            // Handle error with styled message
            $('#productDetailContent').html(`
                <div class="product-detail-header">
                    <div class="product-title-area">
                        <h2>Error Loading Product</h2>
                    </div>
                    <div class="product-detail-actions">
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                </div>
                <div class="product-detail-content" style="justify-content: center; min-height: 200px;">
                    <div style="text-align: center; padding: 40px;">
                        <p>Failed to load product details. Please try again.</p>
                        <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Close</button>
                    </div>
                </div>
            `);
        }
    });
}

// Enhanced function to change the main product image with animation
function changeMainImage(imageUrl) {
    // Fade out the current image
    $('#mainProductImage').fadeOut(150, function () {
        // Change the source
        $(this).attr('src', imageUrl);

        // Fade in the new image
        $(this).fadeIn(200);
    });

    // Update active state of thumbnails
    $('.thumbnail').removeClass('active');
    $(`.thumbnail img[src="${imageUrl}"]`).closest('.thumbnail').addClass('active');
}

// Initialize thumbnails after content is loaded
function initializeThumbnails() {
    // Make sure first thumbnail is active
    $('.thumbnail').first().addClass('active');

    // Add click event to thumbnails if not already added
    $('.thumbnail').off('click').on('click', function () {
        let imageUrl = $(this).find('img').attr('src');
        changeMainImage(imageUrl);
    });
}

// Navigate between products
function navigateProduct(direction) {
    // This is a placeholder - you'll need to implement based on your product list structure
    console.log('Navigate:', direction);

    // Example implementation:
    // You'd need to have a list of product IDs available
    // This could be stored in a data attribute on the page or fetched via AJAX
    let currentProductId = $('#productDetailContent').attr('data-product-id');

    // For demonstration purposes, let's simulate navigation
    // In a real application, you'd fetch the next/prev product ID
    $.ajax({
        url: '/Products/GetAdjacentProduct',
        type: 'GET',
        data: {
            currentId: currentProductId,
            direction: direction
        },
        success: function (response) {
            if (response && response.productId) {
                openProductDetailPopup(response.productId);
            }
        },
        error: function () {
            console.log('Failed to navigate to ' + direction + ' product');
        }
    });
}

// Enhanced function to open adjust stock modal
function openAdjustStockModal(productId) {
    // This would be implemented based on your requirements
    // For now, let's just log and show a simple alert
    console.log('Open adjust stock modal for product ID:', productId);

    // You could implement a separate modal or redirect to a page
    // For demonstration purposes, just show a simple alert
    alert('Stock adjustment functionality will be implemented separately.');

    // In a real implementation, you might do something like:
    /*
    $.ajax({
        url: '/Products/GetAdjustStockForm/' + productId,
        type: 'GET',
        success: function(response) {
            $('#adjustStockModal .modal-body').html(response);
            $('#adjustStockModal').modal('show');
        }
    });
    */
}

// Functions for edit and duplicate
function editProduct(productId) {
    window.location.href = '/Products/Edit/' + productId;
}

function duplicateProduct(productId) {
    window.location.href = '/Products/Duplicate/' + productId;
}

