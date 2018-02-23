ticketTypeApp.controller("ticketTypeController", function($timeout, $scope, crudService, DropdownDatasource, networkService) {
    var vm = this;

    var tickets = [];
    vm.selectedTicket = {Id:0, Name: "", Icon:"" };
    var indexOfRowInEdit = -1;
    vm.ticketTypes = [];
    vm.selectedTicketType = {};
    DropdownDatasource.sendRequest(DropdownDatasource.requestBuilder().setPath("/Icons").setNullValue("-Select Icon-").setCacheKey("icons").build()).then(function(data) {
        console.log(data);
        vm.ticketTypes = data;
        vm.selectedTicketType.Name = vm.ticketTypes[0].Name;
        vm.selectedTicketType.Icon = vm.ticketTypes[0].Icon;
        vm.selectedTicketType.Id = vm.ticketTypes[0].Id;
    });
    vm.actionName = "Edit";

    vm.sideMenu = [];
    crudService.loadMenu().then(function (data) {
        vm.sideMenu = data;
    });

    $timeout(function () {
        var table = $("#sample_1");
        table.DataTable({
           // data: tickets,
            columns: [
                { "data": "Id" },
                { "data" : "Icon"},
                { "data": "Name" },
                { "data": "edit" },
                { "data": "del" }],
            columnDefs: [
                { "width": "7%", "type" : "num" ,"targets" : 0 },
                {
                    "width": "5%","targets" : 1,
                    "render" : function(data, type, row) {
                        return '<div style="text-align:center"><i class="fa ' + data + '"></i></div>';
                    }
                },
                { "width": "50%", "type" : "string","targets": 2 },
                {
                    "width": "19%",
                    "targets": 3,
                    "render" : function(data, type, row) {
                        return '<div style="text-align:center"><a style="cursor: pointer;" class="edit"  data-toggle="modal" href="#edit-ticket">' + data + '</a></div>';
                    }
                },
                {
                    "width": "19%", "targets": 4,
                    "render": function (data, type, row) {
                        return '<div style="text-align:center"><a style="cursor: pointer;" class="delete">' + data + '</a></div>';
                    }
                }
            ]
        });
        $('#sample_1 tbody').on('click', 'a.delete', function () {
            var row = table.DataTable().row($(this).parents('tr'));
            var idToRemove = table.DataTable().data()[row.index()].Id;
            bootbox.confirm("Are you sure you want to delete this ticket type?", function (result) {
                if (result) {              
                    crudService.remove("TicketType", idToRemove).then(function () {
                        table.DataTable().row(row).remove().draw();
                        toastr["success"]("Ticket type was successfully deleted!");
                    });
                }
            });
        });

        $('#sample_1 tbody').on('click', 'a.edit', function () {
            indexOfRowInEdit = table.DataTable().row($(this).parents('tr')).index();
            vm.selectedTicket.Name = table.DataTable().data()[indexOfRowInEdit].Name;
            vm.selectedTicket.Id = table.DataTable().data()[indexOfRowInEdit].Id;

            var index = resolveIndexByIconClass(table.DataTable().data()[indexOfRowInEdit].Icon);
            vm.selectedTicketType.Name = vm.ticketTypes[index].Name;
            vm.selectedTicketType.Icon = vm.ticketTypes[index].Icon;
            vm.selectedTicketType.Id = vm.ticketTypes[index].Id;
            $scope.$apply();
        });
        //load tickets
        crudService.read("TicketType").then(function (data) {
            console.log(data);
            table.DataTable().clear().draw();
            for (var i = 0; i < data.length; i++) {
                tickets.push({ Id: data[i].Id, Icon: data[i].Icon.Icon, Name: data[i].Name , edit: "edit", del: "delete" });
            }
            table.DataTable().rows.add(tickets).draw();
        });     
    });

    vm.addNewTicketType = function () {
        vm.actionName = "Add";
    }

    vm.saveChanges = function () {      
        if (validateForm()) {
            $.blockElement($("#edit-ticket"));
            networkService.isTicketTypeNameUnique(vm.selectedTicket.Name, vm.selectedTicket.Id).then(function (data) {
                if (data) {
                    var table = $("#sample_1").DataTable();
                    if (indexOfRowInEdit === -1) { // actually adding row
                        var newTicket = { Name: vm.selectedTicket.Name, Icon: { Id: vm.selectedTicketType.Id, Icon: vm.selectedTicketType.Icon, Name: vm.selectedTicketType.Name } };
                        crudService.create("TicketType", newTicket).then(function(addedObj) {
                            $.unblockElement($("#edit-ticket"));
                            table.row.add({ Id: addedObj.Id, Icon: addedObj.Icon.Icon, Name: addedObj.Name, edit: "edit", del: "delete" }).draw();
                            closeModal();
                            toastr["success"]("Ticket type was successfully added!");
                        });

                    } else {

                        var updateTicket = { Id: vm.selectedTicket.Id, Name: vm.selectedTicket.Name, Icon: { Id: vm.selectedTicketType.Id, Icon: vm.selectedTicketType.Icon, Name: vm.selectedTicketType.Name } };
                        crudService.update("TicketType", updateTicket).then(function(updatedObj) {
                            //console.log(updatedObj);
                            $.unblockElement($("#edit-ticket"));
                            table.data()[indexOfRowInEdit].Name = updateTicket.Name;
                            table.data()[indexOfRowInEdit].Icon = updateTicket.Icon.Icon;
                            table.row(indexOfRowInEdit).invalidate().draw();
                            closeModal();
                            toastr["success"]("Ticket type was successfully updated!");
                        });
                    }
                } else {
                    $.unblockElement($("#edit-ticket"));
                    toastr["error"]("Ticket type name is not unique!");
                }
            });
        }
    }
    vm.closeModal = function () {
        closeModal();
    }



    function validateForm() {
        vm.selectedTicket.Name = (typeof vm.selectedTicket.Name === 'undefined' ? "" : vm.selectedTicket.Name);
        if (vm.selectedTicket.Name === "" ||
            vm.selectedTicketType.Name === "-Select Icon-") {
            if (vm.selectedTicketType.Name === "-Select Icon-") {
                setDropdownInvalid();
            }
            if (vm.selectedTicket.Name === "") {
                $scope.addEditForm['ticketName'].$touched = true;
            }
            return false;
        }
        return true;
    }



    function closeModal() {
        indexOfRowInEdit = -1;
        vm.selectedTicketType.Name = vm.ticketTypes[0].Name;
        vm.selectedTicketType.Icon = vm.ticketTypes[0].Icon;
        vm.selectedTicketType.Id = vm.ticketTypes[0].Id;
        vm.selectedTicket.Name = "";
        vm.selectedTicket.Id = 0;
        $scope.addEditForm['ticketName'].$touched = false;
        vm.actionName = "Edit";
        setDropdownValid();
        $("#edit-ticket").modal("hide");
    }

    function resolveIndexByIconClass(iconClass) {
        for (var i = 0; i < vm.ticketTypes.length; i++) {
            if (vm.ticketTypes[i].Icon === iconClass) {
                return i;
            }
        }
        return -1;
    }
    /*##################################Customized select###########################*/
    var isShowing = false;
    vm.ddFaceClicked = function() {
        if (!isShowing) {
            setDropdownValid();
            $("#aha-options-list").css("display", "block");
            $("#aha-options-list").css({
                'width': ($("#ticket-type").innerWidth() + 'px')
            });
            isShowing = true;
        } else {
            isShowing = false;
            $("#aha-options-list").css("display", "none");
        }
    }
    vm.ddSelected = function(idx) {
        if (isShowing) {
            $("#aha-options-list").css("display", "none");
            isShowing = false;
            vm.selectedTicketType.Name = vm.ticketTypes[idx].Name;
            vm.selectedTicketType.Icon = vm.ticketTypes[idx].Icon;
            vm.selectedTicketType.Id = vm.ticketTypes[idx].Id;
        }
    }

    function setDropdownInvalid() {
        $("#ticket-type").addClass("aha-invalid");
    }

    function setDropdownValid() {
        $("#ticket-type").removeClass("aha-invalid");
    }

    return vm;
});