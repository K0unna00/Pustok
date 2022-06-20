$(document).ready(function(){

    $(".show-detail").click(function (e) {
        e.preventDefault();
        var url = $(this).attr("href");
        fetch(url).then(response => {

            if (!response.ok) {
                alert("Xeta")
                return;
            }
            return response.text()
        }).then(data => {
            if (data) {
                $("#BookModal .modal-content").html(data);
                $("#BookModal").modal("show");
            }
        })
    })

})