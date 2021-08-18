using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Kudu.Web5;

Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder => {
        webBuilder.UseStartup<Startup>();
    })
    .Build()
    .Run();