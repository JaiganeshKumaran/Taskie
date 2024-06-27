﻿using System;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using TaskieLib;
using Windows.Networking;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using System.Diagnostics;
using Windows.UI.Xaml.Shapes;
using System.Reflection;
using System.Linq;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml.Hosting;
using System.Collections.Generic;
using Windows.Media.Protection.PlayReady;

namespace Taskie
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Windows.Foundation.Size(600, 500));
            InitializeComponent();
            SetupTitleBar();
            SetupNavigationMenu();
            Navigation.Height = rectlist.ActualHeight;
            TaskieLib.Tools.ListCreatedEvent += UpdateLists;
            Tools.ListDeletedEvent += ListDeleted;
            Tools.ListRenamedEvent += ListRenamed;
        }

        private void ListRenamed(string oldname, string newname)
        {
            foreach (var item in Navigation.Items)
            {
                if (item is ListViewItem navigationItem)
                {
                    if (navigationItem.Tag.ToString() == oldname)
                    {
                        navigationItem.Tag = newname;
                        StackPanel content = new StackPanel();
                        content.Orientation = Orientation.Horizontal;
                        content.VerticalAlignment = VerticalAlignment.Center;
                        content.Children.Add(new FontIcon() { Glyph = "📄", FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe UI Emoji"), FontSize = 14 });
                        content.Children.Add(new TextBlock() { Text = newname, Margin = new Thickness(12, 0, 0, 0), TextTrimming = TextTrimming.CharacterEllipsis });
                        navigationItem.Content = content;
                        break;
                    }
                }
            }
        }

        private void ListDeleted(string name)
        {
            contentFrame.Content = new StackPanel();
            Navigation.SelectedItem = null;
            foreach (var item in Navigation.Items)
            {
                if (item is ListViewItem navigationItem)
                {
                    if (navigationItem.Tag.ToString() == name)
                    {
                        Navigation.Items.Remove(item);
                        break;
                    }
                }
            }
        }

        private void SetupTitleBar()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonHoverBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            Window.Current.SetTitleBar(TTB);
        }

        private void SetupNavigationMenu()
        {
            foreach (string listName in TaskieLib.Tools.GetLists())
            {
                StackPanel content = new StackPanel();
                content.Orientation = Orientation.Horizontal;
                content.VerticalAlignment = VerticalAlignment.Center;
                content.Children.Add(new FontIcon() { Glyph = "📄", FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe UI Emoji"), FontSize = 14 });
                content.Children.Add(new TextBlock { Text = listName, Margin = new Thickness(12, 0, 0, 0), TextTrimming = TextTrimming.CharacterEllipsis });
                Navigation.Items.Add(new ListViewItem() { Tag = listName, Content = content, HorizontalContentAlignment = HorizontalAlignment.Left });
                AddRightClickMenu();
            }
        }

        private void AddRightClickMenu()
        {
            foreach (ListViewItem item in Navigation.Items) {
                item.RightTapped += OpenRightClickList;
            }
        }

        private void OpenRightClickList(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            MenuFlyout flyout = new MenuFlyout();
            flyout.Items.Add(new MenuFlyoutItem() { Icon = new SymbolIcon(Symbol.Rename), Text = "Rename list", Tag = (sender as ListViewItem).Tag });
            flyout.Items.Add(new MenuFlyoutItem() { Icon = new SymbolIcon(Symbol.Delete), Text = "Delete list", Tag = (sender as ListViewItem).Tag });
            (flyout.Items[0] as MenuFlyoutItem).Click += RenameList_Click;
            (flyout.Items[1] as MenuFlyoutItem).Click += DeleteList_Click;
            flyout.ShowAt(sender as ListViewItem);
        }

        private async void RenameList_Click(object sender, RoutedEventArgs e)
        {
            string listname = (sender as  MenuFlyoutItem).Tag as string;
            TextBox input = new TextBox() { PlaceholderText = "List name", Text = listname };
            ContentDialog dialog = new ContentDialog() { Title = "Rename list", PrimaryButtonText = "OK", SecondaryButtonText = "Cancel", Content = input };
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                string text = input.Text;
                Tools.RenameList(listname, text);
                listname = text;
            }
        }

        private void DeleteList_Click(object sender, RoutedEventArgs e)
        {
            string listname = (sender as MenuFlyoutItem).Tag as string;
            Tools.DeleteList(listname);
        }

        private void UpdateLists(string name)
        {
            Navigation.Items.Clear();
            SetupNavigationMenu();
            contentFrame.Content = null;
        }

        private void AddList(object sender, RoutedEventArgs e)
        {
            string listName = Tools.CreateList("New list");
            UpdateLists("New list");
            foreach (ListViewItem item in Navigation.Items)
            {
                if (item.Tag.ToString().Contains(listName)) { Navigation.SelectedItem = item; break; }
            }
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            AppWindow window = await AppWindow.TryCreateAsync();
            window.Title = "Settings";
            Frame settingsContent = new Frame();
            settingsContent.Navigate(typeof(SettingsPage));
            window.TitleBar.ExtendsContentIntoTitleBar = true;
            window.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            ElementCompositionPreview.SetAppWindowContent(window, settingsContent);
            window.Closed += SettingsWindowClosed;
            (sender as Button).IsEnabled = false;
            await window.TryShowAsync();
        }

        private void SettingsWindowClosed(AppWindow sender, AppWindowClosedEventArgs args)
        {
            SettingsButton.IsEnabled = true;
        }

        private void Navigation_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            ListView NavList = sender as ListView;
            var selectedItem = NavList.SelectedItem as ListViewItem;
            if (selectedItem != null && selectedItem.Tag is string tag)
            {
                contentFrame.Navigate(typeof(TaskPage), tag);
            }
        }

        private void UpgradeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rectlist_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Navigation.Height = rectlist.ActualHeight;
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            sender.ItemsSource = Array.FindAll<string>(Tools.GetLists(), s => s.Contains(sender.Text));
            if (sender.Text == null) { sender.IsSuggestionListOpen = false; }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            contentFrame.Navigate(typeof(TaskPage), Array.FindAll<string>(Tools.GetLists(), s => s.Contains(sender.Text))[0]);
            foreach (ListViewItem item in Navigation.Items)
            {
                Debug.WriteLine(item.Tag.ToString());
                Debug.WriteLine(sender.Text);
                if (item.Tag.ToString().Contains(sender.Text)) { Navigation.SelectedItem = item; break; }
            }
            sender.Text = "";
            searchbox.ItemsSource = null;

        }
    }
}
