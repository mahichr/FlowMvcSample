using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;

namespace NgFlowSample.Services
{
    public class FlowUploadProcessorNew
    {
        private readonly InMemoryMultipartFormDataStreamProvider streamProvider;

        #region Constructor

        public FlowUploadProcessorNew()
        {
            streamProvider = new InMemoryMultipartFormDataStreamProvider();
        }

        #endregion

        public bool IsComplete { get; private set; }

        public async Task<bool> ProcessUploadChunkRequest(HttpRequestMessage request)
        {
            await request.Content.ReadAsMultipartAsync(streamProvider);
            IsComplete = RegisterSuccessfulChunk(streamProvider.FormData.ToObject<FlowMetaDataNew>());
            return IsComplete;
        }

        public async Task WriteFile()
        {
            var fm = streamProvider.FormData.ToObject<FlowMetaDataNew>();
            var stream = await streamProvider.Files[0].ReadAsStreamAsync();
            using (var fileStream = File.Create("C:\\Test\\" + fm.FlowFilename))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }
        }


        /// <summary>
        /// Ensures the thread safety of our static methods.
        /// </summary>
        private static Object chunkCacheLock = new Object();

        /// <summary>
        /// Track our in progress uploads, by using a cache, we make sure we don't accumulate memory
        /// </summary>
        private static MemoryCache uploadChunkCache = MemoryCache.Default;

        private static FileMetaDataNew GetFileMetaData(string flowIdentifier)
        {
            lock (chunkCacheLock)
            {
                return uploadChunkCache[flowIdentifier] as FileMetaDataNew;
            }
        }

        /// <summary>
        /// (Thread Safe) Marks a chunk as recieved.
        /// </summary>
        private static bool RegisterSuccessfulChunk(FlowMetaDataNew chunkMeta)
        {
            FileMetaDataNew fileMeta;
            lock (chunkCacheLock)
            {
                fileMeta = GetFileMetaData(chunkMeta.FlowIdentifier);
                if (fileMeta == null)
                {
                    fileMeta = new FileMetaDataNew(chunkMeta);
                    uploadChunkCache.Add(chunkMeta.FlowIdentifier, fileMeta, DefaultCacheItemPolicy());
                }

                fileMeta.RegisterChunkAsReceived(chunkMeta);
                if (fileMeta.IsComplete)
                {
                    // Since we are using a cache and memory is automatically disposed,
                    // we don't need to do this, so we won't so we can keep a record of
                    // our completed uploads.
                    //uploadChunkCache.Remove(chunkMeta.FlowIdentifier);
                }
            }
            return fileMeta.IsComplete;
        }

        /// <summary>
        /// Keep an upload in cache for two hours after it is last used
        /// </summary>
        private static CacheItemPolicy DefaultCacheItemPolicy()
        {
            return new CacheItemPolicy()
            {
                SlidingExpiration = TimeSpan.FromMinutes(120)
            };
        }

    }
}