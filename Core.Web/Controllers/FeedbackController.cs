using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Interfaces;
using Core.Application.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Core.Controllers
{
    public class FeedbackController : Controller
    {
        public IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost]
        public IActionResult SaveEntity(FeedbackViewModel model)
        {
            try
            {
                _feedbackService.Add(model);
                _feedbackService.SaveChanges();
                return new OkObjectResult(true);
            }
            catch (Exception)
            {
                return new OkObjectResult(false);
            }
        }
    }
}
