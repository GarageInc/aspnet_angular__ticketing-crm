using System.Collections.Generic;
using System.Web.Mvc;
using LSports.Framework.DataAccess.Repositories;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Areas.Tickets.Controllers
{
    public class CustomFieldController : Controller
    {
        private readonly ICustomFieldRepository _customeFieldRepository;


        public CustomFieldController() : this(new CustomFieldRepository())
        {
        }


        public CustomFieldController(ICustomFieldRepository customeFieldRepository)
        {
            _customeFieldRepository = customeFieldRepository;
        }

        // GET: CustomField
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Central()
        {
            return View();
        }




        public JsonResult List(int? rootId)
        {
            var list = _customeFieldRepository.GetList(rootId);
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Update(CustomField record)
        {
            _customeFieldRepository.Update(record);
            return Json(record);
        }



        [HttpPost]
        public JsonResult Insert(CustomField record)
        {
            _customeFieldRepository.Insert(record);
            return Json(record);
        }



        [HttpPost]
        public JsonResult InsertList(IList<CustomField> record)
        {
            foreach (var customField in record)
            {
                _customeFieldRepository.Insert(customField);
            }

            return Json(record);
        }


        [HttpPost]
        public JsonResult Delete(int id)
        {
            _customeFieldRepository.Delete(id);
            return Json("0");
        }


        /// <summary>
        /// Method to update bunch of Custom Fields
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateCustomFieldCollection(CustomFieldCollection data)
        {
            _customeFieldRepository.Update(data);

            return Json("Ok");
        }
    }
}