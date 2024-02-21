using AnEoT.Uwp.Contracts;
using AnEoT.Uwp.Helpers.Comparer;
using AnEoT.Uwp.Models.Markdown;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.Search;
using YamlDotNet.Core.Tokens;

namespace AnEoT.Uwp.Services;

/// <summary>
/// 基于本地文件的期刊获取器
/// / Journal fetcher based on local files
/// </summary>
public readonly struct FileVolumeProvider : IVolumeProvider
{
    /// <summary>
    /// Currently used folder path
    /// </summary>
    public string CurrentPath { get; }

    /// <summary>
    /// Constructed using the specified parameters <see cref="FileVolumeProvider"/> New example of
    /// </summary>
    /// <param name="path">The path of the folder containing the journal files will search for
    /// the journal directly from here</param>
    public FileVolumeProvider(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException($"“{nameof(path)}” cannot be null or blank. ", nameof(path));
        }

        CurrentPath = path;
    }

    /// <inheritdoc/>
    public async Task<VolumeDetail> GetVolumeAsync(string volume)
    {
        StorageFolder baseFolder = await StorageFolder.GetFolderFromPathAsync(CurrentPath);
        StorageFolder volumeFolder = await GetVolumeFolder(volume,baseFolder);
        return await GetVolumeDetailFromStorageFolderAsync(volumeFolder);
    }

    /// <inheritdoc/>
    public async Task<VolumeInfo> GetVolumeInfoAsync(string volume)
    {
        StorageFolder baseFolder = await StorageFolder.GetFolderFromPathAsync(CurrentPath);
        StorageFolder volumeFolder = await GetVolumeFolder(volume, baseFolder);
        return await GetVolumeInfoFromStorageFolderAsync(volumeFolder);
    }

    /// <inheritdoc/>
    public async Task<VolumeDetail> GetLatestVolumeAsync()
    {
        StorageFolder baseFolder = await StorageFolder.GetFolderFromPathAsync(CurrentPath);
        StorageFolder volumeFolder = 
            (await baseFolder.GetFoldersAsync()).OrderBy(file => file.DisplayName).Reverse().FirstOrDefault();

        if (volumeFolder is null)
        {
            //throw new ArgumentException("使用指定的参数，找不到期刊");
            Debug.WriteLine("[i] 使用指定的参数，找不到期刊 " +
                "/ Using the specified parameters, the journal could not be found");
        }

        return await GetVolumeDetailFromStorageFolderAsync(volumeFolder);
    }

    /// <inheritdoc/>
    public async Task<VolumeInfo> GetLatestVolumeInfoAsync()
    {
        StorageFolder baseFolder = await StorageFolder.GetFolderFromPathAsync(CurrentPath);

        StorageFolder volumeFolder = (await baseFolder.GetFoldersAsync())
                                 .OrderBy(file => file.DisplayName).Reverse().FirstOrDefault();

        if (volumeFolder is null)
        {
            //throw new ArgumentException("使用指定的参数，找不到期刊");
            Debug.WriteLine("[i] 使用指定的参数，找不到期刊 " +
                "/ Using the specified parameters, the journal could not be found");
        }

        return await GetVolumeInfoFromStorageFolderAsync(volumeFolder);
    }

    private static async Task<VolumeDetail> GetVolumeDetailFromStorageFolderAsync(StorageFolder volumeFolder)
    {
        IEnumerable<StorageFile> fileList = (await volumeFolder.GetFilesAsync(CommonFileQuery.OrderByName))
                    .Where(file => file.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase));

        string volumeTitle = null;
        List<ArticleDetail> articles = new(fileList.Count());

        foreach (StorageFile file in fileList)
        {
            string markdown = await FileIO.ReadTextAsync(file);

            if (MarkdownHelper.TryGetFromFrontMatter(markdown, out MarkdownArticleInfo result))
            {
                if (file.Name.Equals("README.md", StringComparison.OrdinalIgnoreCase))
                {
                    volumeTitle = result.Title;
                }
                else
                {
                    if (DateTimeOffset.TryParse(result.Date, out DateTimeOffset date) != true)
                    {
                        date = new DateTimeOffset();
                    }

                    string description = string.IsNullOrWhiteSpace(result.Description) 
                        ? MarkdownHelper.GetArticleQuote(markdown)
                        : result.Description;

                    ArticleDetail articleDetail = new(result.Title, 
                        result.Author ?? "Another end of Terra", 
                        description.Trim(), date, markdown,
                        result.Category, result.Tag, result.Order, result.ShortTitle);

                    articles.Add(articleDetail);
                }
            }
        }

        articles.Sort(new ArticleDetailOrInfoComparer());

        if (volumeTitle is not null && articles.Any())
        {
            VolumeDetail detail = new(volumeTitle, volumeFolder.DisplayName, articles);
            return detail;
        }
        else
        {
            //throw new ArgumentException("使用指定的参数，无法获取指定期刊");
            Debug.WriteLine("[i] 使用指定的参数，无法获取指定期刊 " +
                "/ Using the specified parameters, the specified journal cannot be obtained");
            
        }
        return default;
    }

    private static async Task<VolumeInfo> GetVolumeInfoFromStorageFolderAsync(StorageFolder volumeFolder)
    {
        IEnumerable<StorageFile> fileList = (await volumeFolder.GetFilesAsync(CommonFileQuery.OrderByName))
                    .Where(file => file.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase));

        string volumeTitle = null;
        List<ArticleInfo> articles = new(fileList.Count());

        foreach (StorageFile file in fileList)
        {
            string markdown = await FileIO.ReadTextAsync(file);

            if (MarkdownHelper.TryGetFromFrontMatter(markdown, out MarkdownArticleInfo result))
            {
                if (file.Name.Equals("README.md", StringComparison.OrdinalIgnoreCase))
                {
                    volumeTitle = result.Title;
                }
                else
                {
                    if (DateTimeOffset.TryParse(result.Date, out DateTimeOffset date) != true)
                    {
                        date = new DateTimeOffset();
                    }

                    string description = string.IsNullOrWhiteSpace(result.Description) 
                        ? MarkdownHelper.GetArticleQuote(markdown)
                        : result.Description;

                    ArticleInfo articleInfo = new(result.Title, 
                        result.Author ?? "Another end of Terra", description.Trim(), 
                        date, result.Category, result.Tag, result.Order, result.ShortTitle);

                    articles.Add(articleInfo);
                }
            }
        }

        articles.Sort(new ArticleDetailOrInfoComparer());

        if (volumeTitle is not null && articles.Any())
        {
            VolumeInfo volumeInfo = new(volumeTitle, volumeFolder.DisplayName, articles);
            return volumeInfo;
        }
        else
        {
            //throw new ArgumentException("使用指定的参数，无法获取指定期刊的信息");
            Debug.WriteLine("[i] 使用指定的参数，无法获取指定期刊的信息 " +
                "/ Using the specified parameters, the information of the specified journal cannot be obtained");
            return default;
        }
    }

    private static async Task<StorageFolder> GetVolumeFolder(string volume, StorageFolder baseFolder)
    {
        string volumeFolderPath = Path.Combine(baseFolder.Path, volume);

        if (Directory.Exists(volumeFolderPath) != true)
        {
            throw new DirectoryNotFoundException($"path {volumeFolderPath} does not exist");
        }

        return await baseFolder.GetFolderAsync(volume);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<VolumeListItem>> GetVolumeListAsync()
    {
        StorageFolder baseFolder = await StorageFolder.GetFolderFromPathAsync(CurrentPath);
        IReadOnlyList<StorageFolder> volumeFolders = await baseFolder.GetFoldersAsync();

        if (volumeFolders.Any() is not true)
        {
            //throw new ArgumentException("使用指定的参数，找不到任何期刊");
            Debug.WriteLine("[i] 使用指定的参数，找不到任何期刊 " +
               "/ Using the specified parameters, no journals were found");
        }

        List<VolumeListItem> volumeList = new(volumeFolders.Count);

        foreach (StorageFolder volFolder in volumeFolders)
        {
            StorageFile file = (await volFolder.GetFilesAsync())
                .FirstOrDefault(file => file.Name.Equals("README.md", StringComparison.OrdinalIgnoreCase));

            if (file is null)
            {
                continue;
            }
            
            string markdown = await FileIO.ReadTextAsync(file);

            if (MarkdownHelper.TryGetFromFrontMatter(markdown, out MarkdownArticleInfo result))
            {
                VolumeListItem volumeInfo = new(result.Title, volFolder.DisplayName,
                    $"ms-appx:///Assets/Test/posts/{volFolder.DisplayName}/res/cover.webp");

                volumeList.Add(volumeInfo);
            }
            else
            {
                continue;
            }
        }

        if (volumeList.Any() is not true)
        {
            //throw new ArgumentException("使用指定的参数，找不到任何期刊");
            Debug.WriteLine("[i] 使用指定的参数，找不到任何期刊 " +
                "/ Using the specified parameters, no journals were found");

            return default;
        }

        volumeList.OrderBy(item => item.RawName);

        return volumeList;
    }
}
