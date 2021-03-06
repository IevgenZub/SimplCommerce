﻿/*global angular, jQuery*/
(function ($) {
    angular
        .module('simplAdmin.catalog')
        .controller('ProductFormCtrl', ProductFormCtrl);

    /* @ngInject */
    function ProductFormCtrl($state, $timeout, $stateParams, $http, categoryService, productService, summerNoteService, brandService, translateService, userService) {
        var vm = this;
        vm.translate = translateService;
        // declare shoreDescription and description for summernote
        vm.product = { departure: '', destination: '', specification: '', isPublished: true, price: 0, isCallForPricing: false, isAllowToOrder: true };
        vm.product.categoryIds = [];
        vm.product.options = [];
        vm.product.variations = [];
        vm.product.attributes = [];
        vm.product.relatedProducts = [];
        vm.product.crossSellProducts = [];
        vm.categories = [];
        vm.thumbnailImage = null;
        vm.productImages = [];
        vm.productDocuments = [];
        vm.options = [];
        vm.productTemplates = [];
        vm.addingOption = null;
        vm.attributes = [];
        vm.addingAttribute = null;
        vm.productId = $stateParams.id;
        vm.isEditMode = vm.productId > 0;
        vm.addingVariation = { price: 0 };
        vm.brands = [];
        vm.taxClasses = [];
        vm.vendors = [];
        vm.userIsAdmin = false;       

        vm.datePickerSpecialPriceStart = {};
        
        vm.datePickerSpecialPriceEnd = {};

        vm.datePickerDepartureDate = {};
        
        vm.datePickerReturnDepartureDate = {};
        
        vm.updateSlug = function () {
            vm.product.slug = slugify(vm.product.flightNumber);
            vm.product.name = vm.product.flightNumber
        };

        vm.openCalendar = function (e, picker) {
            vm[picker].open = true;
        };

        vm.shortDescUpload = function (files) {
            summerNoteService.upload(files[0])
                .then(function (response) {
                    $(vm.shortDescEditor).summernote('insertImage', response.data);
                });
        };

        vm.descUpload = function (files) {
            summerNoteService.upload(files[0])
                .then(function (response) {
                    $(vm.descEditor).summernote('insertImage', response.data);
                });
        };

        vm.specUpload = function (files) {
            summerNoteService.upload(files[0])
                .then(function (response) {
                    $(vm.specEditor).summernote('insertImage', response.data);
                });
        };

        vm.addOption = function addOption() {
            onModifyOption(function() {
                vm.addingOption.values = [];
                vm.addingOption.displayType = "text";

                vm.addingOption.type = "text";
                if (vm.addingOption.name.toLowerCase().includes('date')) {
                    vm.addingOption.type =  "date";
                }
                if (vm.addingOption.name.toLowerCase().includes('number') || vm.addingOption.name.toLowerCase().includes('days')) {
                    vm.addingOption.type = "number";
                }
                if (vm.addingOption.name.toLowerCase().includes('time')) {
                    vm.addingOption.type = "time";
                }

                var index = vm.options.indexOf(vm.addingOption);
                vm.product.options.push(vm.addingOption);
                vm.options.splice(index, 1);
                vm.addingOption = null;
            });
        };

        vm.deleteOption = function deleteOption(option) {
            onModifyOption(function() {
                var index = vm.product.options.indexOf(option);
                vm.product.options.splice(index, 1);
                vm.options.push(option);
            });
        };

        function onModifyOption(callback) {
            if (vm.product.variations.length === 0) {
                callback();
                return;
            }

            bootbox.confirm('Add or remove option will clear all existing variations. Are you sure you want to do this?', function (result) {
                if (result) {
                    $timeout(function () {
                        vm.product.variations = [];
                        callback();
                    }, 1);
                }
            });
        };

        
        vm.generateDepartureOptions = function () {
            function generateRange(startDate, endDate) {
                var retVal = [];
                var current = new Date(startDate);
                var end = new Date(endDate);

                while (current <= end) {
                    if ($('#' + (current.getDay())).prop('checked')) {
                        retVal.push(new Date(current));
                    }
                    current.setDate(current.getDate() + 1);
                }

                return retVal;
            };

            var departureDateOption = vm.product.options.filter(function (o) { return o.name === "Departure Date" })[0];
            if (!departureDateOption) {
                departureDateOption = vm.options.filter(function (o) { return o.name === "Departure Date" })[0];
                vm.addingOption = departureDateOption;
                vm.addOption();
            }

            var range = generateRange($('#chipStart').val(), $('#chipEnd').val());

            angular.forEach(range, function (chipToAdd) {
                departureDateOption.values.push({ key: chipToAdd.toLocaleDateString("ru-RU"), value: '' });
            });
        };

        vm.newOptionValue = function (chip) {   
            var dateString = angular.copy(chip);
            dateString = dateString.toString();
            if (dateString.split('-').length == 3) {
                chip = moment(dateString, 'DD.MM.YYYY').toDate().toLocaleDateString("ru-RU");
            }

            return {
                key: chip,
                value: ''
           };
        };

        vm.generateOptionCombination = function generateOptionCombination() {
            var maxIndexOption = vm.product.options.length - 1;
            vm.product.variations = [];

            function getItemValue(item) {
                return item.value;
            }

            function helper(arr, optionIndex) {
                var j, l, variation, optionCombinations, optionValue;

                for (j = 0, l = vm.product.options[optionIndex].values.length; j < l; j = j + 1) {
                    optionCombinations = arr.slice(0);
                    optionValue = {
                        optionName: vm.product.options[optionIndex].name,
                        optionId: vm.product.options[optionIndex].id,
                        value: vm.product.options[optionIndex].values[j].key,
                        sortIndex : optionIndex
                    };
                    optionCombinations.push(optionValue);

                    if (optionIndex === maxIndexOption) {
                        variation = {
                            name: vm.product.name + ' ' + optionCombinations.map(getItemValue).join(' '),
                            normalizedName : optionCombinations.map(getItemValue).join('-'),
                            optionCombinations: optionCombinations,
                            price: vm.product.price,
                            oldPrice: vm.product.oldPrice
                        };
                        vm.product.variations.push(variation);
                    } else {
                        helper(optionCombinations, optionIndex + 1);
                    }
                }
            }

            helper([], 0);
        };

        vm.deleteVariation = function deleteVariation(variation) {
            var index = vm.product.variations.indexOf(variation);
            vm.product.variations.splice(index, 1);
        };

        vm.removeImage = function removeImage(media) {
            var index = vm.product.productImages.indexOf(media);
            vm.product.productImages.splice(index, 1);
            vm.product.deletedMediaIds.push(media.id);
        };

        vm.removeDocument = function removeDocument(media) {
            var index = vm.product.productDocuments.indexOf(media);
            vm.product.productDocuments.splice(index, 1);
            vm.product.deletedMediaIds.push(media.id);
        };

        vm.isAddVariationFormValid = function () {
            var i;
            if (isNaN(vm.addingVariation.price) || vm.addingVariation.price === '') {
                return false;
            }

            for (i = 0; i < vm.product.options.length; i = i + 1) {
                if (!vm.addingVariation[vm.product.options[i].name]) {
                    return false;
                }
            }

            return true;
        };

        vm.addVariation = function addVariation() {
            var variation,
                optionCombinations = [];

            vm.product.options.forEach(function (option, index) {
                var optionValue = {
                    optionName: option.name,
                    optionId: option.id,
                    value: vm.addingVariation[option.name],
                    sortIndex: index
                };
                optionCombinations.push(optionValue);
            });

            variation = {
                name: vm.product.name + ' ' + optionCombinations.map(function (item) {
                    return item.value;
                }).join(' '),
                normalizedName : optionCombinations.map(function (item) {
                    return item.value;
                }).join('-'),
                optionCombinations: optionCombinations,
                price: vm.addingVariation.price || vm.product.price,
                oldPrice: vm.addingVariation.oldPrice || vm.pr.oldPrice

            };

            if (!vm.product.variations.find(function (item) { return item.name === variation.name; })) {
                vm.product.variations.push(variation);
                vm.addingVariation = { price: vm.product.price };
            } else {
                toastr.error('The ' + variation.name + ' has been existing');
            }
        };

        // TODO look for a more concise way
        vm.applyTemplate = function applyTemplate() {
            var template, i, index, workingAttr,
                nonTemplateAttrs = [];

            productService.getProductTemplate(vm.product.template.id).then(function (response) {
                template = response.data;

                for (i = 0; i < template.attributes.length; i = i + 1) {
                    workingAttr = vm.product.attributes.find(function (item) { return item && item.id === template.attributes[i].id; });
                    if (workingAttr) {
                        continue;
                    }
                    workingAttr = vm.attributes.find(function (item) { return item && item.id === template.attributes[i].id; });
                    index = vm.attributes.indexOf(workingAttr);
                    vm.attributes.splice(index, 1);
                    vm.product.attributes.push(workingAttr);
                }

                for (i = 0; i < vm.product.attributes.length; i = i + 1) {
                    workingAttr = template.attributes.find(function (item) { return item && item.id === vm.product.attributes[i].id; });
                    if (!workingAttr) {
                        nonTemplateAttrs.push(vm.product.attributes[i]);
                    }
                }

                for (i = 0; i < nonTemplateAttrs.length; i = i + 1) {
                    workingAttr = vm.product.attributes.find(function (item) { return item && item.id === nonTemplateAttrs[i].id; });
                    index = vm.product.attributes.indexOf(workingAttr);
                    vm.product.attributes.splice(index, 1);
                    vm.attributes.push(workingAttr);
                }
            });
        };

        vm.addAttribute = function addAttribute() {
            var index = vm.attributes.indexOf(vm.addingAttribute);
            vm.product.attributes.push(vm.addingAttribute);
            vm.attributes.splice(index, 1);
            vm.addingAttribute = null;
        };

        vm.deleteAttribute = function deleteAttribute(attribute) {
            var index = vm.product.attributes.indexOf(attribute);
            vm.product.attributes.splice(index, 1);
            vm.attributes.push(attribute);
        };

        vm.toggleCategories = function toggleCategories(categoryId) {
            var index = vm.product.categoryIds.indexOf(categoryId);
            if (index > -1) {
                vm.product.categoryIds.splice(index, 1);
                var childCategoryIds = getChildCategoryIds(categoryId);
                childCategoryIds.forEach(function spliceChildCategory(childCategoryId) {
                    index = vm.product.categoryIds.indexOf(childCategoryId);
                    if (index > -1) {
                        vm.product.categoryIds.splice(index, 1);
                    }
                });
            } else {
                vm.product.categoryIds.push(categoryId);
                var category = vm.categories.find(function (item) { return item.id === categoryId; });
                if (category) {
                    var parentCategoryIds = getParentCategoryIds(category.parentId);
                    parentCategoryIds.forEach(function pushParentCategory(parentCategoryId) {
                        if (vm.product.categoryIds.indexOf(parentCategoryId) < 0) {
                            vm.product.categoryIds.push(parentCategoryId);
                        }
                    });
                }
            }
        };

        vm.filterAddedOptionValue = function filterAddedOptionValue(item) {
            if (vm.product.options.length > 1) {
                return true;
            }
            var optionValueAdded = false;
            vm.product.variations.forEach(function (variation) {
                var optionValues = variation.optionCombinations.map(function (item) {
                    return item.value;
                });
                if (optionValues.indexOf(item) > -1) {
                    optionValueAdded = true;
                }
            });

            return !optionValueAdded;
        };

        vm.save = function save() {
            var promise;

            if (!$('#productForm')[0].checkValidity()) {
                return;
            }

            // ng-upload will post null as text
            vm.product.taxClassId = vm.product.taxClassId === null ? '' : vm.product.taxClassId;
            vm.product.brandId = vm.product.brandId === null ? '' : vm.product.brandId;
            vm.product.oldPrice = vm.product.oldPrice === null ? '' : vm.product.oldPrice;
            vm.product.specialPrice = vm.product.specialPrice === null ? '' : vm.product.specialPrice;
            vm.product.specialPriceStart = vm.product.specialPriceStart === null ? '' : vm.product.specialPriceStart;
            vm.product.specialPriceEnd = vm.product.specialPriceEnd === null ? '' : vm.product.specialPriceEnd;
            vm.product.returnDepartureDate = vm.product.returnDepartureDate == null ? '' : new Date(vm.product.returnDepartureDate.valueOf() - vm.product.returnDepartureDate.getTimezoneOffset() * 60000);
            vm.product.departureDate = vm.product.departureDate == null ? '' : new Date(vm.product.departureDate.valueOf() - vm.product.departureDate.getTimezoneOffset() * 60000);
            vm.product.landingTime = vm.product.landingTime == null ? '' : new Date(vm.product.landingTime.valueOf() - vm.product.landingTime.getTimezoneOffset() * 60000);
            vm.product.returnLandingTime = vm.product.returnLandingTime == null ? '' : new Date(vm.product.returnLandingTime.valueOf() - vm.product.returnLandingTime.getTimezoneOffset() * 60000);
            vm.product.returnAircraftId = vm.product.returnAircraftId === null ? '' : vm.product.returnAircraftId;
            vm.product.returnCarrierId = vm.product.returnCarrierId === null ? '' : vm.product.returnCarrierId;
            vm.product.isRoundTrip = vm.product.isRoundTrip === null ? '' : vm.product.isRoundTrip;
            vm.product.saleRtOnly = vm.product.saleRtOnly === null ? '' : vm.product.saleRtOnly;
            vm.product.adminPayLater = vm.product.adminPayLater === null ? '' : vm.product.adminPayLater;
            vm.product.adminPayLaterRule = vm.product.adminPayLaterRule === null ? '' : vm.product.adminPayLaterRule;
            vm.product.adminRoundTrip = vm.product.adminRoundTrip === null ? '' : vm.product.adminRoundTrip;
            vm.product.adminIsLastMinute = vm.product.adminIsLastMinute === null ? '' : vm.product.adminIsLastMinute;
            vm.product.adminIsSpecialOffer = vm.product.adminIsSpecialOffer === null ? '' : vm.product.adminIsSpecialOffer;
            vm.product.adminNotifyAgencies = vm.product.adminNotifyAgencies === null ? '' : vm.product.adminNotifyAgencies;
            vm.product.adminPasExpirityRule = vm.product.adminPasExpirityRule === null ? '' : vm.product.adminPasExpirityRule;
            vm.product.adminReturnIsLastMinute = vm.product.adminReturnIsLastMinute === null ? '' : vm.product.adminReturnIsLastMinute;
            vm.product.adminNotifyLastPassanger = vm.product.adminNotifyLastPassanger === null ? '' : vm.product.adminNotifyLastPassanger;
            vm.product.adminRoundTripOperatorId = vm.product.adminRoundTripOperatorId === null ? '' : vm.product.adminRoundTripOperatorId;
            vm.product.adminReturnIsSpecialOffer = vm.product.adminReturnIsSpecialOffer === null ? '' : vm.product.adminReturnIsSpecialOffer;
            vm.product.adminReturnNotifyAgencies = vm.product.adminReturnNotifyAgencies === null ? '' : vm.product.adminReturnNotifyAgencies;
            vm.product.adminReturnPasExpirityRule = vm.product.adminReturnPasExpirityRule === null ? '' : vm.product.adminReturnPasExpirityRule;
            vm.product.adminReturnNotifyLastPassanger = vm.product.adminReturnNotifyLastPassanger === null ? '' : vm.product.adminReturnNotifyLastPassanger;
            vm.product.adminReturnPayLater = vm.product.adminReturnPayLater === null ? '' : vm.product.adminReturnPayLater;
            vm.product.adminReturnPayLaterRule = vm.product.adminReturnPayLaterRule === null ? '' : vm.product.adminReturnPayLaterRule;
            vm.product.reservationNumber = vm.product.reservationNumber === null ? '' : vm.product.reservationNumber;
            vm.product.adminBlackList = vm.product.adminBlackList === null ? '' : vm.product.adminBlackList;
            vm.product.adminReturnBlackList = vm.product.adminReturnBlackList === null ? '' : vm.product.adminReturnBlackList;
            vm.product.via = vm.product.via === null ? '' : vm.product.via;
            vm.product.returnVia = vm.product.returnVia === null ? '' : vm.product.returnVia;
            vm.product.vendorId = vm.product.vendorId === null ? '' : vm.product.vendorId;
            vm.product.soldSeats = vm.product.soldSeats === null ? '' : vm.product.soldSeats;
            vm.product.flightClass = vm.product.flightClass === null ? '' : vm.product.flightClass;

            vm.product.variations.forEach(function (item) {
                item.oldPrice = item.oldPrice === null ? '' : item.oldPrice;
            });

            if (vm.isEditMode) {
                promise = productService.editProduct(vm.product, vm.thumbnailImage, vm.productImages, vm.productDocuments);
            } else {
                promise = productService.createProduct(vm.product, vm.thumbnailImage, vm.productImages, vm.productDocuments);
            }

            promise.then(function (result) {
                    $state.go('product');
                })
                .catch(function (response) {
                    var error = response.data;
                    vm.validationErrors = [];
                    if (error && angular.isObject(error)) {
                        for (var key in error) {
                            vm.validationErrors.push(error[key][0]);
                        }
                    } else {
                        vm.validationErrors.push('Could not add product.');
                    }
                });
        };

        function getProduct() {
            productService.getProduct($stateParams.id).then(function (result) {
                var i, index, optionIds, attributeIds;
                vm.product = result.data;
                optionIds = vm.options.map(function (item) { return item.id; });
                for (i = 0; i < vm.product.options.length; i = i + 1) {
                    index = optionIds.indexOf(vm.product.options[i].id);
                    optionIds.splice(index, 1);
                    vm.options.splice(index, 1);
                }

                attributeIds = vm.attributes.map(function (item) { return item.id; });
                for (i = 0; i < vm.product.attributes.length; i = i + 1) {
                    index = attributeIds.indexOf(vm.product.attributes[i].id);
                    attributeIds.splice(index, 1);
                    vm.attributes.splice(index, 1);
                }
                
                if (vm.product.specialPriceStart) {
                    vm.product.specialPriceStart = new Date(vm.product.specialPriceStart);
                }
                if (vm.product.specialPriceEnd) {
                    vm.product.specialPriceEnd = new Date(vm.product.specialPriceEnd);
                }

                if (vm.product.returnDepartureDate) {
                    vm.product.returnDepartureDate = getUtcDate(new Date(vm.product.returnDepartureDate));
                }

                if (vm.product.departureDate) {
                    vm.product.departureDate = getUtcDate(new Date(vm.product.departureDate));
                }

                if (vm.product.landingTime) {
                    vm.product.landingTime = getUtcDate(new Date(vm.product.landingTime));
                }

                if (vm.product.returnLandingTime) {
                    vm.product.returnLandingTime = getUtcDate(new Date(vm.product.returnLandingTime));
                }
            });
        }

        function getUtcDate(date) {
            return new Date(
                date.getUTCFullYear(),
                date.getUTCMonth(),
                date.getUTCDate(),
                date.getUTCHours(),
                date.getUTCMinutes(),
                date.getUTCSeconds());
        }


        function getCategories() {
            categoryService.getCategories().then(function (result) {
                vm.categories = result.data;

                var options = {
                    url: "themes/AirlineTickets/data/airports_utf.json",
                    contentType: "application/json; charset=utf-8",
                    getValue: function (element) {
                        return element.city_eng + ", " + element.name_eng + " (" + element.iata_code + "), " + element.country_eng;
                   },

                    list: {
                        match: {
                            enabled: true
                        },
                        showAnimation: {
                            type: "fade", //normal|slide|fade
                            time: 400,
                            callback: function () { }
                        },
                        hideAnimation: {
                            type: "slide", //normal|slide|fade
                            time: 400,
                            callback: function () { }
                        }
                    }
                };

                $("#flightFrom, #flightTo").easyAutocomplete(options);

                var optionsRus = {
                    url: "themes/AirlineTickets/data/airports_utf.json",
                    contentType: "application/json; charset=utf-8",
                    getValue: function (element) {
                        var name = element.name_rus == "" ? element.name_eng : element.name_rus;
                        return element.city_rus + ", " + name + " (" + element.iata_code + "), " + element.country_rus;
                    },

                    list: {
                        match: {
                            enabled: true
                        },
                        showAnimation: {
                            type: "fade", //normal|slide|fade
                            time: 400,
                            callback: function () { }
                        },
                        hideAnimation: {
                            type: "slide", //normal|slide|fade
                            time: 400,
                            callback: function () { }
                        }
                    }
                };

                $("#flightFromRus, #flightToRus").easyAutocomplete(optionsRus);
            });
        }

        function getProductOptions() {
            productService.getProductOptions().then(function (result) {
                vm.options = result.data;
            });
        }

        function getProductTemplates() {
            productService.getProductTemplates().then(function (result) {
                vm.productTemplates = result.data;
            });
        }

        function getAttributes() {
            productService.getProductAttrs().then(function (result) {
                vm.attributes = result.data;
            });
        }

        function getBrands() {
            brandService.getBrands().then(function (result) {
                vm.brands = result.data;
            });
        }

        function getTaxClasses() {
            productService.getTaxClasses().then(function (result) {
                vm.taxClasses = result.data;
            });
        }

        function getVendors() {
            userService.getVendors().then(function (result) {
                vm.vendors = result.data;
            });
        }

        function init() {

            vm.userIsAdmin = window.userIsAdmin;

            if (vm.isEditMode) {
                getProduct();
            }
            getVendors();
            getProductOptions();
            getProductTemplates();
            getAttributes();
            getCategories();
            getBrands();
            getTaxClasses();
            
        }

        function getParentCategoryIds(categoryId) {
            if (!categoryId) {
                return [];
            }
            var category = vm.categories.find(function (item) { return item.id === categoryId; });

            return category ? [category.id].concat(getParentCategoryIds(category.parentId)) : []; 
        }

        function getChildCategoryIds(categoryId) {
            if (!categoryId) {
                return [];
            }
            var result = [];
            var queue = [];
            queue.push(categoryId);
            while (queue.length > 0) {
                var current = queue.shift();
                result.push(current);
                var childCategories = vm.categories.filter(function (item) { return item.parentId === current; });
                childCategories.forEach(function pushChildCategoryToTheQueue(childCategory) {
                    queue.push(childCategory.id);
                });
            }

            return result;
        }

        init();
    }
})(jQuery);