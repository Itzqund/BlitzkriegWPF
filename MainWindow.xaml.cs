using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shell;

namespace BlitzkriegWPF
{
    public class LevelTableViewModel
    {
        public string LevelName { get; set; }
        public ObservableCollection<RunItem> Runs { get; set; } = new();
    }

    public class RunItem
    {
        public string Run { get; set; }
        public bool IsChecked { get; set; }
        public string Attempts { get; set; }
        public string Note { get; set; }
    }

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool isGeneratorVisible = true;
        private ObservableCollection<LevelTableViewModel> levelTables = new();
        private CollectionViewSource levelTablesViewSource;

        private double currentZoom = 1.0;
        private const double 
            MinZoom = 0.6,
            MaxZoom = 1.8,
            ZoomStep = 0.1;

        private double _tableWidth = double.NaN;
        public double TableWidth
        {
            get => _tableWidth;
            set { _tableWidth = value; OnPropertyChanged(); }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            levelTables.Add(new LevelTableViewModel { LevelName = "Level 1" });
            levelTables.Add(new LevelTableViewModel { LevelName = "Level 2" });
            levelTables.Add(new LevelTableViewModel { LevelName = "Level 3" });

            levelTables.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (LevelTableViewModel item in e.NewItems) HookTable(item);
                UpdateSavesButtonState();
                levelTablesViewSource.View.Refresh();
            };

            foreach (var table in levelTables) HookTable(table);

            levelTablesViewSource = new CollectionViewSource { Source = levelTables };
            levelTablesViewSource.Filter += LevelTablesViewSource_Filter;
            TablesItemsControl.ItemsSource = levelTablesViewSource.View;

            TableScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            TableWidth = double.NaN;
            UpdateUniformGridPanel();

            UpdateSavesButtonState();

