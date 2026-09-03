// bookmarks-page.js
// Dit script draait alleen op de /bookmarks pagina.
// Het haalt de opgeslagen event ID's op uit localStorage,
// vraagt de event data op via de server API, en toont de eventkaarten.

document.addEventListener('DOMContentLoaded', async () => {
    const loadingEl = document.getElementById('EventApp-bookmarks-loading');
    const emptyEl = document.getElementById('EventApp-bookmarks-empty');
    const resultsEl = document.getElementById('EventApp-bookmarks-results');

    // Haal de opgeslagen ID's op via BookmarkManager (gedefinieerd in bookmarks.js)
    const ids = BookmarkManager.getAll();

    // Geen bookmarks opgeslagen: toon de lege toestand
    if (ids.length === 0) {
        loadingEl.hidden = true;
        emptyEl.hidden = false;
        return;
    }

    // Haal de bijbehorende event data op van de server
    let events = [];
    try {
        const response = await fetch(`/api/bookmarked-events?ids=${ids.join(',')}`);
        if (!response.ok) throw new Error(`Server antwoordde met status ${response.status}`);
        events = await response.json();
    } catch (err) {
        console.error('Fout bij ophalen van opgeslagen evenementen:', err);
        loadingEl.hidden = true;
        emptyEl.hidden = false;
        return;
    }

    // Als de server geen events teruggeeft (bijv. verwijderde events)
    if (!events || events.length === 0) {
        loadingEl.hidden = true;
        emptyEl.hidden = false;
        return;
    }

    // Render elke eventkaart en voeg hem toe aan de resultatenlijst
    events.forEach(event => {
        const cardHtml = buildEventCard(event);
        resultsEl.insertAdjacentHTML('beforeend', cardHtml);
    });

    // Toon de resultaten
    loadingEl.hidden = true;
    resultsEl.hidden = false;

    // Initialiseer de bookmark-knoppen op de net-gebouwde kaarten
    // (BookmarkManager.init() pakt alle .EventApp-bookmark-btn elementen op)
    BookmarkManager.init();
});

// Bouwt de HTML voor één eventkaart op, identiek aan de server-side Razor template.
// De API retourneert camelCase JSON (bijv. eventId, eventName).
function buildEventCard(event) {
    const priceText = event.minPrice === 0
        ? 'Gratis'
        : `\u20AC${formatPrice(event.minPrice)}+`;

    const description = event.description && event.description.trim()
        ? event.description
        : 'Geen beschrijving beschikbaar.';

    const imageUrl = event.imageUrl && event.imageUrl.trim()
        ? event.imageUrl
        : '/images/events/placeholder.jpg';

    const categories = Array.isArray(event.categories) ? event.categories : [];

    const accessibilityItems = event.accessibilityText
        ? event.accessibilityText.split(',').map(s => s.trim()).filter(Boolean)
        : [];

    const compactAccessibility = accessibilityItems.length === 0
        ? ''
        : accessibilityItems.length === 1
            ? accessibilityItems[0]
            : accessibilityItems.length === 2
                ? `${accessibilityItems[0]}, ${accessibilityItems[1]}`
                : `${accessibilityItems[0]}, ${accessibilityItems[1]} en meer`;

    const categoryBadgesHtml = categories.map(cat => `
        <span class="EventApp-category-badge" title="${escapeHtml(cat)}">
            <span class="EventApp-category-icon" aria-hidden="true">${getCategoryIcon(cat)}</span>
            <span class="EventApp-category-label">${escapeHtml(cat)}</span>
        </span>
    `).join('');

    return `
        <section class="EventApp-event-row" data-event-id="${event.eventId}">
            <article class="EventApp-card">
                <div class="EventApp-card-media">
                    <img src="${escapeHtml(imageUrl)}" alt="${escapeHtml(event.eventName)}" />
                    <div class="EventApp-category-badges">
                        ${categoryBadgesHtml}
                    </div>
                </div>

                <div class="EventApp-card-content">
                    <div class="EventApp-card-head">
                        <h2>${escapeHtml(event.eventName)}</h2>
                        <button
                            class="EventApp-bookmark-btn is-bookmarked"
                            data-event-id="${event.eventId}"
                            type="button"
                            aria-label="Verwijder uit opgeslagen"
                            title="Verwijder uit opgeslagen">
                            <img class="EventApp-bookmark-icon" src="/images/icons/img_bookmarked.png" alt="" aria-hidden="true" />
                        </button>
                    </div>

                    <div class="EventApp-meta">
                        <div class="EventApp-price">${escapeHtml(priceText)}</div>
                        <div class="EventApp-rating">
                            <img class="EventApp-rating-inline-icon" src="/images/icons/img_rating.png" alt="Rating" />
                            <span>${escapeHtml(event.ratingText || 'Goed')}</span>
                        </div>
                    </div>

                    <div class="EventApp-description-block">
                        <p class="EventApp-description-text">${escapeHtml(description)}</p>
                        <a class="EventApp-read-more-link" href="/event/${event.eventId}">Lees meer</a>
                    </div>

                    <div class="EventApp-bottom-row">
                        <div class="EventApp-extra-meta">${escapeHtml(compactAccessibility)}</div>
                        <div class="EventApp-card-footer">
                            <a class="EventApp-order-btn" href="/event/${event.eventId}">Bestel</a>
                        </div>
                    </div>
                </div>
            </article>
        </section>
    `;
}

// Zet speciale HTML-tekens om zodat ze veilig in HTML ingevoegd kunnen worden (XSS-preventie)
function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

// Geeft een emoji/teken terug dat bij de categorie hoort (identiek aan EventCards.cs)
function getCategoryIcon(category) {
    const cat = category.toLowerCase();
    if (/house|techno|edm|trance|drum|hardstyle|muziek|music/.test(cat)) return '♪';
    if (/schilder|kunst|art|tekenen|illustratie/.test(cat)) return '✎';
    if (/historie|history|erfgoed|museum/.test(cat)) return '⌛';
    if (/film|cinema|documentaire/.test(cat)) return '◉';
    if (/theater|toneel|show|performance/.test(cat)) return '◌';
    if (/dans|dance/.test(cat)) return '♬';
    if (/eten|food|drank/.test(cat)) return '☕';
    return '•';
}

// Formatteert een prijs: verwijdert onnodige nullen (bijv. 10.50 -> 10.5, 10.00 -> 10)
function formatPrice(price) {
    return parseFloat(price).toFixed(2).replace(/\.?0+$/, '');
}
