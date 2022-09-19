using WebApp.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// var sc = Scheduling.Instant;
// var streamReader = new StreamReader("/home/r/test.txt");
// var list = await Utils.Parser(streamReader);
// if (list != null) sc.InitQueue(list);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IHostedService, WebApp.Services.Scheduling>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

var websocketOpts = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(1)
};
app.UseWebSockets(websocketOpts);
app.UseMiddleware<WebsocketHandler>();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();