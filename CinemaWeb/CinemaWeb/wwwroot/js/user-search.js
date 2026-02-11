function filterUsers() {
    var input = document.getElementById("userSearchInput");
    var filter = input.value.toUpperCase();
    var table = document.getElementById("usersTable");
    var tr = table.getElementsByTagName("tr");
    var noResultsDiv = document.getElementById("noResults");

    var hasResults = false;

    for (var i = 1; i < tr.length; i++) {
        var tdEmail = tr[i].getElementsByTagName("td")[0];
        var tdName = tr[i].getElementsByTagName("td")[1];

        if (tdEmail && tdName) {
            var emailValue = tdEmail.textContent || tdEmail.innerText;
            var nameValue = tdName.textContent || tdName.innerText;

            if (emailValue.toUpperCase().indexOf(filter) > -1 ||
                nameValue.toUpperCase().indexOf(filter) > -1) {

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

function clearUserSearch() {
    var input = document.getElementById("userSearchInput");
    input.value = "";
    filterUsers();
    input.focus();
}