            var windowChrome = new WindowChrome
            {
                CaptionHeight = 32,
                ResizeBorderThickness = new Thickness(6),
                CornerRadius = new CornerRadius(0),
                GlassFrameThickness = new Thickness(0)
            };
            WindowChrome.SetWindowChrome(this, windowChrome);
        }

        private void HookTable(LevelTableViewModel table)
        {
            table.Runs.CollectionChanged += (s, e) =>
            {
                UpdateSavesButtonState();
                levelTablesViewSource.View.Refresh();
            };
        }

        private void UpdateSavesButtonState()
        {
            bool hasSaves = levelTables.Any(t => t.Runs.Count > 0);

            if (SavesToggleMenuItem != null)
            {
                SavesToggleMenuItem.IsEnabled = hasSaves;
                SavesToggleMenuItem.Opacity = hasSaves ? 1.0 : 0.35;

                if (!hasSaves && !isGeneratorVisible) SavesToggle_Click(null, null);
            }
        }

        private void LevelTablesViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is LevelTableViewModel table)
            {
                int index = levelTables.IndexOf(table);

                if (isGeneratorVisible) e.Accepted = index < 2;
                else e.Accepted = table.Runs.Count > 0;
            }
        }

        private void SavesToggle_Click(object sender, RoutedEventArgs e)
        {
            if (SavesToggleMenuItem != null && !SavesToggleMenuItem.IsEnabled) return;

            isGeneratorVisible = !isGeneratorVisible;

            if (isGeneratorVisible)
            {
                LeftColumn.Width = new GridLength(260);
                SplitterColumn.Width = new GridLength(15);
                GeneratorPanel.Visibility = Visibility.Visible;

                TableScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                TableWidth = double.NaN;
                UpdateUniformGridPanel();
            }
            else
            {
                LeftColumn.Width = new GridLength(0);
                SplitterColumn.Width = new GridLength(0);
                GeneratorPanel.Visibility = Visibility.Collapsed;

                TableScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                TableWidth = 450.0;
                UpdateHorizontalStackPanel();
            }

            levelTablesViewSource.View.Refresh();
            UpdateMenuHeaderText();
        }

        private void UpdateUniformGridPanel()
        {
            var panelTemplate = new ItemsPanelTemplate();
            var factory = new FrameworkElementFactory(typeof(System.Windows.Controls.Primitives.UniformGrid));
            factory.SetValue(System.Windows.Controls.Primitives.UniformGrid.ColumnsProperty, 2);
            panelTemplate.VisualTree = factory;
            TablesItemsControl.ItemsPanel = panelTemplate;
        }

        private void UpdateHorizontalStackPanel()
        {
            var panelTemplate = new ItemsPanelTemplate();
            var factory = new FrameworkElementFactory(typeof(StackPanel));
            factory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            panelTemplate.VisualTree = factory;
            TablesItemsControl.ItemsPanel = panelTemplate;
        }

        private void UpdateMenuHeaderText()
        {
            if (isGeneratorVisible) SavesToggleMenuItem.Header = Application.Current.TryFindResource("Str_Saves") ?? "Сохранения";
            else SavesToggleMenuItem.Header = Application.Current.TryFindResource("Str_Generator") ?? "Генератор";
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e) => ZoomIn();
        private void ZoomOut_Click(object sender, RoutedEventArgs e) => ZoomOut();

        private void ZoomIn() => ChangeZoom(ZoomStep);
        private void ZoomOut() => ChangeZoom(-ZoomStep);

        private void ChangeZoom(double delta)
        {
            currentZoom = Math.Clamp(Math.Round(currentZoom + delta, 2), MinZoom, MaxZoom);
            ApplyZoom();
        }

        private void ResetZoom()
        {
            currentZoom = 1.0;
            ApplyZoom();
        }

        private void ApplyZoom()
        {
            AppScaleTransform.ScaleX = currentZoom;
            AppScaleTransform.ScaleY = currentZoom;
            if (ZoomIndicatorText != null) ZoomIndicatorText.Text = $"{Math.Round(currentZoom * 100)}%";
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.OemPlus || e.Key == Key.Add)
                {
                    ZoomIn();
                    e.Handled = true;
                }
                else if (e.Key == Key.OemMinus || e.Key == Key.Subtract)
                {
                    ZoomOut();
                    e.Handled = true;
                }
                else if (e.Key == Key.D0 || e.Key == Key.NumPad0)
                {
                    ResetZoom();
                    e.Handled = true;
                }
            }
        }

        private void MoveTableLeft_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is LevelTableViewModel item)
            {
                int index = levelTables.IndexOf(item);
                if (index > 0) levelTables.Move(index, index - 1);
            }
        }

        private void MoveTableRight_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is LevelTableViewModel item)
            {
                int index = levelTables.IndexOf(item);
                if (index < levelTables.Count - 1) levelTables.Move(index, index + 1);
            }
        }

        private void WindowHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                if (WindowState == WindowState.Maximized)
                {
                    var mouseX = e.GetPosition(this).X;
                    var widthBefore = ActualWidth;

                    WindowState = WindowState.Normal;

                    Left = mouseX - (Width * (mouseX / widthBefore));
                    Top = 0;
                }
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void MaximizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void ThemeBlue_Click(object sender, RoutedEventArgs e) => LoadTheme("Themes/BlueTheme.xaml");
        private void ThemeCharcoal_Click(object sender, RoutedEventArgs e) => LoadTheme("Themes/CharcoalTheme.xaml");
        private void ThemeEmerald_Click(object sender, RoutedEventArgs e) => LoadTheme("Themes/EmeraldTheme.xaml");

        private void LoadTheme(string themePath)
        {
            try
            {
                var dictionaries = Application.Current.Resources.MergedDictionaries;
                var oldTheme = dictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Theme"));

                if (oldTheme != null) dictionaries.Remove(oldTheme);

                var uri = new Uri($"pack://application:,,,/{themePath}", UriKind.Absolute);
                dictionaries.Add(new ResourceDictionary { Source = uri });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить тему: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LangRu_Click(object sender, RoutedEventArgs e) => LoadLanguage("Languages/Strings.ru.xaml");

        private void MakeBlitzkriegButton_Click(object sender, RoutedEventArgs e)
        {
            Generator.GenerateBlitzkrieg(souseStartposesTextBox.Text);
        }

        private void LangEn_Click(object sender, RoutedEventArgs e) => LoadLanguage("Languages/Strings.en.xaml");

        private void LoadLanguage(string langPath)
        {
            try
            {
                var dictionaries = Application.Current.Resources.MergedDictionaries;
                var oldLang = dictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Strings."));

                if (oldLang != null) dictionaries.Remove(oldLang);

                var uri = new Uri($"pack://application:,,,/{langPath}", UriKind.Absolute);
                dictionaries.Insert(0, new ResourceDictionary { Source = uri });
                UpdateMenuHeaderText();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить язык: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}