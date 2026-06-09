using OrderManagerSystem.Dao;
using OrderManagerSystem.Dto;
using OrderManagerSystem.Service;
using OrderManagerSystem.Services;//使用這個專案的services

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
//加入 DI 註冊
//告訴 ASP.NET Core：
//以後有人需要 MySqlService，你就幫我自動建立。
//Service
builder.Services.AddScoped<MySqlService>();
builder.Services.AddScoped<addOrderService>();
builder.Services.AddScoped<chartService>();
//Dao

builder.Services.AddScoped<selectDao>();
builder.Services.AddScoped<insertDao>();
builder.Services.AddScoped<updateDao>();
//Dto
builder.Services.AddScoped<ScheduleDto>();
builder.Services.AddScoped<addOrderDto>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

//預設的路由設定
//app.MapControllerRoute(
//自定義路由名稱
    //name: "default",
//決定網址要怎麼對應到程式檔案。第一層/第二層/第三層
    //pattern: "{controller=Home}/{action=Index}/{id?}")
    //controller的意思是網址第一層要是controller的名稱
    //controller=Home則表示controller預設為Home
    //action=Index表示會去HomeController去尋找有沒有叫Index的action
    //第三層的?表示這層無論有無都會對應到這個路由規則

    app.MapControllerRoute(
    name: "addOrder",
    pattern: "{controller=addOrder}/{action=addOrder}/{id?}")

    .WithStaticAssets();


app.Run();
