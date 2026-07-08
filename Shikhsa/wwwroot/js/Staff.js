//=========================
//
// Academic Module
//
//=========================



let AcademicEditIndex = -1;

let ExperienceEditIndex = -1;

let DocumentEditIndex = -1;
$(function () {

    InitializeAcademic();
    InitializeExperience();
    InitializeDocument();
});
function InitializeAcademic() {

    $("#btnAddAcademic").click(function () {

        ClearAcademic();

        AcademicEditIndex = -1;

        $("#AcademicModal").modal('show');

    });

}
function InitializeExperience() {

    $("#btnAddExperience").click(function () {

        ClearExperience();

        ExperienceEditIndex = -1;

        $("#ExperienceModal").modal("show");

    });

}


function InitializeDocument() {

    $("#btnAddDocument").click(function () {

        ClearDocument();

        DocumentEditIndex = -1;

        $("#DocumentModal").modal("show");

    });

}
$(document).on("change", "#FromDate,#ToDate", function () {

    CalculateExperience();

});
function CalculateExperience() {

    var from = new Date($("#FromDate").val());

    var to = new Date($("#ToDate").val());

    if ($("#FromDate").val() == "" || $("#ToDate").val() == "")
        return;

    if (to < from) {

        alert("To Date cannot be less than From Date.");

        return;
    }

    var years = to.getFullYear() - from.getFullYear();

    var months = to.getMonth() - from.getMonth();

    var days = to.getDate() - from.getDate();

    if (days < 0) {

        months--;

        days += 30;

    }

    if (months < 0) {

        years--;

        months += 12;

    }

    $("#ExperienceYears").val(years);

    $("#ExperienceMonths").val(months);

    $("#ExperienceDays").val(days);

}
$(document).on("keyup",
    "#TotalMarks,#ObtainedMarks",
    function () {

        CalculatePercentage();

    });

function CalculatePercentage() {

    var total = parseFloat($("#TotalMarks").val()) || 0;

    var obtain = parseFloat($("#ObtainedMarks").val()) || 0;

    if (total == 0) {

        $("#Percentage").val("");

        return;
    }

    var percentage =
        ((obtain / total) * 100).toFixed(2);

    $("#Percentage").val(percentage);

}
$("#btnAcademicSave").click(async function (e) {

    e.preventDefault();

    await SaveAcademic();

});
async function SaveAcademic() {

    if ($("#DegreeId").val() == "") {

        alert("Select Degree");

        return;

    }
    let marksheetPath = "";

    console.log("SaveAcademic Called");

    console.log($("#MarksheetFile")[0]);

    console.log($("#MarksheetFile")[0].files.length);

    if ($("#MarksheetFile")[0].files.length > 0) {

        console.log("File Found");

        let fd = new FormData();

        fd.append("file", $("#MarksheetFile")[0].files[0]);

        fd.append("fileType",
            $("#DegreeId option:selected").text());

        console.log("Before Ajax Call");

        try {

            let result = await $.ajax({
                url: "/Staff/UploadStaffFile",
                type: "POST",
                data: fd,
                processData: false,
                contentType: false
            });

            console.log("Ajax Success");
            console.log(result);
            marksheetPath = result.filePath;
            console.log("marksheetPath");
            console.log(marksheetPath);

        }
        catch (e) {

            console.error("Ajax Error");
            console.error(e);

        }
    }
    let finalMarksheetPath = marksheetPath;

    // Edit Mode + No New File Selected
    if (AcademicEditIndex != -1 && !finalMarksheetPath) {

        finalMarksheetPath =AcademicList[AcademicEditIndex].MarksheetFile;

    }
    var obj = {

        AcademicId: 0,

        DegreeId: parseInt($("#DegreeId").val()) || 0,
        StaffId: parseInt($("#StaffId").val())||0,
        DegreeName: $("#DegreeId option:selected").text(),

        StreamId: parseInt($("#StreamId").val()) || 0,

        StreamName: $("#StreamId option:selected").text(),

        InstituteName: $("#InstituteName").val(),

        UniversityName: $("#UniversityName").val(),

        PassingYear: parseInt($("#PassingYear").val()) || 0,

        RollNumber: $("#RollNumber").val(),

        TotalMarks: parseFloat($("#TotalMarks").val()) || 0,

        ObtainedMarks: parseFloat($("#ObtainedMarks").val()) || 0,

        Percentage: parseFloat($("#Percentage").val()) || 0,

        GradeId: parseInt($("#GradeId").val()) || 0,

        GradeName: $("#GradeId option:selected").text(),

        Remarks: $("#Remarks").val(),

        MarksheetFile: finalMarksheetPath
    };

    if (AcademicEditIndex == -1)

        AcademicList.push(obj);

    else

        AcademicList[AcademicEditIndex] = obj;

    RenderAcademic();

    $("#AcademicModal").modal("hide");

}
function RenderAcademic() {

    var tbody = $("#tblAcademic tbody");

    tbody.empty();

    $.each(AcademicList, function (i, item) {

        var tr = "<tr>";

        tr += "<td>" + (i + 1) + "</td>";

        tr += "<td>" + item.DegreeName + "</td>";

        tr += "<td>" + item.StreamName + "</td>";

        tr += "<td>" + item.InstituteName + "</td>";

        tr += "<td>" + item.UniversityName + "</td>";

        tr += "<td>" + item.PassingYear + "</td>";

        tr += "<td>" + item.RollNumber + "</td>";

        tr += "<td>" + item.TotalMarks + "</td>";

        tr += "<td>" + item.ObtainedMarks + "</td>";

        tr += "<td>" + item.Percentage + "</td>";

        tr += "<td>" + item.GradeName + "</td>";
        tr += `<td>
        ${item.MarksheetFile ? `<a href='${item.MarksheetFile}' target='_blank'><i class='ti ti-eye text-primary'></i></a>` : ''}
       </td>`;


        tr += `
  <td>
    <button type='button' class='btn btn-warning btn-sm editAcademic' data-index='${i}'>
      <i class='ti ti-edit'></i>
    </button>
    <button type='button' class='btn btn-danger btn-sm deleteAcademic' data-index='${i}'>
      <i class='ti ti-trash'></i>
    </button>
  </td>
`;
        //tr += "<td>" +
        //    (item.MarksheetFile
        //    ? "<a href='" + item.MarksheetFile +
        //        "' target='_blank'><i class='ti ti-eye text-primary'></i></a>"
        //        : "") +
        //    "</td>";

        tr += "</tr>";

        tbody.append(tr);

    });

}

