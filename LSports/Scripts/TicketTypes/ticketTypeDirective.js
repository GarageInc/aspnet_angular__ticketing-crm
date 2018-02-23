ticketTypeApp.directive("ticketTypeDirective", function ($timeout) {
    return {
        restrict: "A",
        //templateUrl: "/TicketType/Central",
        controller: "ticketTypeController",
        controllerAs: "ctrl",
        link: function(s, e, a) {
            
        }
    };
});