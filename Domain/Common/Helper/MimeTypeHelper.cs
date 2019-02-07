using MimeMapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace VueServer.Domain.Helper
{
    public static class MimeTypeHelper
    {
        public static string GetMimeType (string file)
        {
            string mimeType = null;
            string suffix = file.Substring(file.Length - 4);
            if (suffix.Contains("."))
                suffix = suffix.Substring(suffix.LastIndexOf("."));

            // Special rules on non-default supported types
            //if (suffix.Equals(".py"))
            //{
            //    mimeType = "application/x-python-code";
            //}
            // Get file type naturally or set to folder mode
            //else
            //{
                try
                {
                    mimeType = MimeUtility.GetMimeMapping(file);
                }
                catch (Exception)
                {
                    Console.WriteLine("Error getting Mime-type, asumming it's a folder...");
                    mimeType = "application/octet-stream";
                }
            //}
            return mimeType;
        }
    }
}
