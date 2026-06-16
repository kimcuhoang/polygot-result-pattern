namespace UserApi.Domains;
public class User
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }

    public static User DefaultUser = new()
    {
         Id = Guid.Parse("1b4db7eb-4057-5ddf-91e0-36dec72071f5"),
         Name = "John Doe",
         Email = "jdoe@email.com"
    };
}