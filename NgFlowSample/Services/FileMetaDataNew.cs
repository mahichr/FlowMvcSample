using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NgFlowSample.Services
{
    public class FileMetaDataNew
    {
        private static long ChunkIndex(long chunkNumber)
        {
            return chunkNumber - 1;
        }

        public FileMetaDataNew(FlowMetaDataNew flowMeta)
        {
            FlowMeta = flowMeta;
            ChunkArray = new bool[flowMeta.FlowTotalChunks];
            TotalChunksReceived = 0;
        }

        public bool[] ChunkArray { get; set; }

        /// <summary>
        /// Chunks can come out of order, so we must track how many chunks 
        /// we have successfully recieved to determine if the download is complete.
        /// </summary>
        public int TotalChunksReceived { get; set; }

        public FlowMetaDataNew FlowMeta { get; set; }

        public bool IsComplete
        {
            get
            {
                return (TotalChunksReceived == FlowMeta.FlowTotalChunks);
            }
        }

        public void RegisterChunkAsReceived(FlowMetaDataNew flowMeta)
        {
            ChunkArray[ChunkIndex(flowMeta.FlowChunkNumber)] = true;
            TotalChunksReceived++;
        }

        public bool HasChunk(FlowMetaDataNew flowMeta)
        {
            return ChunkArray[ChunkIndex(flowMeta.FlowChunkNumber)];
        }
    }
}