customFieldsApp.directive("customFieldsDirective", function ($timeout) {
    return {
        restrict: "A",
       // templateUrl: "/Department/Central",
        controller: "customFieldsController",
        controllerAs: "ctrl",
        link: function (s, e, a) {
            var table = $("#sample_1");
            table.DataTable({
                columns: [
                    { "data": "Id" },
                    { "data": "Name" },
                    { "data": "Title" },
                    { "data": "Identifier" },
                    { "data": "CustomFieldType.Name" },
                    { "data": "edit" },
                    { "data": "del" }],
                columnDefs: [
                    { "width": "5%", "type": "num", "targets": 0 },
                    { "width": "16%", "type": "string", "targets": 1 },
                    { "width": "16%", "type": "string", "targets": 2 },
                    { "width": "16%", "type": "string", "targets": 3 },
                    { "width": "16%", "type": "string", "targets": 4 },
                    {
                        "width": "10%",
                        "targets": 5,
                        "render": function (data, type, row) {
                            return '<div style="text-align:center"><a style="cursor: pointer;" class="edit"  data-toggle="modal" href="#edit-department">' + data + '</a></div>';
                        }
                    },
                    {
                        "width": "10%", "targets": 6,
                        "render": function (data, type, row) {
                            if (row.Id > 0)
                                return '<div style="text-align:center"><a style="cursor: pointer;" class="delete">' + data + '</a></div>';
                            else
                                return '<div style="text-align:center">Cannot delete</div>';
                        }
                    }
                ]
            });
        }
    };
});