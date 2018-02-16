(function () {
    angular
        .module('simpl.shoppingCart', [])
        .controller('shoppingCartListCtrl', [
            '$scope',
            'shoppingCartService',
            function ($scope, shoppingCartService) {
                var vm = this;
                vm.cart = {};
                // TODO: Apply real model
                vm.passengers = [{
                    sex: "Male",
                    firstName: "Alex",
                    lastName: "Prokofiev",
                    dateOfBirth: new Date("02.03.1975"),
                    documentNo: "EN 758489",
                    dateOfExpiry: new Date("05.07.2030")
                }, {
                    sex: "Male",
                    firstName: "Alex",
                    lastName: "Prokofiev",
                    dateOfBirth: new Date("02.03.1975"),
                    documentNo: "EN 758489",
                    dateOfExpiry: new Date("05.07.2030")
                }];

                function cartDataCallback(result) {
                    vm.cart = result.data;
                    $('.cart-badge .badge').text(vm.cart.items.length);
                }

                function getShoppingCartItems() {
                    shoppingCartService.getShoppingCartItems().then(cartDataCallback);
                };

                vm.removeShoppingCartItem = function removeShoppingCartItem(item) {
                    shoppingCartService.removeShoppingCartItem(item.id).then(cartDataCallback);
                };

                vm.increaseQuantity = function increaseQuantity(item) {
                    item.quantity += 1;
                    shoppingCartService.updateQuantity(item.id, item.quantity, item.quantityChild, item.quantityBaby).then(cartDataCallback);
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
                    shoppingCartService.updateQuantity(item.id, item.quantity, item.quantityChild, item.quantityBaby).then(cartDataCallback);
                };

                vm.decreaseQuantityChild = function decreaseQuantityChild(item) {
                    if (item.quantityChild <= 1) {
                        return;
                    }
                    item.quantityChild -= 1;
                    shoppingCartService.updateQuantity(item.id, item.quantity, item.quantityChild, item.quantityBaby).then(cartDataCallback);
                };

                vm.increaseQuantityBaby = function increaseQuantityBaby(item) {
                    item.quantityBaby += 1;
                    shoppingCartService.updateQuantity(item.id, item.quantity, item.quantityChild, item.quantityBaby).then(cartDataCallback);
                };

                vm.decreaseQuantityBaby = function decreaseQuantityBaby(item) {
                    if (item.quantityBaby <= 1) {
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

                 getShoppingCartItems();
            }
        ]);
})();