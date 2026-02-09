function filterGenres() {
    var input = document.getElementById("searchInput");
    var filter = input.value.toUpperCase();
    var table = document.getElementById("genresTable");
    var tr = table.getElementsByTagName("tr");
    var noResultsDiv = document.getElementById("noResults");

    var hasResults = false;

    for (var i = 1; i < tr.length; i++) {
        var td = tr[i].getElementsByTagName("td")[0];

        if (td) {
            var txtValue = td.textContent || td.innerText;

            if (txtValue.toUpperCase().indexOf(filter) > -1) {
                tr[i].style.display = "";
                hasResults = true;
            } else {
                tr[i].style.display = "none";
            }
        }
    }

    if (hasResults) {
        noResultsDiv.style.display = "none";
        table.style.display = "";
    } else {
        noResultsDiv.style.display = "block";
    }
}

function clearSearch() {
    var input = document.getElementById("searchInput");
    input.value = "";
    filterGenres();
    input.focus();
}