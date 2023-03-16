using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        ButtonCommand = new Command<SolidColorBrush>(ColorButtonClick);
        Canvas.DefaultDrawingAttributes.Color = Color.FromRgb(0, 0, 0);
    }

    public ObservableCollection<SolidColorBrush> Colors { get; set; }
        = new(new[] { "#00ff00", "#ff0000", "#0000ff", "#ffff00", "#ff00ff", "#00ffff", "#ffffff", "#000000", "#333333" }
        .Select(x => new SolidColorBrush((Color)ColorConverter.ConvertFromString(x))));
    public SolidColorBrush SelectedColor { get; set; }
    public ICommand ButtonCommand { get; private set; }

    public int CanvasWidth { get => (int)Canvas.ActualWidth; }
    public int CanvasHeight { get => (int)Canvas.ActualHeight; }

    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }

    public void ColorButtonClick(SolidColorBrush brush)
    {
        Canvas.DefaultDrawingAttributes.Color = brush.Color;
        SelectedColor = brush;
        Beb.Background = brush;
        Debug.WriteLine(brush.Color.ToString());
    }

    private void ClearButtonClick(object sender, RoutedEventArgs e)
    {
        Canvas.Strokes.Clear();
        Canvas.Children.Clear();
    }

    private void CloseButtonClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void SaveButtonClick(object sender, RoutedEventArgs e)
    {
        Canvas.EditingMode = InkCanvasEditingMode.None;
        Save();
        Canvas.EditingMode = InkCanvasEditingMode.Ink;
    }

    private void Save()
    {
        var dialog = new SaveFileDialog();
        dialog.DefaultExt = ".bmp";
        dialog.Filter = "Изображение (.bmp)|*.bmp";
        dialog.FileName = "Рисунок 1";

        var result = dialog.ShowDialog();

        if (!result.HasValue) return;
        if (!result.Value) return;

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

    private void AddColorButtonClick(object sender, RoutedEventArgs e)
    {
        var color = Color.FromRgb(Red, Green, Blue);

        Canvas.DefaultDrawingAttributes.Color = color;
        var brush = new SolidColorBrush(color);
        if (Colors.Any(x => x.Color == color)) return;
        SelectedColor = brush;
        Beb.Background = brush;

        Colors.Add(brush);

    }

    private void SelectionButtonClick(object sender, RoutedEventArgs e)
    {
        Canvas.EditingMode = Canvas.EditingMode switch
        {
            InkCanvasEditingMode.Ink => InkCanvasEditingMode.Select,
            _ => InkCanvasEditingMode.Ink,
        };
    }
    private void AddTextButtonClick(object sender, RoutedEventArgs e)
    {
        var text = new TextBox
        {
            Width = 100,
            Height = 50,
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
            Margin = new Thickness(20, 20, 0, 0),
        };

        Canvas.Children.Add(text);

        text.Focus();
    }
}
