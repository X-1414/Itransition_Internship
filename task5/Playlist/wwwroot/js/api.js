const BASE = '';

export async function fetchLocales(){
    const res = await fetch(`${BASE}/api/locales`);
    if (!res.ok) throw new Error('Failed to load locales');
    return res.json();
}

export async function fetchSongsPage({ locale, seed, likes, page, pageSize }) {
    const params = new URLSearchParams({locale, seed: String(seed), likes: String(likes), page: String(page), pageSize: String(pageSize),});
    const res = await fetch(`${BASE}/api/songs?${params}`);
    if (!res.ok) throw new Error('Failed to load songs');
    return res.json();
}

export async function fetchSongDetail({index, locale, seed, likes}){
    const params = new URLSearchParams({locale, seed: String(seed), likes: String(likes)});
    const res = await fetch(`${BASE}/api/songs/${index}?${params}`);
    if (!res.ok) throw new Error('Failed to load song details');
    return res.json();
}

export async function fetchLyrics(lyricsUrl){
    const res = await fetch(lyricsUrl);
    if (!res.ok) throw new Error('Failed to load lyrics');
    return res.json();
}

export function buildExportUrl({locale, seed, likes, count, startIndex, indexes}){
    const params = new URLSearchParams({locale, seed: String(seed), likes: String(likes)});
    if (Array.isArray(indexes) && indexes.length > 0){
        params.set('indexes', indexes.join(','));
    } else {
        params.set('count', String(count));
        params.set('startIndex', String(startIndex));
    }
    return `${BASE}/api/export.zip?${params}`;
}