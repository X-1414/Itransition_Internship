using Microsoft.EntityFrameworkCore;
using CV_mng_sys.Core.Data;

namespace CV_mng_sys.Core.Services;

public class UserStatsService
{
    private readonly ApplicationDbContext _db;
    public UserStatsService(ApplicationDbContext db) => _db = db;

    public async Task<(int Candidates, int Recruiters)> GetRoleCountsAsync()
    {
        var candidateRoleId = await _db.Roles.Where(r=>r.Name=="Candidate").Select(r => r.Id).FirstOrDefaultAsync();
        var recruiterRoleId = await _db.Roles.Where(r=>r.Name=="Recruiter").Select(r => r.Id).FirstOrDefaultAsync();

        var candidateCount = await _db.UserRoles.CountAsync(ur=>ur.RoleId==candidateRoleId);
        var recruiterCount = await _db.UserRoles.CountAsync(ur=>ur.RoleId==recruiterRoleId);

        return (candidateCount, recruiterCount);
    }
}