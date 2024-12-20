using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;

public class FTPHelper
{
    private string _ftpServer;
    private string _ftpUser;
    private string _ftpPassword;

    public FTPHelper(string ftpServer, string ftpUser, string ftpPassword)
    {
        _ftpServer = ftpServer;
        _ftpUser = ftpUser;
        _ftpPassword = ftpPassword;
    }

    public List<string> GetFileList(string directoryPath)
    {
        var fileList = new List<string>();
        try
        {
            var request = (FtpWebRequest)WebRequest.Create(new Uri($"{_ftpServer}/{directoryPath}"));
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(_ftpUser, _ftpPassword);

            using (var response = (FtpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    fileList.Add(line);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
        }
        return fileList;
    }

    public BitmapImage DownloadImage(string filePath)
    {
        try
        {
            var request = (FtpWebRequest)WebRequest.Create(new Uri($"{_ftpServer}/{filePath}"));
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(_ftpUser, _ftpPassword);

            using (var response = (FtpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                return bitmap;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading image: {ex.Message}");
            return null;
        }
    }
}
