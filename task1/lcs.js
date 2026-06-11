for([,,...k]=process.argv,o=(s=k[0]||'').length;o;o--)for(i=0;(r=s.substr(i++,o))[o-1];)k.every(m=>m.includes(r))&&(console.log(r),o%=0)
