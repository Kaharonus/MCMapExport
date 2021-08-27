using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Kaharonus.Avalonia.DependencyInjection;
using Kaharonus.Avalonia.DependencyInjection.Controls;
using MCMapExport.Reader;
using MCMapExport.Services;
using MessageBox.Avalonia;

namespace MCMapExport.Views {
    public partial class MainWindow : DIWindow {
        public MainWindow() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        [Inject]
        private WorldReaderService _readerService;


        private MapView _map;

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            _map = this.FindControl<MapView>("Map");
        }

        
        private void TestButton_OnClick(object? sender, RoutedEventArgs e) {
            var result = _readerService.Reader!.GetBlockAt(one, two, three);

        }
        

        private async Task HandleClick() {
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
            var success =_readerService.SetLocation(result, out var error);
            if (!success) {
                var messageBox = MessageBoxManager.GetMessageBoxStandardWindow("Error while opening directory", error);
                await messageBox.ShowDialog(this);
                return;
            }
            _map.Invalidate();
            
        }

        private void OpenButton_OnClick(object? sender, RoutedEventArgs e) {
            HandleClick();
        }

        public int one, two, three;
        
        
        
        private void FirstKeyUp(object? sender, KeyEventArgs e) {
            var s = (TextBox) sender!;
            int result;
            switch (s.Name) {
                case "1":
                    if (int.TryParse(s.Text, out  result)) {
                        one = result;
                    }
                    break;
                case "2":
                    if (int.TryParse(s.Text, out  result)) {
                        two = result;
                    }
                    break;
                case "3":
                    if (int.TryParse(s.Text, out  result)) {
                        three = result;
                    }
                    break;
            }
        }
    }
}