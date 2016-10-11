using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco_Dev_cm.Models;

namespace Umbraco_Dev_cm.Controllers
{
    public class BookingController : SurfaceController
    {
        [HttpPost]
        public ActionResult submit(BookingInfo lunch)
        {
            //Check to see if the submission is valid
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            //get the datatype for the Lunch Choices so we can work with it
            IDataTypeDefinition datatype = Services.DataTypeService.GetDataTypeDefinitionByName("Lunch Choices");
            DataTypeService dts = (DataTypeService)Services.DataTypeService;

            //Get the parent ID of content you want to attach the submission to
            var categoryContentType = Services.ContentTypeService.GetContentType("Lunchformpage");
            var parent = Services.ContentService.GetContentOfContentType(categoryContentType.Id).FirstOrDefault(x => x.Name == "Lunch Chooser");

            //Create a new content item under a given parent
            var contentService = Services.ContentService;
            var LunchContent = contentService.CreateContent(
                lunch.FirstName + " " + lunch.LastName + DateTime.Now, //Name of the new Lunch Submission content
                parent.Id, //Content Parent ID, where these submissions will live under
                "Lunchchoicedata", // the alias of the Document Type we are adding
                0); //the user adding the record, 0 is the admin

            //Get the preValue of selected lunch choice
            //The dropdown on our form only contained the text and not the true value (prevalue),
            //we need to find the prevalue id and enter the prevalue id into Umbraco
            var statusEditor = dts.GetAllDataTypeDefinitions().First(x => x.Name == "Lunch Choices");
            int preValueId = dts.GetPreValuesCollectionByDataTypeId(statusEditor.Id).PreValuesAsDictionary.Where(d => d.Value.Value == lunch.LunchChoice).Select(f => f.Value.Id).First();

            //Set the values of each property so that we can add the new item to Umbraco
            LunchContent.SetValue("firstName", lunch.FirstName);
            LunchContent.SetValue("lastName", lunch.LastName);
            LunchContent.SetValue("lunchChoice", preValueId);

            // publish and save the new content
            contentService.SaveAndPublishWithStatus(LunchContent);

            //Return to the same page
            return CurrentUmbracoPage();
        }
    }
}