var TeacherFilter = {
    init: function () {
        $("#BatchId").off("change").on("change", function () {
            console.log("Batch Changed");
            TeacherFilter.load("Batch");
        });
        $("#StaffId").off("change").on("change", function () {
            console.log("Staff Changed");
            debugger
            TeacherFilter.load("Staff");
        });
        $("#ClassId").off("change").on("change", function () {
            TeacherFilter.load("Class");
        });
        $("#SectionId").off("change").on("change", function () {
            TeacherFilter.load("Section");
        });
    },

    load: function (changedBy) {
        let data = {
            batchId: Number($("#BatchId").val()) || 0,
            staffId: Number($("#StaffId").val()) || 0,
            classId: Number($("#ClassId").val()) || 0,
            sectionId: Number($("#SectionId").val()) || 0,
            changedBy: changedBy
        };

        console.log(data);
      
        $.ajax({
            url: "/Exam/GetTeacherFilter",
            type: "POST",
            contentType: "application/json",
            headers: {
                // Required if the endpoint is protected by [ValidateAntiForgeryToken]
                // or a global AutoValidateAntiforgeryTokenFilter.
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            data: JSON.stringify(data),
            //data: JSON.stringify({
            //    batchId: $("#BatchId").val(),
            //    staffId: $("#StaffId").val(),
            //    classId: $("#ClassId").val(),
            //    sectionId: $("#SectionId").val(),
            //    changedBy: changedBy
            //}),
            success: function (r) {
                if (!r) return;
                $("#BatchId").val(r.batchId).trigger("change.select2");

                TeacherFilter.fillDropdown(
                    "#StaffId",
                    r.staffs,
                    "staffId",
                    "fullName",
                    r.staffId
                );
                TeacherFilter.fillDropdown(
                    "#ClassId",
                    r.classes,
                    "id",
                    "name",
                    r.classId
                );
                TeacherFilter.fillDropdown(
                    "#SectionId",
                    r.sections,
                    "id",
                    "name",
                    r.sectionId
                );

                //$("#StaffId").prop("disabled", !!r.lockStaff);
                //$("#BatchId").prop("disabled", !!r.lockBatch);
                //$("#ClassId").prop("disabled", !!r.lockClass);
                //$("#SectionId").prop("disabled", !!r.lockSection);
            },
            error: function (xhr) {
                console.error("GetTeacherFilter failed:", xhr.status, xhr.responseText);
                alert("Unable to load filter.");
            }
        });
    },

    fillDropdown: function (selector, list, valueField, textField, selectedValue) {
        var ddl = $(selector);
        if (ddl.hasClass("select2-hidden-accessible")) {
            ddl.select2("destroy");
        }
        ddl.empty();
        ddl.append($("<option>", { value: "", text: "Select" }));

        if (!list || !list.length) return;

        $.each(list, function (i, item) {
            ddl.append(
                $("<option>", {
                    value: item[valueField],
                    text: item[textField],
                    selected: selectedValue != null && item[valueField] == selectedValue
                })
            );
        });
        ddl.select2({
            width: "100%",
            placeholder: "Select",
            allowClear: true
        });

        // Trigger change so Select2 refreshes
        ddl.trigger("change.select2");
    }
};

$(function () {
    TeacherFilter.init();
    if ($("#BatchId").hasClass("select2-hidden-accessible")) {
        $("#BatchId").select2("destroy");
    }

    $("#BatchId").select2({
        width: "100%"
    });
});