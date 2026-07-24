using Microsoft.AspNetCore.Mvc;
using CV_mng_sys.Core.Services;

namespace CV_mng_sys.Web.Controllers;

public class HomeController : Controller
{
    private readonly CvService _cvs;
    private readonly PositionService _positions;

    public HomeController(CvService cvs, PositionService positions)
    {
        _cvs = cvs;
        _positions = positions;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.NewCvsLast24h = await _cvs.GetCvsCreatedInLastDaysAsync(1);
        ViewBag.ActivePositions = await _positions.GetActivePositionCountAsync();
        return View();
    }

    public IActionResult Privacy() => View();
}