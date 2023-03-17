using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        CloseCommand = new Command(Close);
        SaveCommand = new Command(PreSave);
        EditModeCommand = new Command(ModeChange);
        AddTextCommand = new Command(AddText);
        ClearCommand = new Command(Clear);
        AddColorCommand = new Command(AddColor);
        Canvas.DefaultDrawingAttributes.Color = Color.FromRgb(0, 0, 0);
        selectedColor = Colors.Where(x => x.Color == Canvas.DefaultDrawingAttributes.Color).First();
        brushWidth = Canvas.DefaultDrawingAttributes.Width;
        brushHeight = Canvas.DefaultDrawingAttributes.Height;
    }

    private double brushWidth;
    private double brushHeight;
    private SolidColorBrush selectedColor;


    public ObservableCollection<SolidColorBrush> Colors { get; set; }
        = new(new[] { "#00ff00", "#ff0000", "#0000ff", "#ffff00", "#ff00ff", "#00ffff", "#ffffff", "#000000", "#333333" }
        .Select(x => new SolidColorBrush((Color)ColorConverter.ConvertFromString(x))));
    public List<double> TextSizes { get; set; } = new()
    {
        9,
        10,
        11,
        12,
        14,
        16,
        18,
        20,
        24,
        28,
        30,
        32,
        48,
        72
    };
    public List<FontFamily> FontFamilies { get; set; } = Fonts.SystemFontFamilies.ToList();
    public SolidColorBrush SelectedColor { 
        get => selectedColor;
        set => RaiseAndSetIfChanged(ref selectedColor, value);
    }
    public ICommand ButtonCommand { get; private set; }
    public ICommand CloseCommand { get; private set; }
    public ICommand SaveCommand { get; private set; }
    public ICommand EditModeCommand { get; private set; }
    public ICommand AddTextCommand { get; private set; }
    public ICommand ClearCommand { get; private set; }
    public ICommand AddColorCommand { get; private set; }

    public int CanvasWidth { get => (int)Canvas.ActualWidth; }
    public int CanvasHeight { get => (int)Canvas.ActualHeight; }

    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }
    public bool IsDefaultChecked { get; set; } = true;
    public bool IsSmallChecked { get; set; }
    public bool IsMediumChecked { get; set; }
    public bool IsLargeChecked { get; set; }

    public void ColorButtonClick(SolidColorBrush brush)
    {
        Canvas.DefaultDrawingAttributes.Color = brush.Color;
        SelectedColor = brush;
        //Beb.Background = brush;
        Debug.WriteLine(brush.Color.ToString());
    }

    private void Clear()
    {
        Canvas.Strokes.Clear();
        Canvas.Children.Clear();
    }

    private void PreSave()
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

    private void AddColor()
    {
        var color = Color.FromRgb(Red, Green, Blue);

        Canvas.DefaultDrawingAttributes.Color = color;
        var brush = new SolidColorBrush(color);
        if (Colors.Any(x => x.Color == color)) return;
        SelectedColor = brush;
        //Beb.Background = brush;

        Colors.Add(brush);
    }

    private void ModeChange()
    {
        InkCanvasEditingMode inkCanvasEditingMode = Canvas.EditingMode switch
        {
            InkCanvasEditingMode.Ink => InkCanvasEditingMode.Select,
            _ => InkCanvasEditingMode.Ink,
        };
        Canvas.EditingMode = inkCanvasEditingMode;
    }

    private void AddText()
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

    private void RadioButtonChecked(object sender, RoutedEventArgs e)
    {
        var size = (brushWidth, brushHeight);

        if (IsDefaultChecked)
        {
            size = (brushWidth, brushHeight);
        }
        else if (IsSmallChecked)
        {
            size = (1, 1);
        }
        else if (IsMediumChecked)
        {
            size = (10, 10);
        }
        else if (IsLargeChecked)
        {
            size = (50, 50);
        }
        Canvas.DefaultDrawingAttributes.Width = size.brushWidth;
        Canvas.DefaultDrawingAttributes.Height = size.brushHeight;
    }

    private void FontSizeChanged(object sender, SelectionChangedEventArgs e) => FontSizeChange();

    private void FontSizeChange()
    {
        var texts = Canvas.GetSelectedElements().Where(x => x is TextBox);
        if (texts.Count() == 0) return;
        foreach (TextBox text in texts)
        {
            text.FontSize = (double)FontSizeBox.SelectedItem;
        }
    }

    private void FontFamilyChanged(object sender, SelectionChangedEventArgs e) => FontFamilyChange();

    private void FontFamilyChange()
    {
        var texts = Canvas.GetSelectedElements().Where(x => x is TextBox);
        if (texts.Count() == 0) return;
        foreach (TextBox text in texts)
        {
            text.FontFamily = (FontFamily)FontFamilyBox.SelectedItem;
        }
    }
}
