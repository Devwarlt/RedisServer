﻿using System.IO;

namespace server.app
{
    internal class getLanguageStrings : RequestHandler
    {
        protected override void HandleRequest()
        {
            WriteLine(File.ReadAllText("resources/app/languageStrings.txt"), false);
        }
    }
}