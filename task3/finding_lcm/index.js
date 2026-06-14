const express = require("express");
const app = express();

function isNatural(n){
    return n > 0 && Number.isInteger(n);    
}

function gcd(x,y){
    while(y!==0){
        [x,y] = [y,x%y];
    }
    return x;
}

function lcm(x,y){
    return (x*y)/gcd(x,y);
}

app.get("/", (req, res) => {
    const x = Number(req.query.x);
    const y = Number(req.query.y);

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