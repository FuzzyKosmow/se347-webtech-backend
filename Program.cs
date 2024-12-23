using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using ecommerce_api;
using ecommerce_api.Models;
using ecommerce_api.Profiles;
using ecommerce_api.Services;
using ecommerce_api.Services.JWT;
using ecommerce_api.Services.NotificationService;
using ecommerce_api.Services.OrderService;
using ecommerce_api.Services.PaymentService;
using ecommerce_api.Services.ProductService;
using ecommerce_api.Services.ShippingService;
using ecommerce_api.Services.SiteInfoService;
using ecommerce_api.Services.TaxService;
using ecommerce_api.Services.VoucherService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
// Mark variable to use in memory database
var useInMem = false;
#region Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options =>
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);
    }
);

builder.Services.AddControllers();

// CORS anywhere
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader()
               .WithExposedHeaders("X-Total-Count");
    });
});
#endregion
#region Database
if (useInMem)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseInMemoryDatabase("EcommerceApi");
        options.EnableSensitiveDataLogging();
    });
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    });
}
#endregion

#region Authentication
// JSON Serialization settings
builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
// Database: Register seeding service for development
builder.Services.AddTransient<DatabaseSeeder>();

// JWT Settings and service
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);
builder.Services.AddScoped<JwtService>();
// Identity
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"])),
    };
});
#endregion


#region Policy
builder.Services.AddAuthorization(options =>
{
    // Roles
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
});
#endregion


#region Services
builder.Services.AddScoped<PromotionService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ITaxService, TaxService>();
builder.Services.AddScoped<IShippingService, ShippingService>();

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ISiteInfoService, SiteInfoService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
#endregion


#region Mapping
// Profiles mapping
builder.Services.AddAutoMapper(typeof(UserMappingProfile));
builder.Services.AddAutoMapper(typeof(ProductMappingProfile));
#endregion
var app = builder.Build();

app.UseCors();

// Seed if is development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // //Apply migrations
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.EnsureRoles();

        await seeder.SeedDatabase();
    }
}
app.UseHttpsRedirection();

// JWT Middleware
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();



app.Run();

