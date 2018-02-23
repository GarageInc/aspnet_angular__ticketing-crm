ticketsApp.directive('createTicketWizard', function() {
    return {
        restrict: 'E',
        templateUrl: '/Tickets/Ticket/CreateTicketWizard',
        controller: 'createTicketWizardController',
        controllerAs: 'cwctrl',
        scope: {
            onCreate: '=',
            onClose: '='
        }
    };
});
