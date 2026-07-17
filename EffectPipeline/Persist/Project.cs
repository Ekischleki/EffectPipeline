using EffectPipeline.Effects;
using EffectPipeline.GameObjects;
using EffectPipeline.GameObjects.PipelineManagers;
using EffectPipeline.types;
using SimpleBinaryFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.persist
{
    public class Project : ISerializable
    {
        public NodeStateManager nodeStateManager;
        public RGBImage thumbnail;
        public string Title;
        public Project() { }

        private Project(NodeStateManager nodeStateManager, string title)
        {
            Title = title;
            this.nodeStateManager = nodeStateManager;

            var image = nodeStateManager.OutputImage ?? RGBImage.LoadFrom("./assets/textures/unknown.png");
            if (image.width * image.height == 0)
            {
                image = RGBImage.LoadFrom($"./assets/textures/Consider.png");
            }
            //We're aiming for a 256 x 256 image
            double factor;
            if (image.width > image.height)
            {
                factor = 256.0 / image.width;
            }
            else
            {
                factor = 256.0 / image.height;
            }
            thumbnail = Resize.ResizeImage(image, int.Clamp((int)(image.width * factor), 1, 256), int.Clamp((int)(image.height * factor), 1, 256));

        }

        public static async Task SaveTo(Stream stream, NodeStateManager state, string title)
        {
            var proj = new Project(state, title);
            await Writer.EncodeTo(stream, proj);
            await stream.FlushAsync();
        }
        public static Task<Project> LoadFrom(Stream stream)
        {
           return Reader.Deserialize<Project>(stream);
        }

        public void FromReader(Region reader)
        {
            Title = reader.ReadString("Title");
            byte[] thumbnailBytes = reader.ReadBytes("Thumbnail");
            thumbnail = RGBImage.LoadFrom(thumbnailBytes);
            nodeStateManager = reader.ReadObject<NodeStateManager>("NodeStateManager");
        }

        public async Task WriteToWriter(Writer writer)
        {
            await writer.WriteString("Title", Title);
            using MemoryStream imageBytes = new();
            await thumbnail.SaveTo(imageBytes);
            //Might come in handy :3
            await writer.WriteBytes("Thumbnail", imageBytes.ToArray());
            await writer.WriteObject("NodeStateManager", nodeStateManager);
        }
    }
}
