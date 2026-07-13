using EffectPipeline.Effects;
using EffectPipeline.gameObjects.GUI_Elements;
using EffectPipeline.types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectPipeline.effects
{
    internal class SquareTree : IEffect
    {
        private class Tree
        {
            const int MIN_EDGE_LEN = 1;
            public int start_x, start_y, width, height;

            /// <summary>
            /// In order top left, top right, bottom left, bottom right
            /// </summary>
            public Tree[]? children;

            public float GetMean(GreyscaleImage image)
            {
                double sum = 0.0;
                for (int x = start_x; (x - start_x) < width; x++)
                    for (int y = start_y; (y - start_y) < height; y++)
                    {
                        var index = x + y * image.width;
                        sum += image.image[index];
                    }
                return (float)(sum / (width * height));
            }
            public float GetStdDev(GreyscaleImage image, float mean, float local_representation)
            {
                double squared_dev_sum = 0.0;
                for (int x = start_x; (x - start_x) < width; x++)
                    for (int y = start_y; (y - start_y) < height; y++)
                    {
                        var index = x + y * image.width;
                        squared_dev_sum += MathF.Pow(mean - image.image[index], 2);
                    }
                return MathF.Sqrt((float)(squared_dev_sum / MathF.Pow(width * height, local_representation)));
            }
            public bool TrySplit()
            {
                if (width / 2 < MIN_EDGE_LEN || height / 2 < MIN_EDGE_LEN) 
                    return false;
                var alloc_width_left = width / 2;
                var alloc_top_height = height / 2;
                this.children = [
                    //Top left
                    new() {
                        start_x = start_x,
                        start_y = start_y,
                        width = alloc_width_left,
                        height = alloc_top_height
                    },
                    //Top right
                    new() {
                        start_x = start_x + alloc_width_left,
                        start_y = start_y,
                        width = width - alloc_width_left,
                        height = alloc_top_height
                    },
                    //Bottom left
                    new() {
                        start_x = start_x,
                        start_y = start_y + alloc_top_height,
                        width = alloc_width_left,
                        height = height - alloc_top_height
                    },
                    //Bottom right
                    new() {
                        start_x = start_x + alloc_width_left,
                        start_y = start_y + alloc_top_height,
                        width = width - alloc_width_left,
                        height = height - alloc_top_height,
                    }
                ];
                return true;
            }
            public void Color(GreyscaleImage image, float mean)
            {
                for (int x = start_x; (x - start_x) < width; x++)
                    for (int y = start_y; (y - start_y) < height; y++)
                    {
                        var index = x + y * image.width;
                        image.image[index] = mean;
                    }
            }
        }

        public IEnumerable<(string, Type)> Inputs => [("Image", typeof(GreyscaleImage))];

        public IEnumerable<(string, Type)> Outputs => [("Cubed", typeof(GreyscaleImage))];

        public Property[] Properties => [
            new NumberInputProperty("Iterations") { Min = 10, Max = 999999, Value = 1000},
            new FloatInputProperty("Local representation") {Min = -2, Max = 2, Value = 0.3f}
        ];

        public async Task<IInstance[]> applyEffect(IInstance?[] inputs, IPropertyState[] properties)
        {
            var iterations = ((NumberInputPropertyState)properties[0]).Value;
            var local_representation = ((FloatInputPropertyState)properties[1]).Value;
            var image = (GreyscaleImage?)inputs[0] ?? throw new ArgumentNullException();
            var root = new Tree() { start_x = 0, start_y = 0, width = image.width, height = image.height };
            Dictionary<Tree, (float mean, float std_dev)> all_leafs = [];
            //We dont need to calculate this because were always gonna get at least one iteration and the ca
            all_leafs.Add(root, (0.0f, 0.0f));
            for (var i = 0; i < iterations; i++) {
                //This should work because Enums are evaluated lazily, so we only end up splitting the first splittable leaf, still a bit sketch tho
                var branched_tree = all_leafs.ToList().OrderByDescending(pair => pair.Value.std_dev).Where(pair => pair.Key.TrySplit()).FirstOrDefault().Key;
                if(branched_tree == null)
                    break; //The image is too small to complete all iterations so we just stop now

                all_leafs.Remove(branched_tree);
                foreach(var child in branched_tree.children!)
                {
                    var mean = child.GetMean(image);
                    var std_dev = child.GetStdDev(image, mean, local_representation);
                    all_leafs.Add(child, (mean, std_dev));
                }
            }
            var new_image = new GreyscaleImage(image.width, image.height, new float[image.image.Length]);
            foreach(var (leaf, (mean, _)) in all_leafs)
            {
                leaf.Color(new_image, mean);
            }
            return [new_image];
        }
    }
    internal class SquareTreeSearch : IEffectSearch
    {
        public IEnumerable<string> Tags => ["Quantize", "cube"];

        public string Title => "Square Tree";

        public IEffect CreateEffect() => new SquareTree();
    }
}
