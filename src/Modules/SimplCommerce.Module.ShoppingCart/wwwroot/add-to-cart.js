/*global $ */
$(function () {
    $('body').on('click', '.btn-add-cart', function () {
        var quantity, quantityChild, quantityBaby, productId, mergedProductId;
        
        productId = $(this).attr("id");
        mergedProductId = $(this).attr("data-merged-flight-id") == "null" ? 0 : $(this).attr("data-merged-flight-id");
        quantity = $("input[data-product-id='"+ productId + "'][class='quantity-field quantity-field-adult']").val(); 
        quantityChild = $("input[data-product-id='" + productId + "'][class='quantity-field quantity-field-child']").val();
        quantityBaby = $("input[data-product-id='" + productId + "'][class='quantity-field quantity-field-baby']").val();

        if (!quantity) {
            quantity = 1;
            quantityChild = 0;
            quantityBaby = 0;
        }
        
        $.ajax({
            type: 'POST',
            url: '/cart/addtocart',
            data: JSON.stringify({ productId: productId, mergedProductId: mergedProductId, quantity: quantity, quantityChild : quantityChild, quantityBaby: quantityBaby }),
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