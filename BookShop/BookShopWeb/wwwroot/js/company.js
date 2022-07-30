var companyDataTable;
$(document).ready(function () {
    LoadCompanyDataTable();
});

function LoadCompanyDataTable() {
    companyDataTable = $('#companyTable').DataTable({
        "ajax": {
            "url": "/Admin/Company/GetAll"
        },
        "columns": [
            { "data": "name", "width": "30%" },
            { "data": "address", "width": "60%" },
            { "data": "city", "width": "20%" },
            { "data": "state", "width": "20%" },
            { "data": "postalCode", "width": "10%" },
            { "data": "phoneNumber", "width": "10%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                             <div class="group" style = "width:220px">
                                <a class="btn btn-outline-info" href="/Admin/Company/CompanyDetails?id=${data}" style="width:90px">
                                   <i class="bi bi-pencil"></i>&nbsp; Edit
                                 </a>
                                  <a class="btn btn-outline-danger" onClick=Delete('/Admin/Company/Delete/${data}') style="width:120px">
                                   <i class="bi bi-trash"></i>&nbsp; Remove
                                 </a>
                            </div>
                            `;
                }
            }
        ]
    });
}
function Delete(url) {
    $.ajax({
        url: url,
        type: "Delete",
        success: function (data) {
            if (data.success) {
                companyDataTable.ajax.reload();
                toastr.success(data.message);
            }
            else {
                toastr.error(data.message);
            }
        }
    })
}