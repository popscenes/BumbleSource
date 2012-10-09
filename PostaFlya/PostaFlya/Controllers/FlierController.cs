using System;
using System.Linq;
using System.Web.Mvc;
using Website.Application.Content;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Domain.Flier.Query;
using Website.Infrastructure.Command;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;

namespace PostaFlya.Controllers
{
    [Authorize(Roles = "Participant")]
    public class FlierController : Controller
    {
        public FlierController()
        {
        }

        public ActionResult MyFliers()
        {

            return View("MyFliers");
        }

        public ActionResult Create(FlierBehaviour id = FlierBehaviour.Default, Guid? image = null)
        {
            return View("CreateEdit", new FlierCreateModel()
                                          {
                                              FlierBehaviour = id,
                                              FlierImageId = image.HasValue ? image.Value.ToString() : ""
                                          });
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {         
            //use api controller to load viewmodel, just use below model for editorfor etc
            var model = new FlierCreateModel()
                            {
                                Id = id
                            };

            return View(model);
        }
    }
}