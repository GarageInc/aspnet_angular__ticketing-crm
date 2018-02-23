emailTemplatesApp.service("emailTemplatesService", function (DropdownDatasource, networkService, crudService, $q) {
    var vm = this;

    vm.users = [];
    vm.selectedUsers = [];

    vm.fieldIdentifiers = [];
    vm.customFieldById = {};
    vm.selectedFieldIdentifier = {};

    vm.actions = [];
    vm.selectedAction = {};

    vm.selectedEmailTemplate = {};

    var dds = DropdownDatasource;
    var rb = dds.requestBuilder;
    vm.loadUsers = function () {
        return $q(function (resolve, reject) {
            dds.sendRequest(rb().setPath("/SendEmailTos").setNullValue("All").build()).then(function (users) {
                users.pop();
                users.shift();
                vm.users = users;
                resolve();
            });
        });
    }
    vm.loadIdentifiers= function () {
        return $q(function (resolve, reject) {
            dds.sendRequest(rb().setPath("/CustomFieldIdentifiersForEmailTemplate").
                setNullValue("-Select field-").
                build()).then(function (fieldIdentifiers) {
                    console.log(fieldIdentifiers);
                fieldIdentifiers.forEach(function(val) {
                    vm.customFieldById[val.Id] = val;
                });
                vm.fieldIdentifiers = fieldIdentifiers;
                vm.selectedFieldIdentifier = vm.fieldIdentifiers[0];
                resolve();

            });
        });
    }
    vm.loadActions = function() {
        return $q(function (resolve, reject) {
            dds.sendRequest(rb().setPath("/SendEmailActions").
                setNullValue("-Select action-").
                build()).then(function (actions) {
                   
                    vm.actions = actions;
                    vm.selectedAction = actions[0];
                    resolve();

                });
        });
    }

    vm.createNewTemplate = function (trixModel) {
        var body = {
            Name: vm.selectedEmailTemplate.Name,
            EmailTemplate: trixModel,
            EmailAction: vm.selectedAction.Name,
            EmailSubject: vm.selectedEmailTemplate.EmailSubject,
            EmailActionKey: vm.selectedAction.Id,
            EmailTemplateSendTo:vm.selectedUsers
            
        };
        console.log(body);

        return crudService.create("EmailTemplate", body);
    }

    vm.updateTemplate = function (trixModel) {
        var body = {
            Id: vm.selectedEmailTemplate.Id,
            Name: vm.selectedEmailTemplate.Name,
            EmailTemplate: trixModel,
            EmailAction: vm.selectedAction.Name,
            EmailSubject: vm.selectedEmailTemplate.EmailSubject,
            EmailActionKey: vm.selectedAction.Id,
            EmailTemplateSendTo: vm.selectedUsers

        };
        console.log(body);

        return crudService.update("EmailTemplate", body);
    }

    vm.sendTestEmail = function(renderedBody) {
        return networkService.sendRequest(networkService.requestBuilder().method("POST").url("/EmailTemplate/SendTestEmail").body({ bodyHtml: encodeURI(renderedBody) }).build());
    }



    //main table
    vm.loadEmailTemplates = function () {
        return $q(function (resolve, reject) {
            crudService.read("EmailTemplate").then(function (data) {

                resolve(data.map(function(val, idx, arr) {
                    val["edit"] = "edit";
                    val["del"] = "delete";
                    
                    return val;
                }));
            });
        });
    }

    vm.resetModalSelections = function() {
        vm.selectedUsers = [];
        vm.selectedFieldIdentifier = vm.fieldIdentifiers[0];
        vm.selectedAction = vm.actions[0];
        vm.selectedEmailTemplate = {};
    }


    return vm;

});