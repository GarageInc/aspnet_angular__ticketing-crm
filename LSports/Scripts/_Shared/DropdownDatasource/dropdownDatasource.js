(function () {
    angular.module('dropdown-datasource', []).service('DropdownDatasource', function ($http, $q) {
        var vm = this;

        vm.cache = {};

        vm.requestBuilder = function () {
            this.dataRequest = {nullValue:"--"};

            this.setPath = function (path) {
                this.dataRequest['path'] = path;

                return this;
            }

            this.setCacheKey = function (key) {
                this.dataRequest['cacheKey'] = key;
                return this;
            }

            this.setNullValue = function (nullValue) {
                this.dataRequest.nullValue = nullValue;
                return this;
            }

            this.setParams = function(params) {
                this.dataRequest["params"] = params;
                return this;
            }

            this.build = function () {
                return this.dataRequest;
            }
            return this;
        };

        vm.sendRequest = function (requestObj, shouldAugment) {
            if (typeof requestObj.cacheKey !== 'undefined' &&
                typeof vm.cache[requestObj.cacheKey] == 'undefined') {

                vm.cache[requestObj.cacheKey] = [];
            }
            
           return sendDataRequest(requestObj.path, requestObj.cacheKey, requestObj.nullValue, requestObj.params);
        }

        function sendDataRequest(path, cacheKey, nullValue, params, shouldAugment) {
            console.log(path);
            var shouldCache = !(typeof cacheKey === 'undefined');
            if (!shouldCache || vm.cache[cacheKey].length === 0) {
                var requestObj = {
                    method: "GET",
                    url: "/Tickets/DropDown" + path
                };
                if (params)
                    requestObj["params"] = params;

                var request = $http(requestObj).then(

                function mySucces(response) {
                    var augmentedData = augmentData(response.data, nullValue);
                    
                    if (shouldCache) {
                        return saveCache(cacheKey, augmentedData);
                    }
                    return augmentedData;
                },

                function myError(response) {
                    return response.data;
                });

                return request;
            } else {
                console.log("from cache!");
                return $q(function (resolve, reject) { resolve(vm.cache[cacheKey]); });
            }
        }

        function augmentData(data, nullValue) {
            var r = [];
            
            var shouldBuildObject = false;
            r.push({ "Name": nullValue, "Id": "0", Icon: "" });
            if (typeof data[0] !== "object") {
                shouldBuildObject = true;
            }
            for (var i = 0; i < data.length; i++) {
                if (!shouldBuildObject)
                    r.push(data[i]);
                else
                    r.push({ "Name": data[i], "Id": i+1, Icon: "" });
            }
            return r;
        }

        function saveCache(key, cachebaleData) {
            if (vm.cache[key].length === 0) {
                vm.cache[key] = cachebaleData;
            }
            return vm.cache[key];
        }

        return vm;
    });
}());