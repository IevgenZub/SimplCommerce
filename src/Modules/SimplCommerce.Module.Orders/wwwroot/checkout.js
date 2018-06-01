$(function () {

    var departureDate = new Date($("#expiry").attr("data-departure-date"));
    $("#birth").attr("max", moment(departureDate).format("YYYY-MM-DD"));
    $("#birth").attr("min", "1900-01-01");
    $("#expiry").attr("min", moment(departureDate).format("YYYY-MM-DD"));

    var ruleDays = parseInt($("#expiry").attr("data-exp-rule"));
    if (ruleDays && ruleDays > 0) {
        departureDate.setDate(departureDate.getDate() + ruleDays);
        $("#expiry").attr("min", moment(departureDate).format("YYYY-MM-DD"));
    }

    $(".add-address-form").submit(function (event) {

        // Stop form from submitting normally
        event.preventDefault();
    
        // Get some values from elements on the page:
        var $form = $(this),
            firstName = $form.find("input[name='firstName']").val().toUpperCase(),
            lastName = $form.find("input[name='lastName']").val().toUpperCase(),
            documentNumber = $form.find("input[name='documentNumber']").val().toUpperCase(),
            documentExpiration = $form.find("input[name='documentExpiration']").val(),
            birthDate = $form.find("input[name='birthDate']").val(),
            sex = $form.find("input[name='sex']:checked").val(),
            phone = $form.find("input[name='phone']").val(),
            email = $form.find("input[name='email']").val(),
            countryId = $form.find("select[name='countryId']").val(),
            url = $form.attr("action");

        if (!countryId || countryId == "") {
            alert("Select Country");
            return;
        }

        if (!countryId || countryId == "") {
            alert("Select Country");
            return;
        }


        // Send the data using post
        var posting = $.post(url, {
            FirstName: firstName,
            LastName: lastName,
            DocumentNumber: documentNumber,
            BirthDate: birthDate,
            DocumentExpiration: documentExpiration,
            Sex: sex,
            Phone: phone,
            Email: email,
            CountryId: countryId
        });

        // Put the results in a div
        posting.done(function (data) {
            if (typeof data === 'string' || data instanceof String) {
                alert(data);
                return;
            }
            var nextIndex = $('.address-container > div').length;
            var newAddress = 
                $('.address-container').append('\
                    <div class="row" style="border-bottom: 1px solid #eee; padding:5px">\
                    <div class="col-sm-2 col-xs-12">\
                        <input type="hidden" data-val="true" data-val-required="The UserAddressId field is required." id="ExistingShippingAddresses_'+ nextIndex + '__UserAddressId" name="ExistingShippingAddresses[' + nextIndex + '].UserAddressId" value="' + data.id +'">\
                        <input type="checkbox" data-val="true" onclick="checkIfContinueEnabled()" class="registration-address-check" data-val-required="The Selected field is required." id="ExistingShippingAddresses_'+ nextIndex + '__Selected" name="ExistingShippingAddresses[' + nextIndex +'].Selected" checked="true" value="true">\
                    <label class="visible-xs hidden-label">First Name:&nbsp;</label>\
                    ' + firstName + '\
                    </div >\
                    <div class="col-sm-2 col-xs-12"><label class="visible-xs hidden-label">Last Name:&nbsp;</label>'+ lastName +'</div>\
                    <div class="col-sm-1 col-xs-12"><label class="visible-xs hidden-label">Birth Date:&nbsp;</label>'+ birthDate + '</div>\
                    <div class="col-sm-1 col-xs-12"><label class="visible-xs hidden-label">Document #:&nbsp;</label>'+ documentNumber + '</div>\
                    <div class="col-sm-1 col-xs-12"><label class="visible-xs hidden-label">Expiration:&nbsp;</label>'+ documentExpiration + '</div>\
                    <div class="col-sm-2 col-xs-12"><label class="visible-xs hidden-label">Email:&nbsp;</label>'+ email + '</div>\
                    <div class="col-sm-1 col-xs-12"><label class="visible-xs hidden-label">Phone:&nbsp;</label>'+ phone + '</div>\
                    <div class="col-sm-1 col-xs-12"><label class="visible-xs hidden-label">Country:&nbsp;</label>'+ $form.find("select[name='countryId'] option:selected").text() + '</div>\
                    <div class="col-sm-1 col-xs-12"><label class="visible-xs hidden-label">Sex:&nbsp;</label>'+ sex +'</div>\
               </div>');

            $form[0].reset();
            checkIfContinueEnabled();
        });
    });

    $("#search-txt").on("change paste keyup", function () {
        var filter = $(this).val().toLowerCase();
        if (filter.length > 0) {
            $(".passenger-row").hide();
            $("div[data-first-name*='" + filter + "']").show();
            $("div[data-last-name*='" + filter + "']").show();
            $("div[data-email*='" + filter + "']").show();
            $("div[data-document*='" + filter + "']").show();
        }
        else {
            $(".passenger-row").show();
        }
    });

    checkIfContinueEnabled();
});


function checkIfContinueEnabled() {
    var numberOfPassengers = $('#numberOfPassengers').val();
    var numberOfCheckedPassengers = $('input[class="registration-address-check"]:checked').length;
    var incorrectSelection = numberOfPassengers != numberOfCheckedPassengers;
    $('.btn-order').prop('disabled', incorrectSelection);
    $('#numberOfCheckedPassengers').text(numberOfCheckedPassengers + "/");
    $('#selectionInfo').toggleClass('alert-danger', incorrectSelection);

    if (incorrectSelection) {
        var numberExistingPassengers = $('input[class="registration-address-check"]').length;
        var diff = numberOfPassengers - numberExistingPassengers;

        if (diff > 0) {
            $('input[name=firstName]').focus();
            $('#numberOfPassengersToAdd').text(diff);
            $('#addPassangerAlert').show();
        }
        else
        {
            $('#addPassangerAlert').hide();
        }
    }
    else
    {
        $('.btn-order').focus();
        $('#addPassangerAlert').hide();
    }
}