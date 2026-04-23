using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Equipment;
using ASL.Models.Components;
using ASLInputTool.Converters;
using ASLInputTool.Tests.Fixtures;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace ASLInputTool.Tests;

[Collection("SettingsManager")]
public class ConverterAdditionalTests
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    // ── EnumToStringConverter ─────────────────────────────────────────────────

    [Theory]
    [InlineData("OpenGround", "Open Ground")]
    [InlineData("StoneBuilding", "Stone Building")]
    [InlineData("WoodenBuilding", "Wooden Building")]
    [InlineData("BeyondValor", "Beyond Valor")]
    public void EnumToString_SplitsPascalCase(string input, string expected)
    {
        var converter = new EnumToStringConverter();
        var result = converter.Convert(input, typeof(string), null!, Inv);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EnumToString_NullInput_ReturnsEmpty()
    {
        var converter = new EnumToStringConverter();
        var result = converter.Convert(null!, typeof(string), null!, Inv);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void EnumToString_SingleWord_Unchanged()
    {
        var converter = new EnumToStringConverter();
        var result = converter.Convert("Woods", typeof(string), null!, Inv);
        Assert.Equal("Woods", result);
    }

    // ── EnumToBooleanConverter ────────────────────────────────────────────────

    [Fact]
    public void EnumToBoolean_MatchingParameter_ReturnsTrue()
    {
        var converter = new EnumToBooleanConverter();
        var result = converter.Convert(TerrainType.Woods, typeof(bool), "Woods", Inv);
        Assert.Equal(true, result);
    }

    [Fact]
    public void EnumToBoolean_NonMatchingParameter_ReturnsFalse()
    {
        var converter = new EnumToBooleanConverter();
        var result = converter.Convert(TerrainType.Woods, typeof(bool), "Water", Inv);
        Assert.Equal(false, result);
    }

    [Fact]
    public void EnumToBoolean_ConvertBack_TrueAndParam_ReturnsEnum()
    {
        var converter = new EnumToBooleanConverter();
        var result = converter.ConvertBack(true, typeof(TerrainType), "Woods", Inv);
        Assert.Equal(TerrainType.Woods, result);
    }

    [Fact]
    public void EnumToBoolean_ConvertBack_False_ReturnsDoNothing()
    {
        var converter = new EnumToBooleanConverter();
        var result = converter.ConvertBack(false, typeof(TerrainType), "Woods", Inv);
        Assert.Equal(Binding.DoNothing, result);
    }

    // ── EnumToVisibilityConverter ─────────────────────────────────────────────

    [Fact]
    public void EnumToVisibility_Match_ReturnsVisible()
    {
        var converter = new EnumToVisibilityConverter();
        var result = converter.Convert(TerrainType.Woods, typeof(Visibility), "Woods", Inv);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void EnumToVisibility_NoMatch_ReturnsCollapsed()
    {
        var converter = new EnumToVisibilityConverter();
        var result = converter.Convert(TerrainType.Woods, typeof(Visibility), "Water", Inv);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void EnumToVisibility_MultiValuePipe_MatchesAny()
    {
        var converter = new EnumToVisibilityConverter();
        var result = converter.Convert(TerrainType.Water, typeof(Visibility), "Woods|Water|Marsh", Inv);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void EnumToVisibility_NullValue_ReturnsCollapsed()
    {
        var converter = new EnumToVisibilityConverter();
        var result = converter.Convert(null!, typeof(Visibility), "Woods", Inv);
        Assert.Equal(Visibility.Collapsed, result);
    }

    // ── NullToVisibilityConverter ─────────────────────────────────────────────

    [Fact]
    public void NullToVisibility_NullValue_ReturnsVisible()
    {
        var converter = new NullToVisibilityConverter();
        var result = converter.Convert(null!, typeof(Visibility), null!, Inv);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void NullToVisibility_NonNullValue_ReturnsCollapsed()
    {
        var converter = new NullToVisibilityConverter();
        var result = converter.Convert("hello", typeof(Visibility), null!, Inv);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void NullToVisibility_EmptyString_ReturnsVisible()
    {
        var converter = new NullToVisibilityConverter();
        var result = converter.Convert("", typeof(Visibility), null!, Inv);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void NullToVisibility_WhitespaceString_ReturnsVisible()
    {
        var converter = new NullToVisibilityConverter();
        var result = converter.Convert("   ", typeof(Visibility), null!, Inv);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void NullToVisibility_InverseParam_NullReturnsCollapsed()
    {
        var converter = new NullToVisibilityConverter();
        var result = converter.Convert(null!, typeof(Visibility), "Inverse", Inv);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void NullToVisibility_InverseParam_NonNullReturnsVisible()
    {
        var converter = new NullToVisibilityConverter();
        var result = converter.Convert("value", typeof(Visibility), "Inverse", Inv);
        Assert.Equal(Visibility.Visible, result);
    }

    // ── CenterOffsetConverter ─────────────────────────────────────────────────

    [Fact]
    public void CenterOffset_NoParameter_AppliesDefaultOffset()
    {
        var converter = new CenterOffsetConverter();
        var result = converter.Convert(100.0, typeof(double), null!, Inv);
        Assert.Equal(80.0, result); // 100 + (-20) default
    }

    [Fact]
    public void CenterOffset_WithParameter_AppliesCustomOffset()
    {
        var converter = new CenterOffsetConverter();
        var result = converter.Convert(50.0, typeof(double), "-15", Inv);
        Assert.Equal(35.0, result);
    }

    [Fact]
    public void CenterOffset_NonDoubleInput_ReturnsInputUnchanged()
    {
        var converter = new CenterOffsetConverter();
        var result = converter.Convert("not-a-double", typeof(double), null!, Inv);
        Assert.Equal("not-a-double", result);
    }

    // ── ElevationColorConverter ───────────────────────────────────────────────

    [Theory]
    [InlineData(0, 200, 230, 200)]  // Light Green
    [InlineData(1, 210, 180, 140)]  // Tan
    [InlineData(2, 180, 150, 100)]  // Medium Brown
    [InlineData(3, 150, 120, 70)]   // Brown
    public void ElevationColor_KnownLevel_ReturnsCorrectColor(int elevation, byte r, byte g, byte b)
    {
        var converter = new ElevationColorConverter();
        var result = converter.Convert(elevation, typeof(Brush), null!, Inv);
        var brush = Assert.IsType<SolidColorBrush>(result);
        Assert.Equal(r, brush.Color.R);
        Assert.Equal(g, brush.Color.G);
        Assert.Equal(b, brush.Color.B);
    }

    [Fact]
    public void ElevationColor_Level4Plus_ReturnsDarkBrown()
    {
        var converter = new ElevationColorConverter();
        var result = converter.Convert(5, typeof(Brush), null!, Inv);
        var brush = Assert.IsType<SolidColorBrush>(result);
        Assert.Equal(110, brush.Color.R);
    }

    [Fact]
    public void ElevationColor_NonIntInput_ReturnsTransparent()
    {
        var converter = new ElevationColorConverter();
        var result = converter.Convert("bad", typeof(Brush), null!, Inv);
        Assert.Equal(Brushes.Transparent, result);
    }

    // ── TerrainColorConverter ─────────────────────────────────────────────────

    [Theory]
    [InlineData(TerrainType.OpenGround, 200, 230, 200)]
    [InlineData(TerrainType.Woods, 34, 139, 34)]
    [InlineData(TerrainType.Water, 70, 130, 180)]
    [InlineData(TerrainType.Marsh, 85, 107, 47)]
    public void TerrainColor_KnownTerrain_ReturnsCorrectColor(TerrainType terrain, byte r, byte g, byte b)
    {
        var converter = new TerrainColorConverter();
        var result = converter.Convert(terrain, typeof(Brush), null!, Inv);
        var brush = Assert.IsType<SolidColorBrush>(result);
        Assert.Equal(r, brush.Color.R);
        Assert.Equal(g, brush.Color.G);
        Assert.Equal(b, brush.Color.B);
    }

    [Fact]
    public void TerrainColor_UnknownTerrain_ReturnsLightGray()
    {
        var converter = new TerrainColorConverter();
        var result = converter.Convert((TerrainType)999, typeof(Brush), null!, Inv);
        Assert.Equal(Brushes.LightGray, result);
    }

    [Fact]
    public void TerrainColor_NonTerrainInput_ReturnsTransparent()
    {
        var converter = new TerrainColorConverter();
        var result = converter.Convert("bad", typeof(Brush), null!, Inv);
        Assert.Equal(Brushes.Transparent, result);
    }

    [Fact]
    public void TerrainColor_StoneBuilding_IconMode_ReturnsDimGray()
    {
        var converter = new TerrainColorConverter();
        var result = converter.Convert(TerrainType.StoneBuilding, typeof(Brush), "Icon", Inv);
        Assert.Equal(Brushes.DimGray, result);
    }

    [Fact]
    public void TerrainColor_WoodenBuilding_IconMode_ReturnsSaddleBrown()
    {
        var converter = new TerrainColorConverter();
        var result = converter.Convert(TerrainType.WoodenBuilding, typeof(Brush), "Icon", Inv);
        Assert.Equal(Brushes.SaddleBrown, result);
    }

    [Fact]
    public void TerrainColor_StoneBuilding_NonIconMode_ReturnsBackgroundColor()
    {
        var converter = new TerrainColorConverter();
        var result = converter.Convert(TerrainType.StoneBuilding, typeof(Brush), null!, Inv);
        var brush = Assert.IsType<SolidColorBrush>(result);
        Assert.Equal(200, brush.Color.R); // OpenGround background for buildings
    }

    // ── InverseBooleanConverter ───────────────────────────────────────────────

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void InverseBoolean_Convert_InvertsValue(bool input, bool expected)
    {
        var converter = new InverseBooleanConverter();
        var result = converter.Convert(input, typeof(bool), null!, Inv);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void InverseBoolean_ConvertBack_InvertsValue(bool input, bool expected)
    {
        var converter = new InverseBooleanConverter();
        var result = converter.ConvertBack(input, typeof(bool), null!, Inv);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void InverseBoolean_NonBoolInput_ReturnsFalse()
    {
        var converter = new InverseBooleanConverter();
        var result = converter.Convert("string", typeof(bool), null!, Inv);
        Assert.Equal(false, result);
    }

    // ── EquipmentTypeConverter ────────────────────────────────────────────────

    [Fact]
    public void EquipmentType_MachineGun_ReturnsMgTypeName()
    {
        var unit = new Unit { Name = "MG" };
        unit.AddComponent(new MachineGunComponent { Type = MachineGunType.MMG });
        var converter = new EquipmentTypeConverter();

        var result = converter.Convert(unit, typeof(string), null!, Inv);

        Assert.Equal("MMG", result);
    }

    [Fact]
    public void EquipmentType_RadioWithPortage_ReturnsRadio()
    {
        var unit = new Unit { Name = "Radio" };
        unit.AddComponent(new RadioComponent());
        unit.AddComponent(new PortageComponent());
        var converter = new EquipmentTypeConverter();

        var result = converter.Convert(unit, typeof(string), null!, Inv);

        Assert.Equal("Radio", result);
    }

    [Fact]
    public void EquipmentType_RadioWithoutPortage_ReturnsTelephone()
    {
        var unit = new Unit { Name = "Phone" };
        unit.AddComponent(new RadioComponent());
        var converter = new EquipmentTypeConverter();

        var result = converter.Convert(unit, typeof(string), null!, Inv);

        Assert.Equal("Telephone", result);
    }

    [Fact]
    public void EquipmentType_LATW_ReturnsWeaponTypeName()
    {
        var unit = new Unit { Name = "LATW" };
        unit.AddComponent(new LightAntiTankWeaponComponent { WeaponType = LightAntiTankWeaponType.Panzerschreck });
        var converter = new EquipmentTypeConverter();

        var result = converter.Convert(unit, typeof(string), null!, Inv);

        Assert.Equal("Panzerschreck", result);
    }

    [Fact]
    public void EquipmentType_PlainUnit_ReturnsEmpty()
    {
        var unit = new Unit { Name = "Infantry" };
        var converter = new EquipmentTypeConverter();

        var result = converter.Convert(unit, typeof(string), null!, Inv);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void EquipmentType_NonUnitInput_ReturnsEmpty()
    {
        var converter = new EquipmentTypeConverter();
        var result = converter.Convert("not-a-unit", typeof(string), null!, Inv);
        Assert.Equal(string.Empty, result);
    }

    // ── SvgToImageSourceConverter (input validation only) ────────────────────

    [Fact]
    public void SvgToImageSource_NullInput_ReturnsNull()
    {
        var converter = new SvgToImageSourceConverter();
        var result = converter.Convert(null, typeof(object), null!, Inv);
        Assert.Null(result);
    }

    [Fact]
    public void SvgToImageSource_EmptyString_ReturnsNull()
    {
        var converter = new SvgToImageSourceConverter();
        var result = converter.Convert("", typeof(object), null!, Inv);
        Assert.Null(result);
    }

    [Fact]
    public void SvgToImageSource_NonSvgString_ReturnsNull()
    {
        var converter = new SvgToImageSourceConverter();
        var result = converter.Convert("this is not svg content", typeof(object), null!, Inv);
        Assert.Null(result);
    }

    // ── ImageSourceConverter (path validation only) ───────────────────────────

    [Fact]
    public void ImageSource_NullInput_ReturnsNull()
    {
        var converter = new ASLInputTool.Converters.ImageSourceConverter();
        var result = converter.Convert(null, typeof(object), null!, Inv);
        Assert.Null(result);
    }

    [Fact]
    public void ImageSource_EmptyPath_ReturnsNull()
    {
        var converter = new ASLInputTool.Converters.ImageSourceConverter();
        var result = converter.Convert("", typeof(object), null!, Inv);
        Assert.Null(result);
    }

    [Fact]
    public void ImageSource_NonExistentPath_ReturnsNull()
    {
        var converter = new ASLInputTool.Converters.ImageSourceConverter();
        var result = converter.Convert(@"C:\does\not\exist.png", typeof(object), null!, Inv);
        Assert.Null(result);
    }
}
