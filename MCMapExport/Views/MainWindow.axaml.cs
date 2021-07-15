using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using MCMapExport.Reader;
using MessageBox.Avalonia;

namespace MCMapExport.Views {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }


        private MapView _map;
        private WorldReader? _reader = null!;

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            _map = this.FindControl<MapView>("Map");
        }

        private void TestButton_OnClick(object? sender, RoutedEventArgs e) { }

        public override void Render(DrawingContext context) {
            _map.Invalidate();
            base.Render(context);
        }

        private async void OpenButton_OnClick(object? sender, RoutedEventArgs e) {
            var filter = new FileDialogFilter();
            filter.Extensions.Add("mca");
            filter.Name = "MCA File";
            var dialog = new OpenFolderDialog {
                Directory = Directory.GetCurrentDirectory(),
                Title = "Please select a Minecraft save directory."
            };
            var result = await dialog.ShowAsync(this);
            if (result is null) {
                return;
            }
            _reader = WorldReader.Open(result, out var error);
            if (_reader is null) {
                var messageBox = MessageBoxManager.GetMessageBoxStandardWindow("Error while opening directory", error);
                await messageBox.ShowDialog(this);
                return;
            }
            _reader.Read();
        }
    }
}