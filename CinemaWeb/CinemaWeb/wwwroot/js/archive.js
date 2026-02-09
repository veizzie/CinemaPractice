document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById('liveSearchInput');
    const movieItems = document.querySelectorAll('.movie-item');
    const noResultsMsg = document.getElementById('noResults');

    searchInput.addEventListener('input', function (e) {
        const searchText = e.target.value.toLowerCase().trim();
        let hasResults = false;

        movieItems.forEach(item => {
            const title = item.getAttribute('data-title');
            const year = item.getAttribute('data-year');

            // Перевіряємо чи містить назва або рік текст пошуку
            if (title.includes(searchText) || year.includes(searchText)) {
                item.style.display = "";
                item.classList.add('fade-in');
                hasResults = true;
            } else {
                item.style.display = "none";
            }
        });

        // Показуємо/ховаємо повідомлення "Нічого не знайдено"
        if (hasResults) {
            noResultsMsg.style.display = "none";
        } else {
            noResultsMsg.style.display = "block";
            noResultsMsg.classList.add('fade-in');
        }
    });
});