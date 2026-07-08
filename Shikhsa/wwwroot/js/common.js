//function ShowSuccess(message) {

//    Swal.fire({
//        icon: 'success',
//        title: 'Success',
//        text: message,
//        confirmButtonColor: '#198754'
//    });

//}

//function ShowError(message) {

//    Swal.fire({
//        icon: 'error',
//        title: 'Error',
//        text: message,
//        confirmButtonColor: '#dc3545'
//    });

//}

//function ShowWarning(message) {

//    Swal.fire({
//        icon: 'warning',
//        title: 'Warning',
//        text: message,
//        confirmButtonColor: '#ffc107'
//    });

//}

//function ShowInfo(message) {

//    Swal.fire({
//        icon: 'info',
//        title: 'Information',
//        text: message
//    });

//}
//async function ConfirmDelete(message) {

//    const result = await Swal.fire({
//        title: 'Are you sure?',
//        text: message,
//        icon: 'warning',
//        showCancelButton: true,
//        confirmButtonColor: '#dc3545',
//        cancelButtonColor: '#6c757d',
//        confirmButtonText: 'Yes',
//        cancelButtonText: 'No'
//    });

//    return result.isConfirmed;
//}
toastr.options = {
    closeButton: true,
    progressBar: true,
    newestOnTop: true,
    preventDuplicates: true,
    positionClass: "toast-top-right",
    timeOut: 3000,
    extendedTimeOut: 1000
};

function ShowSuccess(message) {
    toastr.success(message);
}

function ShowError(message) {
    toastr.error(message);
}

function ShowWarning(message) {
    toastr.warning(message);
}

function ShowInfo(message) {
    toastr.info(message);
}