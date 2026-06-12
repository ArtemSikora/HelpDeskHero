using HelpDeskHero.UI;
using HelpDeskHero.UI.Services.Api;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");

builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ =>
    new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5067")
    });

builder.Services.AddScoped<TicketApiClient>();

await builder.Build().RunAsync();