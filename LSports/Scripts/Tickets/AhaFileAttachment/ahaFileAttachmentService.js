ticketsApp.service("ahaFileAttachmentService", function() {
    var vm = this;
    var componentRegistered = false;

    vm.registerComponent = function() {
        if (!componentRegistered)
            componentRegistered = true;
        else {
            throw "AhaFileAttchment component already present on this page!";
        }
    }

    vm.dc_callback = null;
    vm.ds_callback = null;

    vm.setDialogClosedCallBack = function(callback) {
        vm.dc_callback = callback;
    }

    vm.setDialogSavedCallback = function(callback) {
        vm.ds_callback = callback;
    }
    return vm;
});