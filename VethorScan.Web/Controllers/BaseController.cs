using Microsoft.AspNetCore.Mvc;

namespace VethorScan.Web.Controllers
{
    [ResponseCache(Duration = 60)]
    [Route("api/v1/[controller]")]
    public class BaseController : Controller
    {
    }
}