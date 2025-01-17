﻿using AnEoT.Uwp.ViewModels.MainFrame;
using NotificationsVisualizerLibrary;
using System.Diagnostics;
using Windows.Data.Xml.Dom;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace AnEoT.Uwp.Views.MainFrame;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class MainReadPage : Page
{
    private int _latestVolumeTileContentIndex = 0;
    private bool _isFirstShowOfLatestVolumeTile = true;
    private XmlDocument[] _latestVolumeTileContents;

    public MainReadPageViewModel ViewModel { get; } = new();

    public MainReadPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        await SetupTile();
    }

    private async Task SetupTile()
    {
        ViewModel.CreateDefaultTileAsync(FavoriteTile);
        ViewModel.CreateDefaultTileAsync(VolumeListTile);
        ViewModel.CreateDefaultTileAsync(HistoryTile);

        ViewModel.CreateWelcomeTileAsync(WelcomeTile);
        ViewModel.CreateRssTileAsync(RSSTile);

        _latestVolumeTileContents = (await ViewModel.GetLatestVolumeTilesAsync()).ToArray();
        _latestVolumeTileContentIndex = 0;

        if (_latestVolumeTileContents.Length != 0)
        {
            XmlDocument FirstItem = default;
            try
            {
                FirstItem = _latestVolumeTileContents.First();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ex] MainReadPage - SetupTile ex.: " + ex.Message);
            }

            await SetLatestVolumeTile(FirstItem);
        }
    }

    private async void ScrollViewer_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.R)
        {
            await SetupTile();
        }
    }

    private async void OnLatestVolumeTileNewAnimationCompleted(object sender, RoutedEventArgs e)
    {
        _latestVolumeTileContentIndex++;

        if (_latestVolumeTileContents.Length <= _latestVolumeTileContentIndex)
        {
            _latestVolumeTileContentIndex = 0;
        }

        await SetLatestVolumeTile(_latestVolumeTileContents[_latestVolumeTileContentIndex]);
    }

    private async Task SetLatestVolumeTile(XmlDocument doc)
    {
        if (_isFirstShowOfLatestVolumeTile)
        {
            _isFirstShowOfLatestVolumeTile = false;
        }
        else
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        PreviewTileUpdater tileUpdater = LastestVolumeTile.CreateTileUpdater();
        tileUpdater.Update(new Windows.UI.Notifications.TileNotification(doc));
    }
}
