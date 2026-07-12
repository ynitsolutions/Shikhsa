//$(document).ready(function () {

//    // 1. Live Change Trigger: Option select hote hi error automatic gayab ho jaye
//    $(document).on('change', 'select.form-control.select2.required', function () {
//        var $selectBox = $(this);
//        if ($selectBox.val() !== "" && $selectBox.val() !== null) {
//            var $container = $selectBox.next('.select2-container');

//            // Red style aur dynamic message dono remove karein
//            $container.find('.select2-selection--single').parent().removeClass('is-invalid');
//            $container.removeClass('is-invalid');
//            $selectBox.closest('.form-group, .mb-3').find('.select2-required-error').remove();
//        }
//    });
//    // Decimal Only
//    $(document).on('input', '.decimal', function () {
//        var value = $(this).val();

//        // Only digits and one decimal point
//        value = value.replace(/[^0-9.]/g, '');

//        // Allow only one decimal point
//        var parts = value.split('.');
//        if (parts.length > 2) {
//            value = parts[0] + '.' + parts.slice(1).join('');
//        }

//        $(this).val(value);
//    });

//    // Number Only
//    $(document).on('input', '.number-only', function () {
//        $(this).val($(this).val().replace(/\D/g, ''));
//    });
//    // 2. Global Form Submit Validator logic
//    $(document).on('submit', 'form', function (e) {
//        var $currentForm = $(this);
//        var isFormValid = true;

//        // Aapke code ke configuration ".form-control.select2.required" par search karega
//        $currentForm.find('select.form-control.select2.required').each(function () {
//            var $selectBox = $(this);
//            var $container = $selectBox.next('.select2-container');

//            // Purani errors clean karein
//            $container.removeClass('is-invalid');
//            $selectBox.closest('.form-group, .mb-3').find('.select2-required-error').remove();

//            // Check agar Select value null ya empty string hai
//            if ($selectBox.val() === "" || $selectBox.val() === null) {
//                isFormValid = false;

//                // Red line trigger karne ke liye is-invalid add kareinssss
//                $container.addClass('is-invalid');

//                // Error message template creation
//                var errorMessage = '<span class="select2-required-error">Something is required.</span>';

//                // Select2 structure block ke exact baad apply karein
//                $container.after(errorMessage);
//            }
//        });

//        // Validation failed state trigger control
//        if (!isFormValid) {
//            e.preventDefault(); // Stop form post

//            // Screen tracking bounce to component error
//            $('html, body').animate({
//                scrollTop: $currentForm.find('.is-invalid').first().offset().top - 120
//            }, 200);
//        }
//    });
//});
//$(document).ready(function () {

//    // Helper function error display aur highlight karne ke liye
//    function toggleValidationError($element, isValid, errorMessage) {
//        $element.removeClass('is-invalid');
//        $element.parent().find('.jquery-validation-error').remove();

//        if (!isValid && $element.val().trim() !== "") {
//            $element.addClass('is-invalid');
//            $element.after('<span class="jquery-validation-error">' + errorMessage + '</span>');
//            return false;
//        }
//        return true;
//    }

//    // 1. Only alphabets (no spaces)
//    function validateOnlyAlphabets(input) {
//        return /^[a-zA-Z]+$/.test(input);
//    }

//    // 2. Alphabets with spaces
//    function validateAlphabetsWithSpace(input) {
//        return /^[a-zA-Z\s]+$/.test(input);
//    }

//    // 3. No special characters (Letters, numbers, and spaces allowed)
//    function validateNoSpecialChars(input) {
//        return /^[a-zA-Z0-9\s]+$/.test(input);
//    }

//    // 4. Indian Mobile Numbers (Optional +91, 10 digits starting with 6-9)
//    function validateIndianMobile(input) {
//        return /^(?:\+91)?(?:[6-9]\d{9})$/.test(input);
//    }
//    // 5. Decimal Number
//    function validateDecimal(input) {
//        return /^\d+(\.\d+)?$/.test(input);
//    }

//    // 6. Numbers Only (0-9)
//    function validateNumberOnly(input) {
//        return /^\d+$/.test(input);
//    }
//    // Dynamic Typing Validation Trigger: Jab user type karega toh error realtime mein check hogi
//    $(document).on('input change', '.text-only, .textspaceonly, .nospecialchars, .indianmobile', function () {
//        var $el = $(this);
//        var val = $el.val();

