using foodStoreSimulate.Components;
using foodStoreSimulate.Components.Services;
using foodStoreSimulate.Components.Services.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<RestaurantService>();
builder.Services.AddSingleton<TextLogger>();
builder.Services.AddSingleton<UIStateService>();
builder.Services.AddSingleton<CustomerQueueService>();
builder.Services.AddSingleton<FoodQueueService>();
builder.Services.AddSingleton<CustomerFactory>();
builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
