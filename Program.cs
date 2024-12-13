using electronNET.EndpointsGroups;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using System.Net;
using static electronNET.EndpointsGroups.ZKTecoGroups;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Interop.zkemkeeper;



var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseElectron(args);
builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy("Policy1",
                      policy =>
                      {
                          policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                      });
});


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();


// 啟用 Electron
if (HybridSupport.IsElectronActive)
{
    ElectronBootstrap();
}
else
{
    Console.WriteLine("not in Electron env.");
}
var api = app.MapGroup("/api").DisableAntiforgery();


app.UseCors("Policy1");

app.Run();

async void ElectronBootstrap()
{
    //Task.Run(async () =>
    //{
    Console.WriteLine("Electron start");
    var projectRoot = Directory.GetCurrentDirectory();
    var ZKEMServiceList = new List<ZKEMService>();


    var window = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
    {
        Show = false,
        Width = 1280,
        Height = 720,
        WebPreferences = new WebPreferences
        {
            Preload = Path.Combine(projectRoot, "preload.js"),
            NodeIntegration = false,
            ContextIsolation = true,
            WebSecurity = false,
        }
    });

    window.LoadURL("http://localhost:5173");
    //window.LoadURL("file://" + Path.Combine(projectRoot.Replace("\\", "/"), "wwwroot/index.html"));

    window.WebContents.OpenDevTools();
    // 監聽網頁載入完成的事件
    window.WebContents.OnDidFinishLoad += () =>
    {
        CleanIPCEvent();
        RemoveListener();

        Console.WriteLine("Electron window loaded successfully.");
        Electron.IpcMain.On("connectZKTeco", (dynamic args) =>
        {
            if (args is JArray)
            {
                foreach (var item in args)
                {
                    Console.WriteLine(item.ip.ToString());
                    var zkemService = new ZKEMService(item.ip.ToString(), 4370, 987123, window);
                    ZKEMServiceList.Add(zkemService);
                    zkemService.StartAsync();
                    //ZKTecoGroups.TCPConnect(window, item.ip.ToString());
                }
            }
        });
        Electron.IpcMain.On("disconnectZKTeco", (dynamic args) =>
        {
            RemoveListener();
        });
        window.Show(); // 網頁載入完成後顯示視窗

    };


    window.OnClosed += () =>
    {
        Console.WriteLine("Electron window on closed.");
        CleanIPCEvent();
        Electron.App.Quit(); // 當視窗關閉時退出應用
    };
    //});

    void CleanIPCEvent()
    {
        Electron.IpcMain.RemoveAllListeners("connectZKTeco");
        Electron.IpcMain.RemoveAllListeners("disconnectZKTeco");
    }
    void RemoveListener()
    {
        if (ZKEMServiceList.Count > 0) {
            ZKEMServiceList.ForEach((zkem) =>
            {
                zkem.CleanEvent();
                zkem._zkem.Disconnect();
                Electron.IpcMain.Send(window, "check", $"{zkem._ipAddress}離線成功");

            });
            ZKEMServiceList.Clear();
        }
    }
}
