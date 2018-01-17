/*global angular*/
(function () {
    angular
        .module('simplAdmin.core')
        .factory('countryService', countryService);

    /* @ngInject */
    function countryService($http) {
        var service = {
            getCountries: getCountries,
            getAllCountries: getAllCountries,
            editCountry: editCountry
        };
        return service;

        function getCountries(params) {
            return $http.post('api/countries/grid', params);
        }

        function getAllCountries() {
            return $http.get('api/countries/all');
        }

        function editCountry(country) {
            return $http.put('api/countries/' + country.id, country);
        }
    }
})();