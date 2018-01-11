﻿/*global jQuery, window*/
(function ($) {
    $(window).load(function () {
        $('.sp-wrap').smoothproducts();

        var departrueDateVal = $("#departure-date").val();
        if (departrueDateVal.length > 0) {

            var day, month, year, selected;
            var splitChar = departrueDateVal.includes('.') ? '.' : '/'

            day = splitChar == '.' ? departrueDateVal.split(splitChar)[0] : departrueDateVal.split(splitChar)[1];
            month = splitChar == '.' ? departrueDateVal.split(splitChar)[1] : departrueDateVal.split(splitChar)[0];
            year = departrueDateVal.split(splitChar)[2];
            
            selected = $('input[value="' +
                    parseInt(month, 10) + '/' +
                    parseInt(day, 10) + '/' +
                    parseInt(year, 10) + '"]');
            
            if (selected) {
                selected.prop('checked', true);
            }
        }

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
            variationName = selectedproductOptions.join('-');
            $variationDiv = $('div[data-variation-name="' + variationName +'"]');
            $('.product-variation').hide();
            if ($variationDiv.length > 0) {
                $variationDiv.show();
                $('.product-variation-notavailable').hide();
            } else {
                $('.product-variation-notavailable').show();
            }
        });

        $('.quantity-button').on('click', function () {
            var quantityInput = $(this).closest("form").find('.quantity-field');
            if ($(this).val() === '+')
            {
                quantityInput.val(parseInt(quantityInput.val(), 10) + 1);
            }
            else if (quantityInput.val() > 1) {
                quantityInput.val(quantityInput.val() - 1);
            }
        });

        $('#addreview').on('click', '#btn-addreview', function (e) {
            e.preventDefault();
            var $form = $('#form-addreview');
            if (!$form.valid || $form.valid()) {
                $.post($form.attr('action'), $form.serializeArray())
                    .done(function (result) {
                        $('#addreview').html(result);
                        $('input.rating-loading').rating({
                            language: window.simplGlobalSetting.lang,
                            filledStar: '<i class="fa fa-star"></i>',
                            emptyStar: '<i class="fa fa-star-o"></i>'
                        });
                    });
            }
        });
    });
})(jQuery);