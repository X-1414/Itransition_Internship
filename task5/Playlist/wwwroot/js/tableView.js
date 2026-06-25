import { fetchSongsPage, fetchLyrics } from './api.js';
import { renderDetailContent, wireLyricsSync, escapeHtml } from './detailView.js';

const PAGE_SIZE = 20;

export function createTableView({tbody, paginationEl, getParams}){
    let currentPage=1, expandedIndex=null, loadToken=0;

    function rowHtml(record){
        const singleTag = record.isSingle ? '<span class="song-single-tag">Single</span>' : '';
        return `
            <tr data-index="${record.index}" class="song-row">
                <td class="col-index">${record.index}</td>
                <td class="col-title song-title-cell">${escapeHtml(record.title)}${singleTag}</td>
                <td class="col-artist">${escapeHtml(record.artist)}</td>
                <td class="col-album">${escapeHtml(record.albumTitle)}</td>
                <td class="col-genre song-genre-cell">${escapeHtml(record.genre)}</td>
                <td class="col-likes">${record.likes}</td>
            </tr>
            <tr class="detail-row" data-detail-for="${record.index}" hidden>
                <td colspan="6"></td>
            </tr>
        `;
    }

    async function toggleExpand(index, record){
        const detailRow = tbody.querySelector(`tr.detail-row[data-detail-for="${index}"]`);
        const mainRow = tbody.querySelector(`tr.song-row[data-index="${index}"]`);
        if (!detailRow) return;

        if (expandedIndex === index){
            detailRow.hidden = true;
            mainRow.classList.remove('is-expanded');
            expandedIndex=null;
            return;
        }
        
        if (expandedIndex !== null){
            const prevDetail = tbody.querySelector(`tr.detail-row[data-detail-for="${expandedIndex}"]`);
            const prevMain= tbody.querySelector(`tr.song-row[data-index="${expandedIndex}"]`);
            if (prevDetail) prevDetail.hidden=true;
            if (prevMain) prevMain.classList.remove('is-expanded');
        }
        
        const cell = detailRow.querySelector('td');
        cell.innerHTML = `<div class="detail-row-inner">${renderDetailContent(record)}</div>`;
        detailRow.hidden=false;
        mainRow.classList.add('is-expanded');
        expandedIndex = index;
        wireLyricsSync(cell, record, fetchLyrics);
    }

    function renderPagination(){
        paginationEl.innerHTML = '';
        const addBtn = (label, page, opts = {})=> {
            const btn = document.createElement('button');
            btn.className = 'page-btn' + (opts.active ? ' is-active' : '');
            btn.textContent = label;
            btn.disabled = !!opts.disabled;
            btn.addEventListener('click', ()=>goToPage(page));
            paginationEl.appendChild(btn);
        };

        
        addBtn('<', currentPage - 1, { disabled: currentPage === 1 });

        const windowStart = Math.max(1, currentPage-2);
        const windowEnd = currentPage + 2;
        if (windowStart>1){
            addBtn('1', 1);
            if (windowStart > 2){
                const span = document.createElement('span');
                span.className = 'page-ellipsis';
                span.textContent = '...';
                paginationEl.appendChild(span);
            }
        }

        for (let p=windowStart; p<=windowEnd; p++){
            addBtn(String(p), p, {active: p===currentPage});
        }
        const span = document.createElement('span');
        span.className='page-ellipsis';
        span.textContent = '...';
        paginationEl.appendChild(span);
        addBtn('>', currentPage+1);
    }

    async function loadPage(page){
        const token = ++loadToken;
        currentPage = page;
        expandedIndex = null;
        tbody.innerHTML = `<tr class="skeleton-row"><td colspan="6">Loading...</td></tr>`;
        const {locale, seed, likes} = getParams();
        const data = await fetchSongsPage({locale, seed, likes, page, pageSize: PAGE_SIZE});
        if (token !== loadToken) return;

        tbody.innerHTML = data.records.map(rowHtml).join('');
        renderPagination();
        tbody.querySelectorAll('tr.song-row').forEach((row) => {
            row.addEventListener('click', ()=>{
                const idx = parseInt(row.dataset.index, 10);
                const record = data.records.find((r)=>r.index===idx);
                if(record) toggleExpand(idx, record);
            });
        });
    }
         
    function goToPage(page){
        if (page<1) return;
        loadPage(page);
    }

    async function refreshCurrentPage(){
        const token = ++loadToken;
        const wasExpanded = expandedIndex;
        const {locale, seed, likes} = getParams();
        const data = await fetchSongsPage({locale, seed, likes, page: currentPage, pageSize: PAGE_SIZE});
        if (token!==loadToken) return;

        tbody.innerHTML = data.records.map(rowHtml).join('');
        renderPagination();

        tbody.querySelectorAll('tr.song-row').forEach((row)=>{
            row.addEventListener('click', ()=> {
                const idx = parseInt(row.dataset.index, 10);
                const record = data.records.find((r)=>r.index===idx); 
                if (record) toggleExpand(idx, record);
            });
        });

        expandedIndex = null;
        if (wasExpanded !== null){
            const record = data.records.find((r)=>r.index === wasExpanded);
            if (record) toggleExpand(wasExpanded, record);
        }
    }

    function reset() {
        loadPage(1);
    }
    
    function getVisibleIndexes(){
        return Array.from(tbody.querySelectorAll('tr.song-row[data-index]'))
            .map((row)=>parseInt(row.dataset.index, 10))
            .filter((index)=>Number.isInteger(index) && index > 0);
    }
    return {loadPage, reset, goToPage, refreshCurrentPage, getVisibleIndexes};
}
            