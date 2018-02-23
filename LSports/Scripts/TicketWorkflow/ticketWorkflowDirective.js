ticketWorkflowApp.directive("ticketWorkflowDirective", function ($timeout, ticketWorkflowService) {
    return {
        restrict: "A",
        // templateUrl: "/Department/Central",
        controller: "ticketWorkflowController",
        controllerAs: "ctrl",
        link: function (s, e, a) {
            var table = $("#sample_1").DataTable({
                rowReorder: {
                    selector: 'td.row-controll',
                    dataSrc: "SortOrder"
                },
                ordering: true,
                columns: [
                    { "data": "SortOrder" },
                    { "data": "Id" },
                    { "data": "CustomerPriorityId" },
                    { "data": "TicketType" },
                    { "data": "Product" },
                    { "data": "ProductCategory" },
                    { "data": "IssueType" },
                    { "data": "Department.Name" },
                    { "data": "edit" },
                    { "data": "del" }

                ],
                columnDefs: [
                    {
                        "targets": 0,
                        "visible": false,
                        "orderable": true
                    },
                { "width": "5%", "type": "num", "targets": 1, "class": "row-controll", "orderable": false },
                { "width": "16%", "type": "string", "targets": 2, "class": "row-controll", "orderable": false },
                { "width": "16%", "type": "string", "targets": 3, "class": "row-controll", "orderable": false },
                { "width": "16%", "type": "string", "targets": 4, "class": "row-controll", "orderable": false },
                { "width": "16%", "type": "string", "targets": 5, "class": "row-controll", "orderable": false },
                { "width": "16%", "type": "string", "targets": 6, "class": "row-controll", "orderable": false },
                { "width": "16%", "type": "string", "targets": 7, "class": "row-controll", "orderable": false },
                {
                    "width": "16%",
                    "targets": [3,4,5,6],
                    "render": function (data, type, row) {
                        return  (data===null ? "ANY" : data.Name) ;
                    }
                },
                {
                    "width": "16%",
                    "targets": [2],
                    "render": function (data, type, row) {
                        return ((data === null || data === 0) ? "ANY" : data);
                    }
                },
                {
                    "width": "10%",
                    "targets": 8,
                    "orderable": false, 
                    "render": function (data, type, row) {
                        return '<div style="text-align:center"><a style="cursor: pointer;" class="edit"  data-toggle="modal" href="#edit-department">' + data + '</a></div>';
                    }
                },
                {
                    "width": "10%",
                    "targets": 9,
                    "orderable": false,
                    "render": function (data, type, row) {
                        return '<div style="text-align:center"><a style="cursor: pointer;" class="delete">' + data + '</a></div>';
                    }
                }
                ]
            });

            table.on('row-reordered', function (e, details, edit) {
                if (details.length > 0) {
                    var bd = details.map(function (val, idx, arr) {
                        return {
                            Id: parseInt($(val.node).children()[0].innerText),
                            SortOrder: val.newData

                        };
                    });
                    ticketWorkflowService.updateSortOrder(bd).then(function () {
                        toastr["success"]("Ticket workflow sort order updated!");
                    });

                }
            });
        }
    };
});