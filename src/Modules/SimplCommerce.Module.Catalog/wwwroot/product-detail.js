/*global jQuery, window*/
(function ($) {
    $(window).load(function () {
        $('.sp-wrap').smoothproducts();

        $('.product-attrs li').on('click', function () {
            var $variationDiv,
                selectedproductOptions = [],
                variationName,
                $form = $(this).closest("form"),
                $attrOptions = $form.find('.product-attr-options');

            $(this).find('input').prop('checked', true);
            
            $attrOptions.each(function () {
                selectedproductOptions.push($(this).find('input[type=radio]:checked').val());
            });

            var current = $(this);
            var details = current.parents(".my-thumbnail").find('.product-details').first();

            variationName = selectedproductOptions.join('-');
            $variationDiv = details.find('div[data-variation-name="' + variationName + '"]');

            $variationDiv.closest('.my-thumbnail').find('.btn-hide').attr("id", $variationDiv.find("input[type='hidden']").val());
            $variationDiv.closest('.my-thumbnail').find('.btn-hide').html("<strong>" + $variationDiv.find(".variant-price").html() + "</strong>");

            details.find('.product-variation').hide();
            if ($variationDiv.length > 0) {
                $variationDiv.show();
                details.find('.product-variation-notavailable').hide();
            } else {
                details.find('.product-variation-notavailable').show();
            }
        });

        $('.quantity-button-adult').on('click', function () {
            var quantityInput = $(this).closest("form").find('.quantity-field-adult');
            if ($(this).val() === '+')
            {
                quantityInput.val(parseInt(quantityInput.val(), 10) + 1);
            }
            else if (quantityInput.val() > 1) {
                quantityInput.val(quantityInput.val() - 1);
            }
        });

        $('.quantity-button-child').on('click', function () {
            var quantityInput = $(this).closest("form").find('.quantity-field-child');
            if ($(this).val() === '+') {
                quantityInput.val(parseInt(quantityInput.val(), 10) + 1);
            }
            else if (quantityInput.val() > 0) {
                quantityInput.val(quantityInput.val() - 1);
            }
        });

        $('.quantity-button-baby').on('click', function () {
            var quantityInput = $(this).closest("form").find('.quantity-field-baby');
            if ($(this).val() === '+') {
                quantityInput.val(parseInt(quantityInput.val(), 10) + 1);
            }
            else if (quantityInput.val() > 0) {
                quantityInput.val(quantityInput.val() - 1);
            }
        });
    });
})(jQuery);