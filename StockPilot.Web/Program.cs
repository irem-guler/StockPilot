using StockPilot.BusinessLayer.Abstract;
using StockPilot.BusinessLayer.Concrete;
using StockPilot.DataAccessLayer.Abstract;
using StockPilot.DataAccessLayer.Concrete;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockPilot.DataAccessLayer.Context;
using StockPilot.EntityLayer.Entities;

var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<StockPilotContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<StockPilotContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddScoped<IProductDal, ProductRepository>();
builder.Services.AddScoped<IWarehouseDal, WarehouseRepository>();
builder.Services.AddScoped<IWarehouseStockDal, WarehouseStockRepository>();
builder.Services.AddScoped<IStockMovementDal, StockMovementRepository>();

builder.Services.AddScoped<IProductService, ProductManager>();
builder.Services.AddScoped<IWarehouseService, WarehouseManager>();
builder.Services.AddScoped<IInventoryService, InventoryManager>();

builder.Services.AddScoped<ISupplierDal, SupplierRepository>();
builder.Services.AddScoped<ISupplierService, SupplierManager>();
builder.Services.AddScoped<ICustomerDal, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerManager>();

builder.Services.AddScoped<IPurchaseOrderDal, PurchaseOrderRepository>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderManager>();

builder.Services.AddScoped<ISalesOrderDal, SalesOrderRepository>();
builder.Services.AddScoped<ISalesOrderService, SalesOrderManager>();

builder.Services.AddScoped<IReorderService, ReorderManager>();

builder.Services.AddScoped<StockPilot.Web.Services.OrderPdfService>();

builder.Services.AddHttpClient<StockPilot.Web.Services.DistanceService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    await StockPilot.Web.Data.SeedData.InitializeAsync(services);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();