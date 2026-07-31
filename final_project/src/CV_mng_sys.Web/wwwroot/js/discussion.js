const discussionPositionId = window.__positionId;
const thread = document.getElementById('discussionThread');
let lastSeenId = 0;
const renderedIds = new Set();

function renderPost(post) {
    if (renderedIds.has(post.id)) return;
    renderedIds.add(post.id);
    const div = document.createElement('div');
    div.className = 'border-bottom pb-2 mb-2';
    const time = new Date(post.createdAtUtc).toLocaleString();
    div.innerHTML = `
        <div class="d-flex justify-content-between">
            <strong>${post.authorName}</strong>
            <small class="text-muted">${time}</small>
        </div>
        <div class="markdown-body"></div>
    `;
    div.querySelector('.markdown-body').innerHTML = marked.parse(post.contentMarkdown || '');
    thread.appendChild(div);
    thread.scrollTop = thread.scrollHeight;
    if (post.id > lastSeenId) lastSeenId = post.id;
}

async function poll(){
    try {
        const response = await fetch(`/Discussion/Poll?discussionPositionId=${discussionPositionId}&afterId=${lastSeenId}`);
        if (!response.ok) return;
        const posts = await response.json();
        posts.forEach(renderPost);
    } catch(err){}
}

poll();
setInterval(poll, 3000);
document.getElementById('discussionForm').addEventListener('submit', async(e) =>{
    e.preventDefault();
    const input = document.getElementById('discussionInput');
    const content = input.value.trim();
    if(!content) return;
    const body = new URLSearchParams({discussionPositionId, content});
    const response = await fetch('/Discussion/Post', {
        method: 'POST',
        headers: {'Content-Type': 'application/x-www-form-urlencoded'},
        body
    });
    if (response.ok){
        const post = await response.json();
        renderPost(post);
        input.value = '';
    }
});