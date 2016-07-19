using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;

namespace NgFlowSample.Services
{
    public class FlowChunkProcessor
    {
        private readonly InMemoryMultipartFormDataStreamProvider streamProvider;

        #region Constructor

        public FlowChunkProcessor()
        {
            streamProvider = new InMemoryMultipartFormDataStreamProvider();
        }

        #endregion

        public async Task<bool> ProcessChunkRequest(HttpRequestMessage request)
        {
            await request.Content.ReadAsMultipartAsync(streamProvider);
            var isComplete = RegisterSuccessfulChunk(streamProvider.FormData.ToObject<FlowChunk>());
            if (isComplete)
            {
                //Combine all flowChunk byte array to form original stream
            }
            return isComplete;
        }

        /// <summary>
        /// (Thread Safe) Marks a chunk as recieved.
        /// </summary>
        private static bool RegisterSuccessfulChunk(FlowChunk flowChunk)
        {
            var flowFile = FlowFileCache.GetItem(flowChunk.FlowIdentifier, flowChunk) as FlowFile;
            flowFile.RegisterChunk(flowChunk);
            //lock (chunkCacheLock)
            //{
            //    flowFile = GetFileMetaData(flowChunk.FlowIdentifier);
            //    if (flowFile == null)
            //    {
            //        flowFile = new FlowFile(flowChunk);
            //        uploadChunkCache.Add(flowChunk.FlowIdentifier, flowFile, DefaultCacheItemPolicy());
            //    }

            //    flowFile.RegisterChunk(flowChunk);
            //    if (flowFile.IsComplete)
            //    {
            //        // Since we are using a cache and memory is automatically disposed,
            //        // we don't need to do this, so we won't so we can keep a record of
            //        // our completed uploads.
            //        //uploadChunkCache.Remove(chunkMeta.FlowIdentifier);
            //    }
            //}
            return flowFile.IsComplete;
        }
    }

    public static class FlowFileCache
    {
        private static MemoryCache _cache = new MemoryCache("FlowFileCache");

        public static object GetItem(string key, FlowChunk flowChunk)
        {
            return AddOrGetExisting(key, () => InitItem(flowChunk));
        }

        private static T AddOrGetExisting<T>(string key, Func<T> valueFactory)
        {
            var newValue = new Lazy<T>(valueFactory);
            var oldValue = _cache.AddOrGetExisting(key, newValue, new CacheItemPolicy()) as Lazy<T>;
            try
            {
                return (oldValue ?? newValue).Value;
            }
            catch (Exception)
            {
                // Handle cached lazy exception by evicting from cache.
                _cache.Remove(key);
                throw;
            }
        }

        private static FlowFile InitItem(FlowChunk flowChunk)
        {
            return new FlowFile(flowChunk);
        }
    }
}