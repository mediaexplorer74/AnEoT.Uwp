using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.Diagnostics;
using Windows.Storage;

namespace AnEoT.Uwp.Helpers;

/// <summary>
/// 为访问应用包中的文件提供帮助方法的类
/// A class that provides a helper method for accessing files in an application package
/// </summary>
internal static class FileHelper
{
    /// <summary>
    /// 获取指定期刊的封面
    /// / Get the cover of the designated journal
    /// </summary>
    /// <param name="volumeRawName">Original name of the journal</param>
    /// <returns>Point to the designated journal <see cref="Uri"/></returns>
    public static async Task<Uri> GetVolumeCover(string volumeRawName)
    {
        string baseUri = $"ms-appx:///Assets/Test/posts/{volumeRawName}/";

        string readmeFileUri = $"{baseUri}/README.md";

        try
        {
            StorageFile readmeFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(readmeFileUri));

            string readmeMarkdown = await FileIO.ReadTextAsync(readmeFile);
            MarkdownDocument doc = Markdown.Parse(readmeMarkdown, MarkdownHelper.Pipeline);

            LinkInline imgLink = doc.Descendants<LinkInline>().First();

            Uri coverUri = new(new Uri(baseUri, UriKind.Absolute), imgLink.Url);
            return coverUri;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[ex] fileHelper - GetVolumeCover ex.: " + ex.Message);
            return default;
        }
    }

    /// <summary>
    /// Get the catalog page of the specified journal
    /// </summary>
    /// <param name="volumeRawName">Original name of the journal</param>
    /// <returns>Specifies the Markdown string of the journal catalog page</returns>
    public static async Task<string> GetVolumeReadmeMarkdown(string volumeRawName)
    {
        string readmeFileUri = $"ms-appx:///Assets/Test/posts/{volumeRawName}/README.md";

        try
        {
            StorageFile readmeFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(readmeFileUri));
            string readmeMarkdown = await FileIO.ReadTextAsync(readmeFile);

            return readmeMarkdown;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[ex] GetVolumeReadmeMarkdown error: " + ex.Message);
        }

        return default;
    }
}
