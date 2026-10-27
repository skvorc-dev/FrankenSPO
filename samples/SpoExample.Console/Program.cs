using Microsoft.Data.Sqlite;
using Sferix.SPO.Abstractions.ObjectSpace;
using Sferix.SPO.Core.Model;
using Sferix.SPO.Provider.SQLite;
using SpoExample.Module;

// Builds the model, writes a customer to a temporary SQLite file, reads it back in a new ObjectSpace,
// updates it and deletes the file. Each ObjectSpace is disposed as soon as its work is done.
var model = new SpoModelBuilder()
    .AddModule<CustomerModule>()
    .Build(new SqliteProvider());

var path = Path.Combine(Path.GetTempPath(), $"spo-example-{Guid.NewGuid():N}.db");
try
{
    Guid id;
    using (var space = model.OpenObjectSpace($"Data Source={path}"))
    {
        var customer = space.Add(new Customer
        {
            Name = "Ada Lovelace",
            Credit = 1250.75m,
            Tier = CustomerTier.Standard,
            LastOrderAt = DateTime.UtcNow.AddDays(-3),
        });
        space.SaveChanges();
        id = customer.Id;
        Console.WriteLine($"Added    {customer.Id} {customer.Name}, tier {customer.Tier}, credit {customer.Credit}");
    }

    using (var reader = model.OpenObjectSpace($"Data Source={path}"))
    {
        var customer = Load(reader, id);
        Console.WriteLine($"Read     {customer.Id} {customer.Name}, tier {customer.Tier}, credit {customer.Credit}, last order {customer.LastOrderAt:O}");
    }

    using (var updater = model.OpenObjectSpace($"Data Source={path}"))
    {
        var customer = Load(updater, id);
        customer.Tier = CustomerTier.Gold;
        customer.Credit += 100m;
        updater.SaveChanges();
        Console.WriteLine($"Updated  {customer.Id} {customer.Name}, tier {customer.Tier}, credit {customer.Credit}");
    }

    using (var check = model.OpenObjectSpace($"Data Source={path}"))
    {
        var customer = Load(check, id);
        Console.WriteLine($"Checked  {customer.Id} {customer.Name}, tier {customer.Tier}, credit {customer.Credit}");
    }
}
finally
{
    // Pooled connections keep the file open, which blocks deletion on Windows.
    SqliteConnection.ClearAllPools();
    foreach (var file in new[] { path, path + "-wal", path + "-shm" })
        File.Delete(file);
}

static Customer Load(ISpoObjectSpace space, Guid id) =>
    space.Get<Customer>(id) ?? throw new InvalidOperationException($"Customer {id} was not found.");