$(document).on("click", ".editAcademic", function () {

    AcademicEditIndex = $(this).data("index");

    var x = AcademicList[AcademicEditIndex];

    $("#DegreeId").val(x.DegreeId).trigger("change");

    $("#StreamId").val(x.StreamId).trigger("change");

    $("#InstituteName").val(x.InstituteName);

    $("#UniversityName").val(x.UniversityName);

    $("#PassingYear").val(x.PassingYear);

    $("#RollNumber").val(x.RollNumber);

    $("#TotalMarks").val(x.TotalMarks);

    $("#ObtainedMarks").val(x.ObtainedMarks);

    $("#Percentage").val(x.Percentage);

    $("#GradeId").val(x.GradeId).trigger("change");

    $("#Remarks").val(x.Remarks);
    if (x.MarksheetFile) {

        $("#MarksheetPreview")
            .attr("href",x.MarksheetFile)
            .show();

    }
    else {

        $("#MarksheetPreview").hide();

    }

    $("#AcademicModal").modal("show");

});
$(document).on("click", ".deleteAcademic", function () {

    if (!confirm("Delete this record?"))

        return;

    AcademicList.splice($(this).data("index"), 1);

    RenderAcademic();

});
function ClearAcademic() {

    $("#AcademicModal input").val("");

    $("#AcademicModal textarea").val("");

    $("#AcademicModal select").val("").trigger("change");

}
function ClearDocument() {

    $("#DocumentModal input").val("");

    $("#DocumentModal textarea").val("");

    $("#DocumentModal select").val("").trigger("change");

}
$("form").submit(function () {

    $("#AcademicJson").val(JSON.stringify(AcademicList));
    $("#ExperienceJson").val(JSON.stringify(ExperienceList));
    $("#DocumentJson").val(JSON.stringify(DocumentList));
    //$("#EmergencyJson").val(JSON.stringify(EmergencyList));
    //$("#LeaveJson").val(JSON.stringify(LeaveObject));

});
$("#btnExperienceSave").click(function () {

    SaveExperience();

});
async function SaveExperience() {

    if ($("#OrganisationName").val() == "") {

        alert("Enter Organisation Name");

        return;

    }

    if ($("#FromDate").val() == "") {

        alert("Select From Date");

        return;

    }

    if ($("#ToDate").val() == "") {

        alert("Select To Date");

        return;

    }
    let experiencePath = "";
   

  

    if ($("#ExperienceLetterFile")[0].files.length > 0) {

        console.log("File Found");

        let fd = new FormData();

        fd.append("file", $("#ExperienceLetterFile")[0].files[0]);

        fd.append("fileType",
            $("#OrganisationName").val());

        console.log("Before Ajax Call");

        try {

            let result = await $.ajax({
                url: "/Staff/UploadStaffFile",
                type: "POST",
                data: fd,
                processData: false,
                contentType: false
            });

            console.log("Ajax Success");
            console.log(result);
            experiencePath = result.filePath;
            console.log("marksheetPath");
            console.log(experiencePath);

        }
        catch (e) {

            console.error("Ajax Error");
            console.error(e);

        }
    }
    let finalexperiencePath = experiencePath;

    // Edit Mode + No New File Selected
    if (ExperienceEditIndex != -1 && !finalexperiencePath) {

        finalexperiencePath = ExperienceList[ExperienceEditIndex].ExperienceLetterFile;

    }
   
    var obj = {

        ExperienceId: 0,

        DepartmentId: parseInt($("#ExperienceDepartmentId").val()) || 0,
        StaffId: parseInt($("#StaffId").val()) || 0,

        DepartmentName: $("#ExperienceDepartmentId option:selected").text(),

        DesignationId: parseInt($("#ExperienceDesignationId").val()) || 0,

        DesignationName: $("#ExperienceDesignationId option:selected").text(),

        OrganisationName: $("#OrganisationName").val(),

        SubjectName: $("#ExperienceSubject").val(),

        FromDate: $("#FromDate").val(),

        ToDate: $("#ToDate").val(),

        TotalExperienceYears: parseInt($("#ExperienceYears").val()) || 0,

        TotalExperienceMonths: parseInt($("#ExperienceMonths").val()) || 0,

        TotalExperienceDays: parseInt($("#ExperienceDays").val()) || 0,

        LastDrawnSalary: parseFloat($("#LastSalary").val()) || 0,

        Remarks: $("#ExperienceRemarks").val(),

        ExperienceLetterFile: finalexperiencePath
    };

    if (ExperienceEditIndex == -1)

        ExperienceList.push(obj);

    else

        ExperienceList[ExperienceEditIndex] = obj;

    RenderExperience();

    $("#ExperienceModal").modal("hide");

}
function RenderExperience() {

    var tbody = $("#tblExperience tbody");

    tbody.empty();

    $.each(ExperienceList, function (i, item) {

        var experience =
            (item.TotalExperienceYears || 0) + " Y " +
            (item.TotalExperienceMonths || 0) + " M " +
            (item.TotalExperienceDays || 0) + " D";

        var letter = "";

        if (item.File != null) {

            letter = "<i class='ti ti-file text-success'></i>";

        }

        var tr = "<tr>";

        tr += "<td>" + (i + 1) + "</td>";

        tr += "<td>" + item.DesignationName + "</td>";

        tr += "<td>" + item.DepartmentName + "</td>";

        tr += "<td>" + item.OrganisationName + "</td>";

        tr += "<td>" + item.SubjectName + "</td>";

        tr += "<td>" + item.FromDate + "</td>";

        tr += "<td>" + item.ToDate + "</td>";

        tr += "<td>" + experience + "</td>";

        tr += "<td>" + item.LastDrawnSalary + "</td>";

        tr += `<td>
        ${item.ExperienceLetterFile ? `<a href='${item.ExperienceLetterFile}' target='_blank'><i class='ti ti-eye text-primary'></i></a>` : ''}
       </td>`;

        tr += "<td>";

        tr += "<button type='button' class='btn btn-warning btn-sm editExperience' data-index='" + i + "'>";

        tr += "<i class='ti ti-edit'></i>";

        tr += "</button> ";

        tr += "<button type='button' class='btn btn-danger btn-sm deleteExperience' data-index='" + i + "'>";

        tr += "<i class='ti ti-trash'></i>";

        tr += "</button>";

        tr += "</td>";

        tr += "</tr>";

        tbody.append(tr);

    });

}

