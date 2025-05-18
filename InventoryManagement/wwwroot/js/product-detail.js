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
}

// Enhanced function to open the product detail popup
function openProductDetailPopup(productId) {
    // Show the modal with loading state
    $('#productDetailModal').modal('show');


    // Fetch the product details
    $.ajax({
        url: '/Products/GetProductDetail/' + productId,
        type: 'GET',
        success: function (response) {
            // Update the modal content with the fetched details
            $('#productDetailContent').html(response);

            // Store the current product ID for navigation
            //$('#productDetailContent').attr('data-product-id', productId);

            // Initialize thumbnail functionality
            initializeThumbnails();
        },
        error: function () {
            // Handle error with styled message
            $('#productDetailContent').html(`
                <div class="text-center p-5">
                    <h4 class="text-danger">Failed to load product details.</h4>
                    <button class="btn btn-outline-secondary mt-3" data-bs-dismiss="modal">Close</button>
                </div>
            `);
        },
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
    //alert('Stock adjustment functionality will be implemented separately.');

    // In a real implementation, you might do something like:
    
    $.ajax({
        url: '/Products/GetAdjustStockForm/' + productId,
        type: 'GET',
        success: function(response) {
            $('#adjustStockModal .modal-body').html(response);
            $('#adjustStockModal').modal('show');
        }
    });
    
}

// Functions for edit and delete
function editProduct(productId) {
    window.location.href = '/Products/Edit/' + productId;
}

function deleteProduct(button) {
    const productId = $(button).data('product-id');

    if (!productId) {
        alert("No product ID found on button.");
        return;
    }

    if (!confirm("Are you sure you want to delete this product? This action cannot be undone.")) {
        return;
    }

    $.ajax({
        url: `/Products/PerformDelete/${productId}`,
        type: 'POST',
        success: function (response) {
            const modal = bootstrap.Modal.getInstance(document.getElementById('deleteConfirmModal'));
            if (modal) modal.hide();

            if (response.success) {
                showNotification('success', response.message, true); // Show with refresh button
            } else {
                showNotification('danger', response.message);
            }
        },
        error: function (xhr, status, error) {
            alert("An unexpected error occurred while trying to delete the product. Details: " + error);
        }
    });
}


