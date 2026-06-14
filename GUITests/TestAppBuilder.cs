using Avalonia;
using Avalonia.Headless;
using EDSEditorGUI2;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
            .UseSkia() // enable Skia renderer
        .UseHeadless(new AvaloniaHeadlessPlatformOptions() { UseHeadlessDrawing = false });
}