//        if ($el.hasClass('text-only')) {
//            toggleValidationError($el, validateOnlyAlphabets(val), "Only alphabets without spaces are allowed.");
//        }
//        else if ($el.hasClass('textspaceonly')) {
//            toggleValidationError($el, validateAlphabetsWithSpace(val), "Only alphabets and spaces are allowed.");
//        }
//        else if ($el.hasClass('nospecialchars')) {
//            toggleValidationError($el, validateNoSpecialChars(val), "Special characters are not allowed.");
//        }
//        else if ($el.hasClass('indianmobile')) {
//            toggleValidationError($el, validateIndianMobile(val), "Please enter a valid Indian mobile number.");
//        }
//        else if ($el.hasClass('decimal')) {
//            toggleValidationError($el, validateDecimal(val), "Please enter a valid decimal number.");
//        }
//        else if ($el.hasClass('number-only')) {
//            toggleValidationError($el, validateNumberOnly(val), "Only numbers are allowed.");
//        }
//    });

//    // Global Form Submit Interceptor: Form submit hone par saari custom validations check karega
//    $(document).on('submit', 'form', function (e) {
//        var $form = $(this);
//        var isAllValid = true;

//        // Check Only Alphabets
//        $form.find('.text-only').each(function () {
//            if (!toggleValidationError($(this), validateOnlyAlphabets($(this).val()), "Only alphabets without spaces are allowed.")) {
//                isAllValid = false;
//            }
//        });

//        // Check Alphabets with Space
//        $form.find('.textspaceonly').each(function () {
//            if (!toggleValidationError($(this), validateAlphabetsWithSpace($(this).val()), "Only alphabets and spaces are allowed.")) {
//                isAllValid = false;
//            }
//        });

//        // Check No Special Chars
//        $form.find('.nospecialchars').each(function () {
//            if (!toggleValidationError($(this), validateNoSpecialChars($(this).val()), "Special characters are not allowed.")) {
//                isAllValid = false;
//            }
//        });

//        // Check Indian Mobile
//        $form.find('.indianmobile').each(function () {
//            if (!toggleValidationError($(this), validateIndianMobile($(this).val()), "Please enter a valid Indian mobile number.")) {
//                isAllValid = false;
//            }
//        });
//        // Decimal
//        $form.find('.decimal').each(function () {
//            if (!toggleValidationError($(this),
//                validateDecimal($(this).val()),
//                "Please enter a valid decimal number.")) {
//                isAllValid = false;
//            }
//        });

//        // Number Only
//        $form.find('.number-only').each(function () {
//            if (!toggleValidationError($(this),
//                validateNumberOnly($(this).val()),
//                "Only numbers are allowed.")) {
//                isAllValid = false;
//            }
//        });
//        // Agar koi bhi field invalid hai toh form stop karein aur smooth scroll karein
//        if (!isAllValid) {
//            e.preventDefault();
//            $('html, body').animate({
//                scrollTop: $form.find('.is-invalid').first().offset().top - 120
//            }, 200);
//        }
//    });
//});


