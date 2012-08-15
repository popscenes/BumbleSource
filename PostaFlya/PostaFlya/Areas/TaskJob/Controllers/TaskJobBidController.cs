using System.Web.Mvc;
using PostaFlya.Areas.TaskJob.Models;

namespace PostaFlya.Areas.TaskJob.Controllers
{
    public class TaskJobBidController : Controller
    {
        public ActionResult Get(string id)
        {
            return View((object) new TaskJobBidModel()
                                          {
                                              TaskJobId = id
                                          });
        }
    }
}