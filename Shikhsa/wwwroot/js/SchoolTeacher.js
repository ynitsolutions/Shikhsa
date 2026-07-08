//========================================================
// Teacher Allocation
//========================================================

//-----------------------------------------
// Global Variables
//-----------------------------------------

let SubjectCache = {};

let TeacherSearchUrl = "/ClassTeacher/SearchTeacher";
let LoadUrl = "/ClassTeacher/LoadAssignments";
let SubjectUrl = "/ClassTeacher/GetSubjects";
let SaveUrl = "/ClassTeacher/SaveTeacherAllocation";
let CopyUrl = "/ClassTeacher/CopyPreviousBatch";


//========================================================
// Page Ready
//========================================================

$(document).ready(function () {

    InitializePage();

});


//========================================================
// Initialize Page
//========================================================

function InitializePage() {

    //-------------------------------------
    // Select2
    //-------------------------------------

    $(".select2").select2({

        width: "100%"

    });

    //-------------------------------------
    // Events
    //-------------------------------------

    RegisterEvents();

}


//========================================================
// Register Events
//========================================================

function RegisterEvents() {

    //-------------------------------------
    // Load
    //-------------------------------------

    $("#btnLoad").on("click", function () {

        LoadData();

    });


    //-------------------------------------
    // Save
    //-------------------------------------

    $("#btnSave").on("click", function () {

        SaveAssignments();

    });


    //-------------------------------------
    // Copy
    //-------------------------------------

    $("#btnCopy").on("click", function () {

        CopyBatch();

    });


    //-------------------------------------
    // Batch Changed
    //-------------------------------------

    $("#BatchId").on("change", function () {

        SubjectCache = {};

    });


    //-------------------------------------
    // Add Teacher
    //-------------------------------------

    $(document).on("click", ".btnAddTeacher", function () {

        AddTeacherRow($(this).data("class"));

    });


    //-------------------------------------
    // Delete Row
    //-------------------------------------

    $(document).on("click", ".btnDelete", function () {

        $(this).closest("tr").remove();

    });


    //-------------------------------------
    // Only One Class Teacher
    //-------------------------------------

    $(document).on("change", ".isClassTeacher", function () {

        if (!$(this).is(":checked"))
            return;

        $(this)
            .closest("tbody")
            .find(".isClassTeacher")
            .not(this)
            .prop("checked", false);

    });

}


//========================================================
// Load Button
//========================================================

function LoadData() {

    let batchId = $("#BatchId").val();

    let sectionId = $("#SectionId").val();

    if (batchId === "") {

        ShowWarning("Please select Batch.");

        return;
    }

    if (sectionId === "") {

        ShowWarning("Please select Section.");

        return;
    }

    LoadAssignments(batchId, sectionId);

}


//========================================================
// Load Assignments
//========================================================

function LoadAssignments(batchId, sectionId) {

    ShowLoader();

    LoadSubjectCache(batchId)

        .done(function () {

            $.get(

                LoadUrl,

                {
                    batchId: batchId,
                    sectionId: sectionId
                },

                function (html) {

                    $("#assignmentContainer").html(html);

                    InitializeDynamicControls();

                });

        })

        .always(function () {

            HideLoader();

        });

}


//========================================================
// Initialize Controls after Partial Load
//========================================================

function InitializeDynamicControls() {

    //-------------------------------------
    // Subject Select2
    //-------------------------------------

    $(".subject").select2({

        width: "100%"

    });

    //-------------------------------------
    // Teacher Select2
    //-------------------------------------

    $(".teacherSelect").each(function () {

        InitializeTeacherDDL($(this));

    });

}


//========================================================
// Load Subject Cache
//========================================================

function LoadSubjectCache(batchId) {

    SubjectCache = {};

    return $.get(

        SubjectUrl,

        {
            batchId: batchId
        },

        function (data) {

            $.each(data, function (_, item) {

                if (!SubjectCache[item.classId]) {

                    SubjectCache[item.classId] = [];

                }

                SubjectCache[item.classId].push(item);

            });

        });

}
//========================================================
// Initialize Teacher Select2
//========================================================

function InitializeTeacherDDL(control) {

    let selectedId = control.data("id");
    let selectedText = control.data("text");

    control.select2({

        width: "100%",

        placeholder: "Select Teacher",

        allowClear: true,

        ajax: {

            url: TeacherSearchUrl,

            dataType: "json",

            delay: 300,

            cache: true,

            data: function (params) {

                return {
                    term: params.term
                };

            },

            processResults: function (data) {

                return {
                    results: data
                };

            }

        }

    });

    //-----------------------------------------
    // Existing Teacher (Edit Mode)
    //-----------------------------------------

    if (selectedId && selectedText) {

        let option = new Option(
            selectedText,
            selectedId,
            true,
            true
        );

        control.append(option).trigger("change");

    }

}



//========================================================
// Bind Subjects
//========================================================

