using ModelContextProtocol.Server;
using System.ComponentModel;

namespace ManageContacts.McpServer.Tools;

[McpServerToolType]
public class ContactTools
{
    private static readonly List<Contact> _contacts = new();

    public record Contact(string FullName, string Email);

    [McpServerTool(Name = "create-contact"), Description("Creates a new contact with full name and email address, stored in memory.")]
    public Contact CreateContact
    (
        [Description("Full name of the contact.")] string fullName,
        [Description("Email address of the contact.")] string email
    )
    {
        var contact = new Contact(fullName, email);
        _contacts.Add(contact);

        return contact;
    }

    [McpServerTool(Name = "list-contacts"),
     Description("Lists contacts from the in-memory store. Optionally filter by a search term (matches name or email).")]
    public IEnumerable<Contact> ListContacts
    (
        [Description("Optional case-insensitive search that matches full name or email.")] string? searchTerm = null
    )
    {
        var queryable = _contacts.AsQueryable();

        if(!string.IsNullOrWhiteSpace(searchTerm))
        {
            queryable = queryable.Where(x => x.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                             x.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        return queryable.ToList();
    }
}
