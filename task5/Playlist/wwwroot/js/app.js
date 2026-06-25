import { fetchLocales, buildExportUrl } from './api.js';
import { createTableView } from './tableView.js';
import { createGalleryView } from './galleryView.js';

const localeSelect = document.getElementById('locale-select');
const seedInput = document.getElementById('seed-input');
const randomizeSeedBtn = document.getElementById('randomize-seed');
const likesInput = document.getElementById('likes-input');
const likesValueLabel = document.getElementById('likes-value');
const viewButtons = document.querySelectorAll('.view-btn');
const tableViewPanel = document.getElementById('table-view');
const galleryViewPanel = document.getElementById('gallery-view');
const exportBtn = document.getElementById('export-btn');
const toast = document.getElementById('export-toast');

const detailOverlay = document.getElementById('detail-overlay');
const detailBody = document.getElementById('detail-body');
const detailClose = document.getElementById('detail-close');

function randomSeed(){
    const part1 = Math.floor(Math.random() * 1e9);
    const part2 = Math.floor(Math.random() * 1e9);
    return  `${part1}${part2}`.slice(0,18);
}

function getParams(){
    return {locale: localeSelect.value || 'en', seed: seedInput.value.trim() || '0', likes: parseFloat(likesInput.value),};
}

function showToast(message, isError=false){
    toast.textContent = message;
    toast.classList.toggle('is-error', isError);
    toast.hidden = false;
    clearTimeout(showToast._t);
    showToast._t = setTimeout(()=>{toast.hidden=true;}, 4000);
}

function closeOverlay(){
    detailOverlay.hidden = true;
    const audioEl = detailBody.querySelector('audio');
    if (audioEl) audioEl.pause();
    detailBody.innerHTML = '';
}
detailClose.addEventListener('click', closeOverlay);
detailOverlay.addEventListener('click', (e) => {if (e.target === detailOverlay) closeOverlay();});
document.addEventListener('keydown', (e) => {if (e.key === 'Escape' && !detailOverlay.hidden) closeOverlay();});

let tableView;
let galleryView;
let activeView = 'table';

const debouncedLikesChanged = debounce(onLikesChanged, 200);

let tableLikesStale = false;
let galleryLikesStale = false;

function onParamsChanged(){ tableView.reset(); galleryView.reset(); tableLikesStale=false; galleryLikesStale=false;}

function onLikesChanged(){
    if (activeView === 'table') {
        tableView.refreshCurrentPage();
        galleryLikesStale = true;}
    else {
        galleryView.refreshLoaded();
        tableLikesStale=true;
    }
}

async function setActiveView(view){
    activeView = view;
    viewButtons.forEach((btn)=>btn.classList.toggle('is-active', btn.dataset.view===view));
    tableViewPanel.hidden = view !== 'table';
    galleryViewPanel.hidden = view !== 'gallery';
    if (view === 'gallery'){
        await galleryView.activate();
        if (galleryLikesStale){
            galleryView.refreshLoaded();
            galleryLikesStale = false;
        } 
    } else {
        if (tableLikesStale) {
            await tableView.refreshCurrentPage();
            tableLikesStale = false;
        }
    }
}

async function init(){
    const {locales} = await fetchLocales();
    localeSelect.innerHTML = locales.map((l) => `<option value="${l.code}">${l.label}</option>`).join('');
    if (locales.some((l) => l.code === 'en')) localeSelect.value = 'en';

    seedInput.value = randomSeed();
    likesValueLabel.textContent = parseFloat(likesInput.value).toFixed(1);

    tableView = createTableView({
        tbody: document.getElementById('table-body'),
        paginationEl: document.getElementById('pagination'),
        getParams,
    });

    galleryView = createGalleryView({
        gridEl: document.getElementById('gallery-grid'),
        sentinelEl: document.getElementById('gallery-sentinel'),
        loadingEl: document.getElementById('gallery-loading'),
        getParams,
        overlay: {root: detailOverlay, body: detailBody},
    });

    tableView.loadPage(1);

    localeSelect.addEventListener('change', onParamsChanged);
    seedInput.addEventListener('input', debounce(onParamsChanged, 350));
    randomizeSeedBtn.addEventListener('click', ()=>{seedInput.value=randomSeed(); onParamsChanged();});
    likesInput.addEventListener('input', ()=>{
        likesValueLabel.textContent = parseFloat(likesInput.value).toFixed(1);
        debouncedLikesChanged();
    });

    viewButtons.forEach((btn)=>{btn.addEventListener('click', ()=>setActiveView(btn.dataset.view));
    });
    exportBtn.addEventListener('click', handleExport);
}

function debounce(fn, ms){
    let t;
    return (...args)=> {clearTimeout(t); t=setTimeout(()=>fn(...args), ms);};
}

async function handleExport(){
    const {locale, seed, likes} = getParams();
    exportBtn.disabled = true;
    exportBtn.textContent = 'Exporting ... ';
    try {
        let url;
        let message;
        if (activeView === 'table'){
            const indexes = tableView.getVisibleIndexes();
            url = buildExportUrl({locale, seed, likes, indexes});
            message = `export started - ${indexes.length} songs from the table will be downloaded shortly.`;
        } else {
            const indexes = galleryView.getVisibleIndexes();
            if (indexes.length > 0){
                url = buildExportUrl({locale, seed, likes, indexes});
                message = `export started - ${indexes.length} songs from the gallery will be downloaded shortly.`;
            } else {
                url = buildExportUrl({locale, seed, likes, count:20, startIndex:1});
                message = 'export started - first 20 songs will be downloaded shortly.';
            }
        }
        const a = document.createElement('a');
        a.href = url;
        a.download = 'songs-export.zip';
        document.body.appendChild(a);
        a.click();
        a.remove();
        showToast(message);
    } catch (e){showToast('Export failed. Please try again.', true);}
    finally{
        setTimeout(() => {exportBtn.disabled=false; exportBtn.textContent='Export Zip';},1500);
    }
}
init();

