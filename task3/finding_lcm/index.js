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
    const xRaw = req.query.x;
    const yRaw = req.query.y;

    if (!xRaw || !yRaw) {
        return res.send("NaN");
    }

    let x, y;
    try {
        x = BigInt(xRaw);   
        y = BigInt(yRaw);
    }    catch {
        return res.send("NaN");
    }

    if (x <= 0n || y <= 0n) {
        return res.send("NaN");
    }

    const result = lcm(x, y);
    res.type("text/plain").send(result.toString());
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log(`Server is running on port ${PORT}`);
});