using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Hosting;
using OnShop; // Ensure this matches your actual namespace

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set timeout as needed
    options.Cookie.HttpOnly = true; // Make the session cookie HTTP-only
    options.Cookie.IsEssential = true; // Make the session cookie essential for the application
});



// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("VendorPolicy", policy => policy.RequireRole("Vendor"));
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
});



string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddScoped<GuestDbFunctions>(provider =>
    new GuestDbFunctions(connectionString)); // Register GuestDbFunctions
builder.Services.AddScoped<UserDbFunctions>(provider =>
    new UserDbFunctions(connectionString));  // Register UserDbFunctions
builder.Services.AddScoped<AdminDbFunctions>(provider =>
    new AdminDbFunctions(connectionString)); // Register AdminDbFunctions
builder.Services.AddScoped<VendorDbFunctions>(provider =>
    new VendorDbFunctions(connectionString));  // Register VendorDbFunctions



builder.Services.AddSingleton<AdminDbFunctions>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new AdminDbFunctions(connectionString);
});

builder.Services.AddSingleton<GuestDbFunctions>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new GuestDbFunctions(connectionString);
});

builder.Services.AddSingleton<UserDbFunctions>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new UserDbFunctions(connectionString);
});

builder.Services.AddSingleton<VendorDbFunctions>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new VendorDbFunctions(connectionString);
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add session middleware
app.UseSession();


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Guest}/{action=GuestHome}/{id?}");

app.Run();