using Ecommerce.Core.Common;

namespace Ecommerce.Core.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = null!;
    public bool State { get; set; }

    public void SetName(string newName)
    {
        if (newName is null) throw new ArgumentNullException();
        if (newName.Length < 2) throw new ArgumentException("The length of the name could not be less than 1");
        Name = newName;
    }

    public Category(){}

    public Category(string name, bool state)
    {
        SetName(name);
        State = state;
    }
}
