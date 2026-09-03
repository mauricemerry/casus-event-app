// home.js
// Pagina-specifiek script voor de homepagina (/).
// Filterfunctionaliteit en overige home-logica komt hier.

document.addEventListener('DOMContentLoaded', function() {
    // Get all event cards
    const resultsContainer = document.getElementById('EventApp-results');
    const statusElement = document.getElementById('EventApp-status');
    const searchInput = document.getElementById('search-input');
    const clearSearchBtn = document.getElementById('clear-search-btn');

    // Store all event cards
    let allEventCards = [];

    // Initialize event cards array
    function initializeEventCards() {
        // Find all event cards - they should be direct children or articles in the results container
        const cards = resultsContainer.querySelectorAll('.EventApp-card, article');

        console.log('Found event cards:', cards.length); // Debug log

        allEventCards = Array.from(cards).map(card => {
            // Try multiple selectors to find the event name
            const nameElement = card.querySelector('h2, h3, .EventApp-card-head h2');
            const name = nameElement ? nameElement.textContent.toLowerCase() : '';

            console.log('Event name found:', name); // Debug log

            return {
                element: card,
                name: name
            };
        });
    }

    // Filter and search function
    function filterEvents() {
        const searchTerm = searchInput.value.toLowerCase().trim();

        let visibleCount = 0;

        allEventCards.forEach(card => {
            let visible = true;

            // Search filter - only search on event name/title
            if (searchTerm) {
                const matchesSearch = card.name.includes(searchTerm);

                if (!matchesSearch) {
                    visible = false;
                }
            }

            // Show/hide card
            if (visible) {
                card.element.style.display = '';
                visibleCount++;
            } else {
                card.element.style.display = 'none';
            }
        });

        // Update status
        updateStatus(visibleCount);

        // Show clear button if search has text
        if (searchTerm) {
            clearSearchBtn.style.display = 'block';
        } else {
            clearSearchBtn.style.display = 'none';
        }
    }

    // Update status text
    function updateStatus(count) {
        if (count === 1) {
            statusElement.innerHTML = `<span>${count} Evenement gevonden</span>`;
        } else {
            statusElement.innerHTML = `<span>${count} Evenementen gevonden</span>`;
        }
    }

    // Event listeners
    if (searchInput) {
        searchInput.addEventListener('input', filterEvents);
    }

    if (clearSearchBtn) {
        clearSearchBtn.addEventListener('click', function() {
            searchInput.value = '';
            clearSearchBtn.style.display = 'none';
            filterEvents();
        });
    }

    // Initialize on page load
    if (resultsContainer) {
        initializeEventCards();
    }

    // Hide clear button initially
    if (clearSearchBtn) {
        clearSearchBtn.style.display = 'none';
    }
});
