using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace VethorScan.Web.Controllers
{
    [ResponseCache(Duration = 60)]
    [Route("/v1/[controller]")]
    [EnableCors("AllowSpecificOrigin")]
    public class BaseController : Controller
    {
    }
}