document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById('liveSearchInput');
    const genreSelect = document.getElementById('genreSelect');
    const yearSelect = document.getElementById('yearSelect');
    const movieItems = document.querySelectorAll('.movie-item');
    const noResultsMsg = document.getElementById('noResults');

    // Функція фільтрації
    function filterMovies() {
        const searchText = searchInput.value.toLowerCase().trim();
        const selectedGenre = genreSelect.value;
        const selectedYear = yearSelect.value;

        let hasResults = false;

        movieItems.forEach(item => {
            // Отримуємо дані з атрибутів картки
            const itemTitle = item.getAttribute('data-title');
            const itemYear = item.getAttribute('data-year');
            const itemGenres = item.getAttribute('data-genres'); // Рядок жанрів

            // Перевірка: Назва
            const matchSearch = itemTitle.includes(searchText);

            // Перевірка: Рік (якщо вибрано "Всі роки" - пропускаємо перевірку)
            const matchYear = selectedYear === "" || itemYear === selectedYear;

            // Перевірка: Жанр (перевіряємо чи містить список жанрів обраний жанр)
            const matchGenre = selectedGenre === "" || itemGenres.includes(selectedGenre);

            // Якщо ВСІ умови співпали - показуємо картку
            if (matchSearch && matchYear && matchGenre) {
                item.classList.remove('d-none');
                item.classList.add('fade-in');
                hasResults = true;
            } else {
                item.classList.add('d-none');
            }
        });

        // Показуємо/ховаємо повідомлення "Нічого не знайдено"
        if (hasResults) {
            noResultsMsg.style.display = "none";
        } else {
            noResultsMsg.style.display = "block";
            noResultsMsg.classList.add('fade-in');
        }
    }

    if (searchInput) searchInput.addEventListener('input', filterMovies);
    if (genreSelect) genreSelect.addEventListener('change', filterMovies);
    if (yearSelect) yearSelect.addEventListener('change', filterMovies);
});