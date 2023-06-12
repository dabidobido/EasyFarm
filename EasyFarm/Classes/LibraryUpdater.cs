// ///////////////////////////////////////////////////////////////////
// This file is a part of EasyFarm for Final Fantasy XI
// Copyright (C) 2013 Mykezero
//  
// EasyFarm is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// EasyFarm is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// If not, see <http://www.gnu.org/licenses/>.
// ///////////////////////////////////////////////////////////////////
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MahApps.Metro.Controls.Dialogs;
using NLog;

namespace EasyFarm.Classes
{
    public class LibraryUpdater
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const string LibraryPath = "http://ext.elitemmonetwork.com/downloads/eliteapi/EliteAPI.dll";
        private const string LibraryPage = "http://ext.elitemmonetwork.com/downloads/eliteapi/";

        private static string m_filePath = Path.Combine(Environment.CurrentDirectory, "EliteAPI.dll");
        private static FileVersionInfo m_fileInfo = GetFileInfo(m_filePath);
        private bool m_gotNewVersion = false;

        public async void CheckNewVersion()
        {
            var webclient = new HttpClient();
            webclient.Timeout = TimeSpan.FromSeconds(5);
            try
            {
                var request = await webclient.GetStringAsync(new Uri(LibraryPage));
                if (!string.IsNullOrEmpty(request))
                {
                    var document = new HtmlDocument();
                    document.LoadHtml(request);
                    var download = document.DocumentNode
                        .SelectNodes("//a[@id=\"download\"]")
                        .FirstOrDefault();
                    if (download != null)
                    {
                        var versionMatches = Regex.Match(download.InnerText, "v([\\d\\.]+)");
                        if (versionMatches.Success)
                        {
                            m_gotNewVersion = m_fileInfo == null || versionMatches.Groups[1].Value != m_fileInfo.FileVersion;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ViewModels.LogViewModel.Write("Failed to get new version: " + e.Message);
            }   
        }
        public bool HasUpdate()
        {
            return m_gotNewVersion;
        }

        public void Update()
        {
            try
            {
                if (HasUpdate())
                {
                    BackupLibrary(m_fileInfo);
                    DownloadLibrary(m_filePath);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update EliteAPI.dll");
            }
        }

        private static FileVersionInfo GetFileInfo(string filePath)
        {
            FileVersionInfo fileInfo = null;

            if (File.Exists(filePath))
            {
                fileInfo = FileVersionInfo.GetVersionInfo(filePath);
            }

            return fileInfo;
        }

        private void BackupLibrary(FileVersionInfo fileVersionInfo)
        {
            if (fileVersionInfo == null) return;

            var backupPath = Path.Combine(
                Environment.CurrentDirectory,
                "backups",
                fileVersionInfo.FileVersion);

            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }

            var backupFilePath = Path.Combine(backupPath, fileVersionInfo.OriginalFilename);
            if (File.Exists(fileVersionInfo.FileName) && !File.Exists(backupFilePath))
            {
                File.Move(fileVersionInfo.FileName, backupFilePath);
            }
        }

        private static void DownloadLibrary(string filePath)
        {
            var client = new WebClient();
            client.DownloadFile(new Uri(LibraryPath), filePath);
        }
    }
}
