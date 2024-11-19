namespace CryptoProject.Data
{
    public class UserRolesModel
    {
        public string Email { get; set; }
        public List<string> SelectedRoles { get; set; } = new();
    }
}
