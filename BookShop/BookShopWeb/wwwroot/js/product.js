var dataTable;
$(document).ready(function ()
{
    LoadDataTable();
});

function LoadDataTable() {
    dataTable = $('#productTable').DataTable({
        "ajax": {
            "url":"/Admin/Product/GetAll"
        },
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "isbn", "width": "15%" },
            { "data": "author", "width": "15%" },
            { "data": "price", "width": "15%" },
            { "data": "category.name", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="group" style = "width:220px">
                           <a class="btn btn-outline-info" href="/Admin/Product/ProductDetails?id=${data}" style="width:90px">
                              <i class="bi bi-pencil"></i>&nbsp; Edit
                            </a>
                             <a class="btn btn-outline-danger" style="width:120px">
                              <i class="bi bi-trash"></i>&nbsp; Remove
                            </a>
                        </div>
                    `
                },
            }
        ]
    });
}