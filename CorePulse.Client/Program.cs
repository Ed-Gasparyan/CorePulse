using Blazored.LocalStorage;
using CorePulse.Client;
using CorePulse.Client.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Registers the root App component and its location in the index.html
builder.RootComponents.Add<App>("#app");

// Enables dynamic management of the HTML <head> (e.g., changing page titles)
builder.RootComponents.Add<HeadOutlet>("head::after");

// --- 1. LOCAL STORAGE ---
// Registers Blazored.LocalStorage to store JWT tokens and user sessions in the browser
builder.Services.AddBlazoredLocalStorage();

// --- 2. CUSTOM AUTHENTICATION ---
// Registers our custom implementation of AuthenticationStateProvider.
// This service manages who the current user is by reading from LocalStorage.
builder.Services.AddScoped<CustomAuthenticationStateProvider>();

// Maps the built-in AuthenticationStateProvider to our custom implementation
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthenticationStateProvider>());

// Adds core authorization services (e.g., AuthorizeView, [Authorize] attribute support)
builder.Services.AddAuthorizationCore();

// --- 3. HTTP CLIENT ---
// Registers a scoped HttpClient to communicate with the Backend API.
// Uses the base address of where the client app is hosted.
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Starts the WebAssembly application
await builder.Build().RunAsync();