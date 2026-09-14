$(document).ready(function () {

    LoadGrid();

});
function LoadGrid() {

    $.ajax({
        url: '/Staff/GetStaffUserList',
        type: 'GET',
        success: function (res) {
           
            if (res.success) {
                console.log(res.data)
                RenderGrid(res.data);
            }
        },
        error: function () {
            alert("Unable to load data.");
        }
    });
}
function RenderGrid(data) {
    console.log(data);

    if ($.fn.DataTable.isDataTable("#tblStaffUser")) {
        $("#tblStaffUser").DataTable().destroy();
    }

    var tbody = $("#tblStaffUser tbody");
    tbody.empty();

    if (data != null && data.length > 0) {
        $.each(data, function (i, item) {
            tbody.append(`
                <tr>
                    <td>${i + 1}</td>
                    <td>${item.staffName}</td>
                    <td>${item.userName}</td>
                    <td>${item.password}</td>
                    <td>${item.roleName ?? '-'}</td>
                    <td>
                        ${item.isActive
                    ? '<span class="badge bg-success">Active</span>'
                    : '<span class="badge bg-danger">Inactive</span>'}
                    </td>
                    <td>
                        <button class="btn btn-sm btn-primary btnEdit" data-id="${item.userId}">
                            <i class="ti ti-pencil"></i>
                        </button>
                         <button class="btn btn-sm btn-primary btnPassword" data-id="${item.userId}">
                            <i class="ti ti-key"></i>
                        </button>
                    </td>
                </tr>
            `);
        });
    } else {
        tbody.append(`
            <tr>
                <td colspan="6" class="text-center text-muted">No records found.</td>
            </tr>
        `);
    }

    $("#tblStaffUser").DataTable();
}
//function LoadGrid() {

//    $("#UserListDiv").load("/Staff/GetStaffUserList", function () {

//        if ($.fn.DataTable.isDataTable('#tblStaffUser')) {

//            $('#tblStaffUser').DataTable().destroy();

//        }

//        $('#tblStaffUser').DataTable({

//            destroy: true,

//            responsive: true,

//            pageLength: 25,

//            ordering: true,

//            searching: true

//        });

//    });

//}
//function LoadGrid() {

//    $("#UserListDiv").load("/StaffUser/GetStaffUserList", function () {

//        $("#tblStaffUser").DataTable();

//    });

//}
$(document).on("click", "#btnSave", function (e) {
    e.preventDefault();
    Save();
});
function Save() {

    if (!ValidateForm())
        return;
    var userId = $("#UserId").val();
    var staffId = $("#StaffId").val();
    var roleId = $("#RoleId").val();

    if (!staffId) { alert("Please select staff."); return; }
    if (!roleId) { alert("Please select role."); return; }

    if (userId) {
        // UPDATE — UserId hai matlab edit mode hai
        UpdateUserRole(userId, roleId);
    } else {
        // SAVE — UserId nahi hai matlab new user create karna hai
        CreateUser(staffId, roleId);
    }

    

}
function CreateUser(staffId, roleId, userId) {
    var model = {

        UserId: userId,

        StaffId: staffId,

        RoleId: roleId

    };
    $.post("/Staff/SaveStaffLogins", { model: model,}, function (res) {
        if (res.status === 1) {
           alert(res.message);
            ClearForm();
            LoadGrid();
        } else {
            alert(res.message);
        }
    });
}
$(document).on("click", ".btnEdit", function () {

    Edit($(this).data("id"));

});
function Edit(userId) {
    $.get("/Staff/SaveStaffLogins",
        { userId: userId },
        function (res) {
            $("#UserId").val(res.userId);
            $("#OldRoleId").val(res.roleId);  // Old role save karo
            $("#RoleId").val(res.roleId).trigger('change');
            $("#btnSave")
                .removeClass("btn-success")
                .addClass("btn-primary")
                .text("Update");

            ReloadStaffDropdown(res.staffId, function () {
                $("#StaffId").prop("disabled", true);
            });
        });
}
function UpdateUserRole(userId, newRoleId) {
    var oldRoleId = $("#OldRoleId").val();  // Pehle wali role

    $.post("/Staff/UpdateUserRole",
        { userId: userId, oldRoleId: oldRoleId, newRoleId: newRoleId },
        function (res) {
            if (res.status === 1) {
               alert(res.message);
                ClearForm();
                LoadGrid();
            } else {
                alert(res.message);
            }
        });
}
$("#btnClear").click(function () {

    ClearForm();

});
//function ClearForm() {

//    $("#UserId").val("");

//    $("#StaffId").val("");

//    $("#RoleId").val("");

//    $("#StaffId").prop("disabled", false);

//    $("#btnSave").text("Save");

//}
function ClearForm() {

    $("#UserId").val("");

    ReloadStaffDropdown();

    $("#RoleId").val("");

    $("#StaffId").prop("disabled", false);

    $("#btnSave")
        .removeClass("btn-primary")
        .addClass("btn-success")
        .text("Save");

}
//$(document).on("click", ".btnPassword", function () {

