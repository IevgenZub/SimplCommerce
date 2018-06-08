(function () {
    angular
        .module('simpl.shoppingCart', [])
        .controller('shoppingCartListCtrl', [
            '$scope',
            'shoppingCartService',
            function ($scope, shoppingCartService) {
                var vm = this;
                vm.cart = {};


                function cartDataCallback(result) {
                    vm.cart = result.data;
                    $('.cart-badge .badge').text(vm.cart.items.length);

                    recalculatePrices();
                }

                function rejectedCallback(result) {
                    alert("Can't order more than available");
                    location.reload();
                }


                function getShoppingCartItems() {
                    shoppingCartService.getShoppingCartItems().then(cartDataCallback);
                };

                vm.removeShoppingCartItem = function removeShoppingCartItem(item) {
                    shoppingCartService.removeShoppingCartItem(item.id).then(cartDataCallback);
                };

                vm.increaseQuantity = function increaseQuantity(item) {
                    item.quantity += 1;
                    shoppingCartService.updateQuantity(item.id, item.quantity, item.quantityChild, item.quantityBaby).then(cartDataCallback, rejectedCallback);
                };

                vm.decreaseQuantity = function decreaseQuantity(item) {
                    if (item.quantity <= 1) {
                        return;
                    }

                    item.quantity -= 1;
                    shoppingCartService.updateQuantity(item.id, item.quantity, item.quantityChild, item.quantityBaby).then(cartDataCallback);
                };

                vm.increaseQuantityChild = function increaseQuantityChild(item) {                    
                    item.quantityChild += 1;
                    shoppingCartService.updateQuantity(item.id, item.quantity, item.quantityChild, item.quantityBaby).then(cartDataCallback, rejectedCallback);
                };

                vm.decreaseQuantityChild = function decreaseQuantityChild(item) {
                    if (item.quantityChild == 0) {
                        return;
                    }

                    item.quantityChild -= 1;
                    shoppingCartService.updateQuantity(item.id, item.quantity, item.quantityChild, item.quantityBaby).then(cartDataCallback);
                };

                vm.increaseQuantityBaby = function increaseQuantityBaby(item) {
                    item.quantityBaby += 1;
                    shoppingCartService.updateQuantity(item.id, item.quantity, item.quantityChild, item.quantityBaby).then(cartDataCallback, rejectedCallback);
                };

                vm.decreaseQuantityBaby = function decreaseQuantityBaby(item) {
                    if (item.quantityBaby == 0) {
                        return;
                    }

                    item.quantityBaby -= 1;
                    shoppingCartService.updateQuantity(item.id, item.quantity, item.quantityChild, item.quantityBaby).then(cartDataCallback);
                };

                vm.applyCoupon = function applyCoupon() {
                    vm.couponErrorMessage = '';
                    shoppingCartService.applyCoupon(vm.couponCode).then(function (result) {
                        if (result.data.succeeded == false) {
                            vm.couponErrorMessage = result.data.errorMessage;
                        } else {
                            cartDataCallback(result);
                        }
                    });
                };

                vm.applyFee = function applyFee() {
                    vm.couponErrorMessage = '';
                    shoppingCartService.applyFee(vm.feeAmount).then(function (result) {
                        getShoppingCartItems();
                    });
                };

                getShoppingCartItems();
            }
        ]);
})();