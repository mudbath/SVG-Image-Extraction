using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SVGBase64Extraction
{
    public class SVGImageExtractor
    {
        public List<SVGImage> Extract(string source)
        {
            var fileText = File.ReadAllText(source);
            var fileInfo = new FileInfo(source);

            var patternMatch = @"data:image\/(gif|png|jpeg|jpg);base64,((?s).*)";

            var svgDocument = new HtmlAgilityPack.HtmlDocument();
            svgDocument.LoadHtml(fileText);

            var svgNode = svgDocument.DocumentNode.SelectSingleNode("//svg");

            var images = svgNode.Descendants("image");

            var svgImages = new List<SVGImage>();

            var index = 0;

            foreach (var image in images)
            {

                if (image.HasAttributes)
                {

                    var hrefAttribute = image.Attributes.FirstOrDefault(i => i.Name == "xlink:href");
                    if (hrefAttribute != null && !String.IsNullOrEmpty(hrefAttribute.Value))
                    {

                        var match = Regex.Match(hrefAttribute.Value, patternMatch);
                        if (match.Success && match.Groups.Count != 0)
                        {

                            if (match.Groups.Count >= 3)
                            {

                                var fileType = match.Groups[1].Value;
                                var base64 = match.Groups[2].Value;

                                var bytes = Convert.FromBase64String(base64);

                                var fileName = String.Format("{0}-{1}.{2}", Path.GetFileNameWithoutExtension(fileInfo.Name), index, fileType);
                                var filePath = Path.Combine(fileInfo.Directory.FullName, fileName);

                                var svgImage = new SVGImage();

                                svgImage.Base64 = base64;
                                svgImage.FileType = fileType;
                                svgImage.Data = bytes;
                                svgImage.Path = filePath;
                                svgImage.Href = hrefAttribute.Value;
                                svgImage.FileName = fileName;
                                svgImage.Line = hrefAttribute.Line;
                                svgImages.Add(svgImage);

                            }

                        }

                    }


                }

                index += 1;

            }

            foreach (var svgImage in svgImages)
            {

                fileText = fileText.Replace(svgImage.Href, svgImage.FileName);

                File.WriteAllBytes(svgImage.Path, svgImage.Data);

            }

            File.WriteAllText(Path.Combine(fileInfo.Directory.FullName, String.Format("{0}-modified{1}", Path.GetFileNameWithoutExtension(source), fileInfo.Extension)), fileText);

            return svgImages;
        }
    }


    public class SVGImage
    {
        public int Index { get; set; }
        public string Href { get; set; }
        public string Base64 { get; set; }
        public byte[] Data { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string Path { get; set; }
        public int Line { get; set; }
    }
}
