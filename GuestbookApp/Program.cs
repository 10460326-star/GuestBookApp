var builder = WebApplication.CreateBuilder(args);

// 1. 註冊 MVC 服務
builder.Services.AddControllersWithViews();

// 2. 註冊 Session 服務（這行必須在 builder.Build() 上方）
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// 3. 處理開發環境
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔥【關鍵鐵律】UseSession 必須死死地釘在 UseRouting 之後、MapControllerRoute 之前！
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();