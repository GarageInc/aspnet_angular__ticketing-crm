ticketsApp.controller('createTicketWizardController', function ($scope,
    createTicketWizardService,
    formManagementService,
    ahaFileAttachmentService) {
    var vm = this;
    vm.step = 0;
    vm.leftButtonText = '< Back';
    vm.rightButtonText = 'Next >';

    var fileIdsForForm = [];

    vm.sharedData = createTicketWizardService;

    var service = createTicketWizardService;

    service.loadIssueTypes();
    service.loadTicketTypes();
    service.loadProductNames();
    service.loadProductCategories();

    vm.selectedTicketType = function(row, col) {
        service.selectedTicketType.row = row;
        service.selectedTicketType.col = col;
    }
    vm.selectedProductType = function(row, col) {
        service.selectedProduct.row = row;
        service.selectedProduct.col = col;
    }

    vm.selectedProductCategory = function (row, col) {
        service.selectedProductCategory.row = row;
        service.selectedProductCategory.col = col;
    }

   vm.selectedIssueType = function (row, col) {
        service.selectedIssueType.row = row;
        service.selectedIssueType.col = col;
    }

   var createdTicket = null;
   vm.renderedFormFields = [];

   $scope.fieldTypesEnum = fieldTypesEnum;

    var fileAttachmentFieldId = 0;
    var m_addedObj = null;
    vm.rightButtonClick = function () {
        if (vm.step < 4) {
            vm.step++;
            if (vm.step === 4) { //step 5 - create ticket!

                service.createTicket().then(function (addedObj) {
                    console.log(addedObj);
                    m_addedObj = addedObj;
                    createdTicket = addedObj;
                    if (createdTicket.FormTemplate == null) { // if there is no form template
                       // $scope.onCreate(m_addedObj);
                      //  vm.closeWizard();
                    } else { // there is form template
                        $.blockElement($("#create-ticket-wizard"));
                        formManagementService.loadFormFromCreatedTicket(createdTicket.FormTemplate).then(function(loadedForm) {
                            vm.renderedFormFields = loadedForm;
                            console.log(vm.renderedFormFields);
                            var field = vm.renderedFormFields.find(function(val) {
                                return val.CustomFieldType.Id === fieldTypesEnum.Attachments;
                            });
                            if (field) {
                                fileAttachmentFieldId = field.Id;
                            }
                            $.unblockElement($("#create-ticket-wizard"));
                        });
                    }

                    vm.rightButtonText = 'Submit';
                });
                
            } else {
                vm.rightButtonText = 'Next >';
            }
        } else {
            var fileIds = fileIdsForForm.map(function (val) { return val.fileId; });
            $.blockElement($("#create-ticket-wizard"));
            formManagementService.submitForm(createdTicket.TicketModel.Id, fileIds, $scope.ticketWizardForm, true).then(function (data) {
                $.unblockElement($("#create-ticket-wizard"));
                if (data !== "invalid") {
                    console.log(data);
                    vm.closeWizard();
                    
                    $scope.onCreate(data);
                }
            });
        }
    }
    vm.leftButtonClick = function() {
        if (vm.step > 0) {
            if (vm.step === 4) { // we've benn on the form but decided to go back
                vm.renderedFormFields = [];
            }
            vm.step--;
            if (vm.step < 4) {
                vm.rightButtonText = 'Next >';
            }
        }
    }

    vm.showFileAttachmentDialog = function () {
        ahaFileAttachmentService.setDialogSavedCallback(saveFileAttachmentChanges);
        $("#attach-files").modal("show");
    }

    vm.attachmentLinks = [];
    function saveFileAttachmentChanges(loadedFileIds) {
        if (loadedFileIds && loadedFileIds.length > 0) {
            createTicketWizardService.attachFilesToTicket(createdTicket.TicketModel.Id,
                loadedFileIds, fileAttachmentFieldId).then(function() {
                loadedFileIds.forEach(function(val, idx) {
                    fileIdsForForm.push(val);
                    vm.attachmentLinks.push({ link: "/File/Download/" + val.fileId, name: val.fileName });
                });
            });
        }
    }

    vm.detachFile = function (index, link) {
        bootbox.confirm("Are you sure you want to delete this file?", function(result) {
            if (result) {
                service.detachFile(extractFileIdFromLink(link), fileAttachmentFieldId, createdTicket.TicketModel.Id).then(function() {
                    vm.attachmentLinks.splice(index, 1);
                    toastr["success"]("File was successfully detached!");
                });
            }
        });

    }

    vm.dropdownStageSelectionChanged = function (stageIndex, fieldIndex) {
        console.log(stageIndex + " " + fieldIndex);
        formManagementService.dropdownStageSelectionChanged(stageIndex, fieldIndex, $scope.ticketWizardForm);
    }

    
    vm.closeWizard = function() {
        resetState();
        if ($scope.onClose) {
            $scope.onClose();
        }
        $("#create-ticket-wizard").modal("hide");
    }


    function extractFileIdFromLink(link) {
        var t = link.split("/");
        return parseInt(t[3]);
    }

    function resetState() {
        createdTicket = null;
        vm.step = 0;
        vm.leftButtonText = '< Back';
        vm.rightButtonText = 'Next >';
        vm.renderedFormFields.splice(0, vm.renderedFormFields.length);
        service.resetSelections();
    }

    

    return vm;
});