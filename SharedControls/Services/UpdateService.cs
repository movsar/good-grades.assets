﻿using Serilog;
using Shared;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Velopack;
using Velopack.Sources;

namespace Shared.Services
{
    public static class UpdateService
    {
        public static async Task UpdateMyApp(string repoUrl)
        {
            try
            {
                IUpdateSource girHubSource = new GithubSource(repoUrl, "", true);
                var mgr = new UpdateManager(girHubSource, new UpdateOptions() { ExplicitChannel = "win" });

                // Check for new version
                var newVersion = await mgr.CheckForUpdatesAsync();
                if (newVersion == null)
                {
                    return;
                }

                Log.Information($"Before Checking for updates");
                Log.Debug($"future: {JsonSerializer.Serialize(newVersion)}");
                Log.Debug($"current: {JsonSerializer.Serialize(mgr.CurrentVersion)}");

                if (newVersion.TargetFullRelease.Version.Major == mgr.CurrentVersion!.Major
                    && newVersion.TargetFullRelease.Version.Minor == mgr.CurrentVersion!.Minor)
                {
                    MessageBox.Show(Translations.GetValue("LastVersionInstalled"), "Good Grades");
                    return;
                }

                if (MessageBox.Show(string.Format(Translations.GetValue("AvailableNewVersion"), newVersion), "Good Grades", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    // Download new version
                    await mgr.DownloadUpdatesAsync(newVersion);

                    // Install new version and restart app
                    mgr.ApplyUpdatesAndRestart(newVersion);
                }
            }
            catch (Exception ex)
            {
                ExceptionService.HandleError(ex, "Ошибка при попытаке обновления приложения");
            }
        }
    }
}
