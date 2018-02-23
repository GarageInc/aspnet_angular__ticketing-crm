departmentApp.directive("departmentDirective", function ($timeout) {
    return {
        restrict: "A",
        //templateUrl: "/Tickets/Department/Central",
        controller: "departmentController",
        controllerAs: "ctrl",
        link: function(s, e, a) {
            var table = $("#sample_1");
            table.DataTable({
                columns: [
                    { "data": "Id" },
                    { "data": "Name" },
                    { "data": "edit" },
                    { "data": "del" }],
                columnDefs: [
                    { "width": "10%", "type": "num", "targets": 0 },
                    { "width": "50%", "type": "string", "targets": 1 },
                    {
                        "width": "20%",
                        "targets": 2,
                        "render": function (data, type, row) {
                            return '<div style="text-align:center"><a style="cursor: pointer;" class="edit"  data-toggle="modal" href="#edit-department">' + data + '</a></div>';
                        }
                    },
                    {
                        "width": "20%", "targets": 3,
                        "render": function (data, type, row) {
                            return '<div style="text-align:center"><a style="cursor: pointer;" class="delete">' + data + '</a></div>';
                        }
                    }
                ]
            });
            var userRoleTable = $("#user_role_table");
            userRoleTable.DataTable({
                "lengthChange": false,
                "searching": false,
                columns: [
                    { "data": "Name" },
                    { "data": "Role" },
                    { "data": "del" }
                ],
                columnDefs: [
                    { "width": "10%", "type": "num", "targets": 0 },
                    { "width": "10%", "type": "string", "targets": 1 },
                    {
                        "width": "20%",
                        "targets": 2,
                        "render": function (data, type, row) {
                            return '<div style="text-align:center"><a class="delete">' + data + '</a></div>';
                        }
                    }
                ]
            });
        }
    };
});