$(document).on("click", ".editExperience", function () {

    ExperienceEditIndex = $(this).data("index");

    var x = ExperienceList[ExperienceEditIndex];

    $("#ExperienceDepartmentId").val(x.DepartmentId).trigger("change");

    $("#ExperienceDesignationId").val(x.DesignationId).trigger("change");

    $("#OrganisationName").val(x.OrganisationName);

    $("#ExperienceSubject").val(x.SubjectName);

    $("#FromDate").val(x.FromDate);

    $("#ToDate").val(x.ToDate);

    $("#ExperienceYears").val(x.TotalExperienceYears);

    $("#ExperienceMonths").val(x.TotalExperienceMonths);

    $("#ExperienceDays").val(x.TotalExperienceDays);

    $("#LastSalary").val(x.LastDrawnSalary);

    $("#ExperienceRemarks").val(x.Remarks);
    if (x.ExperienceLetterFile) {

        $("#ExperienceLetterPreview")
            .attr("href", x.ExperienceLetterFile)
            .show();

    }
    else {

        $("#ExperienceLetterPreview").hide();

    }


    $("#ExperienceModal").modal("show");

});
$(document).on("click", ".deleteExperience", function () {

    if (!confirm("Delete this Experience?"))
        return;

    var index = $(this).data("index");

    ExperienceList.splice(index, 1);

    RenderExperience();

});
function ClearExperience() {

    $("#ExperienceModal input").val("");

    $("#ExperienceModal textarea").val("");

    $("#ExperienceModal select").val("").trigger("change");

}
function FormatDate(date) {

    if (!date)
        return "";

    var d = new Date(date);

    var day = ("0" + d.getDate()).slice(-2);

    var month = ("0" + (d.getMonth() + 1)).slice(-2);

    var year = d.getFullYear();

    return day + "/" + month + "/" + year;

}
$("#btnDocumentSave").click(function () {

    SaveDocument();

});     
async function SaveDocument() {
    let documentPath = "";




    if ($("#DocumentFile")[0].files.length > 0) {

        console.log("File Found");

        let fd = new FormData();

        fd.append("file", $("#DocumentFile")[0].files[0]);

        fd.append("fileType",
            $("#DocumentTypeId option:selected").text());

        console.log("Before Ajax Call");

        try {

            let result = await $.ajax({
                url: "/Staff/UploadStaffFile",
                type: "POST",
                data: fd,
                processData: false,
                contentType: false
            });

            console.log("Ajax Success");
            console.log(result);
            documentPath = result.filePath;
            console.log("marksheetPath");
            console.log(documentPath);

        }
        catch (e) {

            console.error("Ajax Error");
            console.error(e);

        }
    }
    let finaldocumentPath = documentPath;

    // Edit Mode + No New File Selected
    if (DocumentEditIndex != -1 && !finaldocumentPath) {

        finaldocumentPath = DocumentList[DocumentEditIndex].FilePath;

    }



    var obj = {

        DocumentId: 0,
        StaffId: parseInt($("#StaffId").val()) || 0,
        DocumentTypeId: parseInt($("#DocumentTypeId").val()),

        DocumentTypeName: $("#DocumentTypeId option:selected").text(),

        DocumentNumber: $("#DocumentNumber").val(),

        ExpiryDate: $("#ExpiryDate").val()||null,

        Remarks: $("#DocumentRemarks").val(),

        FilePath: finaldocumentPath,
        OriginalFileName:
            $("#DocumentFile")[0].files.length > 0
                ? $("#DocumentFile")[0].files[0].name
                : ""

    };

    if (DocumentEditIndex == -1)

        DocumentList.push(obj);

    else

        DocumentList[DocumentEditIndex] = obj;

    RenderDocument();

    $("#DocumentModal").modal("hide");

}
function RenderDocument() {

    var tbody = $("#tblDocument tbody");

    tbody.empty();

    $.each(DocumentList, function (i, item) {

        tbody.append(

            "<tr>" +

            "<td>" + (i + 1) + "</td>" +

            "<td>" + item.DocumentTypeName + "</td>" +

            "<td>" + item.DocumentNumber + "</td>" +

            "<td>" + item.ExpiryDate + "</td>" +

          `<td>
        ${item.FilePath ? `<a href='${item.FilePath}' target='_blank'><i class='ti ti-eye text-primary'></i></a>` : ''}
       </td>`+

            "<td>" + item.Remarks + "</td>" +

            "<td>" +

            "<button type='button' class='btn btn-warning btn-sm editDocument' data-index='" + i + "'><i class='ti ti-edit'></i></button> " +

            "<button type='button' class='btn btn-danger btn-sm deleteDocument' data-index='" + i + "'><i class='ti ti-trash'></i></button>" +

            "</td>" +

            "</tr>"

        );

    });

}
$(document).on("click", ".editDocument", function () {

    DocumentEditIndex = $(this).data("index");

    var x = DocumentList[DocumentEditIndex];

    $("#DocumentTypeId").val(x.DocumentTypeId).trigger("change");

    $("#DocumentNumber").val(x.DocumentNumber);

    $("#ExpiryDate").val(x.ExpiryDate);

    $("#DocumentRemarks").val(x.Remarks);
    if (x.FilePath) {

        $("#DocumentFilePreview")
            .attr("href", x.FilePath)
            .show();

    }
    else {

        $("#DocumentFilePreview").hide();

    }

    $("#DocumentModal").modal("show");

});
$(document).on("click", ".deleteDocument", function () {

    if (!confirm("Delete Document?"))

        return;

    DocumentList.splice($(this).data("index"), 1);

    RenderDocument();

});
$(document).on("keyup change", ".salary,.deduction", function () {
    CalculateSalary();
});

