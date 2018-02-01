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
            sex = $form.find("input[name='sex']:checked").val(),
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
                    <div class="row" style="border-bottom: 1px solid #eee; padding:5px">\
                    <div class="col-sm-2">\
                        <input type="hidden" data-val="true" data-val-required="The UserAddressId field is required." id="ExistingShippingAddresses_'+ nextIndex + '__UserAddressId" name="ExistingShippingAddresses[' + nextIndex + '].UserAddressId" value="' + data.id +'">\
                        <input type="checkbox" data-val="true" onclick="checkIfContinueEnabled()" class="registration-address-check" data-val-required="The Selected field is required." id="ExistingShippingAddresses_'+ nextIndex + '__Selected" name="ExistingShippingAddresses[' + nextIndex +'].Selected" checked="true" value="true">\
                    ' + firstName + '\
                    </div >\
                    <div class="col-sm-2">'+ lastName +'</div>\
                    <div class="col-sm-2">'+ documentNumber + '</div>\
                    <div class="col-sm-2">'+ documentExpiration + '</div>\
                    <div class="col-sm-2">'+ birthDate + '</div>\
                    <div class="col-sm-2">'+ sex +'</div>\
               </div>');

            checkIfContinueEnabled();
        });
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
    }
}