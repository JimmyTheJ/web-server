using MimeMapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using VueServer.Domain.Enums;

namespace VueServer.Domain.Helper
{
    public static class MimeTypeHelper
    {
        private static IReadOnlyDictionary<string, MimeFileType> FileTypeMappings = new Dictionary<string, MimeFileType>
        {
            {  MimeUtility.UnknownMimeType, MimeFileType.Other },

            // Document

            // Text
            { "text/css", MimeFileType.Text },
            { "text/csv", MimeFileType.Text },
            { "text/html", MimeFileType.Text },
            { "text/plain", MimeFileType.Text },
            { "text/richtext", MimeFileType.Text },

            // Images            
            { "image/gif", MimeFileType.Photo },
            { "image/pjpeg", MimeFileType.Photo },
            { "image/jpeg", MimeFileType.Photo },
            { "image/png", MimeFileType.Photo },
            { "image/svg+xml", MimeFileType.Photo },
            { "image/bmp", MimeFileType.Photo },
            { "image/tiff", MimeFileType.Photo },
            { "image/x-icon", MimeFileType.Photo },

            // Audio
            { "audio/midi", MimeFileType.Audio },
            { "audio/mp4", MimeFileType.Audio },
            { "audio/mpeg", MimeFileType.Audio },
            { "audio/ogg", MimeFileType.Audio },
            { "audio/webm", MimeFileType.Audio },
            { "audio/x-wav", MimeFileType.Audio },

            // Video
            { "video/h263", MimeFileType.Video },
            { "video/h264", MimeFileType.Video },
            { "video/jpeg", MimeFileType.Video },
            { "video/mp4", MimeFileType.Video },
            { "video/mpeg", MimeFileType.Video },
            { "video/ogg", MimeFileType.Video },
            { "video/quicktime", MimeFileType.Video },
            { "video/webm", MimeFileType.Video },
            { "video/x-ms-wmv", MimeFileType.Video },
            { "video/x-msvideo", MimeFileType.Video },
            { "video/x-matroska", MimeFileType.Video }
        };

        public static string GetMimeType (string file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                return string.Empty;
            }

            if (!file.Contains("."))
            {
                return Constants.Helper.MIMETYPE_FOLDER;
            }

            string mimeType;

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
                mimeType = MimeUtility.UnknownMimeType;
            }
            //}
            return mimeType;
        }

        public static MimeFileType GetFileType(string mimeType)
        {
            if (string.IsNullOrWhiteSpace(mimeType))
                return MimeFileType.Other;

            if (FileTypeMappings.TryGetValue(mimeType, out MimeFileType value))
            {
                return value;
            }
            else
            {
                return MimeFileType.Other;
            }
        }
    }
}
