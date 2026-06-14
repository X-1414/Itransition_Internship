const fs = require('fs');
const path = require('path');
const {sha3_256} = require('js-sha3');

const folder = "data/";
function getAllFiles(dir){
    return fs.readdirSync(dir).filter(f=>f.endsWith('.data')).map(f => path.join(dir, f)).filter(f => fs.statSync(f).isFile());
}

function fileHash(filePath) {
    const data = fs.readFileSync(filePath);
    return sha3_256(data);
}

function sortingKey(hash){
    let mult = 1n;
    for (const ch of hash) {
        const v = BigInt(parseInt(ch, 16))+1n;
        mult *= v;
    }   
    return mult;
}

const files = getAllFiles(folder);

if (files.length !== 256){
    console.error("WARNING: file count = ", files.length, " expected 256");
}

const hashes = files.map(fileHash);

const sorted = hashes.sort((a, b) => {
    const ka = sortingKey(a);
    const kb = sortingKey(b);
    return ka < kb ? -1 : (ka > kb ? 1 : 0);
});

const combined = sorted.join("");
const email = "khv.uzb14@gmail.com";
const finalStr = combined + email;
const finalHash = sha3_256(finalStr);

console.log(finalHash);

// hash code: 3e3d8bac95b25e58746abd2e60cab15b3cff261ce3b693ed7697d465011269a3
