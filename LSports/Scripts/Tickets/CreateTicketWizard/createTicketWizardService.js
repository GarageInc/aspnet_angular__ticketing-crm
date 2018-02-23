ticketsApp.service('createTicketWizardService', function ($q, DropdownDatasource, networkService, crudService) {
    var vm = this;
    vm.selectedIssueType = {row : 0, col : 0};
    vm.selectedTicketType = {row: 0, col : 0};
    vm.selectedProduct = {row : 0, col: 0};
    vm.selectedProductCategory = {row : 0, col : 0};

    vm.issueTypes = [];
    vm.ticketTypes = [];
    vm.products = [];
    vm.productCategories = [];

    var dds = DropdownDatasource;
    var rb = dds.requestBuilder;



    vm.loadIssueTypes = function () {
        return $q(function (resolve, reject) {
            dds.sendRequest(rb().setPath("/IssueTypes").setNullValue("ANY").build()).then(function (issueTypes) {
                issueTypes.shift();
                generateMatrix(issueTypes, vm.issueTypes);
                resolve();

            });
        });
    }

    vm.loadTicketTypes = function () {
        return $q(function (resolve, reject) {
            dds.sendRequest(rb().setPath("/TicketTypes").setNullValue("ANY").build()).then(function (ticketTypes) {
                ticketTypes.shift();
                generateMatrix(ticketTypes, vm.ticketTypes);
                resolve();
            });
        });
    }

    vm.loadProductNames = function () {
        return $q(function (resolve, reject) {
            dds.sendRequest(rb().setPath("/Products").setNullValue("ANY").build()).then(function (productTypes) {
                productTypes.shift();
                generateMatrix(productTypes, vm.products);
                resolve();
            });
        });
    }

    vm.loadProductCategories = function () {
        return $q(function (resolve, reject) {
            dds.sendRequest(rb().setPath("/ProductCategories").setNullValue("ANY").build()).then(function (productCategories) {
                productCategories.shift();
                generateMatrix(productCategories, vm.productCategories);
                resolve();
            });
        });
    }

    vm.createTicket = function() {

        var newTicket = {
            Product: vm.products[vm.selectedProduct.row][vm.selectedProduct.col],
            ProductCategory: vm.productCategories[vm.selectedProductCategory.row][vm.selectedProductCategory.col],
            IssueType: vm.issueTypes[vm.selectedIssueType.row][vm.selectedIssueType.col],
            TicketType: vm.ticketTypes[vm.selectedTicketType.row][vm.selectedTicketType.col]
            
        };
        return crudService.create("Ticket", newTicket);
    }

    vm.attachFilesToTicket = function(ticketId, fileIds, customFieldId) {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").url("/Ticket/AttachTicketFiles").body(
            {
                ticketId: ticketId,
                customFieldId: customFieldId,
                fileIds: fileIds
            }
        ).build());
    }

    vm.resetSelections = function () {
        zeroMatrixIndex(vm.selectedProductCategory);
        zeroMatrixIndex(vm.selectedProduct);
        zeroMatrixIndex(vm.selectedTicketType);
        zeroMatrixIndex(vm.selectedIssueType);
    }

    vm.detachFile = function (fileId, fieldId, ticketId) {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").
            url("/Ticket/DeleteTicketFiles").
            body({
                ticketId: ticketId,
                customFieldId: fieldId,
                fileIds: [fileId]
            })
            .build());
    }

    function zeroMatrixIndex(matrixIdx) {
        matrixIdx.row = 0;
        matrixIdx.col = 0;
    }

    function generateMatrix(arr, matrix) {
        for (var k = 0; k < Math.ceil(arr.length / 3); k++) {
            matrix[k] = [];
        }
        for (var i = 0; i < matrix.length; i++) {
            for (var j = 0; j < 3; j++) {
                if (3*i + j >= arr.length) {
                    i = matrix.length;
                    break;
                }
                matrix[i][j] = arr[3*i + j];
            }
        }
    }

    return vm;
});