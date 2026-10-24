using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

// Pokrece .NET u pregledniku. Nema Razor komponenti:
// stranica je obican HTML (wwwroot/index.html), a script.js poziva Minimizator.MinimizirajJS.
var builder = WebAssemblyHostBuilder.CreateDefault(args);
await builder.Build().RunAsync();
