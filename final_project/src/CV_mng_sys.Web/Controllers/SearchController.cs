using Microsoft.AspNetCore.Mvc;
using CV_mng_sys.Core.Services;
using System.Security;
using System.ComponentModel;

namespace CV_mng_sys.Web.Controllers;

public class SearchController : Controller
{
    private readonly PositionService _positions;
    private readonly CvService _cvs;
    
    public SearchController(PositionService positions, CvService cvs)
    {
        _positions = positions;
        _cvs = cvs;
    }
    
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Query = q ?? "";
        if (string.IsNullOrWhiteSpace(q))
        {
            ViewBag.Positions = new List<CV_mng_sys.Core.Entities.Position>();
            ViewBag.Cvs = new List<CV_mng_sys.Core.Entities.CvDocument>();
            return View();
        }
        ViewBag.Positions = await _positions.SearchAsync(q);
        ViewBag.Cvs = (User.IsInRole("Recruiter") || User.IsInRole("Administrator")) ? await _cvs.SearchAsync(q, includeAllStatuses: User.IsInRole("Administrator")) : new List<CV_mng_sys.Core.Entities.CvDocument>();
        return View();
    }
}



