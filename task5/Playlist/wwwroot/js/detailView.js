function escapeHtml(str){
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}

export function renderDetailContent(record){
    return `
        <div class="detail-content">
        <img class="detail-cover" src="${record.coverUrl}" alt="Cover art for ${escapeHtml(record.title)}" loading="lazy">
        <div class="detailed-meta">
            <h3>${escapeHtml(record.title)}</h3>
            <p class="detail-artist">${escapeHtml(record.artist)}</p>
            <div class="detail-tags">
                <span class="detail-tag">${escapeHtml(record.albumTitle)}</span>
                <span class="detail-tag">${escapeHtml(record.genre)}</span>
                <span class="detail-tag">👍 ${record.likes}</span>
            </div>
            <p class="detail-review">${escapeHtml(record.reviewText)}</p>
            <audio controls preload="none" src="${record.audioUrl}" data-role="player"></audio>
            <div class="lyrics-box" data-role="lyrics">
                <span class="lyrics-line">Loading lyrics...</span>
            </div>
        </div>
    </div>
    `;
}

export async function wireLyricsSync(container, record, fetchLyrics){
    const audioEl = container.querySelector('[data-role="player"]');
    const lyricsBox = container.querySelector('[data-role="lyrics"]');
    if (!audioEl || !lyricsBox) return;

    try{
        const data = await fetchLyrics(record.lyricsUrl);
        lyricsBox.innerHTML = data.lines.map((line,i)=>`<div class="lyrics-line" data-start="${line.startSec}" data-end="${line.endSec}">${escapeHtml(line.text)}</div>`).join('');
    }
    catch(e){
        lyricsBox.innerHTML = '<span class="lyrics-line">Lyrics are unavailable.</span>';
        return;
    }

    const lineEls = Array.from(lyricsBox.querySelectorAll('.lyrics-line[data-start]'));
    let rafId = null;

    function updateHighlight(){
        const t = audioEl.currentTime;
        let activeEl = null;
        for (const el of lineEls){
            const start = parseFloat(el.dataset.start);
            const end = parseFloat(el.dataset.end);
            const isActive = t >= start && t<end;
            el.classList.toggle('is-current', isActive);
            if (isActive) activeEl=el;
        }
        if (activeEl) activeEl.scrollIntoView({block: 'nearest', behavior: 'smooth'});
        if (!audioEl.paused && !audioEl.ended) rafId = requestAnimationFrame(updateHighlight);
    }
    
    audioEl.addEventListener('play', ()=>{
        if (rafId) cancelAnimationFrame(rafId);
        rafId = requestAnimationFrame(updateHighlight);
    });
   
    audioEl.addEventListener('pause', ()=>{
        if (rafId) cancelAnimationFrame(rafId);
    });

    audioEl.addEventListener('seeked', ()=>{
        if (!audioEl.paused) return;
        const t = audioEl.currentTime;
        for (const el of lineEls){
            const start = parseFloat(el.dataset.start);
            const end = parseFloat(el.dataset.end);
            el.classList.toggle('is-current', t>=start && t<end);
        }
    });
}

export { escapeHtml };