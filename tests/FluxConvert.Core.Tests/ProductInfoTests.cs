using System.Reflection;
using FluxConvert.Core;
using Xunit;

namespace FluxConvert.Core.Tests;

public class ProductInfoTests
{
    [Fact]
    public void ProductName_IsNotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(ProductInfo.ProductName));
    }

    [Fact]
    public void ProductName_HasNoSurroundingWhitespace()
    {
        Assert.Equal(ProductInfo.ProductName, ProductInfo.ProductName.Trim());
    }

    [Fact]
    public void WindowTitle_UsesTheProductName()
    {
        Assert.Equal(ProductInfo.ProductName, ProductInfo.WindowTitle);
    }

    [Fact]
    public void AssemblyProduct_MatchesTheProductName()
    {
        var assembly = typeof(ProductInfo).Assembly;
        var product = assembly.GetCustomAttributes<AssemblyProductAttribute>()
            .FirstOrDefault();

        Assert.NotNull(product);
        Assert.Equal(ProductInfo.ProductName, product!.Product);
    }
}
