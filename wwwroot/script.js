let I = document.getElementById("i");
let ILI = document.getElementById("ili");
let NE = document.getElementById("ne");
let otvorena = document.getElementById("otvorena");
let zatvorena = document.getElementById("zatvorena");
let input = document.getElementById("input")
let out = document.getElementById("rezultat");

let minimiziraj = document.getElementById("minimiziraj");
let rezultat = document.getElementById("rezultat");
let tablica = document.getElementById("tablica");

//obradzuem tipke
I.addEventListener("click", function(){
    input.value = input.value + " & "
});

ILI.addEventListener("click",function(){
    input.value = input.value + " | "
});

NE.addEventListener("click",function(){
    input.value = input.value + " ! "
})

otvorena.addEventListener("click",function(){
    input.value = input.value + "("
})

zatvorena.addEventListener("click",function(){
    input.value = input.value + ")"
})




minimiziraj.addEventListener("click", async function () {
    rezultat.textContent = "...";
    tablica.innerHTML = "";

    let podaci;

    // 1. pozvati C# funkciju MinimizirajJS iz PM_minimizacija.cs (radi u pregledniku)
    try {
        podaci = await DotNet.invokeMethodAsync("PM_Projeck", "MinimizirajJS", input.value);
    } catch (e) {
        let poruka = String(e.message || e);

        rezultat.style.backgroundColor = "#3a1616";      
        rezultat.style.border = "1px solid #ff9a91";      
        rezultat.style.color = "#ff9a91";   
        rezultat.style.borderRadius = "10px";    
 

        if (typeof DotNet === "undefined" || poruka.includes("No .NET call dispatcher")) {
            // .NET se jos ucitava
            rezultat.textContent = "Učitavanje... pokušaj ponovno za sekundu.";
            return;
        }
        // C# greska izgleda kao "System.FormatException: tekst\n   at ..." - uzmi samo tekst
        poruka = poruka.split("\n")[0].replace(/^[\w.]+Exception:\s*/, "");
        rezultat.textContent = "Greška: " + poruka;
        return;
    }
    rezultat.style.backgroundColor = "#14171c;";      
    rezultat.style.border = "#14171c;";      
    rezultat.style.color = "#9aa3af;";   
    rezultat.style.borderRadius = "0px"; 
    // 2. ispisati
    rezultat.textContent = podaci.rezultat;

    // tablica istinitosti
    let n = podaci.varijable.length;
    let html = "<tr>";
    for (let v of podaci.varijable) html += "<th>" + v + "</th>";
    html += "<th>Y</th></tr>";

    podaci.izlazi.forEach(function (y, i) {
        let bitovi = i.toString(2).padStart(n, "0");
        html += "<tr>";
        for (let b of bitovi) html += "<td>" + b + "</td>";
        html += "<td>" + (y ? 1 : 0) + "</td></tr>";
    });
    tablica.innerHTML = html;
});