//    $("#PasswordUserId").val($(this).data("id"));

//    $("#passwordModal").modal("show");

//});
$(document).on("click", ".btnPassword", function () {

    $("#PasswordUserId").val($(this).data("id"));

    if ($("#IsAdmin").val() == "True") {

        $("#OldPasswordDiv").hide();

    }
    else {

        $("#OldPasswordDiv").show();

    }

    $("#passwordModal").modal("show");

});
$("#btnPasswordSave").click(function () {

    ChangePassword();

});
function ChangePassword() {

    $.ajax({

        url: "/Staff/ChangePassword",

        type: "POST",

        data: {

            UserId: $("#PasswordUserId").val(),

            OldPassword: $("#OldPassword").val(),

            NewPassword: $("#NewPassword").val(),

            ConfirmPassword: $("#ConfirmPassword").val()

        },

        success: function (res) {

            if (res.status == 1) {

                alert(res.message)

                $("#passwordModal").modal("hide");

            }

            else {

                alert(res.message)

            }

        }

    });

}
$(document).ajaxStart(function () {
    $("#loader").show();
});

$(document).ajaxStop(function () {
    $("#loader").hide();
});
function ValidateForm() {

    if ($("#StaffId").val() == "") {

        Swal.fire("Select Staff");

        $("#StaffId").focus();

        return false;

    }

    if ($("#RoleId").val() == "") {

        Swal.fire("Select Role");

        $("#RoleId").focus();

        return false;

    }

    return true;

}
function ReloadStaffDropdown(selectedValue, callback) {
    console.log("selectedValue:", selectedValue, typeof selectedValue); // 3, number

    var ddl = $("#StaffId");

    $.get("/Staff/GetStaffDropdown", function (data) {
        ddl.empty();
        ddl.append('<option value="">-- Select Staff --</option>');

        $.each(data, function (i, e) {
            console.log("e.value:", e.value, typeof e.value); // "2", string ✅
            var isSelected = selectedValue && e.value == selectedValue; // "2" == 3 → false, "3" == 3 → true ✅
            ddl.append(
                $("<option>")
                    .val(e.value)   // lowercase ✅
                    .text(e.text)   // lowercase ✅
                    .prop("selected", isSelected)
            );
        });

        if (typeof callback === "function") callback();

    }).fail(function () {
        alert("Failed to load staff list.");
    });
}
function ValidatePasswordFields() {
    var currentLoggedInUser = $("#CurrentLoggedInUserId").val();
    var targetUserId = $("#PasswordUserId").val();

    var oldPassword = $("#OldPassword").val();
    var newPassword = $("#NewPassword").val();
    var confirmPassword = $("#ConfirmPassword").val();

    // CONDITION: Agar target user aur logged-in user SAME hain (matlab apna khud ka badal raha hai)
    if (currentLoggedInUser === targetUserId) {
        if (!oldPassword || oldPassword.trim() === "") {
            alert("Kripya apna purana password dalein.");
            return false;
        }
    }

    // Common validations
    if (!newPassword || newPassword.trim() === "") {
        alert("Naya password bharna zaroori hai.");
        return false;
    }

    if (newPassword.length < 6) {
        alert("Naya password kam se kam 6 characters ka hona chahiye.");
        return false;
    }

    if (newPassword !== confirmPassword) {
        alert("Naya password aur confirm password match nahi kar rahe hain!");
        return false;
    }

    return true;
}
function ChangePassword() {
    var currentLoggedInUser = $("#CurrentLoggedInUserId").val();
    var targetUserId = $("#PasswordUserId").val();

    $.ajax({
        url: "/Staff/ChangePassword",
        type: "POST",
        data: {
            UserId: targetUserId,
            // Agar khud ka badal raha hai toh value jayegi, agar admin kisi aur ka badal raha hai toh empty string
            OldPassword: (currentLoggedInUser === targetUserId) ? $("#OldPassword").val() : "",
            NewPassword: $("#NewPassword").val(),
            ConfirmPassword: $("#ConfirmPassword").val()
        },
        success: function (res) {
            if (res.status == 1) {
                alert(res.message);
                $("#passwordModal").modal("hide");
                $("#OldPassword, #NewPassword, #ConfirmPassword").val("");
            }
            else {
                alert(res.message);
            }
        },
        error: function () {
            alert("Server error! Kripya dobara koshish karein.");
        }
    });
}

function OpenPasswordModal(targetUserId) {
    $("#PasswordUserId").val(targetUserId);

    var currentLoggedInUser = $("#CurrentLoggedInUserId").val();
    var isAdmin = $("#IsAdmin").val() === "true";

    // Agar banda khud apna password badal raha hai, toh Old Password dikhao
    if (currentLoggedInUser === targetUserId) {
        $("#OldPasswordDiv").show();
    }
    // Agar logged in banda Admin hai aur kisi aur ka badal raha hai, toh hide karo
    else if (isAdmin) {
        $("#OldPasswordDiv").hide();
        $("#OldPassword").val(""); // Clear old value
    }

    $("#passwordModal").modal("show");
}
