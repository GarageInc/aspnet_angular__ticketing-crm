dashboard.directive("dashboardRightSidebarDirective", function () {

    return {
        restrict: "E",
        templateUrl: "/Dashboard/DashboardRightSidebar",
        controller: "dashboardRightSidebarController",
        constrollerAs: "ctrl",
        link: function(scope, elem, attrs) {
            Metronic.init();
            Layout.init(); // init current layout
            
            Demo.init(); // init demo features
            QuickSidebar.init(); // init quick sidebar
        }
    };
});