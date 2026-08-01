using Microsoft.AspNetCore.Mvc;
using CV_mng_sys.Core.Services;

namespace CV_mng_sys.Web.Controllers;

public class HomeController : Controller
{
    private readonly CvService _cvs;
    private readonly PositionService _positions;
    private readonly ProjectService _projects;
    private readonly UserStatsService _userStats;

    public HomeController(CvService cvs, PositionService positions, ProjectService projects, UserStatsService userStats)
    {
        _cvs = cvs;
        _positions = positions;
        _projects = projects;
        _userStats = userStats;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.NewCvsLast24h = await _cvs.GetCvsCreatedInLastDaysAsync(1);
        ViewBag.TotalPositions = await _positions.GetTotalPositionCountAsync();
        ViewBag.TotalCvs = await _cvs.GetTotalSubmittedCvCountAsync();

        var (candidates, recruiters) = await _userStats.GetRoleCountsAsync();
        ViewBag.TotalCandidates = candidates;
        ViewBag.TotalRecruiters = recruiters;
        ViewBag.LatestPositions = await _positions.GetLatestAsync(5);
        ViewBag.MostPopularPositions = await _cvs.GetMostPopPositionsAsync(5);
        ViewBag.TagCloud = await _projects.GetTagCloudAsync();
        return View();
    }

    public IActionResult Privacy() => View();
}