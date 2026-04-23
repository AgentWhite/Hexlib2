using ASL.Models.Scenarios;

namespace ASL.Tests;

public class FormationTests
{
    [Fact]
    public void Formation_DefaultName_IsEmpty()
    {
        var f = new Formation();
        Assert.Equal(string.Empty, f.Name);
    }

    [Fact]
    public void Formation_SetName_Persists()
    {
        var f = new Formation { Name = "1st Platoon" };
        Assert.Equal("1st Platoon", f.Name);
    }
}

public class InsigniaTests
{
    [Fact]
    public void Insignia_DefaultName_IsEmpty()
    {
        var i = new Insignia();
        Assert.Equal(string.Empty, i.Name);
    }

    [Fact]
    public void Insignia_DefaultImagePath_IsNull()
    {
        var i = new Insignia();
        Assert.Null(i.ImagePath);
    }

    [Fact]
    public void Insignia_SetProperties_Persist()
    {
        var i = new Insignia { Name = "Iron Cross", ImagePath = "images/iron_cross.png" };
        Assert.Equal("Iron Cross", i.Name);
        Assert.Equal("images/iron_cross.png", i.ImagePath);
    }

    [Fact]
    public void Insignia_ImagePath_AcceptsNull()
    {
        var i = new Insignia { Name = "No Image", ImagePath = null };
        Assert.Null(i.ImagePath);
    }
}
