namespace FluxConvert.Core.Tools;

/// <summary>
/// The external programs the application can make use of.
/// </summary>
public enum ExternalToolKind
{
    /// <summary>FFmpeg, used for video and audio conversion.</summary>
    Ffmpeg,

    /// <summary>FFprobe, used to inspect media metadata.</summary>
    Ffprobe,

    /// <summary>ImageMagick, used for image conversion.</summary>
    ImageMagick,
}
