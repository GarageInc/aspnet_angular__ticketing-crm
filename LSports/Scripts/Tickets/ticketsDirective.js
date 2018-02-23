ticketsApp.directive("ticketsDirective", function ($timeout) {
    return {
        restrict: "A",
       // templateUrl: "/Department/Central",
        controller: "ticketsController",
        controllerAs: "ctrl",
        link: function (s, e, a) {
            if (isCustomer) {
                $("#customer_column").remove();
                $("#priority_column").remove();
                $("#assigned_to_column").remove();
            }

            var table = $("#sample_1");
            table.DataTable({
                order: [[ 1, 'desc' ]],
                columns: getColumns(),
                    //{ "data": "del" }],
                columnDefs: getColumnDefs()
            });
            $("#ticket_history_table").DataTable({
                columns: [
                    { "data": "LogDate" },
                    { "data": "EntryValue" }],
                columnDefs: [
                    {
                        "width": "20%", "type": "date", "targets": 0,
                        "render": function (data, type, row) {
                            var creationDate = new Date(parseInt(data.match(/\d+/g)[0]));

                            var month = creationDate.getMonth() + 1;
                            var day = creationDate.getDate();
                            var year = creationDate.getFullYear();
                            var hours = creationDate.getHours();
                            var minutes = creationDate.getMinutes();
                            var seconds = creationDate.getSeconds();

                            function padNumber(num, size) {
                                var s = String(num);
                                while (s.length < (size || 2)) { s = "0" + s; }
                                return s;
                            }

                            return month + "/" + day + "/" + year + " " + padNumber(hours) + ":" + padNumber(minutes) + ":" + padNumber(seconds);
                        }
                    },
                    {
                        "width": "80%",
                        "type": "string",
                        "targets": 1,
                        "render": function (data, type, row) {
                           // console.log(decodeURI(data));
                            return htmlDecode(data);
                        }
                    }
                ]
            });
        }
    };
});

var columns = [
    { "data": "Id" },
    { "data": "CreationDate" },
    { "data": "TicketType.Name" },
    { "data": "Product.Name" },
    { "data": "ProductCategory.Name" },
    { "data": "IssueType.Name" },
    { "data": "PriorityId" },
    { "data": "Customer.Company" },
    { "data": "CustomerContact.Name" },
    { "data": "TicketStatus.Name" },
    { "data": "AssignedToUser.Name" },
    { "data": "edit" }
];

var columnDefs = [
    { "width": "5%", "type": "num", "targets": 0 },
    {
        "width": "9%",
        "type": "date",
        "targets": 1,
        "render": function(data, type, row) {
            var creationDate = new Date(parseInt(data.match(/\d+/g)[0]));

            var month = creationDate.getMonth() + 1;
            var day = creationDate.getDate();
            var year = creationDate.getFullYear();
            var hours = creationDate.getHours();
            var minutes = creationDate.getMinutes();
            var seconds = creationDate.getSeconds();

            function padNumber(num, size) {
                var s = String(num);
                while (s.length < (size || 2)) {
                    s = "0" + s;
                }
                return s;
            }

            return month + "/" + day + "/" + year + " " + padNumber(hours) + ":" + padNumber(minutes) + ":" + padNumber(seconds);
        }
    },
    {
        "width": "9%",
        "type": "string",
        "targets": 2
    },
    {
        "width": "9%",
        "type": "string",
        "targets": 3
    },
    {
        "width": "9%",
        "type": "string",
        "targets": 4
    },
    {
        "width": "9%",
        "type": "string",
        "targets": 5
    },
    {
        "width": "5%",
        "type": "string",
        "targets": 6
    },
    {
        "width": "9%",
        "type": "string",
        "targets": 7
    },
    {
        "width": "9%",
        "type": "string",
        "targets": 8
    },
    {
        "width": "9%",
        "type": "string",
        "targets": 9
    },
    {
        "width": "9%",
        "type": "string",
        "targets": 10
    },
    {
        "width": "9%",
        "targets": 11,
        "orderable": false,
        "render": function(data, type, row) {
            return '<div style="text-align:center"><a style="cursor: pointer;" class="edit"  data-toggle="modal" href="#edit-department">details</a></div>';
        }
    }
    //{
    //    "width": "12%", "targets": 11,
    //    "render": function (data, type, row) {
    //        return '<div style="text-align:center"><a style="cursor: pointer;" class="delete">' + data + '</a></div>';
    //    }
    //}
];

function getColumns() {
    if (isCustomer) {
        columns.splice(10, 1);
        columns.splice(6, 2);
        return columns;   
    } else {
        return columns;
    }
}

function getColumnDefs() {
    if (isCustomer) {
        columnDefs.splice(10, 1);
        columnDefs.splice(6, 2);
        for (var i = 0; i < columnDefs.length; i++) {
            columnDefs[i].targets = i;
        }
        return columnDefs;
        
    } else {
        return columnDefs;
    }
}

