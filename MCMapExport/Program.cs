using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Kaharonus.Avalonia.DependencyInjection;
using MCMapExport.Common;
using MCMapExport.Services;

namespace MCMapExport {
    class Program {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args) {
            Task.Run(EnumHelpers.BuildBlockTypeCache);
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        private static void ConfigureServices(IServiceCollection services) {
            services.AddSingleton(() => new WorldReaderService());
            services.AddSingleton(() => new ColorResolverService());
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseDependencyInjection(ConfigureServices)
                .LogToTrace();
    }
}