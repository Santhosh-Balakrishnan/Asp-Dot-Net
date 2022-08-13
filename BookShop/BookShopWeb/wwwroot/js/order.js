var dataTable;
$(document).ready(function ()
{
    var url = window.location.search;
    var searchStrings = url.split('=');
    var status = "";
    if (searchStrings.length == 2) {
        status = searchStrings[1];
    }
    LoadDataTable(status);
});

function LoadDataTable(status) {
    dataTable = $('#orderTable').DataTable({
        "ajax": {
            "url":"/Admin/Order/GetAll?status="+status
        },
        "columns": [
            { "data": "id", "width": "5%" },
            { "data": "name", "width": "20%" },
            { "data": "applicationUser.email", "width": "25%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "orderStatus", "width": "10%" },
            { "data": "orderTotal", "width": "10%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="group" style = "width:220px">
                           <a class="btn btn-outline-info" href="/Admin/Order/Details?orderId=${data}" style="width:90px">
                              <i class="bi bi-pencil"></i>
                            </a>
                        </div>
                    `
                },
            }
        ]
    });
}
