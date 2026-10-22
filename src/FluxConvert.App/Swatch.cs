namespace FluxConvert.App;

/// <summary>
/// A colour token shown in the design-system preview.
/// </summary>
/// <param name="Name">The token name.</param>
/// <param name="Hex">The colour value.</param>
public sealed record Swatch(string Name, string Hex);
