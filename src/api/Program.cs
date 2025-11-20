using api.Index;
using api.Managers;
using api.Repository;
using api.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:8080/");
});

builder.Services.AddSingleton<CustomerManager>();
builder.Services.AddSingleton<CustomerService>();
builder.Services.AddSingleton<CustomerRepository>();
builder.Services.AddSingleton<SearchService>();
builder.Services.AddSingleton<CustomerLuceneIndex>();

builder.Services.AddHostedService<CustomerManager>();
builder.Services.AddHostedService<CustomerEventManager>();

WebApplication app = builder.Build();

app.MapControllers();
app.Run();