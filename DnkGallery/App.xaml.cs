using DnkGallery.Model;
using DnkGallery.Presentation.Pages;
using Microsoft.UI.Composition.SystemBackdrops;
using Uno.Resizetizer;

namespace DnkGallery;

public partial class App : Application {
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App() {
        this.InitializeComponent();
    }
    
    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }
    
    protected override void OnLaunched(LaunchActivatedEventArgs args) {
        // Load WinUI Resources
        Resources.Build(r => r.Merged(
            new XamlControlsResources()));
        
        // Load Uno.UI.Toolkit Resources
        Resources.Build(r => r.Merged(
            new ToolkitResources()));
        var builder = this.CreateBuilder(args)
            .Configure(host => host
#if DEBUG
                // Switch to Development environment when running in DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseLogging(configure: (context, logBuilder) => {
                    // Configure log levels for different categories of logging
                    logBuilder
                        .SetMinimumLevel(
                            context.HostingEnvironment.IsDevelopment() ? LogLevel.Information : LogLevel.Warning)
                        
                        // Default filters for core Uno Platform namespaces
                        .CoreLogLevel(LogLevel.Warning);
                    
                    // Uno Platform namespace filter groups
                    // Uncomment individual methods to see more detailed logging
                    //// Generic Xaml events
                    //logBuilder.XamlLogLevel(LogLevel.Debug);
                    //// Layout specific messages
                    //logBuilder.XamlLayoutLogLevel(LogLevel.Debug);
                    //// Storage messages
                    //logBuilder.StorageLogLevel(LogLevel.Debug);
                    //// Binding related messages
                    //logBuilder.XamlBindingLogLevel(LogLevel.Debug);
                    //// Binder memory references tracking
                    //logBuilder.BinderMemoryReferenceLogLevel(LogLevel.Debug);
                    //// DevServer and HotReload related
                    //logBuilder.HotReloadCoreLogLevel(LogLevel.Information);
                    //// Debug JS interop
                    //logBuilder.WebAssemblyLogLevel(LogLevel.Debug);
                }, enableUnoLogging: true)
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        .EmbeddedSource<App>()
                        .Section<AppConfig>()
                )
                // Enable localization (see appsettings.json for supported languages)
                .UseLocalization()
                // Register Json serializers (ISerializer and ISerializer)
                .UseSerialization((context, services) => services
                    .AddContentSerializer(context)
                    .AddJsonTypeInfo(WeatherForecastContext.Default.IImmutableListWeatherForecast))
                .UseHttp((context, services) => services
                    // Register HttpClient
#if DEBUG
                    // DelegatingHandler will be automatically injected into Refit Client
                    .AddTransient<DelegatingHandler, DebugHttpHandler>()
#endif
                    .AddSingleton<IWeatherCache, WeatherCache>()
                    .AddRefitClient<IApiClient>(context))
                .ConfigureServices((context, services) => {
                    services.AddSingleton<IGalleryService, GalleryService>();
                })
            );
        MainWindow = builder.Window;
        
#if DEBUG
        MainWindow.EnableHotReload();
#endif
        MainWindow.SetWindowIcon();
        
        Host = builder.Build();
        
        MainWindow.Content = new MainPage(Host);
#if WINDOWS
        // Ensure the current window is active
        MainWindow.ExtendsContentIntoTitleBar = true;
        MainWindow.SystemBackdrop = new MicaBackdrop() {
            Kind = MicaKind.BaseAlt
        };
        Resources.Build(r => {
            r.Add("NavigationViewContentMargin", new Thickness(0, 48, 0, 0));
            r.Add("WindowCaptionBackground", new SolidColorBrush(Colors.Transparent));
            r.Add("WindowCaptionBackgroundDisabled", new SolidColorBrush(Colors.Transparent));
        });
#endif
        // Do not repeat app initialization when the Window already has content,
        
        // Ensure the current window is active
        MainWindow.Activate();
    }
}
