emailTemplatesApp.directive("emailTemplatesDirective", function ($timeout) {
    return {
        restrict: "A",
       // templateUrl: "/Department/Central",
        controller: "emailTemplatesController",
        controllerAs: "ctrl",
        link: function (s, e, a) {
            var table = $("#sample_1");
            table.DataTable({
                columns: [
                    { "data": "Id" },
                    { "data": "Name" },
                    { "data": "EmailAction" },
                    { "data": "EmailTemplateSendToString" },
                    { "data": "edit" },
                    { "data": "del" }],
                columnDefs: [
                    { "width": "5%", "type": "num", "targets": 0 },
                    { "width": "25%", "type": "string", "targets": 1 },
                    { "width": "25%", "type": "string", "targets": 2 },
                    { "width": "25%", "type": "string", "targets": 3 },
                    {
                        "width": "10%",
                        "targets": 4,
                        "render": function (data, type, row) {
                            return '<div style="text-align:center"><a style="cursor: pointer;" class="edit"  data-toggle="modal" href="#edit-department">' + data + '</a></div>';
                        }
                    },
                    {
                        "width": "10%", "targets": 5,
                        "render": function (data, type, row) {
                            return '<div style="text-align:center"><a style="cursor: pointer;" class="delete">' + data + '</a></div>';
                        }
                    }
                ]
            });
        }
    };
});