function BindSubjects(control, classId) {

    control.empty();

    let list = SubjectCache[classId];

    if (!list || list.length === 0)
        return;

    let options = [];

    $.each(list, function (_, item) {

        options.push(

            new Option(
                item.subjectName,
                item.subjectId,
                false,
                false
            )

        );

    });

    control.append(options);

    control.select2({

        width: "100%",

        placeholder: "Select Subjects"

    });

}



//========================================================
// Add Teacher Row
//========================================================

function AddTeacherRow(classId) {

    if (!SubjectCache[classId]) {

        ShowError("Subjects are not loaded.");

        return;

    }

    let row = $($("#teacherRowTemplate").html());

    row.find(".classId").val(classId);

    $("#tbody_" + classId).append(row);

    //-----------------------------------------
    // Teacher Dropdown
    //-----------------------------------------

    InitializeTeacherDDL(

        row.find(".teacherSelect")

    );

    //-----------------------------------------
    // Subject Dropdown
    //-----------------------------------------

    BindSubjects(

        row.find(".subjectSelect"),

        classId

    );

}



//========================================================
// Delete Teacher Row
//========================================================

$(document).on("click", ".btnDelete", function () {

    let tr = $(this).closest("tr");

    tr.remove();

});



//========================================================
// Only One Class Teacher
//========================================================

$(document).on("change", ".isClassTeacher", function () {

    if (!$(this).is(":checked"))
        return;

    let tbody = $(this).closest("tbody");

    tbody.find(".isClassTeacher")
        .not(this)
        .prop("checked", false);

});



//========================================================
// Teacher Changed
// Prevent Duplicate Teacher
//========================================================

$(document).on("change", ".teacherSelect", function () {

    let teacherId = $(this).val();

    if (!teacherId)
        return;

    let tbody = $(this).closest("tbody");

    let count = 0;

    tbody.find(".teacherSelect").each(function () {

        if ($(this).val() == teacherId)

            count++;

    });

    if (count > 1) {

        ShowWarning("Teacher already exists in this class.");

        $(this).val(null).trigger("change");

    }

});
//========================================================
// Collect JSON
//========================================================

function CollectJson() {

    let model = {

        BatchId: parseInt($("#BatchId").val()) || 0,

        SectionId: parseInt($("#SectionId").val()) || 0,

        Classes: []

    };

    $(".teacherTable").each(function () {

        let tbody = $(this).find("tbody");

        let classId = parseInt(

            tbody.attr("id").replace("tbody_", "")

        );

        let classVM = {

            ClassId: classId,

            Teachers: []

        };

        //----------------------------------------
        // Teacher Rows
        //----------------------------------------

        tbody.find("tr").each(function () {

            let teacherId = parseInt(

                $(this).find(".teacherSelect").val()

            ) || 0;

            if (teacherId === 0)
                return;

            //------------------------------------
            // Subject Ids
            //------------------------------------

            let subjects = $(this)

                .find(".subjectSelect")

                .val() || [];

            subjects = [...new Set(

                subjects.map(Number)

            )];

            classVM.Teachers.push({

                StaffId: teacherId,

                IsClassTeacher:

                    $(this)

                        .find(".isClassTeacher")

                        .is(":checked"),

                SubjectIds: subjects

            });

        });

        model.Classes.push(classVM);

    });

    return model;

}
//========================================================
// Validation
//========================================================

function ValidateAssignments(model) {

    //----------------------------------------
    // Batch
    //----------------------------------------

    if (model.BatchId === 0) {

        ShowWarning("Please select Batch.");

        return false;

    }

    //----------------------------------------
    // Section
    //----------------------------------------

    if (model.SectionId === 0) {

        ShowWarning("Please select Section.");

        return false;

    }

    //----------------------------------------
    // At least one class
    //----------------------------------------

    if (model.Classes.length === 0) {

        ShowWarning("No class found.");

        return false;

    }

    //----------------------------------------
    // Class Validation
    //----------------------------------------

    for (const cls of model.Classes) {

        let teacherSet = new Set();

        let classTeacherCount = 0;

        for (const teacher of cls.Teachers) {

            //----------------------------------
            // Duplicate Teacher
            //----------------------------------

            if (teacherSet.has(teacher.StaffId)) {

                ShowWarning(

                    "Duplicate teacher found in same class."

                );

                return false;

            }

            teacherSet.add(

                teacher.StaffId

            );

            //----------------------------------
            // Subject Required
            //----------------------------------

            if (teacher.SubjectIds.length === 0) {

                ShowWarning(

                    "Please select at least one subject."

                );

                return false;

            }

            //----------------------------------
            // CT Count
            //----------------------------------

            if (teacher.IsClassTeacher)

                classTeacherCount++;

        }

        //--------------------------------------
        // Only One CT
        //--------------------------------------

        if (classTeacherCount > 1) {

            ShowWarning(

                "Only one Class Teacher allowed."

            );

            return false;

        }

    }

    return true;

}
$(document).on("click", "#btnSave", function () {

    SaveAssignments();

});
function SaveAssignments() {

    let model = CollectJson();

    if (!ValidateAssignments(model))
        return;

    SaveData(model);

}
//========================================================
// Save Assignment
//========================================================

