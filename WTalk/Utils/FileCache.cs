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
        HttpClient _httpClient;
        string _folder;

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

        public string Root { get { return _folder; } }

        internal FileCache(string rootFolder, string cacheToken)
        {
            _cacheToken = cacheToken;
            _folder = Path.Combine(rootFolder, cacheToken);
            var dir = System.IO.Directory.CreateDirectory(_folder);
            
            _httpClient =  new HttpClient(new HttpClientHandler() { AllowAutoRedirect = true, CookieContainer = Client.CookieContainer, UseCookies = true });
            _cachedFiles = dir.GetFiles().ToDictionary(c => c.Name, c => c.FullName);
        }

        public static void Initialize(string rootFolder, string cacheToken)
        {
            _fileCache = new FileCache(rootFolder,cacheToken);
           
        }

        public string GetOrUpdate(string fileKey, string url)
        {
            if (url.Length < 7)
                return null;
            
            if(!_cachedFiles.ContainsKey(fileKey) )            
            {

               byte[] data = _httpClient.GetByteArrayAsync(new Uri(url)).Result;
                string path = Path.Combine(_folder, fileKey);
                File.WriteAllBytes(path, data);
               _cachedFiles.Add(fileKey, path);
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

        public void Add(string filekey, string content)
        {
            
        }

        public void Reset()
        {
            Directory.Delete(_folder);
        }
    }
}
