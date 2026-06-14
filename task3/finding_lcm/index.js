const express = require("express");
const app = express();

function isNatural(n){
    return n > 0 && Number.isInteger(n);    
}

function gcd(x,y){
    while(y!==0n){
        [x,y] = [y,x%y];
    }
    return x;
}

function lcm(x,y){
    return (x*y)/gcd(x,y);
}

app.get(["/", "/khv_uzb14_gmail_com"], (req, res) => {
    const x = BigInt(req.query.x);
    const y = BigInt(req.query.y);

    if (x <= 0n || y <= 0n) {
        return res.send("NaN");
    }

    if (!isNatural(x) || !isNatural(y)) {
        return res.send("NaN");
    }
    
    const result = lcm(x, y);
    res.type("text/plain").send(result.toString());
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log(`Server is running on port ${PORT}`);
});