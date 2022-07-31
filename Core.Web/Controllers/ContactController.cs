using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Interfaces;
using Core.Application.ViewModels.Common;
using Core.Models;
using Core.Services;
using Core.Utilities.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Core.Controllers
{
    public class ContactController : Controller
    {
        private readonly IFeedbackService _feedbackService;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly IViewRenderService _viewRenderService;

        public ContactController(
            IViewRenderService viewRenderService,
            IConfiguration configuration,
            IEmailSender emailSender, IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
            _emailSender = emailSender;
            _configuration = configuration;
            _viewRenderService = viewRenderService;
        }

        [HttpPost]
        public async Task<IActionResult> SaveEntity(FeedbackViewModel model)
        {
            try
            {
                _feedbackService.Add(model);
                _feedbackService.SaveChanges();

                var content = await _viewRenderService.RenderToStringAsync("Contact/_ContactMail", model);
                await _emailSender.SendEmailAsync(_configuration["MailSettings:AdminMail"], "Contact from website metadefi.network", content);

                return new OkObjectResult(true);
            }
            catch (Exception ex)
            {
                return new OkObjectResult(false);
            }
        }

        [HttpGet]
        public IActionResult ContactMail()
        {
            return View("_ContactMail");
        }
    }
}
