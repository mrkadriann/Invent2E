document.addEventListener('DOMContentLoaded', function () {
    // --- DOM Element Selection ---
    const form = document.getElementById('recordPaymentForm'); // Corrected ID from previous script
    if (!form) {
        console.error('FATAL: Form with ID "recordPaymentForm" not found.');
        return;
    }

    // Product adding elements
    const productSelect = document.getElementById('productSelect');
    const productQty = document.getElementById('productQty');
    const addProductBtn = document.getElementById('addProductBtn');
    const orderProductsBody = document.getElementById('orderProductsBody');

    // Order total elements
    const orderTotalDisplay = document.getElementById('orderTotalDisplay');
    const orderTotalHiddenInput = document.getElementById('orderTotalHiddenInput');

    // Payment status and details elements
    const isPaidSelect = document.getElementById('isPaidSelect');
    const paymentMethodGroup = document.getElementById('paymentMethodGroup');
    const paymentMethodSelect = document.getElementById('paymentMethodSelect');
    const referenceNumberGroup = document.getElementById('referenceNumberGroup');
    const referenceNumberInput = document.getElementById('referenceNumberInput');
    const cashPaymentGroup = document.getElementById('cashPaymentGroup');
    const paymentAmountInput = document.getElementById('paymentAmountInput');
    const changeAmountDisplay = document.getElementById('changeAmountDisplay');
    const insufficientAmountDisplay = document.getElementById('insufficientAmountDisplay');

    // Check if essential elements for product adding and core payment logic are present
    if (!productSelect || !productQty || !addProductBtn || !orderProductsBody ||
        !orderTotalDisplay || !orderTotalHiddenInput || !isPaidSelect) {
        console.error("FATAL: One or more essential product/payment elements are missing from the page. Functionality will be severely limited.");
        // Depending on your needs, you might want to return here or disable parts of the UI
    }
    // Optional: Check for other payment detail elements and warn if missing
    const paymentDetailElements = { paymentMethodGroup, paymentMethodSelect, referenceNumberGroup, referenceNumberInput, cashPaymentGroup, paymentAmountInput, changeAmountDisplay, insufficientAmountDisplay };
    for (const key in paymentDetailElements) {
        if (!paymentDetailElements[key]) {
            console.warn(`Warning: Payment detail element for "${key}" not found. Some payment options might not work as expected.`);
        }
    }

    let orderItems = []; // Array to store items in the current order

    // --- Product Management Functions ---
    function updateOrderTable() {
        if (!orderProductsBody) return;
        orderProductsBody.innerHTML = ''; // Clear existing rows
        orderItems.forEach(item => {
            const row = orderProductsBody.insertRow();
            row.innerHTML = `
                <td>${item.ProductName}</td>
                <td style="text-align:right;">${item.Quantity}</td>
                <td style="text-align:right;">?${item.UnitPrice.toFixed(2)}</td>
                <td style="text-align:right;">?${item.TotalPrice.toFixed(2)}</td>
                <td style="text-align:center;">
                    <button type="button" class="btn btn-danger btn-sm remove-item-btn" data-product-id="${item.ProductID}">
                        Remove
                    </button>
                </td>
            `;
        });
    }

    function updateOrderTotal() {
        const total = orderItems.reduce((sum, item) => sum + item.TotalPrice, 0);
        if (orderTotalDisplay) {
            orderTotalDisplay.textContent = `?${total.toFixed(2)}`;
        }
        if (orderTotalHiddenInput) {
            orderTotalHiddenInput.value = total.toFixed(2);
        }
        if (paymentMethodSelect && paymentMethodSelect.value === 'Cash' && paymentAmountInput && cashPaymentGroup && cashPaymentGroup.style.display !== 'none') {
            validatePaymentAmount();
        }
    }

    if (addProductBtn) {
        addProductBtn.addEventListener('click', function () {
            if (!productSelect || !productQty) {
                console.error("Product select or quantity input not found for adding product.");
                return;
            }

            const selectedOption = productSelect.options[productSelect.selectedIndex];
            if (!selectedOption || !selectedOption.value) { // Check if a valid option (not the placeholder) is selected
                alert('Please select a product.');
                productSelect.focus();
                return;
            }

            const productId = selectedOption.value;
            const productName = selectedOption.dataset.name;
            const unitPriceString = selectedOption.dataset.price;
            const quantity = parseInt(productQty.value, 10);

            if (isNaN(quantity) || quantity <= 0) {
                alert('Please enter a valid quantity (must be a number greater than 0).');
                productQty.focus();
                return;
            }

            if (!productName || typeof productName === 'undefined' || productName.trim() === '' ||
                !unitPriceString || typeof unitPriceString === 'undefined') {
                alert('Product information (name or price) is missing from the selected option. Please check the data-name and data-price attributes in your HTML product options.');
                return;
            }

            const unitPrice = parseFloat(unitPriceString);
            if (isNaN(unitPrice) || unitPrice < 0) {
                alert('Product price is invalid. Please check the data-price attribute.');
                return;
            }

            const existingItem = orderItems.find(item => item.ProductID === parseInt(productId, 10));
            if (existingItem) {
                existingItem.Quantity += quantity;
                existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
            } else {
                orderItems.push({
                    ProductID: parseInt(productId, 10),
                    ProductName: productName,
                    Quantity: quantity,
                    UnitPrice: unitPrice,
                    TotalPrice: quantity * unitPrice
                });
            }
            updateOrderTable();
            updateOrderTotal();
            productSelect.value = ''; // Reset select
            productQty.value = '1';   // Reset quantity
            productSelect.focus();
        });
    } else {
        console.warn("Add Product Button (addProductBtn) not found.");
    }

    if (orderProductsBody) {
        orderProductsBody.addEventListener('click', function (e) {
            if (e.target && e.target.classList.contains('remove-item-btn')) {
                const productIdToRemove = parseInt(e.target.dataset.productId, 10);
                orderItems = orderItems.filter(item => item.ProductID !== productIdToRemove);
                updateOrderTable();
                updateOrderTotal();
            }
        });
    } else {
        console.warn("Order Products Body (orderProductsBody) not found. Cannot remove items.");
    }

    // --- Payment UI Toggling ---
    if (isPaidSelect) {
        isPaidSelect.addEventListener('change', function () {
            const isPaid = this.value === 'true';
            if (paymentMethodGroup) paymentMethodGroup.style.display = isPaid ? 'block' : 'none';

            if (!isPaid) {
                if (referenceNumberGroup) referenceNumberGroup.style.display = 'none';
                if (cashPaymentGroup) cashPaymentGroup.style.display = 'none';
                if (changeAmountDisplay) changeAmountDisplay.style.display = 'none';
                if (insufficientAmountDisplay) insufficientAmountDisplay.style.display = 'none';
                if (paymentMethodSelect) paymentMethodSelect.value = '';
                if (referenceNumberInput) referenceNumberInput.value = '';
                if (paymentAmountInput) {
                    paymentAmountInput.value = '';
                    paymentAmountInput.classList.remove('is-valid', 'is-invalid');
                }
            } else {
                if (paymentMethodSelect) {
                    // Manually trigger change for paymentMethodSelect if it has a value,
                    // otherwise, its dependent fields (like referenceNumberGroup) won't update correctly
                    // if 'isPaid' is toggled back to true and a payment method was already selected.
                    paymentMethodSelect.dispatchEvent(new Event('change'));
                }
            }
        });
    } else {
        console.warn("Is Paid Select (isPaidSelect) not found. Payment UI toggling may not work.");
    }

    if (paymentMethodSelect) {
        paymentMethodSelect.addEventListener('change', function () {
            const method = this.value;
            const showRef = method === 'GCash' || method === 'Maya' || method === 'Bank';
            const showCash = method === 'Cash';

            if (referenceNumberGroup) referenceNumberGroup.style.display = showRef ? 'block' : 'none';
            if (cashPaymentGroup) cashPaymentGroup.style.display = showCash ? 'block' : 'none';

            if (changeAmountDisplay) changeAmountDisplay.style.display = 'none';
            if (insufficientAmountDisplay) insufficientAmountDisplay.style.display = 'none';
            if (referenceNumberInput && !showRef) referenceNumberInput.value = '';


            if (showCash) {
                if (paymentAmountInput) {
                    validatePaymentAmount();
                    paymentAmountInput.setAttribute('required', 'required'); // Make cash amount required if cash selected
                }
            } else {
                if (paymentAmountInput) {
                    paymentAmountInput.value = '';
                    paymentAmountInput.classList.remove('is-valid', 'is-invalid');
                    paymentAmountInput.removeAttribute('required'); // Not required if not cash
                }
            }

            if (showRef && referenceNumberInput) {
                referenceNumberInput.setAttribute('required', 'required');
            } else if (referenceNumberInput) {
                referenceNumberInput.removeAttribute('required');
            }
        });
    } else {
        console.warn("Payment Method Select (paymentMethodSelect) not found.");
    }

    // --- Cash Payment Validation ---
    function validatePaymentAmount() {
        if (!paymentAmountInput || !orderTotalHiddenInput || !changeAmountDisplay || !insufficientAmountDisplay) return;

        const total = parseFloat(orderTotalHiddenInput.value) || 0;
        const paid = parseFloat(paymentAmountInput.value) || 0;

        changeAmountDisplay.style.display = 'none';
        insufficientAmountDisplay.style.display = 'none';
        paymentAmountInput.classList.remove('is-valid', 'is-invalid');

        if (paymentAmountInput.value.trim() === '' && paymentAmountInput.hasAttribute('required')) {
            // If empty but required, mark invalid (will be caught by general required field validation too)
            paymentAmountInput.classList.add('is-invalid');
            return;
        }
        if (paymentAmountInput.value.trim() === '') return; // Don't validate if empty and not required (e.g. method not cash)


        if (paid > total) {
            const change = paid - total;
            changeAmountDisplay.textContent = `Change: ?${change.toFixed(2)}`;
            changeAmountDisplay.style.display = 'block';
            paymentAmountInput.classList.add('is-valid');
        } else if (paid < total) {
            insufficientAmountDisplay.textContent = `Insufficient. Needs ?${(total - paid).toFixed(2)} more.`;
            insufficientAmountDisplay.style.display = 'block';
            paymentAmountInput.classList.add('is-invalid');
        } else if (paid === total) {
            paymentAmountInput.classList.add('is-valid');
        }
    }

    if (paymentAmountInput) {
        paymentAmountInput.addEventListener('input', validatePaymentAmount);
    } else {
        console.warn("Payment Amount Input (paymentAmountInput) not found.");
    }

    // --- Form Submission Logic ---
    form.addEventListener('submit', function (e) {
        e.preventDefault();

        if (isPaidSelect && isPaidSelect.value === 'false') {
            alert('Cannot submit an unpaid order. Please mark the order as paid and provide payment details.');
            isPaidSelect.focus();
            return;
        }

        const requiredFields = form.querySelectorAll('[required]');
        let firstInvalidField = null;
        let isValid = true;

        requiredFields.forEach(field => {
            // Only validate visible fields or fields that should be logically present
            const parentGroup = field.closest('#paymentMethodGroup, #referenceNumberGroup, #cashPaymentGroup');
            const isVisibleOrAlwaysRequired = !parentGroup || parentGroup.style.display !== 'none';

            if (isVisibleOrAlwaysRequired && !field.value.trim()) {
                isValid = false;
                field.classList.add('is-invalid');
                if (!firstInvalidField) firstInvalidField = field;

                const labelElement = document.querySelector(`label[for="${field.id}"]`);
                const labelText = labelElement ? labelElement.textContent.replace(':', '').trim() : field.placeholder || field.name || 'This field';
                // Alert will be shown once after loop
            } else if (isVisibleOrAlwaysRequired && field.classList.contains('is-invalid') && field.id === 'paymentAmountInput' && paymentMethodSelect.value === 'Cash') {
                // Special case for cash payment amount if it's marked invalid by its own validation
                const total = parseFloat(orderTotalHiddenInput ? orderTotalHiddenInput.value : 0) || 0;
                const paid = parseFloat(paymentAmountInput ? paymentAmountInput.value : 0) || 0;
                if (paid < total) {
                    isValid = false;
                    if (!firstInvalidField) firstInvalidField = field;
                } else {
                    field.classList.remove('is-invalid'); // If it was marked invalid but condition is now met
                }
            }
            else {
                field.classList.remove('is-invalid');
            }
        });

        if (!isValid) {
            alert('Please fill in all required fields correctly.');
            if (firstInvalidField) firstInvalidField.focus();
            return;
        }

        if (orderItems.length === 0) {
            alert('Please add at least one product to the order.');
            if (productSelect) productSelect.focus();
            return;
        }

        if (isPaidSelect && isPaidSelect.value === 'true' && paymentMethodSelect && !paymentMethodSelect.value) {
            alert('Please select a payment method.');
            if (paymentMethodSelect) paymentMethodSelect.focus();
            return;
        }

        // Cash payment validation again, just to be absolutely sure before submission
        if (paymentMethodSelect && paymentMethodSelect.value === 'Cash') {
            const total = parseFloat(orderTotalHiddenInput ? orderTotalHiddenInput.value : 0) || 0;
            const paid = parseFloat(paymentAmountInput ? paymentAmountInput.value : 0) || 0;
            if (paid < total) {
                alert('Amount paid must be equal to or greater than the order total for cash payments.');
                if (paymentAmountInput) paymentAmountInput.focus();
                return;
            }
        }

        // Reference number validation for specific payment methods again
        if (isPaidSelect && isPaidSelect.value === 'true' &&
            paymentMethodSelect &&
            (paymentMethodSelect.value === 'GCash' || paymentMethodSelect.value === 'Maya' || paymentMethodSelect.value === 'Bank') &&
            referenceNumberInput && !referenceNumberInput.value.trim()) {
            alert('Please enter a reference number for the selected payment method.');
            if (referenceNumberInput) referenceNumberInput.focus();
            return;
        }

        try {
            const orderItemsData = orderItems.map(item => ({
                ProductID: item.ProductID,
                ProductName: item.ProductName, // Server might ignore if model doesn't have it
                Quantity: item.Quantity,
                UnitPrice: item.UnitPrice,     // Server might ignore
                TotalPrice: item.TotalPrice    // Server might ignore
            }));

            let orderItemsJsonInput = form.querySelector('input[name="OrderItemsJson"]');
            if (!orderItemsJsonInput) {
                orderItemsJsonInput = document.createElement('input');
                orderItemsJsonInput.type = 'hidden';
                orderItemsJsonInput.name = 'OrderItemsJson';
                form.appendChild(orderItemsJsonInput);
            }
            orderItemsJsonInput.value = JSON.stringify(orderItemsData);

            if (isPaidSelect && isPaidSelect.value === 'true') {
                // PaymentDate
                let paymentDateHidden = form.querySelector('input[name="PaymentDate"]');
                if (!paymentDateHidden) { paymentDateHidden = document.createElement('input'); paymentDateHidden.type = 'hidden'; paymentDateHidden.name = 'PaymentDate'; form.appendChild(paymentDateHidden); }
                paymentDateHidden.value = new Date().toISOString();

                // PaymentAmount
                let paymentAmountHidden = form.querySelector('input[name="PaymentAmount"]');
                if (!paymentAmountHidden) { paymentAmountHidden = document.createElement('input'); paymentAmountHidden.type = 'hidden'; paymentAmountHidden.name = 'PaymentAmount'; form.appendChild(paymentAmountHidden); }
                paymentAmountHidden.value = (paymentMethodSelect && paymentMethodSelect.value === 'Cash' && paymentAmountInput) ? paymentAmountInput.value : (orderTotalHiddenInput ? orderTotalHiddenInput.value : '0');

                // ReferenceNumber
                let referenceNumberHidden = form.querySelector('input[name="ReferenceNumber"]');
                if (!referenceNumberHidden) { referenceNumberHidden = document.createElement('input'); referenceNumberHidden.type = 'hidden'; referenceNumberHidden.name = 'ReferenceNumber'; form.appendChild(referenceNumberHidden); }
                referenceNumberHidden.value = (paymentMethodSelect && paymentMethodSelect.value === 'Cash') ? `CASH-${new Date().getTime()}` : (referenceNumberInput ? referenceNumberInput.value : '');

                // PaymentMethod
                let paymentMethodHidden = form.querySelector('input[name="PaymentMethod"]');
                if (!paymentMethodHidden) { paymentMethodHidden = document.createElement('input'); paymentMethodHidden.type = 'hidden'; paymentMethodHidden.name = 'PaymentMethod'; form.appendChild(paymentMethodHidden); }
                paymentMethodHidden.value = paymentMethodSelect ? paymentMethodSelect.value : '';
            }
            // IsPaid (always send this, even if false, so server knows the state)
            let isPaidHidden = form.querySelector('input[name="IsPaid"]');
            if (!isPaidHidden) { isPaidHidden = document.createElement('input'); isPaidHidden.type = 'hidden'; isPaidHidden.name = 'IsPaid'; form.appendChild(isPaidHidden); }
            isPaidHidden.value = (isPaidSelect) ? isPaidSelect.value : 'false';


            // Combine address fields
            const houseNo = form.querySelector('input[name="HouseNo"]')?.value.trim() || '';
            const streetAddress = form.querySelector('input[name="StreetAddress"]')?.value.trim() || '';
            const barangay = form.querySelector('input[name="Barangay"]')?.value.trim() || '';
            const postalId = form.querySelector('input[name="PostalId"]')?.value.trim() || '';
            const city = form.querySelector('input[name="City"]')?.value.trim() || '';
            const addressParts = [houseNo, streetAddress, barangay, city, postalId].filter(part => part !== '');
            const shippingAddress = addressParts.join(', ');

            let shippingAddressInput = form.querySelector('input[name="ShippingAddress"]');
            if (!shippingAddressInput) {
                shippingAddressInput = document.createElement('input');
                shippingAddressInput.type = 'hidden';
                shippingAddressInput.name = 'ShippingAddress';
                form.appendChild(shippingAddressInput);
            }
            shippingAddressInput.value = shippingAddress;

            // Log before actual submission (useful for debugging)
            const formDataToLog = {};
            new FormData(form).forEach((value, key) => formDataToLog[key] = value);
            console.log('Form data being submitted:', formDataToLog);

            // Submit the form
            form.submit();
            // alert("Form submission initiated. This would normally redirect or show a success message from the server.");

        } catch (error) {
            console.error('Error preparing form submission:', error);
            alert('Error preparing order data: ' + error.message);
        }
    });

    // Initial UI setup based on default values
    if (isPaidSelect) {
        isPaidSelect.dispatchEvent(new Event('change'));
    }
    // Trigger initial state for payment method select if it's visible and has a value
    if (paymentMethodSelect && paymentMethodGroup && paymentMethodGroup.style.display !== 'none' && paymentMethodSelect.value) {
        paymentMethodSelect.dispatchEvent(new Event('change'));
    }
    updateOrderTotal(); // Initialize total display

});