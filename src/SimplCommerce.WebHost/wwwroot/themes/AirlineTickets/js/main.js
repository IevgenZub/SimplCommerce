$(document).ready(function() {
    $('#popup-opener').on('click', function() {
        $('.row-passengers').toggle();
    });
    $('.submit-icon').on('click', function() {
        $('.row-passengers').fadeOut(function() {
            $(this).hide();
        });
    });
    $('.btn-one-way').on('click', function() {
        $('#return-date').attr('disabled', 'disabled');
    });
    $('.btn-cift').on('click', function() {
        $('#return-date').removeAttr('disabled')
    });

    $('.spinner-input, #flight-class').change(function() {
        var ids = ['adult', 'child', 'baby'];
        var totalCount = ids.reduce((prev, id) => parseInt($(`#${id}-passenger`).val()) + prev, 0);
        var fc = $('#flight-class option:selected').text();

        $('#number-of-people').val(totalCount + ' - ' + fc);
        $('#adultPassengers').val($("#adult-passenger").val());
        $('#childPassengers').val($("#child-passenger").val());
        $('#babyPassengers').val($("#baby-passenger").val());
    });
});
