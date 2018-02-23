emailTemplatesApp.controller("emailTemplatesController", function ($timeout, $scope, $q, crudService, networkService, emailTemplatesService) {
    var vm = this;
    var indexOfRowInEdit = -1;

    vm.sharedData = emailTemplatesService;

    emailTemplatesService.loadIdentifiers();
    emailTemplatesService.loadActions();


    $scope.trixModel = "";

    vm.sideMenu = [];
    crudService.loadMenu().then(function (data) {
        vm.sideMenu = data;
    });

    emailTemplatesService.loadUsers().then(function () {
        console.log(vm.sharedData.users);
        $timeout(function () {
            ComponentsBootstrapMultiselect.init(function (option, checked, select) {
                $($(".multiselect-native-select").children()[1]).removeClass('aha-invalid');
                console.log($(this.$select));
            }); //for appearance
            $("#send-email-to").multiselect(); // for behavior
        });
    });

    var fieldTypesEnum = {
        Dropdown : 3,
        Text: 1, // check
        LongText: 2, // check
        RelatedDropdowns: 4,
        Checkbox: 5, // check
        Date: 6,
        Attachments: 7,
        Numeric: 8,
        Email: 9, // check
        MultipleSelection:10
    };

    vm.emailPreviewString = "";

    $scope.$watch('trixModel', function () {
        if ($('trix-editor')) {
            $('trix-editor').removeClass('aha-invalid');
        }
    });

    vm.onSrcClick = function () {
        if ($(".src").hasClass("active")) {
            vm.setRTEButtonsDisabled(true);
            $("#srcEditor").val($scope.trixModel.substring(0, $scope.trixModel.length));
            $("#srcEditor").show();
            $("trix-editor").hide();
        } else {
            vm.setRTEButtonsDisabled(false);
            $scope.trixModel = $("#srcEditor").val();
            $("trix-editor").val($scope.trixModel);
            $("#srcEditor").hide();
            $("trix-editor").show();
        }
    }

    vm.setRTEButtonsDisabled = function(disabled) {
        $(".bold").attr('disabled', disabled);
        $(".italic").attr('disabled', disabled);
        $(".underline").attr('disabled', disabled);
        $(".bullets").attr('disabled', disabled);
        $(".numbers").attr('disabled', disabled);
        $("#add-custom-field-id").attr('disabled', disabled);
    }

    vm.showEmailPreview = function () {
        var cfIds = "";
        vm.emailPreviewString = $scope.trixModel.substring(0, $scope.trixModel.length);

        while ((cfIds = vm.emailPreviewString.match(/%.*?%/)) !== null) {
            var customFieldTypeId = vm.sharedData.customFieldById[cfIds[0].substring(1, cfIds[0].length - 1)].TypeId;
            if (customFieldTypeId) {
                switch (customFieldTypeId) {
                    case fieldTypesEnum.Dropdown:
                        vm.emailPreviewString = vm.emailPreviewString.replace(cfIds[0], 'dropdown option');
                        break;
                    case fieldTypesEnum.Text:
                        vm.emailPreviewString = vm.emailPreviewString.replace(cfIds[0], 'some text');
                        break;
                    case fieldTypesEnum.LongText:
                        vm.emailPreviewString = vm.emailPreviewString.replace(cfIds[0], 'Lorem ipsum dolor sit amet, consectetur adipiscing elit.<br/>' +
                            ' Donec ultrices vehicula nunc, et convallis orci.<br/> ' +
                            'Donec hendrerit dignissim consequat. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus.<br/>' +
                            ' Quisque eget ultricies massa. Proin imperdiet sem ut rutrum accumsan');
                        break;
                    case fieldTypesEnum.RelatedDropdowns:
                        vm.emailPreviewString = vm.emailPreviewString.replace(cfIds[0], 'related dropdown option');
                        break;
                    case fieldTypesEnum.Checkbox:
                        vm.emailPreviewString = vm.emailPreviewString.replace(cfIds[0], 'no');
                        break;
                    case fieldTypesEnum.Date:
                        vm.emailPreviewString = vm.emailPreviewString.replace(cfIds[0], '14/02/1946-00:00:00');
                        break;
                    case fieldTypesEnum.Numeric:
                        vm.emailPreviewString = vm.emailPreviewString.replace(cfIds[0], '42');
                        break;
                    case fieldTypesEnum.Email:
                        vm.emailPreviewString = vm.emailPreviewString.replace(cfIds[0], 'example@gmail.com');
                        break;
                    case fieldTypesEnum.MultipleSelection:
                        vm.emailPreviewString = vm.emailPreviewString.replace(cfIds[0], 'selected option 1, selected option 2...');
                        break;
                    case fieldTypesEnum.Attachments:
                        vm.emailPreviewString = vm.emailPreviewString.replace(cfIds[0], '<a>Attachment 1</a>, <a>Attachment 2</a>');
                        break;
                    default: 
                        throw "Encountered unknown field id: " + customFieldTypeId;

                }
                
            } else {
                throw "Encoutered unknown custom field id: " + cfIds[0].substring(1, cfIds[0].length - 1);
            }
            
        }
        console.log(vm.emailPreviewString);

    }




    vm.showEmailEditor = function() {
        vm.emailPreviewString = "";
    }




    emailTemplatesService.loadEmailTemplates().then(function (data) {
        console.log(data);
        var table = $("#sample_1").DataTable();
        table.rows.add(data).draw();

    });




    vm.saveChanges = function () {
        $.blockElement($("#edit-department"));
        validateForm().then(function(isValid) {
            if (isValid) {

                if (indexOfRowInEdit === -1) {
                    emailTemplatesService.createNewTemplate($scope.trixModel).then(function (data) {
                        $.unblockElement($("#edit-department"));
                        data = convertServerToDisplay(data);
                        $("#sample_1").DataTable().row.add(data).draw();
                        toastr["success"]("Email template was successfully created!");
                        vm.closeModal();
                    });
                } else {
                    emailTemplatesService.updateTemplate($scope.trixModel).then(function (data) {
                        $.unblockElement($("#edit-department"));
                        data = convertServerToDisplay(data);
                        $("#sample_1").DataTable().row(indexOfRowInEdit).data(data);
                        $("#sample_1").DataTable().row(indexOfRowInEdit).invalidate().draw();
                        toastr["success"]("Email template was successfully updated!");
                        vm.closeModal();

                    });
                }
            } else {
                $.unblockElement($("#edit-department"));
            }
        });
    }



    function convertServerToDisplay(data){
        data['del'] = 'delete';
        data['edit'] = 'edit';
        var arr = data.EmailTemplateSendTo.map(function (val) {
            return val.SendTo;
        });
        data['EmailTemplateSendToString'] = arr.join(",");
        return data;
    }



    vm.fieldIdentifierSelectionChanged = function () {
        if (vm.sharedData.selectedFieldIdentifier.Id !== "0") {
            var element = document.querySelector("trix-editor");
            element.editor.insertHTML("%" + vm.sharedData.selectedFieldIdentifier.Id + "%");
        }

    }



    vm.sendTestEmail = function() {
        emailTemplatesService.sendTestEmail(vm.emailPreviewString);
    }



    vm.closeModal = function () {
        closeModal();
    }



    function closeModal() {

        $("#edit-department").modal("hide");
        indexOfRowInEdit = -1;
        $scope.trixModel = "";
        emailTemplatesService.resetModalSelections();
        $timeout(function () {
            $("#send-email-to").multiselect('refresh');
        });
        $("#edit-tab-li").addClass("active");
        $("#edit_tab").addClass("active");
        $("#preview-tab-li").removeClass("active");
        $("#preview_tab").removeClass("active");
        $scope.emailForm.$setUntouched();
        $('trix-editor').removeClass('aha-invalid');
        $($(".multiselect-native-select").children()[1]).removeClass('aha-invalid');
    }




    function validateForm() {
        return $q(function(resolve, reject) {

            var valid = true;

            if ($scope.emailForm.templateName.$invalid) {
                $scope.emailForm.templateName.$touched = true;
                valid = false;
            }

            if ($scope.emailForm.templateSubject.$invalid) {
                $scope.emailForm.templateSubject.$touched = true;
                valid = false;
            }

            if ($scope.emailForm.selectedAction.$viewValue.Id === '0') {
                $scope.emailForm.selectedAction.$touched = true;
                valid = false;
            }

            if ($scope.emailForm.selectedSendTos.$viewValue.length === 0) {
                valid = false;
                $($(".multiselect-native-select").children()[1]).addClass('aha-invalid');
            } else
                $($(".multiselect-native-select").children()[1]).removeClass('aha-invalid');


            if (typeof $scope.trixModel == 'undefined' || $scope.trixModel == '') {
                valid = false;
                $('trix-editor').addClass('aha-invalid');
            } else
                $('trix-editor').removeClass('aha-invalid');

            if (!valid) {
                resolve(valid);
                return;
            }


            networkService.isEmailTemplateNameUnique(vm.sharedData.selectedEmailTemplate.Name, vm.sharedData.selectedEmailTemplate.Id).then(function (data) {
                if (!data) {
                    toastr["error"]("Email template name is not unique!");
                    $scope.emailForm.templateName.$invalid = true;
                    $scope.emailForm.templateName.$touched = true;
                }

                resolve(data);
            });
        });
    }



    $timeout(function () {
        var table = $("#sample_1");
        $('#sample_1 tbody').on('click', 'a.delete', function () {
            var row = table.DataTable().row($(this).parents('tr'));
            var idToRemove = table.DataTable().data()[row.index()].Id;
            bootbox.confirm("Are you sure you want delete this email template?", function(result) {
                if (result) {
                    crudService.remove("EmailTemplate", idToRemove).then(function() {
                        table.DataTable().row(row).remove().draw();
                        toastr["success"]("Email template was successfully removed!");
                    });
                }
            });
        });

        $('#sample_1 tbody').on('click', 'a.edit', function() {
            indexOfRowInEdit = table.DataTable().row($(this).parents('tr')).index();
            vm.sharedData.selectedEmailTemplate = $.extend(true, {}, table.DataTable().data()[indexOfRowInEdit]);
            vm.sharedData.selectedUsers = vm.sharedData.selectedEmailTemplate.EmailTemplateSendTo;
            $scope.trixModel = vm.sharedData.selectedEmailTemplate.EmailTemplate;
            $timeout(function () {
                $("#send-email-to").multiselect('refresh');
            });
            vm.sharedData.selectedAction = vm.sharedData.actions.find(function (val) { return val.Name === vm.sharedData.selectedEmailTemplate.EmailAction });

            $(".src").unbind('click', vm.onSrcClick);
            $(".src").bind('click', vm.onSrcClick);

            $scope.$apply();
        });
    });



    $timeout(function () {
        Trix.config.textAttributes.underline = {
            style: { "textDecoration": "underline" },
            inheritable: true
        }
        var buttonHTML = "<button type=\"button\" class=\"underline\" data-attribute=\"underline\" title=\"underline\">Underline</button>";
        var groupElement = Trix.config.toolbar.content.querySelector(".text_tools");
        console.log(groupElement);
        groupElement.insertAdjacentHTML("beforeend", buttonHTML);
    });
    return vm;
});