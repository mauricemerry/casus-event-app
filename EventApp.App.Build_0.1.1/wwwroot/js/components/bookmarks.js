// bookmarks.js
// Beheert de opgeslagen evenementen (bookmarks) via localStorage.
// localStorage is een browser-opslagplek die blijft bestaan na het sluiten van de browser.
//
// Gebruik:
//   BookmarkManager.toggle(eventId)    - voeg toe of verwijder een bookmark
//   BookmarkManager.isBookmarked(id)   - geef true/false terug
//   BookmarkManager.getAll()           - geef alle opgeslagen ID's als array
//   BookmarkManager.init()             - initialiseer alle knoppen op de huidige pagina

const BookmarkManager = (() => {
    // De sleutel waarmee we de bookmarks opslaan in localStorage
    const STORAGE_KEY = 'EventApp_bookmarks';

    // Haal alle opgeslagen event ID's op als array van integers
    function getAll() {
        try {
            const raw = localStorage.getItem(STORAGE_KEY);
            const parsed = raw ? JSON.parse(raw) : [];
            // Zorg altijd voor een array van integers (geen strings)
            return Array.isArray(parsed) ? parsed.map(Number).filter(n => !isNaN(n) && n > 0) : [];
        } catch {
            return [];
        }
    }

    // Sla een bijgewerkte lijst van ID's op
    function saveAll(ids) {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(ids));
    }

    // Controleer of een event is opgeslagen
    function isBookmarked(eventId) {
        return getAll().includes(Number(eventId));
    }

    // Voeg toe of verwijder een bookmark; geeft de nieuwe toestand terug (true = opgeslagen)
    function toggle(eventId) {
        const id = Number(eventId);
        let bookmarks = getAll();

        if (bookmarks.includes(id)) {
            bookmarks = bookmarks.filter(b => b !== id);
        } else {
            bookmarks.push(id);
        }

        saveAll(bookmarks);
        return bookmarks.includes(id);
    }

    // Pas de visuele toestand van een knop aan op basis van of het event is opgeslagen.
    // Wisselt ook de src van het <img> icoon binnen de knop.
    function updateButton(btn, bookmarked) {
        btn.classList.toggle('is-bookmarked', bookmarked);
        btn.setAttribute('aria-label', bookmarked ? 'Verwijder uit opgeslagen' : 'Opslaan');
        btn.setAttribute('title', bookmarked ? 'Verwijder uit opgeslagen' : 'Opslaan');

        const img = btn.querySelector('.EventApp-bookmark-icon');
        if (img) {
            img.src = bookmarked
                ? '/images/icons/img_bookmarked.png'
                : '/images/icons/img_notbookmarked.png';
        }
    }

    // Initialiseer alle bookmark-knoppen die op de pagina staan.
    // Dit werkt zowel op de homepagina (kaarten met data-event-id) als overal anders.
    function init() {
        document.querySelectorAll('.EventApp-bookmark-btn').forEach(btn => {
            const eventId = btn.dataset.eventId;
            if (!eventId) return;

            // Zet de begintoestand op basis van localStorage
            updateButton(btn, isBookmarked(eventId));

            // Voeg een klik-luisteraar toe die de toestand omschakelt
            btn.addEventListener('click', (e) => {
                // Voorkom dat de klik ook naar de kaart (of een link eromheen) gaat
                e.stopPropagation();

                const bookmarked = toggle(eventId);
                updateButton(btn, bookmarked);

                // Kleine animatie: voeg even een klasse toe voor de CSS-transitie
                btn.classList.add('is-animating');
                setTimeout(() => btn.classList.remove('is-animating'), 300);
            });
        });
    }

    // Start init zodra de DOM beschikbaar is
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Maak de publieke functies beschikbaar voor andere scripts
    return { getAll, isBookmarked, toggle, init, updateButton };
})();
