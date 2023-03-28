using Marten;
using Weasel.Core;

var builder = WebApplication.CreateBuilder(args: args);
builder.Services.AddMarten(o =>
{
    o.Connection(builder.Configuration.GetConnectionString(name: "DefaultConnection")!);
    o.AutoCreateSchemaObjects = AutoCreate.None;
    o.Schema.For<User>();
    o.Schema.For<Post>().Index(x => x.By);
})
    .ApplyAllDatabaseChangesOnStartup()
    .AssertDatabaseMatchesConfigurationOnStartup();

WebApplication app = builder.Build();

app.MapGet(pattern: "/", handler: () => "Working with Marten");

app.MapGet(pattern: "/create", handler: async (IDocumentSession session) =>
{
    var existing = new User(Name: "Doug", Age: 36) { Id = new Guid(g: "01872781-782f-41bd-b95d-26556fa18a38") };
    session.Store(existing,new User(Name: "Bruno", Age: 34));
    await session.SaveChangesAsync();
    return "User created";
});

app.MapGet(pattern: "/delete-all-users", handler: async (IDocumentSession session) => 
{ 
    List<User> usersToDelete = session.Query<User>().Where(x => x.Id != Guid.Empty).ToList();
    foreach (User user in usersToDelete)
    {
        session.Delete(entity: user);
    }    await session.SaveChangesAsync();
    return "All users removed";
});

app.MapGet(pattern: "/delete-user/{guid}", handler: async (Guid guid, IDocumentSession session) =>
{ 
    IQueryable<User> userToDelete = session.Query<User>().Where(x => x.Id == guid);
    session.Delete(entity: userToDelete);
    await session.SaveChangesAsync();
    return "All users removed";
});

app.MapGet(pattern: "/get-user/{guid}",  handler: (Guid guid,IQuerySession session) =>
{
    return session.LoadAsync<User>(id: guid);
});

app.MapGet(pattern: "/get-all-users",  handler: (IQuerySession session) =>
{
    return session.Query<User>().ToListAsync();
});

app.Run();