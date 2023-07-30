namespace CorgiOutlineSDF
{
    using UnityEngine;
    using UnityEngine.Rendering;

    [System.Serializable]
    public struct RenderTextureHandlePair
    {
        private int reader;
        private int writer;

        private int width;
        private int height;
        private int depth;
        private int volume;

        private RenderTextureDescriptor desc;

        public RenderTextureHandlePair(int propertyIdA, int propertyIdB, RenderTextureDescriptor desc)
        {
            width = desc.width;
            height = desc.height;
            depth = desc.depthBufferBits;
            volume = desc.volumeDepth;
            this.desc = desc;

            reader = propertyIdA;
            writer = propertyIdB;
        }

        public void AddCommands(CommandBuffer commands)
        {
            commands.GetTemporaryRT(reader, desc);
            commands.GetTemporaryRT(writer, desc);
        }

        public int GetReader()
        {
            return reader;
        }

        public int GetWriter()
        {
            return writer;
        }

        public void Swap()
        {
            var a = reader;
            var b = writer;

            reader = b;
            writer = a;
        }

        public int GetWidth()
        {
            return width;
        }

        public int GetHeight()
        {
            return height;
        }

        public int GetDepth()
        {
            return depth;
        }

        public int GetVolume()
        {
            return volume;
        }
    }
}