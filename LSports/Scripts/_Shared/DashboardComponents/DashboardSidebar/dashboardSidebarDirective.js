dashboard.directive("dashboardSidebarDirective", function () {

    return {
        restrict: "E",
        templateUrl: "/Dashboard/DashboardSidebar",
        controller: "dashboardSidebarController",
        constrollerAs: "ctrl",
        link: function (scope, elem, attrs) { }
    };
});