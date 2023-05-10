
// using SFML.Graphics;
// using SFML.System;
// using SixLabors.ImageSharp;
// using SixLabors.ImageSharp.PixelFormats;
// using SixLabors.ImageSharp.Processing;
// using Image = SixLabors.ImageSharp.Image;

// namespace Touhou;

// public class SpriteAtlas {

//     private Dictionary<string, int> sprites = new();


//     public void LoadSprites(string path) {

//         // var randomX = new Random();
//         // var randomY = new Random();
//         // var random = new Random();


//         // for (int i = 0; i < 100; i++) {
//         //     using var image = new Image<Rgba32>(randomX.Next(50, 200), randomY.Next(50, 200), new Rgba32(random.NextSingle(), random.NextSingle(), random.NextSingle(), 1f));
//         //     image.SaveAsPng($"./assets/sprites/{i}.png");
//         // }


//         uint maxTextureSize = Texture.MaximumSize;
//         System.Console.WriteLine(maxTextureSize);

//         var files = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories).ToList();

//         files = files.OrderByDescending(e => {
//             var info = Image.Identify(e);
//             return info.Width * info.Height;
//         }).ToList();

//         int imageSize = 2048;

//         bool[,] occupied = new bool[imageSize, imageSize];
//         for (int x = 0; x < imageSize; x++) {
//             for (int y = 0; y < imageSize; y++) {
//                 occupied[x, y] = false;
//             }
//         }
//         using var textureImage = new Image<Rgba32>(imageSize, imageSize, new Rgba32(0, 0, 0, 0));
//         var corners = new List<(int X, int Y)>() { (0, 0) };
//         var usedCorners = new HashSet<(int X, int Y)>();



//         foreach (var file in files) {
//             using var image = Image.Load(file);

//             foreach (var corner in corners) {

//                 if (usedCorners.Contains(corner)) continue;

//                 if (corner.X + image.Width > imageSize || corner.Y + image.Height > imageSize) continue;

//                 bool free = true;
//                 for (int i = 0; i < image.Width * image.Height; i++) {
//                     if (occupied[corner.X + i % image.Width, corner.Y + i / image.Height]) {
//                         free = false;
//                         break;
//                     }
//                 }
//                 if (!free) continue;

//                 for (int x = 0; x < image.Width; x++) {
//                     for (int y = 0; y < image.Height; y++) {
//                         occupied[corner.X + x, corner.Y + y] = true;
//                     }
//                 }
//                 corners.Add((corner.X + image.Width, corner.Y));
//                 corners.Add((corner.X, corner.Y + image.Height));
//                 usedCorners.Add(corner);
//                 textureImage.Mutate(e => e.DrawImage(image, new Point(corner.X, corner.Y), 1f));
//                 break;
//             }
//         }

//         textureImage.SaveAsPng();

//     }
// }