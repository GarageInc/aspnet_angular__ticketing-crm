ticketsApp.controller("ahaFileAttachmentController", function(ahaFileAttachmentService, $scope, $timeout, $rootScope) {
    var vm = this;
    ahaFileAttachmentService.registerComponent();

    var loadedFileIds = [];
    var filePicker = null;

    console.log($rootScope);

    $scope.options = {
        maxFileSize: 10 * 1024 * 1024,
        type: "POST",
        url: '/File/UploadFiles',

        acceptFileTypes: /(\.|\/)(gif|jpe?g|png)$/i,

        stop: function (e) {
            ahaFileAttachmentService.ds_callback(loadedFileIds);
            loadedFileIds = [];
            if(filePicker)
                filePicker.scope.clear(filePicker.files);
            $timeout(function() {
                $("#attach-files").modal("hide");
            }, 1000);
            
        },
        done: function (e, data) {
            filePicker = data;
            loadedFileIds.push({ fileId : data.jqXHR.responseJSON[0].FileId, fileName: data.jqXHR.responseJSON[0].Name });
        }
    };
    vm.closeFileAttachmentDialog = function () {
        
        loadedFileIds = [];
        $scope.queue = [];
        if(filePicker)
            filePicker.scope.clear(filePicker.files);
        $("#attach-files").modal("hide");
        if(ahaFileAttachmentService.dc_callback)
            ahaFileAttachmentService.dc_callback();
    }


    $scope.files = [];
    return vm;
});