/*global $ */
$(function () {
    $('body').on('click', '.btn-add-cart', function () {
        var quantity, quantityChild, quantityBaby, productId

        if ($(this).hasClass('btn-hide')) {
            productId = $(this).attr("id");
            quantity = 1; 
            quantityChild = 0;
            quantityBaby = 0;
        }
        else {
            var $form = $(this).closest("form"),
            productId = $(this).closest("form").find('input[name=productId]').val(),
            $quantityAdultInput = $form.find('.quantity-field-adult');
            $quantityChildInput = $form.find('.quantity-field-child');
            $quantityBabyInput = $form.find('.quantity-field-baby');
            quantity = $quantityAdultInput.length === 1 ? $quantityAdultInput.val() : 1;
            quantityChild = $quantityChildInput.length === 1 ? $quantityChildInput.val() : 0;
            quantityBaby = $quantityBabyInput.length === 1 ? $quantityBabyInput.val() : 0;
        }
            
        $.ajax({
            type: 'POST',
            url: '/cart/addtocart',
            data: JSON.stringify({ productId: productId, quantity: quantity, quantityChild : quantityChild, quantityBaby: quantityBaby }),
            contentType: "application/json"
        }).done(function (data) {
            //$('#shopModal').find('.modal-content').html(data);
            //$('#shopModal').modal('show');
            //$('.cart-badge .badge').text($('#shopModal').find('.cart-item-count').text());
            window.location = "cart";
        }).fail(function () {
            /*jshint multistr: true */
            $('#shopModal').find('.modal-content').html(' \
                <div class="modal-header"> \
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button> \
                    <h4 class="modal-title" id="myModalLabel">Opps</h4> \
                </div> \
                <div class="modal-body"> \
                    Something went wrong. \
                </div>');
            $('#shopModal').modal('show');
        });
    });
});