using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            _map = this.FindControl<MapView>("Map");
        }

        private WorldReader reader;
        
        private void TestButton_OnClick(object? sender, RoutedEventArgs e) {
            var result = reader.GetBlockAt(one, two, three);

        }

        public override void Render(DrawingContext context) {
            _map.Invalidate();
            base.Render(context);
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
            reader = WorldReader.Open(result, out var error);
            if (reader is null) {
                var messageBox = MessageBoxManager.GetMessageBoxStandardWindow("Error while opening directory", error);
                await messageBox.ShowDialog(this);
                return;
            }

            // reader.GetBlockAt(6, 62, 0);
            _map.SetWorldReader(reader); 
            //var block = reader.GetChunkAt(0,0, 0);
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