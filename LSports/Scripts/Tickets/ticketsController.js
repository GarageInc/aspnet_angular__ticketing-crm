ticketsApp.controller("ticketsController", function ($timeout,
    $interval,
    $scope,
    $q,
    crudService,
    networkService,
    ticketsService,
    formManagementService,
    ahaFileAttachmentService) {
    var vm = this;
    var indexOfRowInEdit = -1;

    vm.sideMenu = [];

    vm.buttonsConfig = [];

    vm.sharedData = ticketsService;
    var service = ticketsService;

    vm.isCustomer = isCustomer;

    var rteCallersEnum = { Reply: 0, Comment: 1 };
    var anothersEnum = { Department : 0, User : 1, CloseTicket:2 };
    var rteCaller = -1;

    vm.statusIdEnum = { Closed: 9 };

    vm.fileAttachmentFieldInfo = { fieldId: 0, fieldTitle: "" }; // for displaying in the ticket details view

    $scope.controlsToShow = controlsToShow;

    service.loadMenu().then(function (data) {
        vm.sideMenu = data;
    });

    var updateIntervalHandler = null;

    //departmentService.loadRoles();

    //ticketsService.loadFieldTypes();

    var lastUpdatedTimestamp = 0;

    vm.addNewTicket = function() {
        $("#create-ticket-wizard").modal("show");
    }

    vm.openedTicketWizard = function() {
        if (updateIntervalHandler) {
            $interval.cancel(updateIntervalHandler);
            updateIntervalHandler = null;
        }
        
    }

    //is called every 15 secs
    //for promise return explanation see closeModal function
    function updater() {
        return $q(function(resolve, reject) {
            service.shouldUpdateInfo((Date.now()-lastUpdatedTimestamp)/1000).then(function(ans) {
                if (ans) {
                    ticketsService.loadTickets().then(function(data) {
                        var table = $("#sample_1").DataTable();
                        table.clear().draw();
                        table.rows.add(data).draw();

                        console.log("added rows to table!");

                        service.loadMenu().then(function(menu) {
                            vm.sideMenu = menu;
                            lastUpdatedTimestamp = Date.now();
                            resolve();
                        });
                    });
                } else {
                   // lastUpdatedTimestamp = Date.now();
                    resolve();
                }
            });
        });
    }

    ticketsService.loadTickets().then(function (data) {
        console.log("general load tickets!");
        var table = $("#sample_1").DataTable();
        table.rows.add(data).draw();
        lastUpdatedTimestamp = Date.now();
        updateIntervalHandler = $interval(function() {
            updater();
        }, 15000);
        var ticketId = $.urlParam("ticketId");
        if (!!ticketId) {
            var value =  parseInt(ticketId);
            var ticketIndex = -1;

            for (var i = 0; i < data.length; ++i) {
                if (data[i].Id === value) {
                    ticketIndex = i;
                    break;
                }
            }

            if (ticketIndex === -1)
                throw "Ticket not found!";

            //TODO: refactor it or somethig
            $timeout(function() {
                indexOfRowInEdit = ticketIndex;
                service.selectedTicket = table.data()[indexOfRowInEdit];
                service.loadRightButtons().then(function(data) {
                    console.log("Buttons data: ", data);
                    vm.buttonsConfig = data;
                });
                service.loadTicketLog().then(function(data) {
                    $("#ticket_history_table").DataTable().rows.add(data).draw();
                });

                service.loadTicketCustomFieldsAndInstructions().then(function(data) {
                    console.log(data);
                    vm.instructions = data.InstructionsTextHtml;
                    vm.links = data.InstructionsLinksHtml;
                    vm.customFields = chunk(data.CustomFieldValues);

                });

                service.loadAttachedFiles().then(function(data) {
                    data.forEach(function(val) {
                        vm.attachmentLinks.push({ link: "/File/Download/" + val.FileId, name: val.FileName });
                    });
                    if (data.length > 0) {
                        vm.fileAttachmentFieldInfo.fieldId = data[0].CustomFieldId;
                        vm.fileAttachmentFieldInfo.fieldTitle = data[0].CustomFieldTitle;
                    }
                });

                startTime = new Date(parseInt(service.selectedTicket.CreationDate.match(/\d+/g)[0]));
                $("#edit-department").modal("show");
                console.log("Ticket being edited: ", service.selectedTicket);
                //Intetionally do not care if the timer is going when the ticket is closed - as long as user doesn't see it
                displayTimer();
                $scope.$apply();
            }, false);

        }

    });
    vm.fieldTypeSelectionChanged = function() {

        if (vm.sharedData.selectedFieldType.Id === vm.DROPDOWN_TYPE_ID || vm.sharedData.selectedFieldType.Id === vm.MULTIPLE_SELECT_TYPE_ID ||
            vm.sharedData.selectedFieldType.Id === vm.RELATED_DROPDOWN_TYPE_ID) {
            ticketsService.loadTableNames();

            if (vm.sharedData.selectedFieldType.Id === vm.RELATED_DROPDOWN_TYPE_ID) {
                ticketsService.loadTableNames().then(function() {
                    vm.sharedData.stagesForDropdown[0].selectedTable = vm.sharedData.tableNames[0];
                });
            }
        }
    }

    vm.tableNameSelectionChanged = function() {
        ticketsService.loadFieldsForTable();
        
    }

    vm.renderedFormFields = [];
    vm.attachmentLinks = [];

    $scope.fieldTypesEnum = fieldTypesEnum;

    /**********************************************TICKET FORM MANAGEMENT*************************************************************/
    var attachedFieldId = 0;
    vm.editTicket = function () {
        $("#edit-form-fields").modal("show");
        $.blockElement($("#edit-form-fields"));
        service.loadCustomForm().then(function (data) {
            $.unblockElement($("#edit-form-fields"));
            /*if (hasAttachedFilesField(data)) {
                service.loadAttachedFiles().then(function (data) {
                    $.unblockUI();
                    data.forEach(function(val) {
                        vm.attachmentLinks.push({ link: "/File/Download/" + val.fileId, name: val.fileName });
                    });
                });
            }*/

            vm.renderedFormFields = formManagementService.loadFormForEdit(data);
           
        });
    }
    $scope.trixModel2 = "";
    vm.dropdownStageSelectionChanged = function (stageIndex, fieldIndex) {
        formManagementService.dropdownStageSelectionChanged(stageIndex, fieldIndex, $scope.editTicketFields);
    }

    vm.saveEditedForm = function () {
        var fileIds = [];
        vm.attachmentLinks.forEach(function(val) {
           fileIds.push(extractFileIdFromLink(val.link));
        });
        $.blockElement($("#edit-form-fields"));
        formManagementService.submitForm(service.selectedTicket.Id, fileIds, $scope.editTicketFields, false).then(function (data) {
            $.unblockElement($("#edit-form-fields"));
            if (data !== "invalid") {
                attachedFieldId = 0;
                vm.renderedFormFields.splice(0, vm.renderedFormFields.length);
                vm.customFields = chunk(data.CustomFieldValues);
                vm.attachmentLinks = [];
                $("#edit-form-fields").modal('hide');
                addRowToLogTable(data.Log);
                toastr["success"]("Ticket form fields successfully updated!");
            }
        });
    }
    vm.closeFormEditor = function () {
        vm.renderedFormFields = [];
        //vm.attachmentLinks = [];
        attachedFieldId = 0;
        $("#edit-form-fields").modal('hide');
    }
    /*********************************************FILE ATTCHEMENTS MANGEMENT************************************************************/
    vm.attachmentLinks = [];
    function saveFileAttachmentChanges(loadedFileIds) {
        if (loadedFileIds && loadedFileIds.length > 0) {
            var parsedFileIds = loadedFileIds.map(function (val) { return val.fileId });
           
            service.attachFilesToTicket(service.selectedTicket.Id,
                parsedFileIds, attachedFieldId).then(function (data) {
                loadedFileIds.forEach(function(val, idx) {
                    vm.attachmentLinks.push({ link: "/File/Download/" + val.fileId, name: val.fileName });
                });
                addRowToLogTable(data);
                toastr["success"]("File attachments changes successfully updated!");
            });
        }
    }

    vm.detachFile = function (index, link) {
        bootbox.confirm("Are you sure you want to delete this file?", function (result) {
            if (result) {
                $.blockElement($("#edit-department"));
                service.detachFile(extractFileIdFromLink(link), vm.fileAttachmentFieldInfo.fieldId).then(function (data) {
                    $.unblockElement($("#edit-department"));
                    vm.attachmentLinks.splice(index, 1);
                    addRowToLogTable(data);
                    toastr["success"]("File was successfully detached!");
                });
            }
        });

    }

    

    vm.showFileAttachmentDialog = function () {
        ahaFileAttachmentService.setDialogSavedCallback(saveFileAttachmentChanges);
        $("#attach-files").modal("show");
    }
    /*************************************************RIGHT BUTTONS HANDLERS WITH NO DIALOG WINDOWS TO THEM********************************************************************/
    
    vm.unassign = function () {
        $.blockElement($("#edit-department"));
        service.unassignTicket().then(function (data) {
            $.unblockElement($("#edit-department"));
            console.log(data);
            handleReconfigurationServerResponse(data, false);
            toastr["success"]("Ticket successfully unassigned!");
        });
    }
    vm.assignToMe = function () {
        $.blockElement($("#edit-department"));
        service.assignToMe().then(function(data) {
            $.unblockElement($("#edit-department"));
            handleReconfigurationServerResponse(data, false);
            toastr["success"]("Ticket successfully assigned to you!");
        });
    }
    vm.reopenTicket = function() {
        $.blockElement($("#edit-department"));
        service.reopenTicket().then(function (data) {
            $.unblockElement($("#edit-department"));
            handleReconfigurationServerResponse(data, false);
            toastr["success"]("Ticket successfully reopened!");
        });
    }
    /*****************************************************************RTE WITH DROPDOWN HANDLERS*************************************************************/
    vm.anothers = [];
    vm.selectedAnother = {};
    var anotherType = -1;
    vm.anotherName = "";
    vm.assignButtonText = "Assign";
    vm.assignToAnotherDepartment = function() { // open modal
        $("#assign-to-another-window").modal("show");
        vm.anotherName = "department";
        anotherType = anothersEnum.Department;
        service.loadDepartments().then(function (data) {
            vm.anothers = data;
            vm.selectedAnother = data[0];
        });
    }

    vm.closeTicket = function () {
        $("#assign-to-another-window").modal("show");
        vm.anotherName = "closing status";
        anotherType = anothersEnum.CloseTicket;
        vm.assignButtonText = "Close ticket";
        service.loadReasonsForClosing().then(function(data) {
            vm.anothers = data;
            vm.selectedAnother = data[0];
        });
    }

    vm.assignToAnotherUser = function () { // open modal
        $("#assign-to-another-window").modal("show");
        anotherType = anothersEnum.User;
        vm.anotherName = "user";
        service.loadUsersByTicketDepartment().then(function (data) {
            vm.anothers = data;
            vm.selectedAnother = data[0];
        });
    }
    vm.assign = function () {
        if (validateAnotherForm()) {
            $.blockElement($("#assign-to-another-window"));
            if (anotherType === anothersEnum.Department) {
                service.assignToAnotherDepartment($scope.trixModel2, vm.selectedAnother.Id).then(function(data) {
                    vm.closeAssignToAnother(data);
                    toastr["success"]("Ticket was successfully assigned to another department!");
                });
            } else if (anotherType === anothersEnum.User) {
                service.assignToAnotherUser($scope.trixModel2, vm.selectedAnother.Id).then(function (data) {
                    vm.closeAssignToAnother(data);
                    toastr["success"]("Ticket was successfully assigned to another user!");
                });
            } else if (anotherType === anothersEnum.CloseTicket) {
                service.closeTicket($scope.trixModel2, vm.selectedAnother.Id).then(function (data) {
                    vm.closeAssignToAnother(data);
                    vm.closeModal();
                    toastr["success"]("Ticket was successfully closed!");
                });
            }
        }
    }

    vm.closeAssignToAnother = function (data) { // close modal
        $.unblockElement($("#assign-to-another-window"));
        $("#assign-to-another-window").modal("hide");
        handleReconfigurationServerResponse(data, false);
        $scope.trixModel2 = "";
        vm.anothers.splice(0, vm.anothers.length);
        vm.selectedAnother = {};
        anotherType = -1;
        vm.anotherName = "";
        vm.assignButtonText = "Assign";
        $scope.selectAnotherForm.$setUntouched();
    }

    function validateAnotherForm() { // Meaning the form that says select another smth
        var valid = true;
        if ($scope.selectAnotherForm.selectAnother.$viewValue.Id === '0') {
            $scope.selectAnotherForm.selectAnother.$touched = true;
            valid = false;
        }
        return valid;
    }
    /*********************************************MAIN TICKET DETAILS MODAL AND WIZARD CLOSED HANDLERS*********************************************/

    vm.closeModal = function () {
        closeModal();
    }
    vm.ticketWizardCreatedTicket = function(createdEntry) {
        handleReconfigurationServerResponse(createdEntry, true);
        toastr["success"]("Ticket was successfully created!");
    }

    vm.ticketWizardClosed = function () {
        console.log("closed wizard");
        updateIntervalHandler = $interval(function () {
            updater();
        }, 15000);
    }

    /************************************************RTE DIALOGS MANAGEMENT*********************************************************************/
    $scope.rteCaption = "";
    vm.addReply = function () {
        $("#rte-window").modal("show");
        rteCaller = rteCallersEnum.Reply;
        $scope.rteCaption = "Reply";

    }

    vm.addComment = function () {
        $("#rte-window").modal("show");
        rteCaller = rteCallersEnum.Comment;
        $scope.rteCaption = "Comment";
    }

    vm.saveRteDialog = function() {
        //caller depenedent code here
        $.blockElement($("#rte-window"));
        switch (rteCaller) {
            case rteCallersEnum.Reply:
                service.addReply($scope.trixModel).then(function (data) {
                    //$.unblockElement($("#rte-window"));
                    resetRteState();
                    handleReconfigurationServerResponse(data, false);
                    toastr["success"]("Reply was successfully added!");
                });
                break;
            case rteCallersEnum.Comment:
                service.addStaffComment($scope.trixModel).then(function (data) {
                    $.unblockElement($("#rte-window"));
                    resetRteState();
                    handleReconfigurationServerResponse(data, false);
                    toastr["success"]("Comment was successfully added!");
                });
                break;
            default:
                break;
        }
    }
    vm.closeRteDialog = function () {
        resetRteState();
    }
    function resetRteState() {
        rteCaller = -1;
        $scope.trixModel = "";
        $("#rte-window").modal("hide");
        $scope.rteCaption = "";
    }

    vm.instructions = "";
    vm.links = "";
    vm.showCustomerDetails = true;
    vm.customFields = [];
    /******************************************TICKET DETAILS HANDLER******************************************************/
    $timeout(function () {
        var table = $("#sample_1");
        $('#sample_1 tbody').on('click', 'a.edit', function () {
            if (updateIntervalHandler) {
                $interval.cancel(updateIntervalHandler);
            }
            indexOfRowInEdit = table.DataTable().row($(this).parents('tr')).index();
            service.selectedTicket = table.DataTable().data()[indexOfRowInEdit];
            service.loadRightButtons().then(function (data) {
                console.log("Buttons data: ", data);
                vm.buttonsConfig = data;
            });
            service.loadTicketLog().then(function (data) {
                $("#ticket_history_table").DataTable().rows.add(data).draw();
            });

            service.loadTicketCustomFieldsAndInstructions().then(function(data) {
                console.log(data);
                vm.instructions = data.InstructionsTextHtml;
                vm.links = data.InstructionsLinksHtml;
                vm.customFields = chunk(data.CustomFieldValues);
                vm.showCustomerDetails = data.ShowCustomerDetails;
            });

            service.loadAttachedFiles().then(function (data) {
                data.forEach(function (val) {
                    vm.attachmentLinks.push({ link: "/File/Download/" + val.FileId, name: val.FileName });
                });
                if (data.length > 0) {
                    vm.fileAttachmentFieldInfo.fieldId = data[0].CustomFieldId;
                    vm.fileAttachmentFieldInfo.fieldTitle = data[0].CustomFieldTitle;
                }
            });

            startTime = new Date(parseInt(service.selectedTicket.CreationDate.match(/\d+/g)[0]));

            console.log("Ticket being edited: ", service.selectedTicket);
            //Intetionally do not care if the timer is going when the ticket is closed - as long as user doesn't see it
            displayTimer();
            $scope.$apply();
        });

    });
    /**
     * Separates array in two
     * @param {Array<Object>} arr 
     * @returns {Array<Array>} - two arrays 
     */
    function chunk(arr) {
        var newArr = [];
        var arrLength = arr.length;
        if (arrLength / 2 > 1) {
            newArr.push(arr.slice(0, arrLength / 2));
            newArr.push(arr.slice(arrLength / 2, arrLength));
        } else {
            newArr.push(arr.slice(0, arrLength));
            newArr.push([]);
        }


        return newArr;
    }
    /*******************************************HELPERS*******************************************************************/

    function hasAttachedFilesField(formData) {
        var r = formData.find(function(val, idx) {
            return val.CustomField.CustomFieldType.Id === fieldTypesEnum.Attachments;
        });
        if (r)
            attachedFieldId = r.CustomField.Id;

        return !(typeof r === 'undefined');
    }

    function addRowToLogTable(log) {
        $("#ticket_history_table").DataTable().row.add(log).draw();
    }

    vm.timeOpened = "00:00:00";
    var startTime = 0;
    var intervalStop = null;

    function displayTimer() {
        makeTimeOpened();

       intervalStop = $interval(function() {
            makeTimeOpened();
        }, 1000);
    }

    function makeTimeOpened() {
        var endTime = new Date();
        var timeDiff = endTime - startTime;

        timeDiff /= 1000;
        var seconds = Math.round(timeDiff % 60);
        timeDiff = Math.floor(timeDiff / 60);
        var minutes = Math.round(timeDiff % 60);
        timeDiff = Math.floor(timeDiff / 60);
        var hours = timeDiff;
        vm.timeOpened = padNumber(hours) + ":" + (minutes === 60 ? 0 :  padNumber(minutes)) + ":" + (seconds === 60 ? 0 : padNumber(seconds));
    }

    function handleReconfigurationServerResponse(response, addingTicket) {
        console.log("Reconfiguration response: ", response);
        if (response) {
            vm.buttonsConfig = response.Controls;
            if (response.Log)
                addRowToLogTable(response.Log);
            if (response.Ticket) { // last action had something to do with ticket
                console.log("Got ticket updated!");
                response.Ticket["edit"] = "details";
                response.Ticket["del"] = "Delete";
                vm.sharedData.selectedTicket = response.Ticket;
                //$.extend(true, vm.sharedData.selectedTicket, response.Ticket);
                service.shouldBeInTheGrid(response.Ticket.Id).then(function(data) {
                    if (!!!data) {// it shouldn't be in  the grid anymore
                        $("#sample_1").DataTable().row(indexOfRowInEdit).remove().draw();
                    } else { // it should be
                        if (!!addingTicket) {
                            $("#sample_1").DataTable().row.add(response.Ticket).draw();
                            
                        } else {
                            console.log("edited ticket!");
                            $("#sample_1").DataTable().row(indexOfRowInEdit).data(response.Ticket);
                            $("#sample_1").DataTable().row(indexOfRowInEdit).invalidate().draw();
                        }
                    }

                    if (!!addingTicket && !updateIntervalHandler) { // we are adding ticket, and the updater is disabled
                        updateIntervalHandler = $interval(function () {
                            updater();
                        }, 15000);
                    }
                });
            }
            service.loadMenu().then(function(data) {
                console.log(data);
                vm.sideMenu = data;
            });
        }
    }

    function padNumber(num, size) {
        var s = String(num);
        while (s.length < (size || 2)) { s = "0" + s; }
        return s;
    }

    function closeModal() {
        $("#edit-department").modal("hide");
        $("#ticket_history_table").DataTable().clear().draw();
        indexOfRowInEdit = -1;
        vm.timeOpened = "00:00:00";
        $interval.cancel(intervalStop);
        vm.instructions = "";
        vm.links = "";
        vm.customFields = [];
        vm.attachmentLinks = [];
        vm.fileAttachmentFieldInfo = { fieldId: 0, fieldTitle: "" };

        //updater().then(function() {
            updateIntervalHandler = $interval(function() {
                updater();
            }, 15000);
        //});

    }
    function extractFileIdFromLink(link) {
        var t = link.split("/");
        return parseInt(t[3]);
    }

    return vm;
});