$(function () {

    /* ==========================================================
       Helper Function : Show/Hide Validation Error
    ========================================================== */

    function toggleValidationError($element, isValid, errorMessage) {

        $element.removeClass("is-invalid");
        $element.parent().find(".jquery-validation-error").remove();

        if (!isValid && $.trim($element.val()) !== "") {

            $element.addClass("is-invalid");

            $element.after(
                '<span class="jquery-validation-error text-danger">' +
                errorMessage +
                "</span>"
            );

            return false;
        }

        return true;
    }

    /* ==========================================================
       Validation Functions
    ========================================================== */

    //function validateOnlyAlphabets(input) {
    //    return /^[A-Za-z]+$/.test(input);
    //}

    //function validateAlphabetsWithSpace(input) {
    //    return /^[A-Za-z\s]+$/.test(input);
    //}

    //function validateNoSpecialChars(input) {
    //    return /^[A-Za-z0-9\s]+$/.test(input);
    //}
    // Only alphabets (any language)
    function validateOnlyAlphabets(input) {
        return /^\p{L}+$/u.test(input);
    }

    // Alphabets with spaces (any language)
    function validateAlphabetsWithSpace(input) {
        return /^[\p{L}\s]+$/u.test(input);
    }

    // Alphabets + Numbers + Spaces (any language)
    function validateNoSpecialChars(input) {
        return /^[\p{L}\p{N}\s]+$/u.test(input);
    }

    function validateIndianMobile(input) {
        return /^(?:\+91)?[6-9]\d{9}$/.test(input);
    }

    function validateDecimal(input) {
        return /^(\d+(\.\d+)?|\.\d+)$/.test(input);
    }

    function validateNumberOnly(input) {
        return /^\d+$/.test(input);
    }

    /* ==========================================================
       Input Restriction
    ========================================================== */

    // Decimal
    $(document).on("input", ".decimal", function () {

        var value = $(this).val();

        value = value.replace(/[^0-9.]/g, "");

        var parts = value.split(".");

        if (parts.length > 2) {
            value = parts[0] + "." + parts.slice(1).join("");
        }

        $(this).val(value);

    });

    // Number Only
    $(document).on("input", ".number-only", function () {

        $(this).val(
            $(this).val().replace(/\D/g, "")
        );

    });

    /* ==========================================================
       Live Validation
    ========================================================== */

    $(document).on(
        "input change",
        ".text-only,.textspaceonly,.nospecialchars,.indianmobile,.decimal,.number-only",
        function () {

            var $el = $(this);

            var value = $el.val();

            if ($el.hasClass("text-only")) {

                toggleValidationError(
                    $el,
                    validateOnlyAlphabets(value),
                    "Only alphabets without spaces are allowed."
                );

            }

            else if ($el.hasClass("textspaceonly")) {

                toggleValidationError(
                    $el,
                    validateAlphabetsWithSpace(value),
                    "Only alphabets and spaces are allowed."
                );

            }

            else if ($el.hasClass("nospecialchars")) {

                toggleValidationError(
                    $el,
                    validateNoSpecialChars(value),
                    "Special characters are not allowed."
                );

            }

            else if ($el.hasClass("indianmobile")) {

                toggleValidationError(
                    $el,
                    validateIndianMobile(value),
                    "Please enter a valid Indian mobile number."
                );

            }

            else if ($el.hasClass("decimal")) {

                toggleValidationError(
                    $el,
                    validateDecimal(value),
                    "Please enter a valid decimal number."
                );

            }

            else if ($el.hasClass("number-only")) {

                toggleValidationError(
                    $el,
                    validateNumberOnly(value),
                    "Only numbers are allowed."
                );

            }

        });

    /* ==========================================================
       Select2 Required Validation
    ========================================================== */

    $(document).on("change", "select.form-control.select2.required", function () {

        var $select = $(this);

        if ($select.val() !== "" && $select.val() !== null) {

            var $container = $select.next(".select2-container");

            $container.removeClass("is-invalid");

            $container
                .find(".select2-selection")
                .removeClass("is-invalid");

            $select
                .closest(".form-group,.mb-3")
                .find(".select2-required-error")
                .remove();

        }

    });

    /* ==========================================================
       Form Submit Validation
    ========================================================== */

    $(document).on("submit", "form", function (e) {

        var $form = $(this);

        var isValid = true;

        /* -----------------------------
           Select2 Required
        ----------------------------- */

        $form.find("select.form-control.select2.required").each(function () {

            var $select = $(this);

            var $container = $select.next(".select2-container");

            $container.removeClass("is-invalid");

            $select
                .closest(".form-group,.mb-3")
                .find(".select2-required-error")
                .remove();

            if ($select.val() === "" || $select.val() === null) {

                isValid = false;

                $container.addClass("is-invalid");

                $container.after(
                    '<span class="select2-required-error text-danger">This field is required.</span>'
                );

            }

        });

        /* -----------------------------
           Text Only
        ----------------------------- */

        $form.find(".text-only").each(function () {

            if (!toggleValidationError($(this),
                validateOnlyAlphabets($(this).val()),
                "Only alphabets without spaces are allowed.")) {

                isValid = false;

            }

        });

        /* -----------------------------
           Text With Space
        ----------------------------- */

        $form.find(".textspaceonly").each(function () {

            if (!toggleValidationError($(this),
                validateAlphabetsWithSpace($(this).val()),
                "Only alphabets and spaces are allowed.")) {

                isValid = false;

            }

        });

        /* -----------------------------
           No Special Characters
        ----------------------------- */

        $form.find(".nospecialchars").each(function () {

            if (!toggleValidationError($(this),
                validateNoSpecialChars($(this).val()),
                "Special characters are not allowed.")) {

                isValid = false;

            }

        });

        /* -----------------------------
           Mobile
        ----------------------------- */

        $form.find(".indianmobile").each(function () {

            if (!toggleValidationError($(this),
                validateIndianMobile($(this).val()),
                "Please enter a valid Indian mobile number.")) {

                isValid = false;

            }

        });

        /* -----------------------------
           Decimal
        ----------------------------- */

        $form.find(".decimal").each(function () {

            if (!toggleValidationError($(this),
                validateDecimal($(this).val()),
                "Please enter a valid decimal number.")) {

                isValid = false;

            }

        });

        /* -----------------------------
           Number Only
        ----------------------------- */

        $form.find(".number-only").each(function () {

            if (!toggleValidationError($(this),
                validateNumberOnly($(this).val()),
                "Only numbers are allowed.")) {

                isValid = false;

            }

        });

        /* -----------------------------
           Stop Submit
        ----------------------------- */

        if (!isValid) {

            e.preventDefault();

            $("html,body").animate({

                scrollTop:
                    $form.find(".is-invalid").first().offset().top - 120

            }, 300);

        }

    });

});