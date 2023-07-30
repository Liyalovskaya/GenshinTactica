namespace CorgiOutlineSDF
{
    using UnityEngine;
    using UnityEngine.Rendering;

    [System.Serializable]
    public class RenderTexturePair
    {
        private RenderTexture reader;
        private RenderTexture writer;

        private int width;
        private int height;
        private int depth;

        public RenderTexturePair(string name, RenderTextureDescriptor desc)
        {
            this.width = desc.width;
            this.height = desc.height;
            this.depth = desc.depthBufferBits;

            reader = new RenderTexture(desc);
            writer = new RenderTexture(desc);

            reader.name = $"{name}_A";
            writer.name = $"{name}_B";
        }

        public void Create()
        {
            var a = reader.Create();
            var b = writer.Create();

            if(!a || !b)
            {
                Debug.LogError($"Failed to create textures!");
            }
        }

        public void Release()
        {
            if(reader != null && reader.IsCreated())
            {
                reader.Release();
            }

            if(writer != null && writer.IsCreated())
            {
                writer.Release();
            }
        }

        public RenderTexture GetReader()
        {
            return reader;
        }

        public RenderTexture GetWriter()
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
    }
}