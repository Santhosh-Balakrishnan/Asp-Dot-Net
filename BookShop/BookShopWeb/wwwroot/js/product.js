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
            { "data": "name", "width": "30%" },
            { "data": "isbn", "width": "15%" },
            { "data": "author", "width": "20%" },
            { "data": "category.name", "width": "15%" },
            { "data": "price", "width": "10%" },
            { "data": "listPrice", "width": "10%" },
            { "data": "price50", "width": "10%" },
            { "data": "price100", "width": "10%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="group" style = "width:220px">
                           <a class="btn btn-outline-info" href="/Admin/Product/ProductDetails?id=${data}" style="width:90px">
                              <i class="bi bi-pencil"></i>&nbsp; Edit
                            </a>
                             <a class="btn btn-outline-danger" onClick=Delete('/Admin/Product/Delete/${data}') style="width:120px">
                              <i class="bi bi-trash"></i>&nbsp; Remove
                            </a>
                        </div>
                    `
                },
            }
        ]
    });
}
function Delete(url) {
    $.ajax({
        url: url,
        type: "Delete",
        success: function (data)
        {
            if (data.success)
            {
                dataTable.ajax.reload();
                toastr.success(data.message);
            }
            else
            {
                toastr.error(data.message);
            }
        }
    })
}
