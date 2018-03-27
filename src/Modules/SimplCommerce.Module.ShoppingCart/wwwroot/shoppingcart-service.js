(function() {
    angular
        .module('simpl.shoppingCart')
        .factory('shoppingCartService', [
            '$http',
            function ($http) {
                function getShoppingCartItems() {
                    return $http.get('Cart/List');
                }
                
                function removeShoppingCartItem(itemId) {
                    return $http.post('Cart/Remove', itemId);
                }

                function updateQuantity(itemId, quantity, quantityChild, quantityBaby) {
                    return $http.post('Cart/UpdateQuantity', {
                        cartItemId: itemId,
                        quantity: quantity,
                        quantityChild: quantityChild,
                        quantityBaby: quantityBaby
                    });
                }

                function applyCoupon(couponCode) {
                    return $http.post('Cart/ApplyCoupon', { couponCode: couponCode });
                }

                function applyFee(feeAmount) {
                    return $http.post('Cart/ApplyFee', { feeAmount: feeAmount });
                }

                return {
                    getShoppingCartItems: getShoppingCartItems,
                    removeShoppingCartItem: removeShoppingCartItem,
                    updateQuantity: updateQuantity,
                    applyCoupon: applyCoupon,
                    applyFee: applyFee
                };
            }
        ]);
})();