$(document).ready(function () {

    // 1. Live Change Trigger: Option select hote hi error automatic gayab ho jaye
    $(document).on('change', 'select.form-control.select2.required', function () {
        var $selectBox = $(this);
        if ($selectBox.val() !== "" && $selectBox.val() !== null) {
            var $container = $selectBox.next('.select2-container');

            // Red style aur dynamic message dono remove karein
            $container.find('.select2-selection--single').parent().removeClass('is-invalid');
            $container.removeClass('is-invalid');
            $selectBox.closest('.form-group, .mb-3').find('.select2-required-error').remove();
        }
    });

    // 2. Global Form Submit Validator logic
    $(document).on('submit', 'form', function (e) {
        var $currentForm = $(this);
        var isFormValid = true;

        // Aapke code ke configuration ".form-control.select2.required" par search karega
        $currentForm.find('select.form-control.select2.required').each(function () {
            var $selectBox = $(this);
            var $container = $selectBox.next('.select2-container');

            // Purani errors clean karein
            $container.removeClass('is-invalid');
            $selectBox.closest('.form-group, .mb-3').find('.select2-required-error').remove();

            // Check agar Select value null ya empty string hai
            if ($selectBox.val() === "" || $selectBox.val() === null) {
                isFormValid = false;

                // Red line trigger karne ke liye is-invalid add kareinssss
                $container.addClass('is-invalid');

                // Error message template creation
                var errorMessage = '<span class="select2-required-error">Something is required.</span>';

                // Select2 structure block ke exact baad apply karein
                $container.after(errorMessage);
            }
        });

        // Validation failed state trigger control
        if (!isFormValid) {
            e.preventDefault(); // Stop form post

            // Screen tracking bounce to component error
            $('html, body').animate({
                scrollTop: $currentForm.find('.is-invalid').first().offset().top - 120
            }, 200);
        }
    });
});
$(document).ready(function () {

    // Helper function error display aur highlight karne ke liye
    function toggleValidationError($element, isValid, errorMessage) {
        $element.removeClass('is-invalid');
        $element.parent().find('.jquery-validation-error').remove();

        if (!isValid && $element.val().trim() !== "") {
            $element.addClass('is-invalid');
            $element.after('<span class="jquery-validation-error">' + errorMessage + '</span>');
            return false;
        }
        return true;
    }

    // 1. Only alphabets (no spaces)
    function validateOnlyAlphabets(input) {
        return /^[a-zA-Z]+$/.test(input);
    }

    // 2. Alphabets with spaces
    function validateAlphabetsWithSpace(input) {
        return /^[a-zA-Z\s]+$/.test(input);
    }

    // 3. No special characters (Letters, numbers, and spaces allowed)
    function validateNoSpecialChars(input) {
        return /^[a-zA-Z0-9\s]+$/.test(input);
    }

    // 4. Indian Mobile Numbers (Optional +91, 10 digits starting with 6-9)
    function validateIndianMobile(input) {
        return /^(?:\+91)?(?:[6-9]\d{9})$/.test(input);
    }

    // Dynamic Typing Validation Trigger: Jab user type karega toh error realtime mein check hogi
    $(document).on('input change', '.text-only, .textspaceonly, .nospecialchars, .indianmobile', function () {
        var $el = $(this);
        var val = $el.val();

        if ($el.hasClass('text-only')) {
            toggleValidationError($el, validateOnlyAlphabets(val), "Only alphabets without spaces are allowed.");
        }
        else if ($el.hasClass('textspaceonly')) {
            toggleValidationError($el, validateAlphabetsWithSpace(val), "Only alphabets and spaces are allowed.");
        }
        else if ($el.hasClass('nospecialchars')) {
            toggleValidationError($el, validateNoSpecialChars(val), "Special characters are not allowed.");
        }
        else if ($el.hasClass('indianmobile')) {
            toggleValidationError($el, validateIndianMobile(val), "Please enter a valid Indian mobile number.");
        }
    });

    // Global Form Submit Interceptor: Form submit hone par saari custom validations check karega
    $(document).on('submit', 'form', function (e) {
        var $form = $(this);
        var isAllValid = true;

        // Check Only Alphabets
        $form.find('.text-only').each(function () {
            if (!toggleValidationError($(this), validateOnlyAlphabets($(this).val()), "Only alphabets without spaces are allowed.")) {
                isAllValid = false;
            }
        });

        // Check Alphabets with Space
        $form.find('.textspaceonly').each(function () {
            if (!toggleValidationError($(this), validateAlphabetsWithSpace($(this).val()), "Only alphabets and spaces are allowed.")) {
                isAllValid = false;
            }
        });

        // Check No Special Chars
        $form.find('.nospecialchars').each(function () {
            if (!toggleValidationError($(this), validateNoSpecialChars($(this).val()), "Special characters are not allowed.")) {
                isAllValid = false;
            }
        });

        // Check Indian Mobile
        $form.find('.indianmobile').each(function () {
            if (!toggleValidationError($(this), validateIndianMobile($(this).val()), "Please enter a valid Indian mobile number.")) {
                isAllValid = false;
            }
        });

        // Agar koi bhi field invalid hai toh form stop karein aur smooth scroll karein
        if (!isAllValid) {
            e.preventDefault();
            $('html, body').animate({
                scrollTop: $form.find('.is-invalid').first().offset().top - 120
            }, 200);
        }
    });
});