function CalculateSalary() {

    var basic = parseFloat($("#BasicSalary").val()) || 0;
    var hra = parseFloat($("#HRA").val()) || 0;
    var da = parseFloat($("#DA").val()) || 0;
    var medical = parseFloat($("#MedicalAllowance").val()) || 0;
    var conveyance = parseFloat($("#ConveyanceAllowance").val()) || 0;
    var special = parseFloat($("#SpecialAllowance").val()) || 0;
    var other = parseFloat($("#OtherAllowance").val()) || 0;

    var gross = basic + hra + da + medical + conveyance + special + other;

    $("#GrossSalary").val(gross.toFixed(2));

    var pf = parseFloat($("#PFDeduction").val()) || 0;
    var esic = parseFloat($("#ESICDeduction").val()) || 0;
    var pt = parseFloat($("#PTDeduction").val()) || 0;
    var tds = parseFloat($("#TDSDeduction").val()) || 0;
    var otherDeduction = parseFloat($("#OtherDeduction").val()) || 0;

    var totalDeduction = pf + esic + pt + tds + otherDeduction;

    $("#TotalDeduction").val(totalDeduction.toFixed(2));

    var net = gross - totalDeduction;

    $("#NetSalary").val(net.toFixed(2));
}

/* crop*/
let cropper;
let currentInput;
let currentPreview;

