using System.Collections.Generic;
using System.Web.Mvc;
using LSports.Framework.DataAccess.Repositories;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Areas.Tickets.Controllers
{
    public class FormTemplateController : Controller
    {
        private readonly IFormTemplateRepository _formTemplateRepository;


        public FormTemplateController() : this(new FormTemplateRepository())
        {
        }


        public FormTemplateController(IFormTemplateRepository formTemplateRepository)
        {
            _formTemplateRepository = formTemplateRepository;
        }

        // GET: FormTemplate
        public ActionResult Index()
        {
            return View();
        }




        public JsonResult List()
        {
            var list = _formTemplateRepository.GetList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Update(FormTemplate record)
        {
            _formTemplateRepository.Update(record);
            return Json(record);
        }



        [HttpPost]
        public JsonResult Insert(FormTemplate record)
        {
            _formTemplateRepository.Insert(record);
            return Json(record);
        }


        [HttpPost]
        public JsonResult Delete(int id)
        {
            _formTemplateRepository.Delete(id);
            return Json("0");
        }


        [HttpPost]
        public JsonResult GetCustomFields(int formTemplateId)
        {
            var result = _formTemplateRepository.GetCustomFields(formTemplateId);

            return Json(result);
        }


        [HttpPost]
        public JsonResult UpdateCustomFields(int formTemplateId, IList<FormTemplateCustomField> customFields)
        {
            _formTemplateRepository.UpdateCustomFields(formTemplateId, customFields);

            return Json("0");
        }



        /// <summary>
        /// Action to update Form Templates Sort Order
        /// </summary>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateSortOrder(IList<SortOrderItem> sortOrder)
        {
            _formTemplateRepository.UpdateSortOrder(sortOrder);
            return Json("0");
        }


        [HttpPost]
        public JsonResult Validate(FormTemplate record)
        {
            var errors = _formTemplateRepository.Validate(record);
            return Json(errors);
        }
    }
}