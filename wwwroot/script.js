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




// Postavi klasu (uspjeh / greska) i ponovno pokreni CSS animaciju.
// Bez "offsetWidth" preglednik ne bi ponovio animaciju ako je klasa ista kao prije.
function prikazi(klasa, tekst) {
    rezultat.classList.remove("uspjeh", "greska");
    void rezultat.offsetWidth;
    rezultat.textContent = tekst;
    if (klasa) rezultat.classList.add(klasa);
}

minimiziraj.addEventListener("click", async function () {
    prikazi("", "...");
    tablica.innerHTML = "";
    minimiziraj.classList.add("racuna");

    let podaci;

    // 1. pozvati C# funkciju MinimizirajJS iz PM_minimizacija.cs (radi u pregledniku)
    try {
        podaci = await DotNet.invokeMethodAsync("PM_Projeck", "MinimizirajJS", input.value);
    } catch (e) {
        let poruka = String(e.message || e);

        if (typeof DotNet === "undefined" || poruka.includes("No .NET call dispatcher")) {
            // .NET se jos ucitava
            prikazi("greska", "Učitavanje... pokušaj ponovno za sekundu.");
            return;
        }
        // C# greska izgleda kao "System.FormatException: tekst\n   at ..." - uzmi samo tekst
        poruka = poruka.split("\n")[0].replace(/^[\w.]+Exception:\s*/, "");
        prikazi("greska", "Greška: " + poruka);
        return;
    } finally {
        minimiziraj.classList.remove("racuna");
    }

    // 2. ispisati
    prikazi("uspjeh", podaci.rezultat);

    // tablica istinitosti
    let n = podaci.varijable.length;
    let html = "<tr>";
    for (let v of podaci.varijable) html += "<th>" + v + "</th>";
    html += "<th>Y</th></tr>";

    podaci.izlazi.forEach(function (y, i) {
        let bitovi = i.toString(2).padStart(n, "0");
        html += "<tr style='--i:" + (i + 1) + "'>";   // --i = redni broj za kasnjenje animacije
        for (let b of bitovi) html += "<td>" + b + "</td>";
        html += "<td>" + (y ? 1 : 0) + "</td></tr>";
    });
    tablica.innerHTML = html;
});
