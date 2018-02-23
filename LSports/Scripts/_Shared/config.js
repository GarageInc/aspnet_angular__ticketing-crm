fieldTypesEnum = {
    Dropdown: 3,
    Text: 1, // check
    LongText: 2, // check
    RelatedDropdowns: 4,
    Checkbox: 5, // check
    Date: 6,
    Attachments: 7,
    Numeric: 8,
    Email: 9, // check
    MultipleSelection: 10
};
controlsToShow = {
    BtnEditTicket: 1,
    BtnReply: 2,
    BtnComment: 3,
    BtnAssignToAnotherDepartment: 4,
    BtnAssignToAnotherUser: 5,
    BtnAssgnToMe: 6,
    BtnUnassign: 7,
    BtnReopenTicket: 8,
    BtnCloseTicket: 9,

    LblAssignedTo: 100
};
$.urlParam = function (name) {
    var results = new RegExp('[\?&]' + name + '=([^&#]*)').exec(window.location.href);
    if (results != null)
        return results[1];
    else
        return null;
}

$.blockElement = function(elem) {
    
    var height = elem.height();
    elem.block({
        message: '<div>' +
            '<div class="block-spinner-bar">' +
            '<div class="bounce1"></div>' +
            '<div class="bounce2"></div>' +
            '<div class="bounce3"></div>' +
            '</div></div>',
        css: {
            border: 'none', background: 'transparent', top: height - 10
        }
    });
}

$.unblockElement = function(elem) {
    
    elem.unblock();
}

function htmlEncode(value) {
    return $('<div/>').text(value).html();
}

function htmlDecode(value) {
    return $('<div/>').html(value).text();
}

