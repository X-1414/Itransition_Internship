// --------------------------------------------------------------------------------//
// Problem: Longest Common Substring
// Prof: 148 bytes
// --------------------------------------------------------------------------------//

// explicit solution: 671 bytes 
const s = process.argv.slice(2);

if (s.length === 0){
    console.log('');
} else {
    const shortest = s.reduce((a, b) => a.length < b.length ? a : b);
    let r = '';

    for (let len = shortest.length; len>=1; len--){
        let found = false;
        for (let start=0; start<=shortest.length-len; start++){
            let substr = shortest.slice(start, start+len);
            const existsInAll = s.every(str => str.includes(substr));
            if (existsInAll){
                r = substr;
                found=true;
                break;
            }
        }
        if(found) break;
    }
    console.log(r);
}

/* straightforward solution: 413 bytes */
if(a.length){
    let s=a.reduce((x,y)=>x.length<y.length?x:y);
    o:for(let l=s.length;l;l--)
        for(let i=0;i+l<=s.length;i++){
            let t=s.slice(i,i+l);
            if(a.every(x=>x.includes(t))){r=t;break o}
  }
}
console.log(r)

/// 239 bytes
let a=process.argv.slice(2),r='';if(a.length){let s=a.reduce((x,y)=>x.length<=y.length?x:y);for(let i=0;i<s.length;i++)for(let j=i+1;j<=s.length;j++){let c=s.slice(i,j);if(c.length>r.length&&a.every(x=>x.includes(c)))r=c;}}console.log(r);

/// 225 bytes
k=process.argv.slice(2),r='';if(k.length){s=k.reduce((a,b)=>a.length<b.length?a:b);for(i=0;i<s.length;i++)for(j=i+1;j<=s.length;j++){t=s.slice(i,j);if(t.length>r.length&&k.every(m=>m.includes(t)))r = t;}}console.log(r);

///212 bytes
k=process.argv.slice(2),r='';if(k.length){s=k.reduce((a,b)=>a.length<b.length?a:b);for(i=0;i<s.length;i++)for(j=i+1;j<=s.length;j++)if(k.every(m=>m.includes(t=s.slice(i,j)))&&t.length>r.length)r=t}console.log(r)

/// 186 bytes
[,,...k]=process.argv,r='';if(k[0])for(s=k.reduce((a,b)=>b[a.length]?a:b),i=0;s[i];i++)for(j=i;s[j++];)k.every(m=>m.includes(t=s.slice(i,j)))&&t[r.length]&&(r=t);console.log(r)

// 182 bytes
[,,...k]=process.argv,r='';if(k[0])for(s=k.reduce((a,b)=>b[a.length]?a:b),i=0;s[i];i++)for(j=i+r;s[j++];)k.every(m=>m.includes(t=s.slice(i,j)))&&t[r.length]&&(r=t);console.log(r)

// 153 bytes
k=process.argv.slice(2);s=k[0]||'';o:for(o=s.length;o;o--)for(i=0;r=s.slice(i,i+o),r[o-1];i++)if(k.every(m=>m.includes(r))){console.log(r);break o}

// 149bytes
k=process.argv.slice(2);o:for(o=(s=k[0]||'').length;o;o--)for(i=0;r=s.slice(i,i+o),r[o-1];i++)if(k.every(m=>m.includes(r))){console.log(r);break o}

// 137 --> final
for([,,...k]=process.argv,o=(s=k[0]||'').length;o;o--)for(i=0;(r=s.substr(i++,o))[o-1];)k.every(m=>m.includes(r))&&(console.log(r),o%=0)


