ticketsApp.service("ticketsService", function(DropdownDatasource, networkService, crudService, $q) {
    var vm = this;

    vm.users = [];

    vm.fieldTypes = [];
    vm.selectedFieldType = {};

    vm.tableNames = [];
    vm.selectedTable = {};

    vm.fieldsForTable = [];
    vm.selectedIdField = {};
    vm.selectedTitleField = {};

    vm.selectedTicket = {};

    var dds = DropdownDatasource;
    var rb = dds.requestBuilder;

    vm.statusIdEnum = {Closed : 9}


    //main table
    vm.loadTickets = function() {
        return $q(function (resolve, reject) {
            var body = {};

            var status = $.urlParam("Status");
            var assignee = $.urlParam("Assignee");
            var department = $.urlParam("DepartmentId");

            if (status)
                body['status'] = status;
            if (assignee)
                body['assignee'] = assignee;
            if (department)
                body['departmentId'] = parseInt(department);

            networkService.sendRequest(networkService.requestBuilder().method("POST").
            url("/Ticket/List").
            body(body).build()).then(function(data) {
                resolve(data.map(function (val, idx, arr) {
                    val["edit"] = "edit";
                    val["del"] = "delete";

                    return val;
                }));
            });
        });
    }

    /**
     * ********************************RIGHT BUTTON ACTIONS *********************************
     */
    vm.closeTicket = function(comment,statusId) {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").
            url("/Ticket/CloseTicket").
            body({ticket: vm.selectedTicket, comment: htmlEncode(comment), statusId:statusId }).build());
    }

    vm.assignToAnotherUser = function(comment, anotherUserId) {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").
            url("/Ticket/AssignToAnotherUser").
            params().body({
                ticketId: vm.selectedTicket.Id,
                userId: anotherUserId,
                comment: htmlEncode(comment)
            }).build());
    }
    vm.assignToAnotherDepartment = function (comment, anotherDepartmentId) {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").
            url("/Ticket/AssignToAnotherDepartment").
            params().body({
                ticketId: vm.selectedTicket.Id,
                departmentId: anotherDepartmentId,
                comment: htmlEncode(comment)
            }).build());
    }

    vm.addReply = function(reply) {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").
            url("/Ticket/AddReply").
            body({
                ticketId: vm.selectedTicket.Id,
                reply: htmlEncode(reply)
            })
            .build());
    }

    vm.addStaffComment = function(comment) {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").
            url("/Ticket/AddStaffComment").
            body({
                ticketId: vm.selectedTicket.Id,
                comment: htmlEncode(comment)
            })
            .build());
    }


    vm.reopenTicket = function() {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").
            url("/Ticket/ReopenTicket").
            params({ ticketId: vm.selectedTicket.Id }).build());
    }

    vm.unassignTicket = function() {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").
            url("/Ticket/UnassignTicket").
            params({ ticketId: vm.selectedTicket.Id }).build());
    }
    vm.assignToMe = function() {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").
            url("/Ticket/AssignToMe").
            params({ ticketId: vm.selectedTicket.Id }).build());
    }

    vm.loadCustomForm = function() {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").
            url("/Ticket/EditTicketFields").
            params({ ticketId: vm.selectedTicket.Id }).build());
    }

    vm.loadAttachedFiles = function() {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").url("/Ticket/GetAttachedFiles").params({ ticketId: vm.selectedTicket.Id }).build());
    }
    /*******************************************END OF RIGHT BUTTON ACTION FUNCTIONS************************************/

    vm.loadTicketLog = function() {
       return $q(function(resolve, reject) {
            networkService.sendRequest(networkService.requestBuilder().method("POST").
                url("/Ticket/GetTicketLogs").
                params({ ticketId: vm.selectedTicket.Id }).build()).then(function(data) {
                resolve(data);
            });
        });

    }


    vm.attachFilesToTicket = function (ticketId, fileIds, customFieldId) {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").url("/Ticket/AttachTicketFiles").body(
            {
                ticketId: ticketId,
                customFieldId: customFieldId,
                fileIds: fileIds
            }
        ).build());
    }

    vm.loadDepartments = function () {
        return $q(function (resolve, reject) {
            dds.sendRequest(rb().setPath("/Departments").setNullValue("-Select department-").setCacheKey("departments").build()).then(function (departments) {
                resolve(departments);
            });
        });
    }

    vm.loadUsers = function (departmentId) {
        return $q(function (resolve, reject) {
            dds.sendRequest(rb().setPath("/Departments").setNullValue("-Select department-").setCacheKey("departments").build()).then(function (departments) {
                resolve(departments);
            });
        });
    }

    vm.detachFile = function(fileId, fieldId) {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").
            url("/Ticket/DeleteTicketFiles").
            body({
                ticketId: vm.selectedTicket.Id,
                customFieldId: fieldId,
                fileIds: [fileId]
            })
            .build());
    }

    vm.loadMenu = function () {
        return crudService.loadMenu();
    }

    vm.loadRightButtons = function() {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").url("/Ticket/Controls").params(
            { ticketId: vm.selectedTicket.Id }).build());
    }

    vm.loadReasonsForClosing = function() {
        return dds.sendRequest(rb().setPath("/ClosedTicketStatuses").setNullValue("-Select reason-").setCacheKey("closing_reasons").build());
    }

    vm.loadUsersByTicketDepartment = function () {
        return $q(function(resolve, reject) {
            networkService.sendRequest(networkService.requestBuilder().method("POST").
            url("/DropDown/DepartmentUsers").
            body({
                departmentId: vm.selectedTicket.DepartmentId,
                ticketId: vm.selectedTicket.Id
            }).build()).then(function (data) {
                data.unshift({ Name: "-Select user-", Id: "0" });
                resolve(data);
            });
        }); 
    }

    vm.loadTicketCustomFieldsAndInstructions = function() {
        
        return networkService.sendRequest(networkService.requestBuilder().method("POST").url("/Ticket/TicketInstructionsAndCustomFields").params(
            { ticketId: vm.selectedTicket.Id }).build());
    }

    vm.shouldBeInTheGrid = function(ticketId) {
        var status = $.urlParam("Status");
        var assignee = $.urlParam("Assignee");
        var department = $.urlParam("DepartmentId");
       /* if (typeof status !== 'undefined') {
            if(ticket)
        }*/
        //int ticketId, int? departmentId, string status, string assignee
        return $q(function(resolve, reject) {
            networkService.sendRequest(networkService.requestBuilder().method("POST").url("/Ticket/ShouldBeInTheGrid").body({
                ticketId: ticketId,
                departmentId: department,
                status: status,
                assignee: assignee
            }).build()).then(function(data) {
                resolve(data);
            });
        });

    }

    vm.shouldUpdateInfo = function(time) {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").url("/Ticket/TicketsUpdated").params({
            secondsAgo: Math.ceil(time)
        }).build());
    }

    return vm;

});