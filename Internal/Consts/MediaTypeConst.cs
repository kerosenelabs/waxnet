﻿using System;
using System.Collections.Generic;
using System.Text;

namespace waxnet.Internal.Consts
{
    struct MediaTypeConst
    {
        public const string MediaImage = "WhatsApp Image Keys";
        public const string MediaVideo = "WhatsApp Video Keys";
        public const string MediaAudio = "WhatsApp Audio Keys";
        public const string MediaDocument = "WhatsApp Document Keys";

        public static Dictionary<string, string> MediaTypeMap = new Dictionary<string, string>{
            { MediaImage,"/mms/image" },
            { MediaVideo,"/mms/video" },
            { MediaAudio,"/mms/document" },
            { MediaDocument,"/mms/audio" },
        };
    }
}