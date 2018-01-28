$(function () {

    $(".add-address-form").submit(function (event) {

        // Stop form from submitting normally
        event.preventDefault();

        // Get some values from elements on the page:
        var $form = $(this),
            firstName = $form.find("input[name='firstName']").val(),
            lastName = $form.find("input[name='lastName']").val(),
            documentNumber = $form.find("input[name='documentNumber']").val(),
            documentExpiration = $form.find("input[name='documentExpiration']").val(),
            birthDate = $form.find("input[name='birthDate']").val(),
            sex = $form.find("input[name='sex']").val(),
            url = $form.attr("action");

        // Send the data using post
        var posting = $.post(url, {
            FirstName: firstName,
            LastName: lastName,
            DocumentNumber: documentNumber,
            BirthDate: birthDate,
            DocumentExpiration: documentExpiration,
            Sex: sex
        });

        // Put the results in a div
        posting.done(function (data) {
            var nextIndex = $('.address-container > div').length;
            var newAddress = 
                $('.address-container').append('\
                    <div class="row">\
                    <div class="col-sm-2">\
                        <input type="hidden" data-val="true" data-val-required="The UserAddressId field is required." id="ExistingShippingAddresses_'+ nextIndex + '__UserAddressId" name="ExistingShippingAddresses[' + nextIndex + '].UserAddressId" value="' + data.id +'">\
                        <input type="checkbox" data-val="true" data-val-required="The Selected field is required." id="ExistingShippingAddresses_'+ nextIndex + '__Selected" name="ExistingShippingAddresses[' + nextIndex +'].Selected" value="true">\
                    ' + firstName + '\
                    </div >\
                    <div class="col-sm-2">'+ lastName +'</div>\
                    <div class="col-sm-2">'+ documentNumber + '</div>\
                    <div class="col-sm-2">'+ documentExpiration + '</div>\
                    <div class="col-sm-2">'+ birthDate + '</div>\
                    <div class="col-sm-2">'+ sex +'</div>\
               </div>');
        });
    });

    function toggleCreateShippingAddress() {
        var shippingAddressId = $('input[name=shippingAddressId]:checked').val(),
            $createShippingAddress = $('.create-shipping-address');

        if (shippingAddressId === "0") {
            $createShippingAddress.show();
        } else {
            $createShippingAddress.hide();
        }
    }

    function updateShippingInfo() {
        if ($('input[name=shippingAddressId]:checked').val() === "0" && !$('#NewAddressForm_StateOrProvinceId').val()) {
            return;
        }
        var postData = {
            existingShippingAddressId: $('input[name=shippingAddressId]:checked').val(),
            selectedShippingMethodName: $('input[name=shippingMethod]:checked').val(),
            newShippingAddress: {
                countryId: $('#NewAddressForm_CountryId').val() || 0,
                stateOrProvinceId: $('#NewAddressForm_StateOrProvinceId').val() || 0,
            }
        };

        $.ajax({
            type: "POST",
            url: "/checkout/update-tax-and-shipping-prices",
            data: JSON.stringify(postData),
            contentType: "application/json",
            success: function (data) {
                var $shippingMethods = $('#shippingMethods');
                $shippingMethods.empty();
                if (data.shippingPrices.length > 0) {
                    $.each(data.shippingPrices, function (index, value) {
                        $shippingMethods.append('<div class="radio"> \
                        <label> \
                        <input type="radio" name="shippingMethod" data-price ="'+ value.priceText + '" value="' + value.name + '"> \
                            <strong> ' + value.name + ' (' + value.priceText + ')</strong> \
                        </label> \
                       </div>')
                    });
                    $('.btn-order').prop('disabled', false);
                } else {
                    //$shippingMethods.append("Sorry, this items can't be shipped to your selected address");
                    $('.btn-order').prop('disabled', false);
                }

                $('#orderSummaryTax').text(data.cart.taxAmountString);
                $('#orderTotal').text(data.cart.orderTotalString);
                $('#orderSummaryShipping').text(data.cart.shippingAmountString);

                $shippingMethods.find('input[value="' + data.selectedShippingMethodName + '"]').prop('checked', true);
            }
        });
    }

    $('input[name=shippingAddressId]').on('change', function () {
        toggleCreateShippingAddress();
    });

    $(document).on('change', 'input[name=shippingAddressId], #NewAddressForm_StateOrProvinceId, #shippingMethods input:radio', function () {
        updateShippingInfo();
    });


    function resetSelect($select) {
        var $defaultOption = $select.find("option:first-child");
        $select.empty();
        $select.append($defaultOption);
    }

    $('#NewAddressForm_CountryId').on('change', function () {
        var countryId = this.value;

        $.getJSON('/api/countries/' + countryId + '/states-provinces', function (data) {
            var $stateOrProvinceSelect = $("#NewAddressForm_StateOrProvinceId");
            resetSelect($stateOrProvinceSelect);

            var $districtSelect = $("#NewAddressForm_DistrictId");
            resetSelect($districtSelect);

            $.each(data, function (index, option) {
                $stateOrProvinceSelect.append($("<option></option>").attr("value", option.id).text(option.name));
            });
        });
    });

    $('#NewAddressForm_StateOrProvinceId').on('change', function () {
        var selectedStateOrProvinceId = this.value;

        $.getJSON("/Location/GetDistricts/" + selectedStateOrProvinceId, function (data) {
            var $districtSelect = $("#NewAddressForm_DistrictId");
            resetSelect($districtSelect);

            $.each(data, function (index, option) {
                $districtSelect.append($("<option></option>").attr("value", option.value).text(option.text));
            });
        });
    });

    toggleCreateShippingAddress();
    updateShippingInfo();
});