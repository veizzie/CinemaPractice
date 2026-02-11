document.addEventListener("DOMContentLoaded", function () {
    const genreSelectElement = document.querySelector("#genre-select");

    // Перевіряємо, чи існує елемент на сторінці перед ініціалізацією
    if (genreSelectElement) {
        new TomSelect("#genre-select", {
            plugins: ['remove_button'],
            create: false,
            maxItems: null,
            placeholder: "Пошук жанрів...",

            onItemAdd: function () {
                this.setTextboxValue('');
                this.refreshOptions();
            },
        });
    }
});