$("input[type=file]").change(function (e) {

    const file = e.target.files[0];

    if (!file)
        return;

    currentInput = this;

    if ($(this).attr("id") == "PhotoFile")
        currentPreview = "#imgPhotoPreview";
    else
        currentPreview = "#imgSignaturePreview";

    let reader = new FileReader();

    reader.onload = function (event) {

        $("#cropImage").attr("src", event.target.result);

        $("#cropModal").modal("show");

    }

    reader.readAsDataURL(file);

});
$('#cropModal').on('shown.bs.modal', function () {

    cropper = new Cropper(document.getElementById('cropImage'), {

        aspectRatio: currentInput.id === "PhotoFile"
            ? 150 / 170
            : NaN,      // Free Crop
        viewMode: 1,
        autoCropArea:1,
        movable: true,
        zoomable: true,
        scalable: true,
        rotatable: true,
        cropBoxResizable: true,
        cropBoxMovable: true,
        responsive: true,
        restore: false,


    });

}).on('hidden.bs.modal', function () {

    if (cropper) {
        cropper.destroy();
        cropper = null;
    }

});
$("#btnCrop").click(function () {

    const canvas = cropper.getCroppedCanvas();

    canvas.toBlob(function (blob) {

        const file = new File([blob], "cropped.png", {
            type: "image/png"
        });

        const dt = new DataTransfer();
        dt.items.add(file);

        currentInput.files = dt.files;

        $(currentPreview).attr("src", URL.createObjectURL(blob));

        $("#cropModal").modal("hide");

    }, "image/png");

}); $("#btnCrop").click(function () {

    const canvas = cropper.getCroppedCanvas();

    canvas.toBlob(function (blob) {

        const file = new File([blob], "cropped.png", {
            type: "image/png"
        });

        const dt = new DataTransfer();
        dt.items.add(file);

        currentInput.files = dt.files;

        $(currentPreview).attr("src", URL.createObjectURL(blob));

        $("#cropModal").modal("hide");

    }, "image/png");

});