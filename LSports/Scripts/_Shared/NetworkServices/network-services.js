(function() {
    angular.module("network-services", [])
        .service("networkService", function($http) {
            var vm = this;
            vm.requestBuilder = function() {
                var request = {};
                this.method = function(method) {
                    request["method"] = method;
                    return this;
                }
                this.url = function(url) {
                    request["url"] = "/Tickets"+url;
                    return this;
                }
                this.body = function(body) {
                    request["data"] = body;
                    return this;
                }
                this.params = function(params) {
                    request["params"] = params;
                    return this;
                }
                this.build = function() {
                    return request;
                }
                return this;
            }
            vm.sendRequest = function(request) {
                return $http(request).then(function(response) {
                    return response.data;
                }, function(error) {
                    console.log(error);
                    return error;
                });
            }

            vm.isIssueTypeNameUnique = function(name, id) {
                return vm.sendRequest(vm.requestBuilder("GET").url("/Naming/IsIssueTypeNameUnique").params({name : name, id: id}).build());
            }
            vm.isTicketTypeNameUnique = function (name, id) {
                return vm.sendRequest(vm.requestBuilder("GET").url("/Naming/IsTicketTypeNameUnique").params({ name: name, id: id }).build());
            }
            vm.isDepartmentNameUnique = function (name, id) {
                return vm.sendRequest(vm.requestBuilder("GET").url("/Naming/IsDepartmentNameUnique").params({ name: name, id: id }).build());
            }
            vm.isEmailUnique = function (name, id) {
                return vm.sendRequest(vm.requestBuilder("GET").url("/Naming/IsEmailUnique").params({ email: name, id: id }).build());
            }
            vm.isCustomFieldValuesUnique = function (name, identifier, id) {
                return vm.sendRequest(vm.requestBuilder("GET").url("/Naming/IsCustomFieldValuesUnique").params({ name: name, identifier: identifier, id: id }).build());
            }
            vm.isEmailTemplateNameUnique = function (name, id) {
                return vm.sendRequest(vm.requestBuilder("GET").url("/Naming/isEmailTemplateNameUnique").params({ name: name, id: id }).build());
            }
            return vm;
        })
        .service("crudService", function(networkService, $q, $timeout) {
            var vm = this;

            vm.read = function(controllerName) {
                var queryObj = networkService.requestBuilder().method("GET").url(buildUrl(controllerName, "List")).build();
                return networkService.sendRequest(queryObj);
            }
            vm.create = function(controllerName, record) {
                var queryObj = networkService.requestBuilder().method("POST").url(buildUrl(controllerName, "Insert")).body(record).build();
                return networkService.sendRequest(queryObj);
            }
            vm.update = function(controllerName, record) {
                var queryObj = networkService.requestBuilder().method("POST").url(buildUrl(controllerName, "Update")).body(record).build();
                return networkService.sendRequest(queryObj);
            }
            vm.remove = function(controllerName, id) {
                var queryObj = networkService.requestBuilder().method("POST").url(buildUrl(controllerName, "Delete")).params({id : id}).build();
                return networkService.sendRequest(queryObj);
            }
            vm.loadMenu = function () {
                return $q(function(resolve, reject) {
                    networkService.sendRequest(networkService.requestBuilder().method("POST").url("/Menu/Staff").build()).then(function (data) {
                        
                        
                        
                        var openedSections = [];

                        if (window.name[0] === "|") {

                            openedSections = JSON.parse(window.name.split("|")[1]);
                        }

                        $timeout(function () {
                            console.log("dkvbdfv");
                            openedSections.forEach(function (val) {
                                if (val.listId[0] == "_") {
                                    $("#" + val.listId).addClass("in"); // open the list
                                    $("#" + val.arrowParentId).children('a > span.arrow').toggleClass('open');
                                }
                            });
                        });
                        resolve(data);
                    });
                });
                }

            function buildUrl(controllerName, methodName) {
                return "/" + controllerName + "/" + methodName;
            }

            return vm;
        });
}());