function SaveAssignments() {

    let model = CollectJson();

    if (!ValidateAssignments(model))
        return;

    ShowLoader();

    $("#btnSave")
        .prop("disabled", true);

    $.ajax({

        url: SaveUrl,

        type: "POST",

        contentType: "application/json",

        data: JSON.stringify(model),

        success: function (res) {

            if (res.success) {

                ShowSuccess(res.message);

                LoadAssignments(

                    model.BatchId,

                    model.SectionId

                );

            }
            else {

                ShowError(res.message);

            }

        },

        error: function (xhr) {

            let message = "Unexpected server error.";

            if (xhr.responseText)
                message = xhr.responseText;

            ShowError(message);

        },

        complete: function () {

            HideLoader();

            $("#btnSave")
                .prop("disabled", false);

        }

    });

}
//========================================================
// Copy Previous Batch
//========================================================

function CopyBatch() {

    let oldBatchId = $("#CopyBatchId").val();

    let newBatchId = $("#BatchId").val();

    let sectionId = $("#SectionId").val();

    if (!newBatchId) {

        ShowWarning("Please select Batch.");

        return;

    }

    if (!sectionId) {

        ShowWarning("Please select Section.");

        return;

    }

    if (!oldBatchId) {

        ShowWarning("Please select Copy Batch.");

        return;

    }

    if (oldBatchId == newBatchId) {

        ShowWarning("Source and Destination Batch cannot be same.");

        return;

    }

    Swal.fire({

        title: "Copy Assignments?",

        text: "Existing assignments will be replaced.",

        icon: "question",

        showCancelButton: true,

        confirmButtonText: "Yes",

        cancelButtonText: "No"

    }).then(function (result) {

        if (!result.isConfirmed)
            return;

        ExecuteCopy(
            oldBatchId,
            newBatchId,
            sectionId
        );

    });

}
//========================================================
// Execute Copy
//========================================================

function ExecuteCopy(oldBatchId, newBatchId, sectionId) {

    ShowLoader();

    $("#btnCopy")
        .prop("disabled", true);

    $.ajax({

        url: CopyUrl,

        type: "POST",

        data: {

            oldBatchId: oldBatchId,

            newBatchId: newBatchId,

            sectionId: sectionId

        },

        success: function (res) {

            if (res.success) {

                ShowSuccess(res.message);

                LoadAssignments(

                    newBatchId,

                    sectionId

                );

            }
            else {

                ShowError(res.message);

            }

        },

        error: function (xhr) {

            let message = "Unable to copy assignments.";

            if (xhr.responseText)
                message = xhr.responseText;

            ShowError(message);

        },

        complete: function () {

            HideLoader();

            $("#btnCopy")
                .prop("disabled", false);

        }

    });

}
//========================================================
// Execute Copy
//========================================================

function ExecuteCopy(oldBatchId, newBatchId, sectionId) {

    ShowLoader();

    $("#btnCopy")
        .prop("disabled", true);

    $.ajax({

        url: CopyUrl,

        type: "POST",

        data: {

            oldBatchId: oldBatchId,

            newBatchId: newBatchId,

            sectionId: sectionId

        },

        success: function (res) {

            if (res.success) {

                ShowSuccess(res.message);

                LoadAssignments(

                    newBatchId,

                    sectionId

                );

            }
            else {

                ShowError(res.message);

            }

        },

        error: function (xhr) {

            let message = "Unable to copy assignments.";

            if (xhr.responseText)
                message = xhr.responseText;

            ShowError(message);

        },

        complete: function () {

            HideLoader();

            $("#btnCopy")
                .prop("disabled", false);

        }

    });

}
//========================================================
// Success Message
//========================================================

function ShowSuccess(message) {

    Swal.fire({

        icon: "success",

        title: "Success",

        text: message,

        confirmButtonText: "OK"

    });

}


//========================================================
// Error Message
//========================================================

function ShowError(message) {

    Swal.fire({

        icon: "error",

        title: "Error",

        text: message,

        confirmButtonText: "OK"

    });

}


//========================================================
// Warning Message
//========================================================

function ShowWarning(message) {

    Swal.fire({

        icon: "warning",

        title: "Warning",

        text: message,

        confirmButtonText: "OK"

    });

}
//========================================================
// Loader
//========================================================

function ShowLoader() {

    $("#pageLoader").show();

}


function HideLoader() {

    $("#pageLoader").hide();

}
$(document).ajaxError(function (event, xhr) {

    HideLoader();

    ShowError(

        xhr.responseText ||

        "Unexpected server error."

    );

});
