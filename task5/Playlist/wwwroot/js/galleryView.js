import { fetchSongsPage, fetchLyrics } from './api.js';
import { renderDetailContent, wireLyricsSync, escapeHtml } from './detailView.js';

const BATCH_SIZE = 24;

export function createGalleryView({ gridEl, sentinelEl, loadingEl, getParams, overlay }){
    let nextBatch=1, isLoading=false, loadToken=0, observer=null, allRecords=[];

    function cardHtml(record){
        return `
            <div class="gallery-card" data-index="${record.index}">
                <img src="${record.coverUrl}" alt="Cover art for ${escapeHtml(record.title)}" loading="lazy">
                <div class="gallery-card-body">
                    <div class="gallery-card-title"><span>${escapeHtml(record.title)} - ${escapeHtml(record.artist)}</span></div>
                    <div class="gallery-card-meta">
                        <span>${escapeHtml(record.genre)}</span>
                        <span class="likes">👍 ${record.likes}</span>
                    </div>
                </div>
            </div>
        `;
    }
    
    function openOverlay(record){
        overlay.body.innerHTML = renderDetailContent(record);
        overlay.root.hidden = false;
        wireLyricsSync(overlay.body, record, fetchLyrics);
    }

    async function loadNextBatch(){
        if (isLoading) return;
        isLoading = true;
        loadingEl.hidden = false;
        const token = loadToken;
        const { locale, seed, likes } = getParams();
        try{
            const data = await fetchSongsPage({locale, seed, likes, page: nextBatch, pageSize: BATCH_SIZE});
            if (token !== loadToken) return;
            allRecords.push(...data.records);
            const frag = document.createElement('div');
            frag.innerHTML = data.records.map(cardHtml).join('');
            Array.from(frag.children).forEach((card)=>{
                gridEl.appendChild(card);
                const titleEl = card.querySelector('.gallery-card-title');
                const titleSpan = titleEl?.querySelector('span');
                if (titleEl && titleSpan){
                    const overflow = titleSpan.scrollWidth > titleEl.clientWidth;
                    if (overflow){
                        titleEl.classList.add('is-scrollable');
                        const distance = titleEl.clientWidth - titleSpan.scrollWidth;
                        titleSpan.style.setProperty('--scroll-distance', `${distance}px`);
                    }
                }
                card.addEventListener('click', ()=>{
                    const idx = parseInt(card.dataset.index, 10);
                    const record = allRecords.find((r) => r.index === idx);
                    if (record) openOverlay(record);
                });
            });
            nextBatch += 1;
        } finally {
            if (token === loadToken){
                isLoading = false;
                loadingEl.hidden = true;
            }
        }
    }
    
    async function refreshLoaded(){
        if(allRecords.length === 0) return;
        const token = loadToken;
        const {locale, seed, likes} = getParams();
        const loadedBatches = nextBatch - 1;
        const refreshed = [];
        for (let page=1; page<=loadedBatches; page++){
            const data = await fetchSongsPage({locale, seed, likes, page, pageSize: BATCH_SIZE});
            if (token!==loadToken) return;
            refreshed.push(...data.records);
        }
        allRecords = refreshed;
        const likesByIndex = new Map(refreshed.map((r)=>[r.index, r.likes]));
        gridEl.querySelectorAll('.gallery-card').forEach((card)=>{
            const idx = parseInt(card.dataset.index, 10);
            const newLikes = likesByIndex.get(idx);
            if (newLikes !==undefined){
                const likesEl = card.querySelector('.likes');
                if (likesEl) likesEl.textContent = `👍 ${newLikes}`;
            }
        });
    }
    
    function setupObserver() {
        if (observer) observer.disconnect();
        observer = new IntersectionObserver((entries) => {
            for (const entry of entries) {
                if (entry.isIntersecting) loadNextBatch();
            }
        }, { rootMargin: '400px' });
        observer.observe(sentinelEl);
    }

    function reset(){
        loadToken+=1; nextBatch=1; allRecords=[]; gridEl.innerHTML=''; window.scrollTo({top: 0, behavior: 'instant' in window ? 'instant' : 'auto'});
        loadNextBatch();
    }

    async function activate() {
        if (gridEl.children.length === 0){
            await loadNextBatch();
        }
        setupObserver();
    }
    function getVisibleIndexes(){
        return allRecords.map((record)=>record.index).filter((index)=>Number.isInteger(index) && index > 0);
    }
    return { reset, activate, refreshLoaded, getVisibleIndexes };
}
        
        
    