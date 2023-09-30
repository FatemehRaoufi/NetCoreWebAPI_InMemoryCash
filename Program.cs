using NetCoreWebAPI_InMemoryCash.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();//Install Nuget in console: PM> Install-Package Swashbuckle.AspNetCore -Version 5.3.3

builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


//https://code-maze.com/aspnetcore-in-memory-caching/
//https://www.c-sharpcorner.com/article/how-to-implement-caching-in-the-net-core-web-api-application/
//https://stackoverflow.com/questions/35788911/500-error-when-setting-up-swagger-in-asp-net-core-mvc-6-app
//https://medium.com/berkut-teknoloji/in-memory-cache-use-of-memory-caching-in-net-core-41d99153ebd0
//https://www.c-sharpcorner.com/article/asp-net-core-in-memory-caching/?source=post_page-----41d99153ebd0--------------------------------

/*
 For most types of apps, IMemoryCache is enabled out of the box. 
For example, if we are calling AddMvc(), AddControllersWithViews(), AddRazorPages(), 
AddMvcCore().AddRazorViewEngine() etc., 

it enables the IMemoryCache. However, for apps that don’t call any of these methods, 
it may be needed to call AddMemoryCache() in the Program class. 
Of course, if we’re using an older version of .NET that comes with the Startup class, 
we need to call the AddMemoryCache() in the Startup class.
 
 */