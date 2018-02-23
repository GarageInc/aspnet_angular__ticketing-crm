staffManagementApp.directive("staffManagementDirective", function ($timeout) {
    return {
        restrict: "A",
        //templateUrl: "/Department/Central",
        controller: "staffManagementController",
        controllerAs: "ctrl",
        link: function (s, e, a) {
            var table = $("#sample_1");
            table.DataTable({
                columns: [
                    { "data": "Name" },
                    { "data": "FirstName" },
                    { "data": "LastName" },
                    { "data": "Name" },
                    { "data": "edit" },
                    { "data": "del" }],
                columnDefs: [
                    { "width": "15%", "type": "string", "targets": 0 },
                    { "width": "25%", "type": "string", "targets": 1 },
                    { "width": "10%", "type": "string", "targets": 2 },
                    { "width": "10%", "type": "string", "targets": 3 },
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
                            return '<div style="text-align:center"><a style="cursor: pointer;" class="delete">' + data + '</a></div>';
                        }
                    }
                ]
            });
        }
    };
});