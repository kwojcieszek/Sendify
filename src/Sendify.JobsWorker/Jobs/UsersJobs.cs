using Sendify.DataManager;
using Sendify.Shared;

namespace Sendify.JobsWorker.Jobs;

public class UsersJobs
{
    public void CheckUsersPasswords()
    {
        var db = new DataContext();
        var password = new PasswordSha256();

        var users = db.Users.Where(u => !u.IsHashed).ToList();

        foreach (var user in users)
        {
            user.Password = password.HashPassword(user.Password);
            user.IsHashed = true;
        }

        db.SaveChanges();
    }
}
