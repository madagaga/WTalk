using PCLStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WTalk.Core.Utils
{
    public class FileCache
    {

        string _cacheToken;
        Dictionary<string, string> _cachedFiles;
        IFolder _folder;
        HttpClient _httpClient;


        static FileCache _fileCache;        
        public static FileCache Current
        {
            get
            {
                if (_fileCache == null)
                    throw new Exception("Image Cache is not initialized");
                return _fileCache;
            }
        }

        internal FileCache(string cacheToken)
        {
            _cacheToken = cacheToken;
            _folder = FileSystem.Current.LocalStorage.CreateFolderAsync(_cacheToken, CreationCollisionOption.OpenIfExists).Result;
            _httpClient =  new HttpClient(new HttpClientHandler() { AllowAutoRedirect = true, CookieContainer = Client.CookieContainer, UseCookies = true });
            _cachedFiles = new Dictionary<string, string>();

            IList<IFile> files = _folder.GetFilesAsync().Result;

            foreach(IFile file in files)
            {
                _cachedFiles.Add(file.Name, file.Path);
            }
        }

        public static void Initialize(string cacheToken)
        {
            _fileCache = new FileCache(cacheToken);
           
        }

        public string GetOrUpdate(string fileKey, string file)
        {
            if (file.Length < 7)
                return null;
            
            if(!_cachedFiles.ContainsKey(fileKey) )            
            {

               byte[] data = _httpClient.GetByteArrayAsync(new Uri(file)).Result;
               IFile downloadedFile = _folder.CreateFileAsync(fileKey, CreationCollisionOption.OpenIfExists).Result;
               using(Stream stream = downloadedFile.OpenAsync(FileAccess.ReadAndWrite).Result)
               {
                   stream.Write(data, 0, data.Length);
               }

               _cachedFiles.Add(downloadedFile.Name, downloadedFile.Path);
            }
            return _cachedFiles[fileKey];
        }

        public string Get(string fileKey)
        {
            if (_cachedFiles.ContainsKey(fileKey))
                return _cachedFiles[fileKey];
            else
                return null;
        }

        public void Reset()
        {
            _folder.DeleteAsync().Wait();
        }
    }
}
