using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NgFlowSample.Services
{
    public class FlowFile
    {
        private readonly FlowChunk _flowChunk;
        private readonly bool[] _chunkArray;
        private int _totalChunksReceived;

        public FlowFile(FlowChunk flowChunk)
        {
            _flowChunk = flowChunk;
            _chunkArray = new bool[_flowChunk.FlowTotalChunks];
            _totalChunksReceived = 0;
        }

        public bool IsComplete
        {
            get { return (_totalChunksReceived == _flowChunk.FlowTotalChunks); }
        }

        public void RegisterChunk(FlowChunk flowChunk)
        {
            _chunkArray[flowChunk.FlowChunkNumber - 1] = true;
            _totalChunksReceived++;
        }
    }
}