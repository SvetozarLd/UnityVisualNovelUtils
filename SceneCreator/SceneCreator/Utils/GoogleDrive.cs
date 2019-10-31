using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
namespace SceneCreator.Utils
{
    public static class GoogleDrive
    {
        public static DriveService GService { get; set; }
        public static Exception GException { get; set; }

        private static bool Initialize(string accountEmail, string ServiceAccountPKC12Path, string AppName)
        {
            GService = BuildService(accountEmail, ServiceAccountPKC12Path, AppName);
            if (GService != null) { return true; } else { return false; }
        }

        private static DriveService BuildService(string accountEmail, string ServiceAccountPKC12Path, string AppName)
        {
            try
            {
                GException = null;
                return new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(accountEmail)
                    {
                        Scopes = new[] { DriveService.Scope.Drive }
                    }.FromCertificate(new X509Certificate2(ServiceAccountPKC12Path, "notasecret", X509KeyStorageFlags.Exportable))),
                    ApplicationName = AppName
                });
            }
            catch (Exception ex) { GException = ex; return null; }
        }

        public static bool DeleteFile(DriveService service, string fileId)
        {
            try { service.Files.Delete(fileId).Execute(); return true; }
            catch (Exception ex) { GException = ex; return false; }
        }

        public static Google.Apis.Drive.v2.Data.File createDirectory(DriveService _service, string _title, string _description, string _parent)
        {
            Google.Apis.Drive.v2.Data.File NewDirectory = null;
            Google.Apis.Drive.v2.Data.File body = new Google.Apis.Drive.v2.Data.File
            {
                Title = _title,
                Description = _description,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<ParentReference>() { new ParentReference() { Id = _parent } }
            };

            try
            {
                GException = null;
                FilesResource.InsertRequest request = _service.Files.Insert(body);
                NewDirectory = request.Execute();
            }
            catch (Exception ex) { GException = ex; }

            return NewDirectory;
        }

        public static Google.Apis.Drive.v2.Data.File UploadFile(DriveService _service, string _uploadFile, string _parent)
        {
            GException = null;
            if (System.IO.File.Exists(_uploadFile))
            {
                Google.Apis.Drive.v2.Data.File body = new Google.Apis.Drive.v2.Data.File
                {
                    Title = System.IO.Path.GetFileName(_uploadFile),
                    Description = "File uploaded by " + Application.ProductName,
                    MimeType = GetMimeType(_uploadFile),
                    Parents = new List<ParentReference>() { new ParentReference() { Id = _parent } }
                };
                byte[] byteArray = System.IO.File.ReadAllBytes(_uploadFile);
                System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                try
                {
                    FilesResource.InsertMediaUpload request = _service.Files.Insert(body, stream, GetMimeType(_uploadFile));
                    request.Upload();
                    return request.ResponseBody;
                }
                catch (Exception ex) { GException = ex; return null; }
            }
            else { GException = new Exception("File does not exist: " + _uploadFile); return null; }

        }

        public static List<Google.Apis.Drive.v2.Data.File> GetAllFiles(DriveService service)
        {
            List<Google.Apis.Drive.v2.Data.File> result = new List<Google.Apis.Drive.v2.Data.File>();
            FilesResource.ListRequest request = service.Files.List();
            GException = null;
            do
            {
                try
                {
                    FileList files = request.Execute();
                    result.AddRange(files.Items);
                    request.PageToken = files.NextPageToken;
                }
                catch (Exception ex) { GException = ex; request.PageToken = null; }
            } while (!string.IsNullOrEmpty(request.PageToken));
            return result;
        }




        #region GET Mime
        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
            {
                mimeType = regKey.GetValue("Content Type").ToString();
            }

            return mimeType;
        }
        #endregion
    }
}
