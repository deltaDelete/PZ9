using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PZ9;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
        DataContext = this;
        ButtonCommand = new Command<SolidColorBrush>(ColorButtonClick);
        Canvas.DefaultDrawingAttributes.Color = Color.FromRgb(0, 0, 0);
    }

    public List<SolidColorBrush> Colors { get; set; }
        = new[] { "#00ff00", "#ff0000", "#0000ff", "#ffff00", "#ff00ff", "#00ffff", "#ffffff", "#000000", "#333333" }
        .Select(x => new SolidColorBrush((Color)ColorConverter.ConvertFromString(x)))
        .ToList();
    public ICommand ButtonCommand { get; private set; }

    public int CanvasWidth { get => (int)Canvas.ActualWidth; }
    public int CanvasHeight { get => (int)Canvas.ActualHeight; }

    public void ColorButtonClick(SolidColorBrush brush) {
        Canvas.DefaultDrawingAttributes.Color = brush.Color;
        Debug.WriteLine(brush.Color.ToString());
    }

    private void ClearButtonClick(object sender, RoutedEventArgs e) {
        Canvas.Strokes.Clear();
    }

    private void CloseButtonClick(object sender, RoutedEventArgs e) {
        Close();
    }

    private void SaveButtonClick(object sender, RoutedEventArgs e) {
        var dialog = new SaveFileDialog();
        dialog.DefaultExt = ".bmp";
        dialog.Filter = "Изображение (.bmp)|*.bmp";
        dialog.FileName = "Рисунок 1";

        var result = dialog.ShowDialog();

        if (!result.HasValue && !result.Value) return;

        string filename = dialog.FileName;
        if (string.IsNullOrEmpty(filename)) return;

        using var fs = new FileStream(filename, FileMode.Create);

        var rtb = new RenderTargetBitmap(CanvasWidth, CanvasHeight, 96, 96, PixelFormats.Default);
        rtb.Render(Canvas);

        var bmpEncoder = new BmpBitmapEncoder();
        bmpEncoder.Frames.Add(BitmapFrame.Create(rtb));
        bmpEncoder.Save(fs);

        MessageBox.Show($"Файл успешно сохранен:\n{filename}");
    }
}
