﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MediaInfoNET
{
    public partial class MainWindow : Window
    {
        string SettingsFolder = "";
        string SettingsFile = "";
        string SourcePath = "";
        String ActiveGroup = "";
        List<Item> Items = new List<Item>();

        public MainWindow()
        {
            InitializeComponent();

            ContentTextBox.SelectionChanged += ContentTextBox_SelectionChanged;
         
            SettingsFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" +
                FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()?.Location).ProductName + @"\";
            
            SettingsFile = SettingsFolder + "settings.conf";

            if (!Directory.Exists(SettingsFolder))
                Directory.CreateDirectory(SettingsFolder);

            if (!File.Exists(SettingsFile))
            {
                string content = @"
font = Consolas
font-size = 13
window-width = 700
window-height = 550
center-screen = yes
raw-view = no
word-wrap = no
";
                File.WriteAllText(SettingsFile, content);
            }

            ReadSettings();

            if (Environment.GetCommandLineArgs().Length > 1)
                LoadFile(Environment.GetCommandLineArgs()[1]);
        }

        private void ReadSettings()
        {
            foreach (string line in File.ReadAllLines(SettingsFile))
            {
                if (!line.Contains("="))
                    continue;

                string left = line.Substring(0, line.IndexOf("=")).Trim();
                string right = line.Substring(line.IndexOf("=") + 1).Trim();

                try
                {
                    switch (left)
                    {
                        case "font":
                            FontFamily = new FontFamily(right); break;
                        case "font-size":
                            FontSize = int.Parse(right); break;
                        case "window-width":
                            Width = int.Parse(right); break;
                        case "window-height":
                            Height = int.Parse(right); break;
                        case "raw-view":
                            MediaInfo.RawView = right == "yes"; break;
                        case "word-wrap":
                            ContentTextBox.TextWrapping = right == "yes" ? TextWrapping.Wrap : TextWrapping.NoWrap; break;
                        case "center-screen":
                            WindowStartupLocation = right == "yes" ? WindowStartupLocation.CenterScreen : WindowStartupLocation.Manual; break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to read setting " + left + "." + "\n\n" + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        void LoadFile(string file)
        {
            if (!File.Exists(file))
                return;

            PreviousMenuItem.IsEnabled = Directory.GetFiles(Path.GetDirectoryName(file)).Length > 1;
            NextMenuItem.IsEnabled = PreviousMenuItem.IsEnabled;
            SourcePath = file;
            Title = file + " - " + FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()?.Location).ProductName;
            List<TabItem> tabItems = new List<TabItem>();
            tabItems.Clear();
            HashSet<string> captionNames = new HashSet<string>();
            captionNames.Add("Basic");
            captionNames.Add("Advanced");
            Items = GetItems();

            foreach (Item item in Items)
                captionNames.Add(item.Group);

            foreach (string name in captionNames)
                tabItems.Add(new TabItem { Name = name, Value = name });

            foreach (TabItem tabItem in tabItems)
                foreach (Item item in Items)
                    if (item.Group == tabItem.Name && item.Name == "Format")
                        tabItem.Name += " (" + item.Value + ")";

            TabListBox.ItemsSource = tabItems;

            if (tabItems.Count > 0)
                TabListBox.SelectedIndex = 0;
        }

        public class TabItem
        {
            public string Name { get; set; } = "";
            public string Value { get; set; } = "";
        }

        List<Item> GetItems()
        {
            List<Item> items = new List<Item>();
            using MediaInfo mediaInfo = new MediaInfo(SourcePath);
            string summary = mediaInfo.GetSummary(true);
            string group = "";

            foreach (string line in summary.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Contains(":"))
                {
                    Item item = new Item();
                    item.Name = line.Substring(0, line.IndexOf(":")).Trim();
                    item.Value = line.Substring(line.IndexOf(":") + 1).Trim();
                    item.Group = group;
                    item.IsComplete = true;
                    items.Add(item);
                }
                else
                    group = line.Trim();
            }

            summary = mediaInfo.GetSummary(false);

            foreach (string line in summary.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Contains(":"))
                {
                    Item item = new Item();
                    item.Name = line.Substring(0, line.IndexOf(":")).Trim();
                    item.Value = line.Substring(line.IndexOf(":") + 1).Trim();
                    item.Group = group;
                    items.Add(item);
                }
                else
                    group = line.Trim();
            }

            return items;
        }

        void UpdateItems()
        {
            StringBuilder newText = new StringBuilder();
            IEnumerable<Item> items;

            if (ActiveGroup == "Advanced")
                items = Items.Where(i => i.IsComplete);
            else if (ActiveGroup == "Basic")
                items = Items.Where(i => !i.IsComplete);
            else
            {
                var newItems = new List<Item>();
                newItems.AddRange(Items.Where(i => !i.IsComplete && i.Group == ActiveGroup));
                newItems.Add(new Item { Name = "", Value = "", Group = ActiveGroup });
                newItems.AddRange(Items.Where(i => i.IsComplete && i.Group == ActiveGroup));
                items = newItems;
            }

            string search = SearchTextBox.Text.ToLower();
            
            if (search != "")
                items = items.Where(i => i.Name.ToLower().Contains(search) || i.Value.ToLower().Contains(search));

            List<string> groups = new List<string>();

            foreach (Item item in items)
                if (item.Group != "" && !groups.Contains(item.Group))
                    groups.Add(item.Group);

            foreach (string group in groups)
            {
                if (newText.Length == 0)
                    newText.Append(group + "\r\n\r\n");
                else
                    newText.Append("\r\n" + group + "\r\n\r\n");

                var itemsInGroup = items.Where(i => i.Group == group);

                foreach (Item item in itemsInGroup)
                {
                    if (item.Name != "")
                    {
                        newText.Append(item.Name.PadRight(25));
                        newText.Append(": ");
                    }

                    newText.Append(item.Value);
                    newText.Append("\r\n");
                }
            }

            ContentTextBox.Text = newText.ToString();
        }

        void Previous()
        {
            if (!File.Exists(SourcePath))
                return;

            string[] files = Directory.GetFiles(Path.GetDirectoryName(SourcePath));

            if (files.Length < 2)
                return;

            int index = Array.IndexOf(files, SourcePath);

            if (--index < 0)
                index = files.Length - 1;

            LoadFile(files[index]);
        }

        void Next()
        {
            if (!File.Exists(SourcePath))
                return;

            string[] files = Directory.GetFiles(Path.GetDirectoryName(SourcePath));

            if (files.Length < 2)
                return;

            int index = Array.IndexOf(files, SourcePath);

            if (++index > files.Length - 1)
                index = 0;

            LoadFile(files[index]);
        }

        void ShowSettings()
        {
            SettingsWindow window = new SettingsWindow();
            window.ShowInTaskbar = false;
            window.Owner = this;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.FontFamily = FontFamily;
            window.FontSize = FontSize;
            window.TextBox.Background = ContentTextBox.Background;
            window.TextBox.Foreground = ContentTextBox.Foreground;
            window.TextBox.Text = File.ReadAllText(SettingsFile);
            window.ShowDialog();
            File.WriteAllText(SettingsFile, window.TextBox.Text);
            ReadSettings();
            LoadFile(SourcePath);
        }

        private void TabListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabListBox.SelectedItem != null)
                ActiveGroup = (TabListBox.SelectedItem as TabItem)?.Value ?? "";

            UpdateItems();
            ContentTextBox.ScrollToHome();
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ContentTextBox.SelectedText);
        }

        private void ContentTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            CopyMenuItem.IsEnabled = ContentTextBox.SelectedText.Length > 0;
        }

        private void PreviousMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Previous();
        }

        private void NextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (SearchTextBox.Text == "")
                        Close();
                    else
                        SearchTextBox.Text = "";
                    break;
                case Key.F11:
                    Previous(); break;
                case Key.F12:
                    Next(); break;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = SearchTextBox.Text;
            HintTextBlock.Text = text == "" ? "Search" : "";
            ClearButton.Visibility = text == "" ? Visibility.Hidden : Visibility.Visible;

            if (TabListBox.Items.Count > 1)
            {
                TabListBox.SelectedIndex = 1;
                UpdateItems();
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            Keyboard.Focus(SearchTextBox);
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            HandleDrop(e);
        }

        private void ContentTextBox_Drop(object sender, DragEventArgs e)
        {
            HandleDrop(e);
        }

        void HandleDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                LoadFile(files[0]);
            }
        }

        private void ContentTextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }

        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }

        private void SetupMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Click yes to install and no to uninstall.", "Setup", MessageBoxButton.YesNoCancel);

            string args = result switch {
                MessageBoxResult.Yes => "--install",
                MessageBoxResult.No => "--uninstall",
                _ => ""
            };

            if (args != "")
            {
                try
                {
                    using Process proc = new Process();
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
                    proc.StartInfo.Arguments = args;
                    proc.StartInfo.Verb = "runas";
                    proc.Start();
                } catch {}
            }
        }
    }
}