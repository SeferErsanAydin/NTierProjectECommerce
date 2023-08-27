using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Project.MVCUI.Tools
{
    public static class ImageUploader
    {
        //our string value method will return image's path. If there is a problem with image upload or image itself it'll return its error code "1", "2", "3", "C:/Images/..."

        //if there is a file being uploaded in MVC architecture this file is saved at HttpPostedFileBase type
        public static string UploadImage(string serverPath, HttpPostedFileBase file, string name)
        {
            if (file != null)
            {
                Guid uniqueName = Guid.NewGuid();

                string[] fileArray = file.FileName.Split('.');//this method splits incoming image's name by '.' and last element of the array will give us the extension.

                string extension = fileArray[fileArray.Length - 1].ToLower(); //we get the extension

                string fileName = $"{uniqueName}.{name}.{extension}"; //since we're using Guid while setting the file name there wont be a same file name/path.


                //below code is if we dont use Guid in our image path/name
                if (extension == "jpg" || extension == "gif" || extension == "png")
                {
                    //if file name already exists
                    if (File.Exists(HttpContext.Current.Server.MapPath(serverPath + fileName)))
                    {
                        return "1";//since we used Guid we're safe "1" is the code for there is a file with that name already)
                    }
                    else
                    {
                        string filePath = HttpContext.Current.Server.MapPath(serverPath + fileName);
                        file.SaveAs(filePath);
                        return serverPath + fileName;
                    }
                }
                else
                {
                    return "2"; //added file is not a picture
                }
            }
            else 
            {
                return "3"; //no picture
            }
        }
    }
}